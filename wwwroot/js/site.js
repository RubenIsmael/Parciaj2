// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Esperar a que el DOM esté completamente cargado
document.addEventListener('DOMContentLoaded', function () {
    // Elementos del chatbot
    const chatTrigger = document.getElementById('chatTrigger');
    const chatContainer = document.getElementById('chatContainer');
    const chatClose = document.getElementById('chatClose');
    const chatBody = document.getElementById('chatBody');
    const chatInput = document.getElementById('chatInput');
    const sendBtn = document.getElementById('sendBtn');
    const suggestionBtns = document.querySelectorAll('.suggestion-btn');

    // Mensajes predeterminados
    const initialMessage = {
        text: '¡Hola! Soy el asistente virtual del Cementerio San Agustín. ¿En qué puedo ayudarte?',
        time: formatTime(new Date())
    };

    // Función para abrir el chat
    function openChat() {
        chatContainer.style.display = 'flex';
        // Agregar mensaje inicial si el chat está vacío
        if (chatBody.children.length === 0) {
            addBotMessage(initialMessage.text, initialMessage.time);
        }
    }

    // Función para cerrar el chat
    function closeChat() {
        chatContainer.style.display = 'none';
    }

    // Función para formatear la hora
    function formatTime(date) {
        return `${date.getHours()}:${String(date.getMinutes()).padStart(2, '0')}`;
    }

    // Función para agregar mensaje del bot
    function addBotMessage(text, time) {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chat-message bot-message';
        messageDiv.innerHTML = `
            <div class="message-content">
                <p>${formatMessageText(text)}</p>
                <span class="message-time">${time}</span>
            </div>
        `;
        chatBody.appendChild(messageDiv);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Formatear texto del mensaje (convertir saltos de línea y viñetas)
    function formatMessageText(text) {
        // Reemplazar saltos de línea por etiquetas <br>
        let formattedText = text.replace(/\n/g, '<br>');

        // Reemplazar viñetas por elementos HTML
        formattedText = formattedText.replace(/•/g, '<br>• ');

        return formattedText;
    }

    // Función para agregar mensaje del usuario
    function addUserMessage(text) {
        const time = formatTime(new Date());
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chat-message user-message';
        messageDiv.innerHTML = `
            <div class="message-content">
                <p>${text}</p>
                <span class="message-time">${time}</span>
            </div>
        `;
        chatBody.appendChild(messageDiv);
        chatBody.scrollTop = chatBody.scrollHeight;

        // Mostrar indicador de carga
        showLoadingIndicator();

        // Enviar mensaje a la API y procesar respuesta
        procesarMensajeAPI(text);
    }

    // Mostrar indicador de carga mientras se procesa el mensaje
    function showLoadingIndicator() {
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'chat-message bot-message';
        loadingDiv.id = 'loading-indicator';
        loadingDiv.innerHTML = `
            <div class="message-content">
                <p><i class="fas fa-spinner fa-spin"></i> Escribiendo...</p>
            </div>
        `;
        chatBody.appendChild(loadingDiv);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Ocultar indicador de carga
    function hideLoadingIndicator() {
        const loadingIndicator = document.getElementById('loading-indicator');
        if (loadingIndicator) {
            loadingIndicator.remove();
        }
    }

    // Función para enviar mensaje a la API y procesar respuesta
    async function procesarMensajeAPI(mensaje) {
        try {
            const response = await fetch('/api/Chatbot/procesar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    mensaje: mensaje
                })
            });

            if (!response.ok) {
                throw new Error(`Error en la respuesta de la API: ${response.status} ${response.statusText}`);
            }

            const data = await response.json();

            // Ocultar indicador de carga
            hideLoadingIndicator();

            // Mostrar respuesta del bot
            const time = formatTime(new Date());
            addBotMessage(data.respuesta, time);
        } catch (error) {
            console.error('Error:', error);

            // Ocultar indicador de carga
            hideLoadingIndicator();

            // Mostrar mensaje de error
            const time = formatTime(new Date());
            addBotMessage('Lo siento, ocurrió un error al procesar tu mensaje. Por favor, intenta nuevamente.', time);
        }
    }

    // Event listeners
    if (chatTrigger) {
        chatTrigger.addEventListener('click', openChat);
    }

    if (chatClose) {
        chatClose.addEventListener('click', closeChat);
    }

    // Enviar mensaje al hacer clic en el botón
    if (sendBtn) {
        sendBtn.addEventListener('click', function () {
            const message = chatInput.value.trim();
            if (message) {
                addUserMessage(message);
                chatInput.value = '';
            }
        });
    }

    // Enviar mensaje al presionar Enter
    if (chatInput) {
        chatInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const message = chatInput.value.trim();
                if (message) {
                    addUserMessage(message);
                    chatInput.value = '';
                }
            }
        });
    }

    // Botones de sugerencias
    suggestionBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const suggestionText = this.textContent;
            addUserMessage(suggestionText);
        });
    });

    // Detectar scroll para efectos de la barra de navegación
    window.addEventListener('scroll', function () {
        const navbar = document.querySelector('.sticky-nav');
        if (navbar) {
            if (window.scrollY > 50) {
                navbar.classList.add('navbar-scrolled');
            } else {
                navbar.classList.remove('navbar-scrolled');
            }
        }
    });
});