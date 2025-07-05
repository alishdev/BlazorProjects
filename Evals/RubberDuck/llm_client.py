import asyncio
import threading
from typing import List, Dict, Optional
import openai
import anthropic
import google.generativeai as genai
import configparser

class LLMClient:
    def __init__(self, config_path: str = "config.ini"):
        self.config = configparser.ConfigParser()
        self.config.read(config_path)
        self._setup_clients()
    
    def _setup_clients(self):
        """Setup API clients for different providers"""
        # OpenAI setup
        self.openai_client = openai.OpenAI(
            api_key=self.config.get('LLM_A', 'api_key')
        )
        
        # Anthropic setup
        self.anthropic_client = anthropic.Anthropic(
            api_key=self.config.get('LLM_B', 'api_key')
        )
        
        # Google setup (if needed)
        genai.configure(api_key=self.config.get('LLM_A', 'api_key', fallback=''))
    
    async def get_response_async(self, llm_type: str, messages: List[Dict[str, str]]) -> str:
        """Get response from LLM asynchronously"""
        if llm_type.upper() == 'A':
            return self._get_openai_response_sync(messages)
        elif llm_type.upper() == 'B':
            return await self._get_anthropic_response(messages)
        else:
            raise ValueError(f"Unknown LLM type: {llm_type}")
    
    def _get_openai_response_sync(self, messages: List[Dict[str, str]]) -> str:
        """Get response from OpenAI (synchronous)"""
        try:
            model = self.config.get('LLM_A', 'model')
            response = self.openai_client.chat.completions.create(
                model=model,
                messages=messages,
                max_tokens=10000,
                temperature=0.7
            )
            return response.choices[0].message.content
        except Exception as e:
            return f"Error: {str(e)}"
    
    def get_response_sync(self, llm_type: str, messages: List[Dict[str, str]]) -> str:
        """Get response from LLM synchronously (for threading)"""
        if llm_type.upper() == 'A':
            return self._get_openai_response_sync(messages)
        elif llm_type.upper() == 'B':
            # For Anthropic, we still need to use async
            loop = asyncio.new_event_loop()
            asyncio.set_event_loop(loop)
            try:
                return loop.run_until_complete(self._get_anthropic_response(messages))
            finally:
                loop.close()
        else:
            raise ValueError(f"Unknown LLM type: {llm_type}")
    

    
    async def _get_anthropic_response(self, messages: List[Dict[str, str]]) -> str:
        """Get response from Anthropic"""
        try:
            model = self.config.get('LLM_B', 'model')
            # Convert messages to Anthropic format
            system_message = None
            conversation = []
            for msg in messages:
                if msg["role"] == "system" and system_message is None:
                    system_message = msg["content"]
                elif msg["role"] == "user":
                    conversation.append({"role": "user", "content": msg["content"]})
                elif msg["role"] == "assistant":
                    conversation.append({"role": "assistant", "content": msg["content"]})
            # Build kwargs for API call
            kwargs = dict(
                model=model,
                max_tokens=10000,
                messages=conversation
            )
            if system_message:
                kwargs["system"] = system_message
            # Call the synchronous API (no await)
            response = self.anthropic_client.messages.create(**kwargs)
            return response.content[0].text
        except Exception as e:
            return f"Error: {str(e)}"
    
    def get_llm_name(self, llm_type: str) -> str:
        """Get the display name for an LLM"""
        if llm_type.upper() == 'A':
            provider = self.config.get('LLM_A', 'provider')
            model = self.config.get('LLM_A', 'model')
            return f"{provider.title()}: {model}"
        elif llm_type.upper() == 'B':
            provider = self.config.get('LLM_B', 'provider')
            model = self.config.get('LLM_B', 'model')
            return f"{provider.title()}: {model}"
        else:
            return f"Unknown LLM: {llm_type}" 