using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms; // For MessageBox
using Microsoft.Win32; // For Registry operations

namespace SendToLibrarian
{
    // --- COM Interface Definitions (Simplified) ---

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e8-0000-0000-c000-000000000046")]
    internal interface IShellExtInit
    {
        void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hkeyProgID);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e4-0000-0000-c000-000000000046")]
    internal interface IContextMenu
    {
        [PreserveSig]
        int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);

        [PreserveSig]
        int InvokeCommand(IntPtr pici);

        [PreserveSig]
        int GetCommandString(uint idCmd, uint uType, IntPtr pReserved, IntPtr pszName, uint cchMax);
    }

    // Structure for InvokeCommand
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CMINVOKECOMMANDINFO
    {
        public uint cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public IntPtr lpVerb;
        public string lpParameters;
        public string lpDirectory;
        public int nShow;
        public uint dwHotKey;
        public IntPtr hIcon;
    }

    // --- Shell Extension Implementation ---

    [ComVisible(true)]
    [Guid("A2F5C3A1-7B4F-4BF8-A1DE-858C68F56B37")] // Replace with your own unique GUID
    public class SendToLibrarianHandler : IShellExtInit, IContextMenu
    {
        private List<string> selectedFiles = new List<string>();
        private const uint IDM_SENDTOLIBRARIAN = 0; // Command ID

        // IShellExtInit Implementation
        public void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hkeyProgID)
        {
            selectedFiles.Clear();

            if (pDataObj == IntPtr.Zero)
            {
                return;
            }

            // Use FORMATETC and STGMEDIUM to get file paths from the data object
            FORMATETC formatEtc = new FORMATETC
            {
                cfFormat = CLIPFORMAT.CF_HDROP,
                ptd = IntPtr.Zero,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                tymed = TYMED.TYMED_HGLOBAL
            };

            STGMEDIUM medium = new STGMEDIUM();

            // Using IDataObject interface (implicitly available from pDataObj)
            IDataObject dataObject = (IDataObject)Marshal.GetObjectForIUnknown(pDataObj);

            try
            {
                dataObject.GetData(ref formatEtc, out medium);

                IntPtr hDrop = medium.unionmember;
                if (hDrop != IntPtr.Zero)
                {
                    uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                    StringBuilder sb = new StringBuilder(260);
                    for (uint i = 0; i < count; i++)
                    {
                        if (DragQueryFile(hDrop, i, sb, (uint)sb.Capacity) > 0)
                        {
                            selectedFiles.Add(sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or show error - simplified for example
                MessageBox.Show("Error initializing SendToLibrarian: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ReleaseStgMedium(ref medium);
            }
        }

        // IContextMenu Implementation
        public int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags)
        {
            // If the flags include CMF_DEFAULTONLY, don't do anything
            if ((uFlags & 0x00000001 /* CMF_NORMAL */) == 0 && (uFlags & 0x00000010 /* CMF_EXPLORE */) == 0)
                return HRESULT.MAKE_HRESULT(0, 0, 0); // Severity_SUCCESS, Facility_NULL, Code 0

            // Add our menu item - Explicitly cast uint flags to int for the call
            InsertMenu(hMenu, indexMenu, (int)(MF_BYPOSITION | MF_STRING), (int)(idCmdFirst + IDM_SENDTOLIBRARIAN), "Send To Librarian");

            // Return the HRESULT indicating success and the number of items added
            // (Highest command ID + 1)
            return HRESULT.MAKE_HRESULT(0, 0, (int)(IDM_SENDTOLIBRARIAN + 1));
        }

        public int InvokeCommand(IntPtr pici)
        {
            CMINVOKECOMMANDINFO ici = (CMINVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typeof(CMINVOKECOMMANDINFO));

            // Check if the command ID matches ours (low-word of lpVerb)
            uint cmdId;
            if (Environment.Is64BitProcess && ici.cbSize >= Marshal.SizeOf(typeof(CMINVOKECOMMANDINFO))) // Check size for potential CMINVOKECOMMANDINFOEX
            {
                 // For newer systems/CMINVOKECOMMANDINFOEX, verb might be offset
                 // This is a simplified check. Real world needs more robust handling.
                 if (ici.lpVerb != IntPtr.Zero && (uint)ici.lpVerb <= ushort.MaxValue)
                     cmdId = (uint)ici.lpVerb;
                 else
                     return HRESULT.E_FAIL; // Unrecognized command
            }
            else if (!Environment.Is64BitProcess && ici.cbSize == Marshal.SizeOf(typeof(CMINVOKECOMMANDINFO)))
            {
                 if (ici.lpVerb != IntPtr.Zero && (uint)ici.lpVerb <= ushort.MaxValue)
                     cmdId = (uint)ici.lpVerb;
                 else
                     return HRESULT.E_FAIL; // Unrecognized command
            }
            else
            {
                 // Could be a string verb - not handling this case here
                 return HRESULT.E_FAIL;
            }


            if (cmdId == IDM_SENDTOLIBRARIAN)
            {
                int successCount = 0;
                int failCount = 0;
                List<string> errors = new List<string>();

                foreach (string filePath in selectedFiles)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            string dir = Path.GetDirectoryName(filePath);
                            string baseName = Path.GetFileNameWithoutExtension(filePath);
                            string newPath = Path.Combine(dir, baseName + ".librarian");

                            File.Copy(filePath, newPath, true); // true to overwrite
                            successCount++;
                        }
                        else
                        {
                           errors.Add($"File not found: {Path.GetFileName(filePath)}");
                           failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to copy {Path.GetFileName(filePath)}: {ex.Message}");
                        failCount++;
                    }
                }

                // Optional: Show summary message
                string message = $"{successCount} file(s) sent to Librarian (copied as .librarian).";
                if (failCount > 0)
                {
                    message += $"\n\n{failCount} file(s) failed:\n" + string.Join("\n", errors);
                }
                MessageBox.Show(message, "Send To Librarian Result", MessageBoxButtons.OK, failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                return HRESULT.S_OK;
            }

            return HRESULT.E_FAIL; // Command not recognized
        }

        public int GetCommandString(uint idCmd, uint uType, IntPtr pReserved, IntPtr pszName, uint cchMax)
        {
            // GCS_VERBA = 0, GCS_HELPTEXTA = 1, GCS_VALIDATEA = 2
            // GCS_VERBW = 4, GCS_HELPTEXTW = 5, GCS_VALIDATEW = 6
            // GCS_UNICODE = 4

            if (idCmd == IDM_SENDTOLIBRARIAN)
            {
                switch (uType)
                {
                    case 0: // GCS_VERBA or GCS_VERBW (ANSI or Unicode verb)
                    case 4:
                        // Optional: Can provide a canonical verb name
                        string verb = "SendToLibrarian";
                        if (cchMax > verb.Length)
                        {
                           Marshal.Copy(verb.ToCharArray(), 0, pszName, verb.Length);
                           Marshal.WriteInt16(pszName, verb.Length * 2, 0); // Null terminate
                           return HRESULT.S_OK;
                        }
                        break;
                    case 1: // GCS_HELPTEXTA or GCS_HELPTEXTW (Tooltip/Status bar text)
                    case 5:
                        string helpText = "Copies the selected file(s) with a .librarian extension";
                         if (cchMax > helpText.Length)
                        {
                            Marshal.Copy(helpText.ToCharArray(), 0, pszName, helpText.Length);
                            Marshal.WriteInt16(pszName, helpText.Length * 2, 0); // Null terminate
                            return HRESULT.S_OK;
                        }
                        break;
                    case 2: // GCS_VALIDATEA or GCS_VALIDATEW
                    case 6:
                        return HRESULT.S_OK; // Validate command exists
                }
            }
            return HRESULT.E_INVALIDARG;
        }

        // --- COM Registration --- 
        // Called by regasm /codebase or when RegisterForComInterop=true
        [ComRegisterFunction()]
        public static void Register(Type t)
        {
            try
            {
                 // Key path for Send To menu for all file types
                 string sendToPath = @"Software\Classes\*\shellex\ContextMenuHandlers\SendToLibrarian"; // Use a descriptive name 

                 using (RegistryKey key = Registry.CurrentUser.CreateSubKey(sendToPath))
                 {
                     if (key != null)
                     {
                         key.SetValue(null, t.GUID.ToString("B")); // Set default value to CLSID {GUID}
                     }
                     else
                     {
                        MessageBox.Show($"Failed to create registry key: HKCU\\{sendToPath}", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     }
                 }

                 // Also register under Approved shell extensions (recommended)
                 string approvedPath = @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved";
                 using(RegistryKey key = Registry.LocalMachine.CreateSubKey(approvedPath)) // Requires admin
                 {
                     if(key != null)
                     {
                          key.SetValue(t.GUID.ToString("B"), "SendToLibrarian Handler");
                     }
                     else
                     {
                         // Might fail if not running as admin - this part is optional but good practice
                         Console.WriteLine($"Warning: Could not register under HKLM\\{approvedPath}. Run regasm as Administrator.");
                     }
                 }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error during registration: " + ex.ToString(), "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Throwing here might prevent regasm from succeeding
            }
        }

        [ComUnregisterFunction()]
        public static void Unregister(Type t)
        {
           try
           {
                string sendToPath = @"Software\Classes\*\shellex\ContextMenuHandlers\SendToLibrarian";
                Registry.CurrentUser.DeleteSubKeyTree(sendToPath, false); // false: do not throw if not found

                 // Remove from Approved list
                 string approvedPath = @"Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved";
                 try
                 {
                     using(RegistryKey key = Registry.LocalMachine.OpenSubKey(approvedPath, true)) // Requires admin
                     {
                         if(key != null)
                         {
                              key.DeleteValue(t.GUID.ToString("B"), false);
                         }
                     }
                 }
                 catch(Exception ex)
                 {
                      Console.WriteLine($"Warning: Could not unregister from HKLM\\{approvedPath}. Run regasm as Administrator. Error: {ex.Message}");
                 }
           }
           catch(Exception ex)
           {
                 MessageBox.Show("Error during unregistration: " + ex.ToString(), "Unregistration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
        }

        // --- P/Invoke Definitions ---
        internal const int MF_STRING = 0x00000000;
        internal const int MF_BYPOSITION = 0x00000400;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool InsertMenu(IntPtr hMenu, uint uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern uint DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder lpszFile, uint cch);

        [DllImport("ole32.dll")]
        internal static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        // Simplified IDataObject definition (adjust as needed)
        [ComImport, Guid("0000010e-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDataObject
        {
            [PreserveSig]
            int GetData(ref FORMATETC pformatetcIn, out STGMEDIUM pmedium);
            // Add other IDataObject methods if needed
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FORMATETC
        {
            public CLIPFORMAT cfFormat;
            public IntPtr ptd;
            public DVASPECT dwAspect;
            public int lindex;
            public TYMED tymed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STGMEDIUM
        {
            public TYMED tymed;
            public IntPtr unionmember;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object pUnkForRelease;
        }

        public enum CLIPFORMAT : ushort
        {
            CF_TEXT = 1,
            CF_BITMAP = 2,
            CF_METAFILEPICT = 3,
            CF_SYLK = 4,
            CF_DIF = 5,
            CF_TIFF = 6,
            CF_OEMTEXT = 7,
            CF_DIB = 8,
            CF_PALETTE = 9,
            CF_PENDATA = 10,
            CF_RIFF = 11,
            CF_WAVE = 12,
            CF_UNICODETEXT = 13,
            CF_ENHMETAFILE = 14,
            CF_HDROP = 15,
            CF_LOCALE = 16,
            CF_DIBV5 = 17,
            CF_MAX = 18,
            CF_OWNERDISPLAY = 0x80,
            CF_DSPTEXT = 0x81,
            CF_DSPBITMAP = 0x82,
            CF_DSPMETAFILEPICT = 0x83,
            CF_DSPENHMETAFILE = 0x8E,
        }

        [Flags]
        public enum DVASPECT : uint
        {
            DVASPECT_CONTENT = 1,
            DVASPECT_THUMBNAIL = 2,
            DVASPECT_ICON = 4,
            DVASPECT_DOCPRINT = 8
        }

        [Flags]
        public enum TYMED : uint
        {
            TYMED_HGLOBAL = 1,
            TYMED_FILE = 2,
            TYMED_ISTREAM = 4,
            TYMED_ISTORAGE = 8,
            TYMED_GDI = 16,
            TYMED_MFPICT = 32,
            TYMED_ENHMF = 64,
            TYMED_NULL = 0
        }
        // Helper class for HRESULTs
        internal static class HRESULT
        {
            public static int S_OK = 0x00000000;
            public static int E_FAIL = unchecked((int)0x80004005);
            public static int E_INVALIDARG = unchecked((int)0x80070057);
            public static int E_OUTOFMEMORY = unchecked((int)0x8007000E);

            public static int MAKE_HRESULT(int severity, int facility, int code)
            {
                 return (int)((uint)(severity) << 31 | (uint)(facility) << 16 | (uint)(code));
            }
        }
    }
} 