function validarCedula(cedula) {
    // Verificar longitud
    if (cedula.length !== 10) {
        return false;
    }

    // Verificar que todos los caracteres sean dígitos
    if (!/^\d+$/.test(cedula)) {
        return false;
    }

    // Verificar el código de provincia (01 al 24)
    const provincia = parseInt(cedula.substring(0, 2));
    if (provincia < 1 || provincia > 24) {
        return false;
    }

    // Algoritmo de validación del último dígito
    const coeficientes = [2, 1, 2, 1, 2, 1, 2, 1, 2];
    const verificador = parseInt(cedula.charAt(9));
    let suma = 0;

    for (let i = 0; i < 9; i++) {
        let valor = parseInt(cedula.charAt(i)) * coeficientes[i];
        suma += (valor >= 10) ? valor - 9 : valor;
    }

    const digitoVerificador = (suma % 10 !== 0) ? 10 - (suma % 10) : 0;

    return verificador === digitoVerificador;
}

document.addEventListener("DOMContentLoaded", function () {
    const cedulaInput = document.getElementById("Clave");
    const cedulaError = document.getElementById("Clave-error");
    const form = document.getElementById("clienteForm");

    if (cedulaInput && cedulaError && form) {
        cedulaInput.addEventListener("input", function () {
            const cedula = cedulaInput.value;
            if (cedula.length === 10) {
                if (!validarCedula(cedula)) {
                    cedulaError.textContent = "La cédula ingresada no es válida.";
                    cedulaError.style.display = "block";
                } else {
                    cedulaError.textContent = "";
                    cedulaError.style.display = "none";
                }
            } else {
                cedulaError.textContent = "";
                cedulaError.style.display = "none";
            }
        });

        form.addEventListener("submit", function (event) {
            const cedula = cedulaInput.value;
            if (cedula.length === 10 && !validarCedula(cedula)) {
                cedulaError.textContent = "La cédula ingresada no es válida.";
                cedulaError.style.display = "block";
                event.preventDefault(); // Evita el envío del formulario
            }
        });
    }
});