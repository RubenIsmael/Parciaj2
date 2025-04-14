document.addEventListener('DOMContentLoaded', function () {
    const searchForm = document.getElementById('searchReserva');
    const reservaDetails = document.getElementById('reservaDetails');
    const paymentForm = document.getElementById('paymentForm');
    const paymentOptions = document.querySelectorAll('.payment-option');

    searchForm.addEventListener('submit', function (e) {
        e.preventDefault();
        const cedula = document.getElementById('cedulaSearch').value;

        // Aquí deberías hacer una llamada AJAX al controlador para buscar la reserva
        fetch(`/Reserva/BuscarPorCedula?cedula=${cedula}`)
            .then(response => response.json())
            .then(data => {
                if (data) {
                    document.getElementById('nombreCliente').textContent = `${data.nombre} ${data.apellido}`;
                    document.getElementById('cedulaCliente').textContent = data.cedula;
                    document.getElementById('bovedaDetalle').textContent = data.bobeda.nombre;
                    document.getElementById('estadoPagoDetalle').textContent = data.estadoPago;
                    document.getElementById('montoPendiente').textContent = `$${data.precio.monto}`;

                    reservaDetails.style.display = 'block';
                } else {
                    alert('No se encontró reserva con esa cédula');
                }
            });
    });

    paymentOptions.forEach(option => {
        option.addEventListener('click', function () {
            const method = this.dataset.method;
            document.getElementById('metodoPago').value = method;
            paymentForm.style.display = 'block';

            // Mostrar/ocultar secciones según método de pago
            const comprobanteSection = document.getElementById('comprobante-section');
            const tarjetaSection = document.getElementById('tarjeta-section');

            if (method === 'transferencia' || method === 'deposito') {
                comprobanteSection.style.display = 'block';
                tarjetaSection.style.display = 'none';
            } else if (method === 'tarjeta') {
                tarjetaSection.style.display = 'block';
                comprobanteSection.style.display = 'none';
            }
        });
    });

    const paymentProcessForm = document.getElementById('paymentProcessForm');
    paymentProcessForm.addEventListener('submit', function (e) {
        e.preventDefault();
        // Lógica para procesar el pago
        alert('Pago procesado exitosamente');
    });
    // Búsqueda de Reserva
    searchForm.addEventListener('submit', function (e) {
        e.preventDefault();
        const cedula = document.getElementById('cedulaSearch').value;

        fetch('/Gestion/BuscarReserva', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: `Cedula=${cedula}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    document.getElementById('nombreCliente').textContent = `${data.nombre} ${data.apellido}`;
                    document.getElementById('cedulaCliente').textContent = data.cedula;
                    document.getElementById('bovedaDetalle').textContent = data.bobeda;
                    document.getElementById('estadoPagoDetalle').textContent = data.estadoPago;
                    document.getElementById('montoPendiente').textContent = `$${data.montoPendiente}`;

                    reservaDetails.style.display = 'block';
                } else {
                    alert(data.message || 'No se encontró reserva');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Ocurrió un error al buscar la reserva');
            });
    });

    // Métodos de Pago
    paymentOptions.forEach(option => {
        option.addEventListener('click', function () {
            const method = this.dataset.method;
            document.getElementById('metodoPago').value = method;
            paymentForm.style.display = 'block';

            const comprobanteSection = document.getElementById('comprobante-section');
            const tarjetaSection = document.getElementById('tarjeta-section');

            if (method === 'transferencia' || method === 'deposito') {
                comprobanteSection.style.display = 'block';
                tarjetaSection.style.display = 'none';
            } else if (method === 'tarjeta') {
                tarjetaSection.style.display = 'block';
                comprobanteSection.style.display = 'none';
            }
        });
    });

    // Procesamiento de Pago
    const paymentProcessForm = document.getElementById('paymentProcessForm');
    paymentProcessForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const formData = new FormData(this);

        fetch('/Gestion/ProcesarPago', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                alert(data.message);
                // Resetear formularios
                paymentProcessForm.reset();
                reservaDetails.style.display = 'none';
                paymentForm.style.display = 'none';
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Ocurrió un error al procesar el pago');
            });
    });
});