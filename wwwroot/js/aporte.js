$(document).ready(function () {
    // Inicialmente ocultar todos los contenidos
    $('.text-largee').css({
        'max-height': '0',
        'opacity': '0',
        'padding-top': '0',
        'padding-bottom': '0'
    });

    $('.contact-subtitlee').click(function () {
        var targetId = $(this).data('target');
        var $targetContent = $('#' + targetId);
        var $thisSubtitle = $(this);

        // Si la sección ya está abierta, cerrarla
        if ($targetContent.hasClass('active')) {
            $targetContent.css({
                'max-height': '0',
                'opacity': '0',
                'padding-top': '0',
                'padding-bottom': '0'
            }).removeClass('active');
            $thisSubtitle.removeClass('active');
        } else {
            // Cerrar todas las secciones abiertas
            $('.text-largee').css({
                'max-height': '0',
                'opacity': '0',
                'padding-top': '0',
                'padding-bottom': '0'
            }).removeClass('active');
            $('.contact-subtitlee').removeClass('active');

            // Abrir la sección seleccionada
            $targetContent.css({
                'max-height': '1000px', // Ajusta este valor según tu contenido
                'opacity': '1',
                'padding-top': '20px',
                'padding-bottom': '20px'
            }).addClass('active');
            $thisSubtitle.addClass('active');
        }
    });
    // Map card fade-in animation
    $('.contact-map-card').addClass('fade-in');

    // Chatbot logic
    $('#chatTrigger').click(function () {
        $('#chatContainer').toggle();
    });

    $('#chatClose').click(function () {
        $('#chatContainer').hide();
    });
});