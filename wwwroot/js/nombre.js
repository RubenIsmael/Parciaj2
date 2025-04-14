$(document).ready(function () {
    console.log("Script cargado correctamente");

   

    $("#CedulaSelect").change(function () {
        var idSeleccionado = $(this).val();
        console.log("ID seleccionado:", idSeleccionado);

        if (idSeleccionado) {
            // Asignar directamente el IdCliente con el ID seleccionado
            $("#IdCliente").val(idSeleccionado);

            console.log("Iniciando petición AJAX a /Reservas/GetClientePorCedula con ID:", idSeleccionado);

            $.ajax({
                url: '/Reservas/GetClientePorCedula',
                type: 'GET',
                data: { id: idSeleccionado },
                success: function (data) {
                    console.log("Respuesta recibida:", data);

                    if (data.success) {
                        console.log("Nombres:", data.nombres);
                        console.log("Apellidos:", data.apellidos);
                        console.log("ID Cliente:", idSeleccionado);

                        $("#Nombre").val(data.nombres);
                        $("#Apellido").val(data.apellidos);

                        // Campos ocultos si los tienes
                        $("input[name='Nombre']").val(data.nombres);
                        $("input[name='Apellido']").val(data.apellidos);

                        // Asegurarse de que el IdCliente se establece correctamente
                        $("#IdCliente").val(idSeleccionado);
                        console.log("IdCliente establecido:", $("#IdCliente").val());
                    }
 else {
                        console.log("No se encontraron datos del cliente");
                        $("#Nombre").val('');
                        $("#Apellido").val('');
                        $("#IdCliente").val('');
                        alert("No se encontraron datos para este cliente.");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error en la petición AJAX:", status, error);
                    console.log("Respuesta del servidor:", xhr.responseText);
                    $("#Nombre").val('');
                    $("#Apellido").val('');
                    $("#IdCliente").val('');
                    alert("Error al obtener los datos del cliente: " + error);
                }
            });
        } else {
            console.log("No hay ID seleccionado, limpiando campos");
            $("#Nombre").val('');
            $("#Apellido").val('');
            $("#IdCliente").val('');
        }
    });

    // Verificar que los elementos existen
    console.log("Elemento #Nombre existe:", $("#Nombre").length > 0);
    console.log("Elemento #Apellido existe:", $("#Apellido").length > 0);
   // console.log("Elemento #CedulaSelect existe:", $("#CedulaSelect").length > 0);
    console.log("Elemento #IdCliente existe:", $("#IdCliente").length > 0);
});

