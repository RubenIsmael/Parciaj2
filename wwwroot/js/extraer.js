document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("clienteSearch");
    const resultsContainer = document.getElementById("clienteResults");
    const clienteSeleccionado = document.getElementById("clienteSeleccionado");
    const clienteInfo = document.getElementById("clienteInfo");

    searchInput.addEventListener("input", function () {
        const query = searchInput.value;

        if (query.length > 0) {
            fetch(`/Contratoes/SearchClientes?query=${query}`)
                .then(response => response.json())
                .then(data => {
                    resultsContainer.innerHTML = ""; // Limpiar resultados anteriores
                    if (data.length > 0) {
                        data.forEach(cliente => {
                            const listItem = document.createElement("a");
                            listItem.className = "list-group-item list-group-item-action";
                            listItem.textContent = `${cliente.Clave} ${cliente.Apellidos}`;
                            listItem.onclick = function () {
                                searchInput.value = `${cliente.Clave} ${cliente.Apellidos}`;
                                clienteInfo.textContent = `Cédula: ${cliente.Clave}, Apellido: ${cliente.Apellidos}`;
                                clienteSeleccionado.style.display = "block"; // Mostrar el contenedor
                                resultsContainer.style.display = "none"; // Ocultar resultados
                            };
                            resultsContainer.appendChild(listItem);
                        });
                        resultsContainer.style.display = "block"; // Mostrar resultados
                    } else {
                        resultsContainer.innerHTML = "<a class='list-group-item list-group-item-action'>Cliente no registrado</a>";
                        resultsContainer.style.display = "block"; // Mostrar mensaje de cliente no registrado
                    }
                });
        } else {
            resultsContainer.style.display = "none"; // Ocultar resultados si no hay texto
            clienteSeleccionado.style.display = "none"; // Ocultar el contenedor de cliente seleccionado
        }
    });
});