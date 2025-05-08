document.addEventListener('DOMContentLoaded', () => {
    const chatMessages = document.getElementById('chat-messages');
    const userInput = document.getElementById('user-input');
    const sendButton = document.getElementById('send-button');
    const clearHistoryButton = document.getElementById('clear-history');
    
    // Add event listener for the send button
    sendButton.addEventListener('click', sendMessage);
    
    // Add event listener for the clear history button
    clearHistoryButton.addEventListener('click', clearChatHistory);
    
    // Add event listener for Enter key (with Shift+Enter for new line)
    userInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
    
    // Function to send message
    async function sendMessage() {
        const message = userInput.value.trim();
        
        if (!message) {
            return;
        }
        
        // Add user message to chat
        addMessageToChat('user', message);
        
        // Clear input
        userInput.value = '';
        
        // Add typing indicator
        const typingIndicator = document.createElement('div');
        typingIndicator.className = 'typing-indicator';
        typingIndicator.textContent = 'AI is thinking...';
        chatMessages.appendChild(typingIndicator);
        
        try {
            // Send message to API
            const response = await fetch('/api/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ message })
            });
            
            // Remove typing indicator
            chatMessages.removeChild(typingIndicator);
            
            if (response.ok) {
                const data = await response.json();
                addMessageToChat('ai', data.message);
            } else {
                const errorData = await response.json();
                showError(errorData.errorMessage || 'An error occurred while sending your message');
            }
        } catch (error) {
            // Remove typing indicator
            if (typingIndicator.parentNode === chatMessages) {
                chatMessages.removeChild(typingIndicator);
            }
            showError('Network error. Please try again later.');
        }
    }
    
    // Function to clear chat history
    async function clearChatHistory() {
        try {
            const response = await fetch('/api/chat/history', {
                method: 'DELETE'
            });
            
            if (response.ok) {
                // Clear all messages from the UI
                chatMessages.innerHTML = '';
                
                // Show confirmation message
                const confirmationDiv = document.createElement('div');
                confirmationDiv.className = 'system-message';
                confirmationDiv.textContent = 'Chat history has been cleared.';
                chatMessages.appendChild(confirmationDiv);
                
                // Remove confirmation after 3 seconds
                setTimeout(() => {
                    if (confirmationDiv.parentNode === chatMessages) {
                        chatMessages.removeChild(confirmationDiv);
                    }
                }, 3000);
            } else {
                showError('Failed to clear chat history.');
            }
        } catch (error) {
            showError('Network error. Failed to clear chat history.');
        }
    }
    
    // Function to add message to chat
    function addMessageToChat(sender, content) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}-message`;
        
        const formattedContent = content.replace(/\n/g, '<br>');
        messageDiv.innerHTML = formattedContent;
        
        chatMessages.appendChild(messageDiv);
        
        // Scroll to bottom
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    // Function to show error
    function showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.textContent = message;
        
        chatMessages.appendChild(errorDiv);
        
        // Scroll to bottom
        chatMessages.scrollTop = chatMessages.scrollHeight;
        
        // Remove error after 5 seconds
        setTimeout(() => {
            if (errorDiv.parentNode === chatMessages) {
                chatMessages.removeChild(errorDiv);
            }
        }, 5000);
    }
});