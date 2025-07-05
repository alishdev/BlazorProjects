#!/usr/bin/env python3
"""
Test script to verify OpenAI API fix
"""

import asyncio
from llm_client import LLMClient

async def test_openai_api():
    """Test the OpenAI API connection"""
    try:
        client = LLMClient()
        
        # Test message
        messages = [
            {"role": "user", "content": "Hello! This is a test message."}
        ]
        
        print("Testing OpenAI API...")
        response = await client.get_response_async('A', messages)
        print(f"Response: {response}")
        
        if response.startswith("Error:"):
            print("❌ API test failed")
            return False
        else:
            print("✅ API test successful")
            return True
            
    except Exception as e:
        print(f"❌ Error testing API: {e}")
        return False

def test_sync_api():
    """Test the synchronous API wrapper"""
    try:
        client = LLMClient()
        
        # Test message
        messages = [
            {"role": "user", "content": "Hello! This is a test message."}
        ]
        
        print("Testing synchronous API wrapper...")
        response = client.get_response_sync('A', messages)
        print(f"Response: {response}")
        
        if response.startswith("Error:"):
            print("❌ Sync API test failed")
            return False
        else:
            print("✅ Sync API test successful")
            return True
            
    except Exception as e:
        print(f"❌ Error testing sync API: {e}")
        return False

if __name__ == "__main__":
    print("Testing OpenAI API Fix...")
    print("=" * 40)
    
    # Test async API
    async_result = asyncio.run(test_openai_api())
    
    print()
    
    # Test sync API
    sync_result = test_sync_api()
    
    print()
    print("=" * 40)
    if async_result and sync_result:
        print("🎉 All tests passed! OpenAI API fix is working.")
    else:
        print("❌ Some tests failed. Check the error messages above.") 