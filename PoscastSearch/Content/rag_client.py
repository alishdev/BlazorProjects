from langchain_google_genai import GoogleGenerativeAI, GoogleGenerativeAIEmbeddings
from langchain_community.vectorstores import Chroma
from langchain.chains import RetrievalQA, ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory
from dotenv import load_dotenv
import os

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
        persist_directory="./podcast_db"
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

def ask_question(qa_chain, question):
    # Get response from the chain
    result = qa_chain({"question": question})
    
    print("\nQuestion:", question)
    print("\nAnswer:", result['answer'])
    print("\nSources:")
    # Use a set to track unique sources
    unique_sources = {doc.metadata['source'] for doc in result['source_documents']}
    for source in sorted(unique_sources):
        print(f"- {source}")

def main():
    qa_chain = initialize_rag()
    print("\nRAG Question-Answering System")
    print("Type 'quit' to exit")
    print("Type 'clear' to clear chat history")
    
    while True:
        print("\nEnter your question:")
        question = input("> ").strip()
        
        if question.lower() == 'quit':
            print("Goodbye!")
            break
            
        if question.lower() == 'clear':
            qa_chain.memory.clear()
            print("Chat history cleared!")
            continue
            
        if question:
            ask_question(qa_chain, question)

if __name__ == "__main__":
    main() 