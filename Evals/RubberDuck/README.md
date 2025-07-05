# Mind Meld - Personal Brainstorming Assistant

A desktop-based personal brainstorming assistant that provides an organized, project-based, and comparative environment for creative thinking, problem-solving, and idea generation. By sending user prompts to two different Large Language Models (LLMs) simultaneously within a specific project context, users can compare and contrast responses, fostering a richer and more nuanced brainstorming session.

## Features

### Core Functionality
- **Project Management**: Organize conversations by project with easy switching
- **Dual LLM Interaction**: Simultaneously interact with two different LLM providers
- **Contextual Memory**: Each LLM maintains conversation history specific to the selected project
- **Local Storage**: All conversations are stored locally in SQLite database
- **Asynchronous Responses**: Non-blocking UI with threaded LLM requests

### User Interface
- **Project Bar**: Dropdown selector for projects with ability to create new ones
- **Dual Chat Windows**: Side-by-side chat interfaces for each LLM
- **Prompt Area**: Single input field with send button
- **Scrollable History**: Both chat windows are fully scrollable
- **Visual Distinction**: User and assistant messages are color-coded

## Installation

python -m venv venv
source venv/bin/activate 

### Prerequisites
- Python 3.8 or higher
- API keys for your chosen LLM providers

### Setup

1. **Clone or download the project files**

2. **Install dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

3. **Configure API keys**:
   Edit `config.ini` and replace the placeholder API keys with your actual keys:
   ```ini
   [LLM_A]
   provider=openai
   model=gpt-4o-mini
   api_key=your_openai_api_key_here

   [LLM_B]
   provider=anthropic
   model=claude-3-5-sonnet-20241022
   api_key=your_anthropic_api_key_here
   ```

4. **Run the application**:
   ```bash
   python main.py
   ```

## Usage

### Getting Started
1. Launch the application
2. The app will automatically create a "General" project if no projects exist
3. The most recently accessed project will be loaded automatically

### Creating Projects
- Type a new project name in the project dropdown and press Enter
- The new project will be created and automatically selected

### Switching Projects
- Select a different project from the dropdown
- The chat windows will clear and load the conversation history for the selected project

### Sending Prompts
- Type your prompt in the input field at the bottom
- Press Enter or click "Send"
- Your prompt will appear in both chat windows
- Both LLMs will respond asynchronously

### Project Persistence
- All conversations are automatically saved to the local database
- Project access timestamps are updated automatically
- The last accessed project is remembered between sessions

## Configuration

### Supported LLM Providers
- **OpenAI**: GPT models (GPT-4, GPT-3.5, etc.)
- **Anthropic**: Claude models (Claude 3.5 Sonnet, etc.)
- **Google**: Gemini models (configurable)

### Database
- **File**: `assistant_storage.db` (SQLite)
- **Tables**:
  - `projects`: Project information and metadata
  - `llm_a_history`: Conversation history for LLM A
  - `llm_b_history`: Conversation history for LLM B

## Architecture

### Components
- **DatabaseManager**: Handles all SQLite operations
- **LLMClient**: Manages API calls to different LLM providers
- **MindMeldApp**: Main GUI application using tkinter

### Data Flow
1. User enters prompt
2. Prompt is saved to database for both LLMs
3. Context is retrieved from database for each LLM
4. API requests are sent asynchronously in separate threads
5. Responses are queued and displayed as they arrive
6. Responses are saved to database

## Requirements

### Functional Requirements (from PRD)
- ✅ Project selection and creation
- ✅ Dual LLM interaction with contextual memory
- ✅ Side-by-side chat interface
- ✅ Local SQLite storage
- ✅ Configuration file for API keys
- ✅ Asynchronous response handling
- ✅ Project-based conversation history

### Non-Functional Requirements
- ✅ Responsive UI
- ✅ Secure API key storage
- ✅ Error handling
- ✅ Clean and intuitive interface

## Troubleshooting

### Common Issues

**API Key Errors**
- Ensure your API keys are correctly set in `config.ini`
- Verify your API keys have sufficient credits/quota
- Check that the specified models are available with your API plan

**Database Errors**
- Ensure the application has write permissions in the directory
- Delete `assistant_storage.db` to reset the database if corrupted

**Import Errors**
- Make sure all dependencies are installed: `pip install -r requirements.txt`
- Verify Python version is 3.8 or higher

### Error Messages
- **"Error getting response"**: Usually indicates API key or network issues
- **"Failed to create project"**: Database write permission issues
- **Import errors**: Missing dependencies

## Future Enhancements (V2.0)

The following features are planned for future versions:
- Project renaming and deletion
- Conversation export functionality
- Graphical settings menu
- Streaming responses (typing effect)
- Search functionality within project history
- Additional LLM provider support
- Conversation analytics and insights

## License

This project is open source. Feel free to modify and distribute according to your needs.

## Support

For issues or questions, please check the troubleshooting section above or create an issue in the project repository. 