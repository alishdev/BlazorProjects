(function($) {
    'use strict';

    $(document).ready(function() {
        // --- CACHE DOM ELEMENTS ---
        const $chatWindow = $('#scp-chat-window');
        const $chatToggle = $('#scp-chat-toggle');
        const $chatClose = $('#scp-chat-close');
        const $chatForm = $('#scp-chat-form');
        const $chatInput = $('#scp-chat-input');
        const $messagesContainer = $('#scp-chat-messages');
        const $typingIndicator = $('#scp-typing-indicator');

        // --- STATE MANAGEMENT (FR-2 Session Persistence) ---
        let chatHistory = [];

        function saveHistory() {
            try {
                sessionStorage.setItem('scp_chat_history', JSON.stringify(chatHistory));
            } catch (e) {
                console.error("Could not save chat history to sessionStorage.", e);
            }
        }

        function loadHistory() {
            try {
                const storedHistory = sessionStorage.getItem('scp_chat_history');
                if (storedHistory) {
                    chatHistory = JSON.parse(storedHistory);
                    chatHistory.forEach(msg => addMessageToDisplay(msg.role, msg.content));
                }
            } catch (e) {
                console.error("Could not load chat history from sessionStorage.", e);
                chatHistory = [];
            }
        }

        // --- UI FUNCTIONS ---
        function toggleChatWindow() {
            $chatWindow.toggleClass('scp-is-hidden');
            if (!$chatWindow.hasClass('scp-is-hidden')) {
                $chatInput.focus();
                scrollToBottom();
            }
        }

        function addMessageToDisplay(role, content) {
            const messageEl = $('<div></div>').addClass('scp-message').addClass(role).text(content);
            $messagesContainer.append(messageEl);
            scrollToBottom();
        }


        function scrollToBottom() {
            $messagesContainer.scrollTop($messagesContainer[0].scrollHeight);
        }

        // --- CORE LOGIC (FR-3 Backend Communication) ---
        function sendMessage(event) {
            event.preventDefault();
            const userMessage = $chatInput.val().trim();

            if (!userMessage) return;

            // 1. Add user message to UI and history
            addMessageToDisplay('user', userMessage);
            chatHistory.push({ role: 'user', content: userMessage });
            saveHistory();

            $chatInput.val('');
            $typingIndicator.removeClass('scp-is-hidden');
            scrollToBottom();

            // 2. Send data to WordPress Proxy
            $.ajax({
                url: scp_chat_params.ajax_url,
                type: 'POST',
                data: {
                    action: 'send_chat_message', // The PHP hook
                    nonce: scp_chat_params.nonce,  // Security nonce
                    chat_history: JSON.stringify(chatHistory) // Send entire history
                },
                success: function(response) {
                    if (response.success && response.data.response) {
                        const assistantMessage = response.data.response;
                        // 3. Add assistant response to UI and history
                        addMessageToDisplay('assistant', assistantMessage);
                        chatHistory.push({ role: 'assistant', content: assistantMessage });
                        saveHistory();
                    } else {
                        const errorMessage = response.data ? response.data.message : 'An unknown error occurred.';
                        addMessageToDisplay('assistant', 'Error: ' + errorMessage);
                    }
                },
                error: function() {
                    addMessageToDisplay('assistant', 'Error: Could not connect to the server.');
                },
                complete: function() {
                    $typingIndicator.addClass('scp-is-hidden');
                    scrollToBottom();
                }
            });
        }

        // --- EVENT LISTENERS ---
        $chatToggle.on('click', toggleChatWindow);
        $chatClose.on('click', toggleChatWindow);
        $chatForm.on('submit', sendMessage);

        // --- INITIALIZATION ---
        loadHistory();
    });

})(jQuery);