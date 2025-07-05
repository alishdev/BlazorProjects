#!/usr/bin/env python3
"""
Mind Meld - Demo Version
A demonstration version that simulates LLM responses without requiring API keys
"""

import tkinter as tk
from tkinter import ttk, scrolledtext, messagebox
import threading
import queue
import time
import random
from typing import Optional
from database import DatabaseManager

class DemoLLMClient:
    """Demo LLM client that simulates responses"""
    
    def __init__(self):
        self.demo_responses = {
            'A': [
                "I understand you're asking about {topic}. From my perspective, this is an interesting challenge that requires careful consideration of multiple factors.",
                "That's a fascinating question about {topic}! Let me break this down into several key points that might help you think through this.",
                "Regarding {topic}, I think there are several approaches we could consider. Here's my analysis...",
                "This is a great question about {topic}. I'd suggest looking at it from these different angles...",
                "When it comes to {topic}, I believe the key is to understand the underlying principles first. Here's what I mean..."
            ],
            'B': [
                "Interesting question about {topic}! I have a different take on this - let me share my perspective.",
                "From my viewpoint on {topic}, I think we need to consider some alternative approaches that might not be immediately obvious.",
                "That's a thought-provoking question about {topic}. I'd like to offer a different angle on this...",
                "Regarding {topic}, I see this differently than you might expect. Here's my reasoning...",
                "This question about {topic} is quite nuanced. Let me provide a contrasting perspective that might be helpful..."
            ]
        }
    
    def get_llm_name(self, llm_type: str) -> str:
        """Get the display name for an LLM"""
        if llm_type.upper() == 'A':
            return "Demo LLM A: GPT-Simulator"
        elif llm_type.upper() == 'B':
            return "Demo LLM B: Claude-Simulator"
        else:
            return f"Unknown LLM: {llm_type}"
    
    def get_response_sync(self, llm_type: str, messages: list) -> str:
        """Simulate LLM response with delay"""
        # Simulate API delay
        time.sleep(random.uniform(1.0, 3.0))
        
        # Get the last user message
        last_user_msg = ""
        for msg in reversed(messages):
            if msg["role"] == "user":
                last_user_msg = msg["content"]
                break
        
        # Extract topic from message (simple simulation)
        topic = "this topic"
        if last_user_msg:
            words = last_user_msg.split()[:3]  # First 3 words as topic
            topic = " ".join(words)
        
        # Get a random response for this LLM
        responses = self.demo_responses.get(llm_type.upper(), ["I'm not sure about that."])
        response = random.choice(responses).format(topic=topic)
        
        # Add some variety to responses
        variations = [
            f"{response} This is just a demo response to show how the interface works.",
            f"{response} In a real implementation, this would be an actual AI response.",
            f"{response} The real version would connect to actual LLM APIs.",
            f"{response} This simulation helps demonstrate the dual-chat interface."
        ]
        
        return random.choice(variations)

class MindMeldDemoApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Mind Meld - Demo Version (No API Keys Required)")
        self.root.geometry("1200x800")
        
        # Initialize components
        self.db = DatabaseManager()
        self.llm_client = DemoLLMClient()
        self.current_project_id = None
        self.response_queue = queue.Queue()
        
        # Setup UI
        self.setup_ui()
        self.load_projects()
        self.load_last_project()
        
        # Start response handler
        self.check_response_queue()
        
        # Show demo info
        self.show_demo_info()
    
    def setup_ui(self):
        """Setup the user interface"""
        # Configure grid weights - make chat area take full height
        self.root.grid_rowconfigure(1, weight=1)  # Chat area gets all available space
        self.root.grid_columnconfigure(0, weight=1)
        self.root.grid_columnconfigure(1, weight=1)
        
        # Top - Project Bar
        self.setup_project_bar()
        
        # Middle - Chat Panes (full height)
        self.setup_chat_panes()
        
        # Bottom - Prompt Area
        self.setup_prompt_area()
    
    def setup_project_bar(self):
        """Setup the project selection bar"""
        project_frame = ttk.Frame(self.root)
        project_frame.grid(row=0, column=0, columnspan=2, sticky="ew", padx=10, pady=5)
        
        ttk.Label(project_frame, text="Project:").pack(side="left")
        
        # Project dropdown
        self.project_var = tk.StringVar()
        self.project_combo = ttk.Combobox(
            project_frame, 
            textvariable=self.project_var,
            state="readonly",
            width=25
        )
        self.project_combo.pack(side="left", padx=(5, 5))
        self.project_combo.bind("<<ComboboxSelected>>", self.on_project_change)
        
        # New project button
        new_project_btn = ttk.Button(
            project_frame,
            text="+ New Project",
            command=self.create_new_project
        )
        new_project_btn.pack(side="left")
        
        # Demo indicator
        demo_label = ttk.Label(project_frame, text="(DEMO MODE)", foreground="orange")
        demo_label.pack(side="right")
    
    def setup_chat_panes(self):
        """Setup the dual chat windows"""
        chat_frame = ttk.Frame(self.root)
        chat_frame.grid(row=1, column=0, columnspan=2, sticky="nsew", padx=10, pady=(5, 10))
        chat_frame.grid_columnconfigure(0, weight=1)
        chat_frame.grid_columnconfigure(1, weight=1)
        chat_frame.grid_rowconfigure(0, weight=1)  # Make chat windows take full height
        
        # LLM A Chat Window
        llm_a_frame = ttk.LabelFrame(chat_frame, text=self.llm_client.get_llm_name('A'))
        llm_a_frame.grid(row=0, column=0, sticky="nsew", padx=(0, 5))
        llm_a_frame.grid_columnconfigure(0, weight=1)
        llm_a_frame.grid_rowconfigure(0, weight=1)
        
        self.chat_a = scrolledtext.ScrolledText(
            llm_a_frame,
            wrap=tk.WORD,
            width=50,
            height=1,  # Height will be determined by grid weight
            state=tk.DISABLED,
            font=("Arial", 18)  # Increased font size by 50% from 12 to 18
        )
        self.chat_a.grid(row=0, column=0, sticky="nsew", padx=5, pady=5)
        
        # LLM B Chat Window
        llm_b_frame = ttk.LabelFrame(chat_frame, text=self.llm_client.get_llm_name('B'))
        llm_b_frame.grid(row=0, column=1, sticky="nsew", padx=(5, 0))
        llm_b_frame.grid_columnconfigure(0, weight=1)
        llm_b_frame.grid_rowconfigure(0, weight=1)
        
        self.chat_b = scrolledtext.ScrolledText(
            llm_b_frame,
            wrap=tk.WORD,
            width=50,
            height=1,  # Height will be determined by grid weight
            state=tk.DISABLED,
            font=("Arial", 18)  # Increased font size by 50% from 12 to 18
        )
        self.chat_b.grid(row=0, column=0, sticky="nsew", padx=5, pady=5)
    
    def setup_prompt_area(self):
        """Setup the prompt input area"""
        prompt_frame = ttk.Frame(self.root)
        prompt_frame.grid(row=2, column=0, columnspan=2, sticky="ew", padx=10, pady=5)
        prompt_frame.grid_columnconfigure(0, weight=1)
        
        # Prompt input
        self.prompt_var = tk.StringVar()
        self.prompt_entry = ttk.Entry(
            prompt_frame,
            textvariable=self.prompt_var,
            font=("Arial", 21)  # Increased font size by 50% from 14 to 21
        )
        self.prompt_entry.grid(row=0, column=0, sticky="ew", padx=(0, 5), pady=5)
        self.prompt_entry.bind("<Return>", self.send_prompt)
        
        # Send button
        self.send_button = ttk.Button(
            prompt_frame,
            text="Send",
            command=self.send_prompt,
            style="Large.TButton"  # Use larger button style
        )
        self.send_button.grid(row=0, column=1)
    
    def show_demo_info(self):
        """Show demo information"""
        info_text = """
🎯 DEMO MODE - No API Keys Required

This is a demonstration version of Mind Meld that simulates LLM responses.
The interface and functionality are identical to the real version.

Try these example prompts:
• "Tell me about artificial intelligence"
• "How can I improve my writing skills?"
• "What are the best practices for project management?"
• "Explain quantum computing in simple terms"

The responses are simulated but show how the dual-chat interface works.
        """
        
        # Display in both chat windows
        self.display_message('A', 'assistant', info_text.strip())
        self.display_message('B', 'assistant', info_text.strip())
    
    def load_projects(self):
        """Load projects into the dropdown"""
        projects = self.db.get_all_projects()
        project_names = [project[1] for project in projects]
        self.project_combo['values'] = project_names
    
    def load_last_project(self):
        """Load the most recently accessed project"""
        projects = self.db.get_all_projects()
        if projects:
            last_project = projects[0]
            self.current_project_id = last_project[0]
            self.project_var.set(last_project[1])
            self.load_project_history()
    
    def load_project_history(self):
        """Load conversation history for the current project"""
        if not self.current_project_id:
            return
        
        # Clear chat windows
        self.chat_a.config(state=tk.NORMAL)
        self.chat_a.delete(1.0, tk.END)
        self.chat_a.config(state=tk.DISABLED)
        
        self.chat_b.config(state=tk.NORMAL)
        self.chat_b.delete(1.0, tk.END)
        self.chat_b.config(state=tk.DISABLED)
        
        # Load history for both LLMs
        history_a = self.db.get_project_history(self.current_project_id, 'A')
        history_b = self.db.get_project_history(self.current_project_id, 'B')
        
        # Display history
        for timestamp, role, content in history_a:
            self.display_message('A', role, content)
        
        for timestamp, role, content in history_b:
            self.display_message('B', role, content)
    
    def on_project_change(self, event):
        """Handle project selection change"""
        selected_name = self.project_var.get()
        if not selected_name:
            return
        
        # Update last accessed timestamp for previous project
        if self.current_project_id:
            self.db.update_project_access(self.current_project_id)
        
        # Get new project
        project = self.db.get_project_by_name(selected_name)
        if project:
            self.current_project_id = project[0]
            self.load_project_history()
    
    def on_project_keypress(self, event):
        """Handle keypress in project dropdown for creating new projects"""
        if event.keysym == 'Return':
            new_name = self.project_var.get().strip()
            if new_name and new_name not in self.project_combo['values']:
                try:
                    project_id = self.db.create_project(new_name)
                    self.load_projects()
                    self.project_var.set(new_name)
                    self.current_project_id = project_id
                    self.load_project_history()
                except Exception as e:
                    messagebox.showerror("Error", f"Failed to create project: {str(e)}")
    
    def create_new_project(self):
        """Create a new project via dialog"""
        # Create a simple dialog for project name input
        dialog = tk.Toplevel(self.root)
        dialog.title("Create New Project")
        dialog.geometry("300x150")
        dialog.transient(self.root)  # Make dialog modal
        dialog.grab_set()  # Make dialog modal
        
        # Center the dialog
        dialog.geometry("+%d+%d" % (self.root.winfo_rootx() + 50, self.root.winfo_rooty() + 50))
        
        # Project name input
        ttk.Label(dialog, text="Project Name:").pack(pady=(20, 5))
        
        name_var = tk.StringVar()
        name_entry = ttk.Entry(dialog, textvariable=name_var, width=30)
        name_entry.pack(pady=5)
        name_entry.focus()
        
        # Buttons
        button_frame = ttk.Frame(dialog)
        button_frame.pack(pady=20)
        
        def create_project():
            new_name = name_var.get().strip()
            if not new_name:
                messagebox.showerror("Error", "Please enter a project name", parent=dialog)
                return
            
            # Check if project already exists
            existing_projects = [project[1] for project in self.db.get_all_projects()]
            if new_name in existing_projects:
                messagebox.showerror("Error", f"Project '{new_name}' already exists", parent=dialog)
                return
            
            try:
                project_id = self.db.create_project(new_name)
                self.load_projects()
                self.project_var.set(new_name)
                self.current_project_id = project_id
                self.load_project_history()
                dialog.destroy()
                messagebox.showinfo("Success", f"Project '{new_name}' created successfully!")
            except Exception as e:
                messagebox.showerror("Error", f"Failed to create project: {str(e)}", parent=dialog)
        
        def cancel():
            dialog.destroy()
        
        ttk.Button(button_frame, text="Create", command=create_project).pack(side="left", padx=5)
        ttk.Button(button_frame, text="Cancel", command=cancel).pack(side="left", padx=5)
        
        # Bind Enter key to create project
        name_entry.bind("<Return>", lambda e: create_project())
        name_entry.bind("<Escape>", lambda e: cancel())
    
    def send_prompt(self, event=None):
        """Send prompt to both LLMs"""
        prompt = self.prompt_var.get().strip()
        if not prompt or not self.current_project_id:
            return
        
        # Clear prompt field
        self.prompt_var.set("")
        
        # Display user message in both chats
        self.display_message('A', 'user', prompt)
        self.display_message('B', 'user', prompt)
        
        # Save user message to database
        self.db.save_message(self.current_project_id, 'A', 'user', prompt)
        self.db.save_message(self.current_project_id, 'B', 'user', prompt)
        
        # Get conversation context
        context_a = self.db.get_project_context(self.current_project_id, 'A')
        context_b = self.db.get_project_context(self.current_project_id, 'B')
        
        # Add current prompt to context
        context_a.append({"role": "user", "content": prompt})
        context_b.append({"role": "user", "content": prompt})
        
        # Send requests to LLMs in separate threads
        threading.Thread(
            target=self._get_llm_response,
            args=('A', context_a),
            daemon=True
        ).start()
        
        threading.Thread(
            target=self._get_llm_response,
            args=('B', context_b),
            daemon=True
        ).start()
    
    def _get_llm_response(self, llm_type: str, messages: list):
        """Get response from LLM in a separate thread"""
        try:
            response = self.llm_client.get_response_sync(llm_type, messages)
            
            # Save response to database
            self.db.save_message(self.current_project_id, llm_type, 'assistant', response)
            
            # Queue response for display
            self.response_queue.put((llm_type, 'assistant', response))
        except Exception as e:
            error_msg = f"Error getting response: {str(e)}"
            self.response_queue.put((llm_type, 'assistant', error_msg))
    
    def display_message(self, llm_type: str, role: str, content: str):
        """Display a message in the appropriate chat window"""
        chat_widget = self.chat_a if llm_type.upper() == 'A' else self.chat_b
        
        chat_widget.config(state=tk.NORMAL)
        
        # Add role label
        role_label = "You: " if role == "user" else "Assistant: "
        chat_widget.insert(tk.END, role_label, f"role_{role}")
        
        # Add content
        chat_widget.insert(tk.END, content + "\n\n")
        
        # Configure tags for styling
        chat_widget.tag_config("role_user", foreground="blue", font=("Arial", 18, "bold"))
        chat_widget.tag_config("role_assistant", foreground="green", font=("Arial", 18, "bold"))
        
        # Scroll to bottom
        chat_widget.see(tk.END)
        chat_widget.config(state=tk.DISABLED)
    
    def check_response_queue(self):
        """Check for responses from LLMs and display them"""
        try:
            while True:
                llm_type, role, content = self.response_queue.get_nowait()
                self.display_message(llm_type, role, content)
        except queue.Empty:
            pass
        
        # Schedule next check
        self.root.after(100, self.check_response_queue)

def main():
    root = tk.Tk()
    app = MindMeldDemoApp(root)
    root.mainloop()

if __name__ == "__main__":
    main() 