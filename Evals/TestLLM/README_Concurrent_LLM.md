# Concurrent LLM Requests

This document describes the concurrent LLM request functionality implemented in the TestLLM application.

## Overview

The application now supports sending questions to multiple LLMs simultaneously and storing their responses in a thread-safe manner. Users can switch between tabs to view responses from different LLMs without losing any data.

## Key Features

### 1. Concurrent Request Processing
- When the "Submit Question" button is clicked, the application creates separate tasks for each enabled LLM
- All requests are sent simultaneously to improve response time
- Each request runs independently and stores its response in a thread-safe dictionary

### 2. Thread-Safe Response Storage
- Responses are stored in a `ConcurrentDictionary<string, string>` where the key is the LLM name and model
- This ensures thread safety when multiple requests complete simultaneously
- Responses persist until the user starts typing a new question or refreshes the LLM list

### 3. Real-Time UI Updates
- Tab buttons show loading status (⏳) while requests are in progress
- The currently selected tab updates immediately when its response is received
- The Settings tab shows a summary of all responses as they arrive

### 4. Response Management
- Responses are automatically cleared when:
  - User starts typing a new question
  - LLM list is refreshed
  - An LLM is disabled (its response is removed)
- Cancellation support for ongoing requests when new requests are started

## User Interface

### Tab Behavior
- **Settings Tab**: Shows a summary of all responses from enabled LLMs
- **LLM Tabs**: Show the full response from the specific LLM
- **Loading Indicator**: Tab buttons show ⏳ while requests are in progress

### Response Display
- **Settings Tab**: Shows truncated responses (150 characters) with LLM names
- **LLM Tabs**: Show the complete response from that specific LLM
- **Error Handling**: Displays error messages if requests fail

## Technical Implementation

### Key Components

1. **ConcurrentDictionary<string, string> _llmResponses**
   - Thread-safe storage for LLM responses
   - Key: LLM name and model (e.g., "GPT-4:gpt-4")
   - Value: Response text or error message

2. **CancellationTokenSource _currentRequestCancellation**
   - Manages cancellation of ongoing requests
   - Prevents resource leaks and ensures clean state

3. **Task-based Request Processing**
   - Each LLM request runs in its own Task
   - Uses Task.WhenAll() to wait for all requests to complete
   - Proper exception handling for individual request failures

### Methods

- **OnSubmitClicked()**: Creates concurrent tasks for all enabled LLMs
- **AskLLM()**: Sends individual requests to the Python server
- **ShowLLMTab()**: Displays stored response for selected LLM
- **GetResponsesSummary()**: Formats responses for Settings tab display
- **UpdateTabLoadingStatus()**: Shows/hides loading indicators
- **ClearResponses()**: Clears all stored responses
- **OnQuestionTextChanged()**: Clears responses when user starts typing

## Usage

1. **Enable LLMs**: Use the Settings tab to enable/disable LLMs
2. **Ask Question**: Type a question and click "Submit Question"
3. **View Responses**: 
   - Switch between LLM tabs to see individual responses
   - Use Settings tab to see a summary of all responses
4. **New Question**: Start typing a new question to clear previous responses

## Error Handling

- Individual LLM failures don't affect other requests
- Error messages are stored and displayed in the response area
- Network errors and API failures are properly logged
- Request cancellation is handled gracefully

## Performance Considerations

- All requests are sent concurrently for maximum speed
- Thread-safe collections prevent race conditions
- UI updates are performed on the main thread
- Memory usage is managed by clearing responses when appropriate 