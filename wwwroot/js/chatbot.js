document.addEventListener('DOMContentLoaded', function () {
    // Elementos del DOM
    const chatTrigger = document.getElementById('chatTrigger');
    const chatContainer = document.getElementById('chatContainer');
    const chatClose = document.getElementById('chatClose');
    const chatBody = document.getElementById('chatBody');
    const chatInput = document.getElementById('chatInput');
    const sendBtn = document.getElementById('sendBtn');
    const suggestionBtns = document.querySelectorAll('.suggestion-btn');

    // Función para mostrar/ocultar el chatbot
    chatTrigger.addEventListener('click', function () {
        chatContainer.style.display = 'flex';
        // Agregar mensaje de bienvenida si el chat está vacío
        if (chatBody.children.length === 0) {
            addBotMessage("¡Hola! Soy el asistente virtual del Cementerio San Agustín. ¿En qué puedo ayudarte hoy?");
        }
    });

    chatClose.addEventListener('click', function () {
        chatContainer.style.display = 'none';
    });

    // Función para enviar mensaje
    function sendMessage() {
        const message = chatInput.value.trim();
        if (message === '') return;

        // Agregar mensaje del usuario
        addUserMessage(message);

        // Limpiar input
        chatInput.value = '';

        // Mostrar indicador de carga
        const loadingMessage = addBotMessage("Escribiendo...");

        // Llamar a la API
        fetch('/api/ChatBot', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ message: message })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la respuesta del servidor');
                }
                return response.json();
            })
            .then(data => {
                // Eliminar indicador de carga y agregar respuesta real
                chatBody.removeChild(loadingMessage);
                addBotMessage(data.message);
            })
            .catch(error => {
                console.error('Error:', error);
                chatBody.removeChild(loadingMessage);
                addBotMessage("Lo siento, ha ocurrido un error. Por favor, intenta de nuevo más tarde.");
            });
    }

    // Agregar mensaje del usuario al chat
    function addUserMessage(message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chat-message user-message';

        const now = new Date();
        const timeString = now.getHours().toString().padStart(2, '0') + ':' +
            now.getMinutes().toString().padStart(2, '0');

        messageDiv.innerHTML = `
            <div class="message-content">
                <p>${message}</p>
                <span class="message-time">${timeString}</span>
            </div>
        `;

        chatBody.appendChild(messageDiv);
        chatBody.scrollTop = chatBody.scrollHeight;
        return messageDiv;
    }

    // Agregar mensaje del bot al chat
    function addBotMessage(message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chat-message bot-message';

        const now = new Date();
        const timeString = now.getHours().toString().padStart(2, '0') + ':' +
            now.getMinutes().toString().padStart(2, '0');

        messageDiv.innerHTML = `
            <div class="message-content">
                <p>${message}</p>
                <span class="message-time">${timeString}</span>
            </div>
        `;

        chatBody.appendChild(messageDiv);
        chatBody.scrollTop = chatBody.scrollHeight;
        return messageDiv;
    }

    // Event listeners para enviar mensaje
    sendBtn.addEventListener('click', sendMessage);

    chatInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            sendMessage();
            e.preventDefault();
        }
    });

    // Event listeners para botones de sugerencias
    suggestionBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const message = btn.textContent;
            chatInput.value = message;
            sendMessage();
        });
    });
});