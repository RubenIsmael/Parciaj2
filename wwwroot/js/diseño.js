document.getElementById("sendButton").addEventListener("click", function () {
    const userInput = document.getElementById("userInput");
    const message = userInput.value;

    if (message.trim() !== "") {
        const chatBox = document.getElementById("chatBox");
        const userMessage = document.createElement("p");
        userMessage.textContent = "Tú: " + message;
        chatBox.appendChild(userMessage);
        userInput.value = "";

        // Simulación de respuesta del chatbot
        setTimeout(() => {
            const botMessage = document.createElement("p");
            botMessage.textContent = "Bot: " + "Respuesta a: " + message;
            chatBox.appendChild(botMessage);
            chatBox.scrollTop = chatBox.scrollHeight; // Desplazar hacia abajo
        }, 1000);
    }
});