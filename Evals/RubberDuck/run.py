#!/usr/bin/env python3
"""
Mind Meld - Personal Brainstorming Assistant
Launcher script with error handling and setup verification
"""

import sys
import os
import configparser
from pathlib import Path

def check_dependencies():
    """Check if required dependencies are installed"""
    missing_deps = []
    
    try:
        import tkinter
    except ImportError:
        missing_deps.append("tkinter")
    
    try:
        import openai
    except ImportError:
        missing_deps.append("openai")
    
    try:
        import anthropic
    except ImportError:
        missing_deps.append("anthropic")
    
    try:
        import google.generativeai
    except ImportError:
        missing_deps.append("google-generativeai")
    
    if missing_deps:
        print("❌ Missing dependencies:")
        for dep in missing_deps:
            print(f"   - {dep}")
        print("\nPlease install missing dependencies:")
        print("pip install -r requirements.txt")
        return False
    
    return True

def check_config():
    """Check if configuration file exists and has valid API keys"""
    config_path = Path("config.ini")
    
    if not config_path.exists():
        print("❌ config.ini not found!")
        print("Please create config.ini with your API keys.")
        return False
    
    config = configparser.ConfigParser()
    config.read(config_path)
    
    # Check if required sections exist
    required_sections = ['LLM_A', 'LLM_B']
    for section in required_sections:
        if section not in config:
            print(f"❌ Missing section [{section}] in config.ini")
            return False
    
    # Check if API keys are set (not placeholder values)
    for section in required_sections:
        api_key = config.get(section, 'api_key', fallback='')
        if not api_key or 'your_' in api_key:
            print(f"❌ Please set a valid API key for [{section}] in config.ini")
            return False
    
    return True

def main():
    """Main launcher function"""
    print("🚀 Starting Mind Meld - Personal Brainstorming Assistant")
    print("=" * 50)
    
    # Check dependencies
    print("Checking dependencies...")
    if not check_dependencies():
        sys.exit(1)
    print("✅ Dependencies OK")
    
    # Check configuration
    print("Checking configuration...")
    if not check_config():
        sys.exit(1)
    print("✅ Configuration OK")
    
    # Import and run the main application
    try:
        from main import main as run_app
        print("✅ Starting application...")
        run_app()
    except ImportError as e:
        print(f"❌ Failed to import main application: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"❌ Application error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main() 