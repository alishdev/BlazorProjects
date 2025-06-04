import gradio as gr
import os
from dotenv import load_dotenv
from langchain_google_genai import GoogleGenerativeAI, GoogleGenerativeAIEmbeddings
from langchain_community.vectorstores import Chroma
from langchain.chains import ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory

def browse_file(file_path):
    return f'<iframe src="{file_path}" width="100%" height="600px" frameborder="0"></iframe>'

def chat_response(message, history):
    return f"You said: {message}"

def ask_question(message, history):
    # Get response from the chain
    result = qa_chain({"question": message})

    print(result)
    
    answer = result['answer']
    answer += "\n\nSources:"
    # Use a set to track unique sources and limit to 3
    unique_sources = list({doc.metadata['source'] for doc in result['source_documents']})[:3]
    for source in sorted(unique_sources):
        answer += f"\n- {source}"
    return answer
        
def initialize_rag():
    # Load environment variables
    load_dotenv()
    if not os.getenv('GOOGLE_API_KEY'):
        raise ValueError("GOOGLE_API_KEY not found in environment variables")

    # Initialize embeddings and vector store
    embeddings = GoogleGenerativeAIEmbeddings(model="models/text-embedding-004")
    db = Chroma(
        collection_name="pod_collection",
        embedding_function=embeddings,
        persist_directory="./camper.db"
    )

    print(db)

    # Initialize Gemini model
    llm = GoogleGenerativeAI(model="gemini-1.5-pro", temperature=0.5)

    # Initialize conversation memory
    memory = ConversationBufferMemory(
        memory_key="chat_history",
        return_messages=True,
        output_key='answer'
    )

    # Create conversational chain
    qa_chain = ConversationalRetrievalChain.from_llm(
        llm=llm,
        retriever=db.as_retriever(search_kwargs={"k": 12}),
        memory=memory,
        return_source_documents=True,
        return_generated_question=True
    )

    return qa_chain

def create_interface():
    # Configure Gemini
    load_dotenv()
    api_key = os.getenv('GOOGLE_API_KEY')
    if not api_key:
        raise ValueError("GOOGLE_API_KEY not found in environment variables")

    global qa_chain
    qa_chain = initialize_rag()

    # Create the interface
    with gr.Blocks(css="footer {display: none}") as interface:
        gr.Markdown("# FIRE Movement Assistant")
        
        with gr.Tabs() as tabs:
            # HTML Viewer Tab
            with gr.Tab("HTML Viewer"):
                html_output = gr.HTML("""
                    <iframe 
                        src="http://localhost:1234/main.html" 
                        width="100%" 
                        height="800px" 
                        frameborder="0" 
                        style="min-height: 800px;"
                    ></iframe>
                """)

            # Chat Tab
            with gr.Tab("Chat"):
                chatbot = gr.ChatInterface(
                    fn=ask_question,
                    chatbot=gr.Chatbot(height=700),
                    textbox=gr.Textbox(
                        placeholder="Ask me anything about Summper Camps...",
                        container=False,
                        scale=7
                    ),
                )

    return interface

if __name__ == "__main__":
    interface = create_interface()
    interface.launch(share=True)