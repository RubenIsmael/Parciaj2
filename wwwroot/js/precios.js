$(document).ready(function () {
    $('#PrecioSelect').on('change', function () {
        var idSeleccionado = $(this).val();

        if (idSeleccionado) {
            $.ajax({
                url: '/Reservas/ObtenerPrecioPorId', 
                type: 'GET',
                data: { id: idSeleccionado },
                success: function (data) {
                    $('#PrecioValor').val(data); // Coloca el valor en el input readonly
                },
                error: function () {
                    console.error('Error al obtener el precio');
                    $('#PrecioValor').val('');
                }
            });
        } else {
            $('#PrecioValor').val('');
        }
    });
});