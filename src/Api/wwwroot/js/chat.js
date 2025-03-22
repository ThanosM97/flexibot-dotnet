document.addEventListener('DOMContentLoaded', () => {
    // DOM Elements
    const chatHistory = document.getElementById('chatHistory');
    const messageInput = document.getElementById('messageInput');
    const sendButton = document.getElementById('sendButton');
    const newSessionButton = document.getElementById('newSessionButton');
    const pastMessagesInput = document.getElementById("pastMessagesInput")
    const configToggleButton = document.getElementById('configToggleButton');
    const configPanel = document.getElementById('configPanel');

    // Initialize SignalR Connection
    function initConnection() {
        var connection = new signalR.HubConnectionBuilder()
            .withUrl("http://127.0.0.1:5229/chatHub")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("ReceiveToken", (token, isLast) => {
            addMessageChunk(token);
            if (isLast) enableInput();
        });

        connection.start()
            .then(() => console.log("Connected to ChatHub."))
            .catch((err) => console.error(err.toString()));

        connection.onclose(() => console.log('SignalR connection closed'));

        return connection;
    }

    let connection = initConnection();

    // Session Handling
    let sessionId = localStorage.getItem('sessionId') || crypto.randomUUID();
    localStorage.setItem('sessionId', sessionId);

    function createNewSession() {
        connection.stop();
        connection = initConnection();
        resetChat();
        sessionId = crypto.randomUUID();
        localStorage.setItem('sessionId', sessionId);
    }

    function resetChat() {
        chatHistory.innerHTML = '';
        enableInput();
    }

    function enableInput() {
        messageInput.disabled = false;
        sendButton.disabled = false;
        sendButton.querySelector('.button-loader').hidden = true;
        sendButton.querySelector('.button-text').textContent = 'Send';
    }

    newSessionButton.addEventListener('click', createNewSession);

    // Message Sending
    async function sendMessage() {
        const message = messageInput.value.trim();
        if (!message) return;

        disableInput();
        addMessage(message, 'user');
        messageInput.value = '';

        const pastMessagesIncluded = parseInt(pastMessagesInput.value) || 10;

        try {
            const response = await fetch('/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ sessionId, prompt: message, pastMessagesIncluded: pastMessagesIncluded })
            });

            if (!response.ok) throw new Error('Failed to send message');

            const { jobId } = await response.json();
            await joinGroup(jobId);
        } catch (error) {
            addMessage('Error connecting to the chatbot. Please try again.', 'bot');
        }
    }

    async function joinGroup(jobId) {
        connection.invoke("SubscribeToJob", jobId)
            .catch(err => console.error(err.toString()));
    }

    function disableInput() {
        messageInput.disabled = true;
        sendButton.disabled = true;
        sendButton.querySelector('.button-loader').hidden = false;
        sendButton.querySelector('.button-text').textContent = 'Processing...';
    }

    // Message Display
    function addMessage(content, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}-message`;
        messageDiv.textContent = content;
        chatHistory.appendChild(messageDiv);
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }

    function addMessageChunk(chunk) {
        const lastMessage = chatHistory.lastElementChild;

        if (!lastMessage || !lastMessage.classList.contains('bot-message')) {
            addMessage(chunk, 'bot');
        } else {
            lastMessage.textContent += chunk;
            chatHistory.scrollTop = chatHistory.scrollHeight;
        }
    }

    // Event Listeners
    sendButton.addEventListener('click', sendMessage);
    messageInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });
    configToggleButton.addEventListener('click', () => {
        configPanel.style.display = configPanel.style.display === 'block' ? 'none' : 'block';
    });
});
