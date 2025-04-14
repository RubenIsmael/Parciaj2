// Archivo: wwwroot/js/gestion.js
$(document).ready(function () {
    // Ocultar las secciones al cargar la página
    $('#resultsSection').hide();
    $('#reservaDetails').hide();
    $('#paymentForm').hide();

    // Manejar el formulario de búsqueda
    $('#searchReserva').submit(function (e) {
        e.preventDefault();  // Prevenir el postback

        var cedula = $('#cedulaSearch').val();

        // Realizar la búsqueda mediante AJAX
        $.ajax({
            url: '/Home/BuscarReserva',  // Asegúrate que esta URL sea correcta
            type: 'POST',
            data: { Cedula: cedula },
            success: function (response) {
                if (response.success) {
                    // Mostrar los resultados en la tabla
                    mostrarResultados(response.reservas);
                } else {
                    // Mostrar mensaje de error
                    alert(response.message);
                }
            },
            error: function () {
                alert('Error al procesar la solicitud');
            }
        });
    });

    // Función para mostrar los resultados en la tabla
    function mostrarResultados(reservas) {
        var tbody = $('#resultsTableBody');
        tbody.empty();

        // Mostrar la sección de resultados
        $('#resultsSection').show();

        // Ocultar los detalles si estaban visibles
       // $('#reservaDetails').hide();
        //$('#paymentForm').hide();
        //$('.payment-methods').hide();

        // Agregar cada reserva a la tabla
        reservas.forEach(function (reserva) {
            var estadoClase = reserva.estadoPago === 'pagado' ? 'pagado' : 'pendiente';
            var fila = `
                <tr data-reserva-id="${reserva.reservaId}">
                    <td>${reserva.reservaId}</td>
                    <td>${reserva.cedula}</td>
                    <td>${reserva.nombre}</td>
                    <td>${reserva.apellido}</td>
                    <td>${reserva.sector}</td>
                    <td>$${reserva.precioValor.toFixed(2)}</td>
                    <td><span class="status-badge ${estadoClase}">${reserva.estadoPago}</span></td>
                    <td>${reserva.descripcion}</td>
                    <td>
                        <button class="btn btn-info btn-ver-detalles my-1" data-id="${reserva.reservaId}">
                            <i class="fas fa-info-circle me-1"></i> Ver Detalles
                        </button>
                        ${reserva.estadoPago === 'pendiente' ?
                    `<button class="btn btn-success btn-pagar my-1" data-id="${reserva.reservaId}" data-monto="${reserva.montoPendiente}">
                                <i class="fas fa-money-bill-wave me-1"></i> Pagar
                            </button>` : ''}
                    </td>
                </tr>
            `;
            tbody.append(fila);
        });

        // Agregar eventos a los botones
        $('.btn-ver-detalles').click(function () {
            var reservaId = $(this).data('id');
            mostrarDetallesReserva(reservaId, reservas);
        });

        $('.btn-pagar').click(function () {
            var reservaId = $(this).data('id');
            var montoPendiente = $(this).data('monto');
            iniciarProcesoDePago(reservaId, montoPendiente, reservas);
        });
    }

    // Función para mostrar los detalles de una reserva
    function mostrarDetallesReserva(reservaId, reservas) {
        var reserva = reservas.find(r => r.reservaId === reservaId);
        if (reserva) {
            $('#nombreCliente').text(reserva.nombre + ' ' + reserva.apellido);
            $('#cedulaCliente').text(reserva.cedula);
            $('#bovedaDetalle').text(reserva.sector + ' - ' + reserva.descripcion);
            $('#estadoPagoDetalle').text(reserva.estadoPago).removeClass().addClass('status-badge ' + (reserva.estadoPago === 'pagado' ? 'pagado' : 'pendiente'));
            $('#montoPendiente').text('$' + reserva.montoPendiente.toFixed(2));

            $('#reservaDetails').show();
        }
    }
});
