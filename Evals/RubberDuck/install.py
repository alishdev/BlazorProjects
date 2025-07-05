#!/usr/bin/env python3
"""
Mind Meld - Installation Script
Helps users set up the application and configure API keys
"""

import os
import sys
import subprocess
import configparser
from pathlib import Path

def check_python_version():
    """Check if Python version is compatible"""
    if sys.version_info < (3, 8):
        print("❌ Python 3.8 or higher is required")
        print(f"Current version: {sys.version}")
        return False
    print(f"✅ Python version: {sys.version.split()[0]}")
    return True

def install_dependencies():
    """Install required dependencies"""
    print("\n📦 Installing dependencies...")
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", "requirements.txt"])
        print("✅ Dependencies installed successfully")
        return True
    except subprocess.CalledProcessError as e:
        print(f"❌ Failed to install dependencies: {e}")
        return False

def create_config_template():
    """Create a config template if it doesn't exist"""
    config_path = Path("config.ini")
    
    if config_path.exists():
        print("✅ config.ini already exists")
        return True
    
    print("\n⚙️  Creating config.ini template...")
    
    config = configparser.ConfigParser()
    config['LLM_A'] = {
        'provider': 'openai',
        'model': 'gpt-4o-mini',
        'api_key': 'your_openai_api_key_here'
    }
    config['LLM_B'] = {
        'provider': 'anthropic',
        'model': 'claude-3-5-sonnet-20241022',
        'api_key': 'your_anthropic_api_key_here'
    }
    config['Database'] = {
        'path': 'assistant_storage.db'
    }
    
    with open(config_path, 'w') as f:
        config.write(f)
    
    print("✅ config.ini template created")
    return True

def get_api_keys():
    """Interactive API key setup"""
    print("\n🔑 API Key Setup")
    print("=" * 40)
    print("You'll need API keys for at least one LLM provider.")
    print("You can get them from:")
    print("• OpenAI: https://platform.openai.com/api-keys")
    print("• Anthropic: https://console.anthropic.com/")
    print()
    
    config = configparser.ConfigParser()
    config.read("config.ini")
    
    # OpenAI setup
    print("OpenAI API Key:")
    print("(Press Enter to skip if you don't have one)")
    openai_key = input("Enter your OpenAI API key: ").strip()
    if openai_key:
        config['LLM_A']['api_key'] = openai_key
        print("✅ OpenAI API key saved")
    else:
        print("⏭️  Skipping OpenAI setup")
    
    # Anthropic setup
    print("\nAnthropic API Key:")
    print("(Press Enter to skip if you don't have one)")
    anthropic_key = input("Enter your Anthropic API key: ").strip()
    if anthropic_key:
        config['LLM_B']['api_key'] = anthropic_key
        print("✅ Anthropic API key saved")
    else:
        print("⏭️  Skipping Anthropic setup")
    
    # Save config
    with open("config.ini", 'w') as f:
        config.write(f)
    
    return bool(openai_key or anthropic_key)

def test_setup():
    """Test the setup by importing modules"""
    print("\n🧪 Testing setup...")
    
    try:
        import tkinter
        print("✅ tkinter available")
    except ImportError:
        print("❌ tkinter not available")
        return False
    
    try:
        import openai
        print("✅ openai available")
    except ImportError:
        print("⚠️  openai not available (will use demo mode)")
    
    try:
        import anthropic
        print("✅ anthropic available")
    except ImportError:
        print("⚠️  anthropic not available (will use demo mode)")
    
    try:
        from database import DatabaseManager
        print("✅ database module available")
    except ImportError as e:
        print(f"❌ database module error: {e}")
        return False
    
    return True

def show_next_steps():
    """Show next steps to the user"""
    print("\n🎉 Installation Complete!")
    print("=" * 40)
    print("Next steps:")
    print()
    print("1. If you have API keys:")
    print("   python run.py")
    print()
    print("2. To try the demo (no API keys required):")
    print("   python demo.py")
    print()
    print("3. To edit your configuration:")
    print("   Edit config.ini in a text editor")
    print()
    print("4. For help:")
    print("   Read README.md")

def main():
    """Main installation function"""
    print("🚀 Mind Meld - Installation Script")
    print("=" * 40)
    
    # Check Python version
    if not check_python_version():
        sys.exit(1)
    
    # Install dependencies
    if not install_dependencies():
        print("\n❌ Installation failed. Please check the error messages above.")
        sys.exit(1)
    
    # Create config template
    if not create_config_template():
        print("\n❌ Failed to create config template.")
        sys.exit(1)
    
    # Test setup
    if not test_setup():
        print("\n❌ Setup test failed.")
        sys.exit(1)
    
    # Interactive API key setup
    has_api_keys = get_api_keys()
    
    if not has_api_keys:
        print("\n⚠️  No API keys configured.")
        print("You can still run the demo version with: python demo.py")
        print("Or add API keys later by editing config.ini")
    
    # Show next steps
    show_next_steps()

if __name__ == "__main__":
    main() 