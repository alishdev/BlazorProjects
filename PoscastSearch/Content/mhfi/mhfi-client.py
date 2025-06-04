import gradio as gr
import os
from dotenv import load_dotenv
from langchain_google_genai import GoogleGenerativeAI, GoogleGenerativeAIEmbeddings
from langchain_community.vectorstores import Chroma
from langchain.chains import ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory
from datetime import datetime
import sys

def browse_file(file_path):
    return f'<iframe src="{file_path}" width="100%" height="600px" frameborder="0"></iframe>'

def chat_response(message, history):
    return f"You said: {message}"

def get_usermessages_collection(read_only=False):
    # Only use embeddings if not read_only
    embeddings = None if read_only else GoogleGenerativeAIEmbeddings(model="models/embedding-001")
    user_db = Chroma(
        collection_name="usermessages",
        embedding_function=embeddings,
        persist_directory="./firedb"
    )
    return user_db

def ask_question(message, history):
    # Get response from the chain
    result = qa_chain({"question": message})

    # Save user message to Chroma DB with timestamp
    user_db = get_usermessages_collection()
    timestamp = datetime.now().isoformat()
    # Chroma expects a list of documents and metadatas
    user_db.add_texts(
        texts=[message],
        metadatas=[{"timestamp": timestamp}]
    )

    answer = result['answer']
    answer += "\n\nSources:"
    # Use a set to track unique sources
    unique_sources = {doc.metadata['source'] for doc in result['source_documents']}
    for source in sorted(unique_sources):
        answer += f"\n- {source}"
    return answer
        
def initialize_rag():
    # Load environment variables
    load_dotenv()
    if not os.getenv('GOOGLE_API_KEY'):
        raise ValueError("GOOGLE_API_KEY not found in environment variables")

    # Initialize embeddings and vector store
    embeddings = GoogleGenerativeAIEmbeddings(model="models/embedding-001")
    db = Chroma(
        collection_name="pod_collection",
        embedding_function=embeddings,
        persist_directory="./firedb"
    )

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
                        placeholder="Ask me anything about FIRE movement...",
                        container=False,
                        scale=7
                    ),
                )

    return interface

def print_usermessages():
    user_db = get_usermessages_collection(read_only=True)
    results = user_db.get()
    documents = results.get('documents', [])
    metadatas = results.get('metadatas', [])
    # Combine docs and metadatas, and sort by timestamp descending
    combined = [
        (doc, meta.get('timestamp', 'N/A'))
        for doc, meta in zip(documents, metadatas)
    ]
    # Sort by timestamp descending (most recent first)
    combined.sort(key=lambda x: x[1], reverse=True)
    print("User Messages in Chroma DB (most recent first):")
    for i, (doc, timestamp) in enumerate(combined, 1):
        print(f"{i}. [{timestamp}] {doc}")

def ContentIdeas():
    # Ensure environment variables are loaded
    load_dotenv()
    # Step 1: Get last 2 user messages
    user_db = get_usermessages_collection(read_only=True)
    results = user_db.get()
    documents = results.get('documents', [])
    metadatas = results.get('metadatas', [])
    combined = [
        (doc, meta.get('timestamp', 'N/A'))
        for doc, meta in zip(documents, metadatas)
    ]
    # Sort by timestamp descending and get last 2
    combined.sort(key=lambda x: x[1], reverse=True)
    last_two = combined[:2]
    if not last_two:
        print("No user messages found.")
        return
    user_messages = [doc for doc, _ in last_two]

    # Step 2: Prepare context from RAG database (e.g., top 3 relevant docs for the last message)
    # We'll use the RAG retriever to get relevant context for the most recent message
    rag_db = Chroma(
        collection_name="pod_collection",
        embedding_function=GoogleGenerativeAIEmbeddings(model="models/embedding-001"),
        persist_directory="./firedb"
    )
    # Use the most recent user message for context retrieval
    context_docs = rag_db.similarity_search(user_messages[0], k=3)
    context_texts = [doc.page_content for doc in context_docs]
    context = "\n\n".join(context_texts)

    # Step 3: Compose the prompt for the LLM
    prompt = (
        "You are a social media content creator. "
        "Given the following recent user messages and relevant context from a knowledge base, "
        "generate 2 creative Twitter posts (with hashtags) that would engage an audience interested in the FIRE movement.\n\n"
        f"User messages:\n- " + "\n- ".join(user_messages) +
        f"\n\nRelevant context:\n{context}\n\n"
        "Twitter posts:"
    )

    # Step 4: Call the LLM
    llm = GoogleGenerativeAI(model="gemini-1.5-pro", temperature=0.7)
    response = llm.invoke(prompt)
    print("Generated Twitter Content Ideas:\n")
    print(response)

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "print_usermessages":
        print_usermessages()
    elif len(sys.argv) > 1 and sys.argv[1] == "content_ideas":
        ContentIdeas()
    else:
        interface = create_interface()
        interface.launch(share=True)