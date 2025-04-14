document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("btnGuardarDirecto").addEventListener("click", function (e) {
        e.preventDefault(); // Evita que el formulario se envíe normalmente

        // Capturar datos correctamente
        const reserva = {
            Id: 0, // Se generará automáticamente en la BD si es identity
            IdCliente: parseInt(document.getElementById("CedulaSelect").value) || 0,
            Nombre: document.getElementById("Nombre").value.trim(),
            Apellido: document.getElementById("Apellido").value.trim(),
            IdPrecio: parseInt(document.getElementById("PrecioSelect").value) || 0,
            EstadoPago: document.querySelector("[name='EstadoPago']").value.trim(),
            FechaReserva: document.querySelector("[name='FechaReserva']").value
        };

        // Validar que los campos obligatorios no estén vacíos
        if (reserva.IdCliente === 0 || reserva.IdPrecio === 0 || !reserva.Nombre || !reserva.Apellido || !reserva.EstadoPago || !reserva.FechaReserva) {
            alert("Por favor, completa todos los campos.");
            return;
        }

        // Enviar datos a la API
        fetch("/api/reservas/crear", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(reserva)
        })
            .then(response => response.json())
            .then(data => {
                alert(data.mensaje); // Confirmar que se guardó
                limpiarFormulario();
            })
            .catch(error => console.error("Error al guardar:", error));
    });

    function limpiarFormulario() {
        document.getElementById("CedulaSelect").value = "";
        document.getElementById("Nombre").value = "";
        document.getElementById("Apellido").value = "";
        document.getElementById("PrecioSelect").value = "";
        document.getElementById("PrecioValor").value = "";
        document.querySelector("[name='EstadoPago']").value = "";
        document.querySelector("[name='FechaReserva']").value = "";
    }
});
