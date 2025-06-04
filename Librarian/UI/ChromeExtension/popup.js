const chatbox = document.getElementById('chatbox');
const userInput = document.getElementById('userInput');
const sendButton = document.getElementById('sendButton');

// --- IMPORTANT: API Configuration ---
// Replace this with the actual URL of your backend API endpoint
const API_URL = 'https://localhost:7261/chat';
// ------------------------------------

// Function to add a message to the chatbox
function displayMessage(text, sender) {
  const messageDiv = document.createElement('div');
  messageDiv.classList.add('message', `${sender}-message`); // e.g., 'user-message' or 'bot-message'
  messageDiv.textContent = text;
  chatbox.appendChild(messageDiv);
  // Scroll to the bottom of the chatbox
  chatbox.scrollTop = chatbox.scrollHeight;
}

// Function to send the question to the API
async function askApi(question) {
  // Display a temporary "thinking" message (optional)
  displayMessage("...", "bot"); // Simple loading indicator

  try {
    const response = await fetch(API_URL, {
      method: 'POST', // Or 'GET', depending on your API
      headers: {
        'Content-Type': 'application/json',
        // Add any other headers your API needs (e.g., Authorization)
      },
      // --- Adjust the body format to what your API expects ---
      body: JSON.stringify({ message: question }) // Changed from query to message to match API
      // If using GET, you might append the question as a query parameter to the URL
      // --------------------------------------------------------
    });

    // Remove the temporary "thinking" message
    const thinkingMessage = chatbox.querySelector('.bot-message:last-child');
    if (thinkingMessage && thinkingMessage.textContent === '...') {
        chatbox.removeChild(thinkingMessage);
    }


    if (!response.ok) {
      // Handle HTTP errors (e.g., 404, 500)
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();

    // --- Adjust how you extract the answer based on your API's response structure ---
    const answer = data.answer || "Sorry, I couldn't get an answer."; // Example: expecting { "answer": "..." }
    // -----------------------------------------------------------------------------

    displayMessage(answer, "bot");

  } catch (error) {
     // Remove the temporary "thinking" message if it exists on error
    const thinkingMessage = chatbox.querySelector('.bot-message:last-child');
    if (thinkingMessage && thinkingMessage.textContent === '...') {
        chatbox.removeChild(thinkingMessage);
    }

    console.error("Error contacting API:", error);
    displayMessage(`Error: ${error.message}`, "bot");
  }
}

// Function to handle sending a message
function handleSend() {
    const userText = userInput.value.trim();
    if (userText) {
        displayMessage(userText, "user"); // Display user's question immediately
        userInput.value = ''; // Clear the input field
        askApi(userText); // Send the question to the API
    }
}

// Event Listeners
sendButton.addEventListener('click', handleSend);

userInput.addEventListener('keypress', function (e) {
    // Send message if Enter key is pressed
    if (e.key === 'Enter') {
        handleSend();
    }
});

// Optional: Display a welcome message on load
displayMessage("Hello! Ask me something.", "bot");