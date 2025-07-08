#!/usr/bin/env python3
"""
Launcher script for LLM Arena ChromaDB version
"""

import os
import sys
import subprocess
from pathlib import Path

def check_dependencies():
    """Check if required dependencies are installed."""
    required_packages = [
        'chromadb',
        'click',
        'pyyaml',
        'streamlit',
        'gradio',
        'PyPDF2',
        'numpy'
    ]
    
    missing_packages = []
    for package in required_packages:
        try:
            __import__(package)
        except ImportError:
            missing_packages.append(package)
    
    if missing_packages:
        print(f"❌ Missing required packages: {', '.join(missing_packages)}")
        print("Please install them using: pip install -r requirements.txt")
        return False
    
    print("✅ All required packages are installed")
    return True

def check_config():
    """Check if configuration file exists."""
    if not os.path.exists("config.yaml"):
        print("❌ config.yaml not found")
        print("Please create a config.yaml file with your API keys and settings")
        return False
    
    print("✅ Configuration file found")
    return True

def check_data_directory():
    """Check if data directory exists and contains documents."""
    data_dir = Path("data/source_docs")
    if not data_dir.exists():
        print("❌ Data directory not found: data/source_docs")
        print("Please create the directory and add your source documents")
        return False
    
    # Check if directory contains any documents
    documents = list(data_dir.glob("*.pdf")) + list(data_dir.glob("*.txt")) + list(data_dir.glob("*.md"))
    if not documents:
        print("❌ No documents found in data/source_docs")
        print("Please add PDF, TXT, or MD files to the directory")
        return False
    
    print(f"✅ Found {len(documents)} documents in data/source_docs")
    return True

def run_command(command, description):
    """Run a command and handle errors."""
    print(f"\n🔄 {description}...")
    try:
        result = subprocess.run(command, shell=True, check=True, capture_output=True, text=True)
        print(f"✅ {description} completed successfully")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ {description} failed:")
        print(f"Error: {e.stderr}")
        return False

def main():
    """Main launcher function."""
    print("=" * 60)
    print("🚀 LLM Arena ChromaDB Launcher")
    print("=" * 60)
    
    # Check prerequisites
    if not check_dependencies():
        sys.exit(1)
    
    if not check_config():
        sys.exit(1)
    
    if not check_data_directory():
        sys.exit(1)
    
    print("\n" + "=" * 60)
    print("Available Commands:")
    print("1. Initialize system")
    print("2. Build RAG systems")
    print("3. Generate questions")
    print("4. Generate answers")
    print("5. Run complete pipeline")
    print("6. Launch Streamlit UI")
    print("7. Launch Gradio UI")
    print("8. Run tests")
    print("9. Exit")
    print("=" * 60)
    
    while True:
        try:
            choice = input("\nEnter your choice (1-9): ").strip()
            
            if choice == "1":
                run_command("python main.py init", "Initializing system")
            elif choice == "2":
                run_command("python main.py build", "Building RAG systems")
            elif choice == "3":
                run_command("python main.py questions", "Generating questions")
            elif choice == "4":
                run_command("python main.py answers", "Generating answers")
            elif choice == "5":
                run_command("python main.py run-all", "Running complete pipeline")
            elif choice == "6":
                print("\n🔄 Launching Streamlit UI...")
                print("The UI will open in your browser. Press Ctrl+C to stop.")
                subprocess.run("streamlit run eval_ui.py", shell=True)
            elif choice == "7":
                print("\n🔄 Launching Gradio UI...")
                print("The UI will open in your browser. Press Ctrl+C to stop.")
                subprocess.run("python eval_ui_gradio.py", shell=True)
            elif choice == "8":
                run_command("python test_chroma.py", "Running tests")
            elif choice == "9":
                print("👋 Goodbye!")
                break
            else:
                print("❌ Invalid choice. Please enter a number between 1-9.")
                
        except KeyboardInterrupt:
            print("\n👋 Goodbye!")
            break
        except Exception as e:
            print(f"❌ An error occurred: {e}")

if __name__ == "__main__":
    main() 