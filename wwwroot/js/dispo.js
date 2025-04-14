document.addEventListener('DOMContentLoaded', function () {
    // Obtener precio de la bóveda
    document.getElementById('IdBobeda').addEventListener('change', function () {
        const idBobeda = this.value;
        if (idBobeda) {
            fetch(`/Reservas/ObtenerPrecio?idBobeda=${idBobeda}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        document.getElementById('PrecioValor').value = data.precio;
                    } else {
                        document.getElementById('PrecioValor').value = 'No disponible';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    document.getElementById('PrecioValor').value = 'Error al cargar';
                    mostrarAlerta('Error al obtener el precio', 'error');
                });
        }
    });

    // Enviar formulario de reserva
    document.getElementById('reservaForm').addEventListener('submit', function (e) {
        e.preventDefault();

        // Validar que se haya seleccionado un cliente
        if (!document.getElementById('CedulaSelect').value) {
            mostrarAlerta('Por favor seleccione un cliente', 'error');
            return;
        }

        // Validar que se haya cargado un precio
        if (!document.getElementById('IdPrecio').value) {
            mostrarAlerta('No se ha podido cargar el precio correctamente', 'error');
            return;
        }

        const formData = new FormData(this);
        const reservaData = {
            IdCliente: formData.get('Cedula'),
            IdBobeda: formData.get('IdBobeda'),
            IdPrecio: formData.get('IdPrecio'),
            Nombre: document.getElementById('Nombre').value,
            Apellido: document.getElementById('Apellido').value,
            EstadoPago: formData.get('EstadoPago'),
            FechaReserva: formData.get('FechaReserva')
        };

        // Obtener el token antifalsificación
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const metaToken = !token ? document.querySelector('meta[name="__RequestVerificationToken"]')?.content : null;
        const csrfToken = token || metaToken || '';

        // Verificar si ya existe una reserva para esta bóveda
        fetch(`/Reservas/VerificarDisponibilidad?idBobeda=${reservaData.IdBobeda}`)
            .then(response => response.json())
            .then(disponibilidad => {
                if (!disponibilidad.disponible) {
                    mostrarAlerta('Esta bóveda ya está reservada', 'error');
                    return;
                }

                // Si está disponible, proceder con la reserva
                realizarReserva(reservaData, csrfToken);
            })
            .catch(error => {
                console.error('Error al verificar disponibilidad:', error);
                realizarReserva(reservaData, csrfToken); // Intentar hacer la reserva de todos modos
            });
    });

    function realizarReserva(reservaData, token) {
        fetch('/Reservas/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': token,
                'RequestVerificationToken': token
            },
            body: JSON.stringify(reservaData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la respuesta del servidor');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    mostrarAlerta('¡Reserva creada con éxito!', 'success');

                    // Actualizar el estado de la bóveda en la UI
                    const bobedaCard = document.querySelector(`.reservar-btn[data-id="${reservaData.IdBobeda}"]`).closest('.bobeda-card');
                    if (bobedaCard) {
                        bobedaCard.dataset.estado = 'arriendo';
                        bobedaCard.classList.remove('available');
                        bobedaCard.classList.add('unavailable');

                        // Actualizar el estado y el botón
                        const estadoElement = bobedaCard.querySelector('strong.text-gray-800').nextSibling;
                        if (estadoElement) {
                            estadoElement.textContent = 'arriendo';
                        }

                        const btnContainer = bobedaCard.querySelector('.mt-6');
                        if (btnContainer) {
                            btnContainer.remove();
                        }

                        // Actualizar el indicador de estado
                        const estadoIndicator = bobedaCard.querySelector('.absolute.top-4.right-4 span');
                        if (estadoIndicator) {
                            estadoIndicator.className = 'bg-red-500 text-white px-3 py-1 rounded-full text-sm';
                            estadoIndicator.textContent = 'Ocupada';
                        }
                    }

                    setTimeout(() => {
                        reservaModal.style.display = 'none';
                        location.reload();
                    }, 2000);
                } else {
                    mostrarAlerta('Error: ' + (data.message || 'No se pudo procesar la reserva'), 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarAlerta('Ocurrió un error al procesar la solicitud', 'error');
            });
    }

    // Enviar formulario de cliente
    document.getElementById('clienteForm').addEventListener('submit', function (e) {
        e.preventDefault();

        // Validar cédula ecuatoriana
        const clave = document.getElementById('Clave').value;
        if (!validarCedulaEcuatoriana(clave)) {
            document.getElementById('Clave-error').textContent = 'La cédula ecuatoriana no es válida';
            document.getElementById('Clave-error').style.display = 'block';
            return;
        } else {
            document.getElementById('Clave-error').style.display = 'none';
        }

        const formData = new FormData(this);
        const clienteData = {
            Clave: formData.get('Clave'),
            Nombres: formData.get('Nombres'),
            Apellidos: formData.get('Apellidos'),
            Correo: formData.get('Correo')
        };

        // Obtener el token antifalsificación
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const metaToken = !token ? document.querySelector('meta[name="__RequestVerificationToken"]')?.content : null;
        const csrfToken = token || metaToken || '';

        fetch('/Clientes/CreateAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': csrfToken,
                'RequestVerificationToken': csrfToken
            },
            body: JSON.stringify(clienteData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error en la respuesta del servidor');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    mostrarAlerta('Cliente registrado con éxito', 'success');

                    // Añadir el nuevo cliente al dropdown
                    const select = document.getElementById('CedulaSelect');
                    const option = document.createElement('option');
                    option.value = data.clienteId || data.cliente.id;
                    option.text = clienteData.Clave;
                    option.setAttribute('data-nombre', clienteData.Nombres);
                    option.setAttribute('data-apellido', clienteData.Apellidos);
                    select.add(option);

                    // Seleccionar el cliente recién creado
                    select.value = data.clienteId || data.cliente.id;
                    document.getElementById('Nombre').value = clienteData.Nombres;
                    document.getElementById('Apellido').value = clienteData.Apellidos;

                    // Limpiar el formulario
                    document.getElementById('clienteForm').reset();

                    setTimeout(() => {
                        clienteModal.style.display = 'none';
                        reservaModal.style.display = 'flex';
                    }, 60000);
                } else {
                    // Mostrar errores
                    if (data.errors) {
                        Object.keys(data.errors).forEach(key => {
                            const errorElement = document.getElementById(key + '-error');
                            if (errorElement) {
                                errorElement.textContent = data.errors[key];
                                errorElement.style.display = 'block';
                            }
                        });
                    } else {
                        mostrarAlerta('Error: ' + (data.message || 'No se pudo registrar el cliente'), 'error');
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarAlerta('Ocurrió un error al procesar la solicitud', 'error');
            });
    });

    // Función para validar cédula ecuatoriana
    function validarCedulaEcuatoriana(cedula) {
        if (!/^\d{10}$/.test(cedula)) {
            return false;
        }

        const provincia = parseInt(cedula.substring(0, 2));
        if (provincia < 1 || provincia > 24) {
            return false;
        }

        const tercerDigito = parseInt(cedula.charAt(2));
        if (tercerDigito < 0 || tercerDigito > 6) {
            return false;
        }

        let suma = 0;
        for (let i = 0; i < 9; i++) {
            let multiplicador = (i % 2 === 0) ? 2 : 1;
            let valor = multiplicador * parseInt(cedula.charAt(i));
            suma += (valor >= 10) ? valor - 9 : valor;
        }

        const ultimoDigito = parseInt(cedula.charAt(9));
        const digitoVerificador = (10 - (suma % 10)) % 10;

        return ultimoDigito === digitoVerificador;
    }

    // Función para mostrar alertas personalizadas
    function mostrarAlerta(mensaje, tipo) {
        const alertasExistentes = document.querySelectorAll('.custom-alert');
        alertasExistentes.forEach(alerta => alerta.remove());

        const alertDiv = document.createElement('div');
        alertDiv.className = 'custom-alert ' + tipo;
        alertDiv.innerHTML = `
            <div class="alert-icon">
                <i class="fas fa-${tipo === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
            </div>
            <div class="alert-content">
                <h4>${tipo === 'success' ? '¡Éxito!' : '¡Error!'}</h4>
                <p>${mensaje}</p>
            </div>
            <button class="alert-close">&times;</button>
        `;
        document.body.appendChild(alertDiv);

        // Cerrar alerta al hacer clic en el botón de cerrar
        alertDiv.querySelector('.alert-close').addEventListener('click', function () {
            alertDiv.remove();
        });

        // Cerrar automáticamente después de 10 minutos
       // setTimeout(() => {
         //   if (document.body.contains(alertDiv)) {
           //     alertDiv.remove();
            //}
       // }, 600000);
    }
});
