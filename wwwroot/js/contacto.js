// contacto.js
document.addEventListener("DOMContentLoaded", () => {
    const chatTrigger = document.getElementById("chatTrigger");
    const chatContainer = document.getElementById("chatContainer");
    const chatClose = document.getElementById("chatClose");

    chatTrigger.addEventListener("click", () => {
        chatContainer.style.display = "flex";
    });

    chatClose.addEventListener("click", () => {
        chatContainer.style.display = "none";
    });

    const suggestionButtons = document.querySelectorAll(".suggestion-btn");
    const chatBody = document.getElementById("chatBody");
    const chatInput = document.getElementById("chatInput");
    const sendBtn = document.getElementById("sendBtn");

    suggestionButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            appendMessage("👤", btn.textContent);
            simulateBotResponse(btn.textContent);
        });
    });

    sendBtn.addEventListener("click", () => {
        const message = chatInput.value.trim();
        if (message !== "") {
            appendMessage("👤", message);
            simulateBotResponse(message);
            chatInput.value = "";
        }
    });

    function appendMessage(sender, text) {
        const msg = document.createElement("div");
        msg.classList.add("chat-message");
        msg.innerHTML = `<strong>${sender}:</strong> ${text}`;
        chatBody.appendChild(msg);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    function simulateBotResponse(message) {
        setTimeout(() => {
            let response = "Lo siento, no entendí eso 🤖";
            if (message.includes("bóveda")) response = "Sí, tenemos bóvedas disponibles. Contáctanos para más detalles.";
            if (message.includes("reserva")) response = "Puedes hacer una reserva directamente desde nuestra página web.";
            if (message.includes("pago")) response = "Revisa tu cuenta para ver los pagos pendientes.";
            if (message.includes("Horario") || message.includes("horario")) response = "Lunes a Viernes: 8AM - 6PM. Fines de semana: 9AM - 4PM.";

            appendMessage("🤖", response);
        }, 1000);
    }
});
