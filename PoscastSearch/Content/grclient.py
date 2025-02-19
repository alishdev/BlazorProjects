import gradio as gr
import os
from dotenv import load_dotenv
import google.generativeai as genai

def browse_file(file_path):
    return f'<iframe src="{file_path}" width="100%" height="600px" frameborder="0"></iframe>'

def chat_response(message, history):
    return f"You said: {message}"

def create_interface():
    # Configure Gemini
    load_dotenv()
    api_key = os.getenv('GOOGLE_API_KEY')
    if not api_key:
        raise ValueError("GOOGLE_API_KEY not found in environment variables")
        
    genai.configure(api_key=api_key)
    global model
    model = genai.GenerativeModel('gemini-2.0-pro')

    # Create the interface
    with gr.Blocks() as interface:
        gr.Markdown("# College Admissions Assistant")
        
        with gr.Tabs():
            # HTML Viewer Tab
            # in another terminal run `python -m http.server 1234`
            with gr.Tab("HTML Viewer"):
                html_output = gr.HTML("""
                                        <iframe src="http://localhost:1234/main.html" width="100%" height="600px" frameborder="0"></iframe>
                                      """)

            # Chat Tab
            with gr.Tab("Chat"):
                chatbot = gr.ChatInterface(fn=chat_response)

    return interface

if __name__ == "__main__":
    interface = create_interface()
    interface.launch(share=True)