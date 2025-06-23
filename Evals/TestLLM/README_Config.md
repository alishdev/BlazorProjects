# LLM Configuration System

This document explains how to configure the available LLMs in the TestLLM application.

## Configuration File

The application uses a JSON configuration file (`llm_config.json`) to define the available LLMs. This file is located in the app's data directory and can be modified to add, remove, or configure LLMs.

## Configuration Format

The configuration file has the following structure:

```json
{
  "llms": [
    {
      "name": "GPT-4",
      "description": "OpenAI's GPT-4 model",
      "enabled": true,
      "apiKey": "openai",
      "defaultModel": "gpt-4"
    }
  ]
}
```

### Properties

- **name**: Display name for the LLM (used in UI)
- **description**: Description of the LLM (shown in settings)
- **enabled**: Whether the LLM should be available in the app
- **apiKey**: The API key identifier used when making requests to the Python server
- **defaultModel**: The default model to use for this LLM

## Adding New LLMs

To add a new LLM:

1. Edit the `llm_config.json` file in the app's data directory
2. Add a new entry to the `llms` array
3. Set `enabled` to `true` to make it available
4. Restart the application or use the refresh functionality

## Disabling LLMs

To disable an LLM without removing it:

1. Set `enabled` to `false` in the configuration
2. The LLM will not appear in the UI

## Default Configuration

If the configuration file is missing or invalid, the application will fall back to these default LLMs:

- GPT-4 (OpenAI)
- Claude (Anthropic)
- Gemini (Google)
- Llama (Meta)

## File Locations

- **Development**: `TestLLM/llm_config.json` (included in app bundle)
- **Runtime**: App data directory (copied from bundle on first run)

## API Integration

The configuration integrates with the Python server's AI agents. The `apiKey` property should match the keys expected by the Python server (e.g., "openai", "anthropic", "gemini").

## Runtime Updates

The application can refresh the LLM list from the configuration file at runtime using the `RefreshLLMListFromConfig()` method. This allows for dynamic updates without restarting the application. 