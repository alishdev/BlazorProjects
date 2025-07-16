using Microsoft.AspNetCore.Mvc;
using Syncfusion.Blazor.FileManager;
using System.Text.Json;

namespace TestSyncfusionBlazorCursor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileManagerController : ControllerBase
    {
        private readonly Dictionary<string, object> _fileSystemData;

        public FileManagerController()
        {
            // Initialize the file system data from the JSON
            var jsonData = @"{
              ""Programs"": {
                ""Camps"": {
                  ""Summer Camps"": [
                    ""camps_dancelcamp.pdf"",
                    ""camps_parkvillecamp.pdf"",
                    ""camps_catonsvillecamp.pdf"",
                    ""camps_wardcamp.pdf"",
                    ""camps_hillcamp.pdf"",
                    ""camps_pasadenacamp.pdf"",
                    ""programs_generalinfoforms.pdf""
                  ],
                  ""Indoor Camps"": [
                    ""camps_highlandscamp.pdf""
                  ],
                  ""Day & Overnight Camp"": [
                    ""camps_camphashawha.pdf""
                  ],
                  ""Camp Whippoorwill"": [
                    ""camps_campwhippoorwill.pdf""
                  ],
                  ""Day and Overnight Camps"": [
                    ""camps_puhtok.pdf""
                  ],
                  ""2025 Camp Locations"": [
                    ""programs_camplocations.pdf""
                  ],
                  ""Overnight Camps"": [
                    ""programs_overnightcamps.pdf""
                  ],
                  ""Teen Adventure Day Camp"": [
                    ""programs_teenleadershipcamps.pdf""
                  ],
                  ""Specialty Day Camps"": [
                    ""programs_specialtycamps.pdf""
                  ],
                  ""FAQs"": [
                    ""programs_ycampfaqs.pdf""
                  ],
                  ""Outdoor Adventure Day Camps"": [
                    ""programs_outdooradventurecamps.pdf""
                  ],
                  ""Day Camps"": [
                    ""programs_traditionaldaycamps.pdf""
                  ]
                },
                ""Community Schools"": {
                  ""Moravia Park Elementary"": [
                    ""locations_commoost_moraviapark.pdf""
                  ],
                  ""Fort Worthington Elementary/Middle"": [
                    ""locations_commoost_fortworthington.pdf""
                  ],
                  ""Graceland Park-O'Donnell Heights"": [
                    ""locations_commoost_gracelandparkodonnellheights.pdf""
                  ],
                  ""Waverly Elementary"": [
                    ""locations_commoost_waverly.pdf""
                  ]
                },
                ""Before & After School Enrichment"": {
                  ""Harford County Locations"": [
                    ""area_harford-county.pdf""
                  ],
                  ""Mechanicsville Elementary"": [
                    ""locations_beforeafterschoolenrichment_mechanicsville.pdf""
                  ],
                  ""Monarch Global Academy"": [
                    ""locations_beforeafterschoolenrichment_monarchglobal.pdf""
                  ],
                  ""Youth's Benefit Elementary"": [
                    ""locations_beforeafterschoolenrichment_youthsbenefit.pdf""
                  ],
                  ""Jessup Elementary"": [
                    ""locations_beforeafterschoolenrichment_jessup.pdf""
                  ],
                  ""Catonsville Elementary"": [
                    ""locations_beforeafterschoolenrichment_catonsville.pdf""
                  ],
                  ""Clay Hill Public Charter School"": [
                    ""locations_beforeafterschoolenrichment_clayhill.pdf""
                  ],
                  ""Red Pump Elementary"": [
                    ""locations_beforeafterschoolenrichment_redpump.pdf""
                  ],
                  ""Churchville Elementary"": [
                    ""locations_beforeafterschoolenrichment_churchville.pdf""
                  ],
                  ""Homestead-Wakefield Elementary"": [
                    ""locations_beforeafterschoolenrichment_homesteadwakefield.pdf""
                  ],
                  ""Bel Air Elementary"": [
                    ""locations_beforeafterschoolenrichment_belair.pdf""
                  ],
                  ""Rippling Woods Elementary"": [
                    ""locations_beforeafterschoolenrichment_ripplingwoods.pdf""
                  ],
                  ""Lake Shore Elementary"": [
                    ""locations_beforeafterschoolenrichment_lakeshore.pdf""
                  ],
                  ""Meadowvale Elementary"": [
                    ""locations_beforeafterschoolenrichment_meadowvale.pdf""
                  ],
                  ""Riverside Elementary"": [
                    ""locations_beforeafterschoolenrichment_riverside.pdf""
                  ],
                  ""George Cromwell Elementary"": [
                    ""locations_beforeafterschoolenrichment_georgecromwell.pdf""
                  ],
                  ""Westowne Elementary"": [
                    ""locations_beforeafterschoolenrichment_westowne.pdf""
                  ],
                  ""William Winchester Elementary"": [
                    ""locations_beforeafterschoolenrichment_williamwinchester.pdf""
                  ],
                  ""Monarch Academy Glen Burnie"": [
                    ""locations_beforeafterschoolenrichment_monarchglenburnie.pdf"",
                    ""locations_y-after-school-enrichment-monarch-academy-glenburnie.pdf""
                  ],
                  ""Betty Sterner Y Preschool in Catonsville"": [
                    ""locations_beforeafterschoolenrichment_bettysternercatonsville.pdf""
                  ],
                  ""Windsor Farm Elementary"": [
                    ""locations_beforeafterschoolenrichment_windsorfarm.pdf""
                  ],
                  ""Prospect Mill Elementary"": [
                    ""locations_beforeafterschoolenrichment_prospectmill.pdf""
                  ],
                  ""Monarch Academy Annapolis"": [
                    ""locations_beforeafterschoolenrichment_monarchannapolis.pdf""
                  ],
                  ""Cranberry Station Elementary"": [
                    ""locations_beforeafterschoolenrichment_cranberrystation.pdf""
                  ],
                  ""Robert Moton Elementary"": [
                    ""locations_beforeafterschoolenrichment_robertmoton.pdf""
                  ],
                  ""Jacobsville Elementary"": [
                    ""locations_beforeafterschoolenrichment_jacobsville.pdf""
                  ]
                },
                ""Head Start"": {
                  ""Fleming Center"": [
                    ""locations_headstart_baltimorecounty_fleming.pdf""
                  ],
                  ""Dickey Hill Center"": [
                    ""locations_headstart_baltimorecity_dickeyhill.pdf""
                  ],
                  ""Campfield Center"": [
                    ""locations_headstart_baltimorecounty_campfield.pdf""
                  ],
                  ""East Fayette Center"": [
                    ""locations_headstart_baltimorecity_eastfayette.pdf""
                  ],
                  ""Back River Center"": [
                    ""locations_headstart_baltimorecounty_backriver.pdf""
                  ],
                  ""Baltimore County - Towson"": [
                    ""locations_headstart_baltimorecounty_towson.pdf""
                  ],
                  ""Baltimore City - Sherwood Center"": [
                    ""locations_headstart_baltimorecity_sherwood.pdf""
                  ],
                  ""Baltimore City - West Preston Center"": [
                    ""locations_headstart_baltimorecity_westpreston.pdf""
                  ],
                  ""Baltimore City - Belair Center"": [
                    ""locations_headstart_baltimorecity_belair.pdf""
                  ],
                  ""Early Head Start - Kenwood Center"": [
                    ""locations_headstart_baltimorecounty_kenwood.pdf""
                  ],
                  ""Lloyd Keaser Center"": [
                    ""locations_headstart_annearundel_lloydkeaser.pdf""
                  ],
                  ""Randallstown Center"": [
                    ""locations_headstart_baltimorecounty_randallstown.pdf""
                  ],
                  ""Sherman Early Childhood Center"": [
                    ""locations_y-head-start-sherman-early-childhood-center.pdf""
                  ],
                  ""Riverview Center"": [
                    ""locations_headstart_baltimorecounty_riverview.pdf""
                  ],
                  ""Early Head Start - Eastern Family Resource Center"": [
                    ""locations_y-head-startearly-head-start-eastern-family-resource-center.pdf""
                  ]
                },
                ""Mentoring"": {
                  ""Volunteer Opportunities"": [
                    ""volunteer_mentoring.pdf""
                  ]
                },
                ""Early Head Start"": {
                  ""Merritt Park Center II"": [
                    ""locations_y-early-head-start-merritt-park-center-ii.pdf""
                  ]
                },
                ""Registration"": {
                  ""Y Family Center Program Sessions"": [
                    ""register.pdf""
                  ]
                },
                ""Financial Assistance"": {
                  ""Y Open Doors Savings"": [
                    ""opendoors.pdf""
                  ]
                },
                ""After School Programs"": {
                  ""Y Achievers at the Weinberg Y in Waverly"": [
                    ""locations_outofschooltime_achievers.pdf""
                  ]
                },
                ""Out-of-School Time"": {
                  ""College Gardens"": [
                    ""locations_outofschooltime_collegegardens.pdf""
                  ],
                  ""Johnston Square Elementary"": [
                    ""locations_outofschooltime_johnstonsquare.pdf""
                  ]
                },
                ""Out-of-School-Time"": {
                  ""Goodnow Community Center"": [
                    ""locations_outofschooltime_goodnow.pdf""
                  ]
                },
                ""Community School Programs"": {
                  ""Holabird Academy Elementary/Middle"": [
                    ""locations_commoost_holabirdacademy.pdf""
                  ]
                },
                ""Group Exercise and Activities"": {
                  ""Swimming and Climbing Schedules"": [
                    ""schedules.pdf""
                  ]
                },
                ""Achievement Gap Intervention"": {
                  ""Community Schools"": [
                    ""programs_achievementgap_communityschools.pdf""
                  ]
                },
                ""Swimming"": {
                  ""Swim Band Guidelines"": [
                    ""YCM%20Swim%20Guidelines%20By%20Age%20-%20Updated%202.26.25.pdf""
                  ]
                }
              },
              ""Locations"": {
                ""YMCA Facilities"": {
                  ""Dundalk Swim Center"": [
                    ""locations_dundalky.pdf""
                  ]
                },
                ""Amenities"": {
                  ""The Y in Ellicott City (Dancel)"": [
                    ""locations_dancely_amenities.pdf""
                  ]
                },
                ""Dancel Y"": {
                  ""Schedules"": [
                    ""locations_dancely_dancelschedules.pdf""
                  ]
                },
                ""Howard County"": {
                  ""The Y in Ellicott City (Dancel)"": [
                    ""area_howard-county.pdf""
                  ]
                },
                ""Community Centers"": {
                  ""Hill Y in Westminster"": [
                    ""locations_hilly.pdf""
                  ],
                  ""Baltimore County"": [
                    ""area_baltimore-county.pdf""
                  ],
                  ""OroKawa Y in Towson"": [
                    ""locations_orokaway.pdf""
                  ],
                  ""Baltimore City"": [
                    ""area_baltimore-city.pdf""
                  ],
                  ""Anne Arundel County"": [
                    ""area_anne-arundel-county.pdf""
                  ],
                  ""The Y in Abingdon"": [
                    ""locations_wardy.pdf""
                  ],
                  ""The Y in Parkville"": [
                    ""locations_parkvilley.pdf""
                  ]
                },
                ""Preschools"": {
                  ""Betty Sterner Y Preschool in Catonsville"": [
                    ""locations_ypreschoolcatonsville.pdf""
                  ],
                  ""Y Preschool Towson"": [
                    ""locations_ypreschooltowson.pdf""
                  ]
                },
                ""Community Schools"": {
                  ""Reginald F. Lewis High Community School"": [
                    ""locations_communityschool_reginaldflewis.pdf""
                  ],
                  ""Armistead Gardens Elementary"": [
                    ""locations_communityschool_armistead.pdf""
                  ],
                  ""Patterson High"": [
                    ""locations_communityschool_pattersonhigh.pdf""
                  ],
                  ""Achievement Academy"": [
                    ""locations_communityschool_achievementacademy.pdf""
                  ],
                  ""Frederick Douglass High Community School"": [
                    ""locations_communityschool_frederickdouglass.pdf""
                  ],
                  ""Academy for College & Career Exploration"": [
                    ""locations_communityschool_acce.pdf""
                  ]
                },
                ""YMCA Centers"": {
                  ""Maryland Locations"": [
                    ""locations.pdf""
                  ]
                },
                ""YMCA Branches"": {
                  ""The Y in Ellicott City"": [
                    ""locations_dancely.pdf""
                  ]
                },
                ""Preschool"": {
                  ""Y Preschool at UMBC"": [
                    ""locations_ypreschoolumbc.pdf""
                  ]
                }
              },
              ""Information"": {
                ""Parent Manual"": {
                  ""Stay & Play Overview"": [
                    ""Stay%20%26%20Play%20Parent%20Manual%20-%20Updated%204.7.25.pdf""
                  ]
                },
                ""Contact"": {
                  ""Contact Form"": [
                    ""contactus.pdf""
                  ]
                },
                ""Member Handbook"": {
                  ""Policies and Guidelines"": [
                    ""memberhandbook.pdf""
                  ]
                },
                ""Contact Information"": {
                  ""General Inquiries"": [
                    ""locations_dancely_https3A2F2Foperations.daxko.com2FOnline2F50362FMembership2FMemberAppointments.mvc.pdf""
                  ]
                },
                ""General Info"": {
                  ""Contact Information"": [
                    ""www.weatherbug.com.pdf""
                  ]
                }
              },
              ""Membership"": {
                ""Join Information"": {
                  ""Promotional Offers"": [
                    ""join.pdf""
                  ]
                },
                ""Membership Options"": {
                  ""Membership Types and Rates"": [
                    ""membership_membershipoptions.pdf""
                  ]
                }
              },
              ""Forms"": {
                ""Application"": {
                  ""Membership and Program Application"": [
                    ""2025OpenDoorsFormfill_ENG.pdf""
                  ]
                },
                ""Camps"": {
                  ""Medication Administration Authorization"": [
                    ""Camp_Medication%20Administration%20Authorization%20Form_072120.pdf""
                  ]
                },
                ""Health Forms"": {
                  ""Immunization Certificate"": [
                    ""Maryland%20ImmunizationForm.pdf""
                  ]
                }
              },
              ""Get Involved"": {
                ""Volunteer Opportunities"": {
                  ""Mentoring and Events"": [
                    ""locations_getinvolved.pdf""
                  ],
                  ""Mentoring, Special Event Volunteer, Program Volunteer"": [
                    ""getinvolved.pdf""
                  ]
                }
              }
            }";

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            _fileSystemData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData, options);
        }

        [HttpPost("FileOperations")]
        public IActionResult FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            if (args.Action == "read")
            {
                var path = args.Path ?? "/";
                Console.WriteLine($"FileManager requested path: '{path}'");
                var result = GetFileSystemItems(path);
                Console.WriteLine($"Returning {((dynamic)result).files.Count} items");
                return Ok(result);
            }
            
            return Ok(new { files = new List<object>(), cwd = new { name = "Root", size = 0, isFile = false, dateModified = DateTime.Now, dateCreated = DateTime.Now, hasChild = true, type = "Folder" } });
        }

        [HttpPost("Upload")]
        public IActionResult Upload()
        {
            return Ok(new { files = new List<object>() });
        }

        [HttpGet("Download")]
        public IActionResult Download(string path)
        {
            return NotFound();
        }

        [HttpGet("GetImage")]
        public IActionResult GetImage(string path)
        {
            return NotFound();
        }

        private object GetFileSystemItems(string path)
        {
            var items = new List<object>();
            var currentPath = path.TrimStart('/').TrimEnd('/');
            var pathParts = string.IsNullOrEmpty(currentPath) ? new string[0] : currentPath.Split('/');

            Console.WriteLine($"Path parts: [{string.Join(", ", pathParts)}]");

            object node = _fileSystemData;
            if (pathParts.Length > 0)
            {
                foreach (var part in pathParts)
                {
                    Console.WriteLine($"Looking for part: '{part}' in node type: {node?.GetType().Name}");
                    if (node is Dictionary<string, object> dict)
                    {
                        Console.WriteLine($"Available keys: [{string.Join(", ", dict.Keys)}]");
                        if (dict.ContainsKey(part))
                        {
                            node = dict[part];
                            Console.WriteLine($"Found '{part}', new node type: {node?.GetType().Name}");
                        }
                        else
                        {
                            Console.WriteLine($"Key '{part}' not found in dictionary");
                            node = null;
                            break;
                        }
                    }
                    else if (node is JsonElement jsonElement)
                    {
                        // Handle JsonElement by converting it to the appropriate type
                        if (jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            var convertedDict = new Dictionary<string, object>();
                            foreach (var property in jsonElement.EnumerateObject())
                            {
                                convertedDict[property.Name] = ConvertJsonElement(property.Value);
                            }
                            node = convertedDict;
                            
                            Console.WriteLine($"Converted JsonElement to Dictionary, available keys: [{string.Join(", ", convertedDict.Keys)}]");
                            if (convertedDict.ContainsKey(part))
                            {
                                node = convertedDict[part];
                                Console.WriteLine($"Found '{part}', new node type: {node?.GetType().Name}");
                            }
                            else
                            {
                                Console.WriteLine($"Key '{part}' not found in converted dictionary");
                                node = null;
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"JsonElement is not an object, it's: {jsonElement.ValueKind}");
                            node = null;
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Node is not a dictionary or JsonElement, it's: {node?.GetType().Name}");
                        node = null;
                        break;
                    }
                }
            }

            Console.WriteLine($"Final node type: {node?.GetType().Name}");

            if (node == null)
            {
                // Not found, return empty
                return new
                {
                    files = items,
                    cwd = new
                    {
                        name = string.IsNullOrEmpty(currentPath) ? "Root" : pathParts.LastOrDefault() ?? "Root",
                        size = 0,
                        isFile = false,
                        dateModified = DateTime.Now,
                        dateCreated = DateTime.Now,
                        hasChild = false,
                        type = "Folder"
                    }
                };
            }

            // Convert JsonElement to appropriate type if needed
            if (node is JsonElement jsonNode)
            {
                node = ConvertJsonElement(jsonNode);
            }

            // If at root, show top-level folders
            if (pathParts.Length == 0 && node is Dictionary<string, object> rootDict)
            {
                foreach (var item in rootDict)
                {
                    items.Add(new
                    {
                        name = item.Key,
                        size = 0,
                        isFile = false,
                        dateModified = DateTime.Now,
                        dateCreated = DateTime.Now,
                        hasChild = true, // All top-level folders are expandable
                        type = "Folder"
                    });
                }
            }
            else if (node is Dictionary<string, object> folderData)
            {
                Console.WriteLine($"Processing folder with {folderData.Count} items");
                foreach (var item in folderData)
                {
                    Console.WriteLine($"Item: {item.Key}, Type: {item.Value?.GetType().Name}");
                    if (item.Value is Dictionary<string, object> || item.Value is JsonElement)
                    {
                        // Subfolder
                        items.Add(new
                        {
                            name = item.Key,
                            size = 0,
                            isFile = false,
                            dateModified = DateTime.Now,
                            dateCreated = DateTime.Now,
                            hasChild = true,
                            type = "Folder"
                        });
                    }
                    else if (item.Value is List<object>)
                    {
                        // Folder containing files (clicking this will show files)
                        items.Add(new
                        {
                            name = item.Key,
                            size = 0,
                            isFile = false,
                            dateModified = DateTime.Now,
                            dateCreated = DateTime.Now,
                            hasChild = true, // Mark as expandable to show files
                            type = "Folder"
                        });
                    }
                }
            }
            else if (node is List<object> files)
            {
                Console.WriteLine($"Processing files list with {files.Count} files");
                foreach (var file in files)
                {
                    items.Add(new
                    {
                        name = file.ToString(),
                        size = 1024, // Mock size
                        isFile = true,
                        dateModified = DateTime.Now,
                        dateCreated = DateTime.Now,
                        hasChild = false,
                        type = "File"
                    });
                }
            }

            Console.WriteLine($"Returning {items.Count} items");
            return new
            {
                files = items,
                cwd = new
                {
                    name = string.IsNullOrEmpty(currentPath) ? "Root" : pathParts.LastOrDefault() ?? "Root",
                    size = 0,
                    isFile = false,
                    dateModified = DateTime.Now,
                    dateCreated = DateTime.Now,
                    hasChild = items.Any(i => !(i as dynamic).isFile),
                    type = "Folder"
                }
            };
        }

        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        dict[property.Name] = ConvertJsonElement(property.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    return element.GetInt64();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                default:
                    return null;
            }
        }
    }
} 