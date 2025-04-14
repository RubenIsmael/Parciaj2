// Inicialización de variables globales
let mainChart = null;
let secondaryChart = null;
let currentQuestion = null;

// Configuración inicial de colores y temas
const colors = {
    primary: '#4e73df',
    success: '#1cc88a',
    danger: '#e74a3b',
    warning: '#f6c23e',
    secondary: '#858796',
    info: '#36b9cc',
    light: '#f8f9fc',
    dark: '#5a5c69'
};

// Inicializar animaciones y eventos al cargar la página
document.addEventListener('DOMContentLoaded', function () {
    // Inicializar selectores y eventos
    initializeEventListeners();

    // Configurar gráficos vacíos
    setupEmptyCharts();

    // Establecer fecha actual en el filtro de fecha fin
    const today = new Date();
    document.getElementById('endDate').valueAsDate = today;

    // Establecer fecha 3 meses atrás para el filtro de fecha inicio
    const threeMonthsAgo = new Date();
    threeMonthsAgo.setMonth(today.getMonth() - 3);
    document.getElementById('startDate').valueAsDate = threeMonthsAgo;
});

// Inicializar los listeners de eventos
function initializeEventListeners() {
    // Evento al cambiar la pregunta seleccionada
    document.getElementById('questionSelector').addEventListener('change', function (e) {
        const selectedQuestion = e.target.value;
        if (selectedQuestion) {
            currentQuestion = selectedQuestion;
            showLoading();
            setTimeout(() => {
                fetchDataAndUpdateUI(selectedQuestion);
            }, 600); // Pequeño delay para ver el efecto de carga
        }
    });

    // Evento al aplicar filtros de fecha
    document.getElementById('applyFilters').addEventListener('click', function () {
        if (currentQuestion) {
            showLoading();
            setTimeout(() => {
                fetchDataAndUpdateUI(currentQuestion);
            }, 600);
        } else {
            alert('Por favor, seleccione una pregunta primero.');
        }
    });
}

// Mostrar el overlay de carga
function showLoading() {
    document.getElementById('loadingOverlay').classList.remove('d-none');
}

// Ocultar el overlay de carga
function hideLoading() {
    document.getElementById('loadingOverlay').classList.add('d-none');
}

// Configurar gráficos vacíos para mostrar al inicio
function setupEmptyCharts() {
    // Configuración del gráfico principal vacío
    const mainChartOptions = {
        series: [{
            name: 'Sin datos',
            data: [0, 0, 0]
        }],
        chart: {
            type: 'bar',
            height: 400,
            fontFamily: 'Plom, sans-serif',
            toolbar: {
                show: true
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800
            }
        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '55%',
                borderRadius: 5
            },
        },
        colors: [colors.primary],
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent']
            }
        },
        xaxis: {
            categories: ['Seleccione', 'una', 'pregunta'],
        },
        yaxis: {
            title: {
                text: 'Valor'
            }
        },
        fill: {
            opacity: 1
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val
                }
            }
        },
        noData: {
            text: 'Seleccione una pregunta para visualizar datos',
            align: 'center',
            verticalAlign: 'middle',
            style: {
                color: colors.secondary,
                fontSize: '18px'
            }
        }
    };

    // Configuración del gráfico secundario vacío
    const secondaryChartOptions = {
        series: [0, 0, 0],
        chart: {
            type: 'donut',
            height: 400,
            fontFamily: 'Plom, sans-serif',
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800
            }
        },
        labels: ['Sin datos', 'Sin datos', 'Sin datos'],
        colors: [colors.primary, colors.success, colors.warning],
        legend: {
            position: 'bottom'
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 200
                },
                legend: {
                    position: 'bottom'
                }
            }
        }],
        plotOptions: {
            pie: {
                donut: {
                    size: '50%'
                }
            }
        },
        noData: {
            text: 'Sin datos',
            align: 'center',
            verticalAlign: 'middle',
            style: {
                color: colors.secondary,
                fontSize: '18px'
            }
        }
    };

    // Crear instancias de los gráficos
    mainChart = new ApexCharts(document.getElementById('mainChart'), mainChartOptions);
    secondaryChart = new ApexCharts(document.getElementById('secondaryChart'), secondaryChartOptions);

    // Renderizar los gráficos
    mainChart.render();
    secondaryChart.render();
}

// Función para obtener datos y actualizar la interfaz
function fetchDataAndUpdateUI(question) {
    // Obtener fechas de los filtros
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;

    // Hacer la petición al backend para obtener los datos según la pregunta
    fetch(`/api/Dashboard/GetData?question=${question}&startDate=${startDate}&endDate=${endDate}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error en la respuesta del servidor');
            }
            return response.json();
        })
        .then(data => {
            updateUI(question, data);
            hideLoading();
        })
        .catch(error => {
            console.error('Error:', error);
            // En caso de error, usar datos de ejemplo para demostración
            const mockData = getMockData(question);
            updateUI(question, mockData);
            hideLoading();
        });
}

// Datos de ejemplo para demostración
function getMockData(question) {
    // Dependiendo de la pregunta seleccionada, devolver datos de ejemplo específicos
    switch (question) {
        case 'reservas-periodo':
            return {
                kpis: [
                    { title: 'Total Reservas', value: 42, description: 'en el período seleccionado' },
                    { title: 'Promedio Mensual', value: 14, description: 'reservas por mes' },
                    { title: 'Crecimiento', value: '12%', description: 'vs. período anterior' }
                ],
                chartData: {
                    categories: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
                    series: [
                        { name: 'Reservas', data: [8, 12, 15, 7, 9, 11, 13, 14, 12, 10, 9, 12] }
                    ]
                },
                secondaryChart: {
                    labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                    series: [35, 27, 39, 29]
                },
                tableData: [
                    { categoria: 'Enero', valor: '8 reservas' },
                    { categoria: 'Febrero', valor: '12 reservas' },
                    { categoria: 'Marzo', valor: '15 reservas' },
                    { categoria: 'Abril', valor: '7 reservas' },
                    { categoria: 'Mayo', valor: '9 reservas' }
                ]
            };

        case 'estado-reservas':
            return {
                kpis: [
                    { title: 'Total Reservas', value: 42, description: 'en el período' },
                    { title: 'Pagadas', value: '71%', description: 'del total' },
                    { title: 'Pendientes', value: '29%', description: 'del total' }
                ],
                chartData: {
                    categories: ['Pagadas', 'Pendientes', 'Canceladas'],
                    series: [
                        { name: 'Reservas', data: [30, 12, 0] }
                    ]
                },
                secondaryChart: {
                    labels: ['Pagadas', 'Pendientes', 'Canceladas'],
                    series: [30, 12, 0]
                },
                tableData: [
                    { categoria: 'Pagadas', valor: '30 reservas (71%)' },
                    { categoria: 'Pendientes', valor: '12 reservas (29%)' },
                    { categoria: 'Canceladas', valor: '0 reservas (0%)' }
                ]
            };

        case 'bobedas-estado':
            return {
                kpis: [
                    { title: 'Total Bóvedas', value: 11, description: 'registradas' },
                    { title: 'Disponibles', value: '55%', description: 'del total' },
                    { title: 'Ocupadas', value: '45%', description: 'del total' }
                ],
                chartData: {
                    categories: ['Libre', 'Arriendo', 'Propietario'],
                    series: [
                        { name: 'Bóvedas', data: [6, 2, 3] }
                    ]
                },
                secondaryChart: {
                    labels: ['Libre', 'Arriendo', 'Propietario'],
                    series: [6, 2, 3]
                },
                tableData: [
                    { categoria: 'Libre', valor: '6 bóvedas (55%)' },
                    { categoria: 'Arriendo', valor: '2 bóvedas (18%)' },
                    { categoria: 'Propietario', valor: '3 bóvedas (27%)' }
                ]
            };

        case 'ingresos-periodo':
            return {
                kpis: [
                    { title: 'Total Ingresos', value: '$5,302', description: 'en el período' },
                    { title: 'Promedio Mensual', value: '$1,767', description: 'por mes' },
                    { title: 'Mayor Ingreso', value: '$2,200', description: 'en marzo' }
                ],
                chartData: {
                    categories: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
                    series: [
                        { name: 'Ingresos ($)', data: [600, 850, 2200, 1652, 0, 0, 0, 0, 0, 0, 0, 0] }
                    ]
                },
                secondaryChart: {
                    labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                    series: [3650, 1652, 0, 0]
                },
                tableData: [
                    { categoria: 'Enero', valor: '$600' },
                    { categoria: 'Febrero', valor: '$850' },
                    { categoria: 'Marzo', valor: '$2,200' },
                    { categoria: 'Abril', valor: '$1,652' }
                ]
            };

        case 'precio-promedio':
            return {
                kpis: [
                    { title: 'Precio Promedio', value: '$915', description: 'por bóveda' },
                    { title: 'Precio Máximo', value: '$1,800', description: 'bóveda premium' },
                    { title: 'Precio Mínimo', value: '$80', description: 'bóveda básica' }
                ],
                chartData: {
                    categories: ['Parte Alta', 'Torre', 'Jardines', 'Parte Baja', 'Capilla'],
                    series: [
                        { name: 'Precio ($)', data: [1200, 1800, 1222, 80, 80] }
                    ]
                },
                secondaryChart: {
                    labels: ['Parte Alta', 'Torre', 'Jardines', 'Parte Baja', 'Capilla'],
                    series: [1200, 1800, 1222, 80, 80]
                },
                tableData: [
                    { categoria: 'Parte Alta', valor: '$1,200' },
                    { categoria: 'Torre', valor: '$1,800' },
                    { categoria: 'Jardines', valor: '$1,222' },
                    { categoria: 'Parte Baja', valor: '$80' },
                    { categoria: 'Capilla', valor: '$80' }
                ]
            };

        case 'mensajes-estado':
            return {
                kpis: [
                    { title: 'Total Mensajes', value: 12, description: 'recibidos' },
                    { title: 'No Leídos', value: 8, description: '67% del total' },
                    { title: 'Tiempo Respuesta', value: '48h', description: 'promedio' }
                ],
                chartData: {
                    categories: ['Ene', 'Feb', 'Mar', 'Abr'],
                    series: [
                        { name: 'Recibidos', data: [3, 5, 2, 2] },
                        { name: 'Respondidos', data: [2, 1, 1, 0] }
                    ]
                },
                secondaryChart: {
                    labels: ['Leídos', 'No Leídos'],
                    series: [4, 8]
                },
                tableData: [
                    { categoria: 'Total Mensajes', valor: '12' },
                    { categoria: 'Mensajes Leídos', valor: '4 (33%)' },
                    { categoria: 'Mensajes No Leídos', valor: '8 (67%)' }
                ]
            };

        case 'tendencias-tiempo':
            return {
                kpis: [
                    { title: 'Tendencia Reservas', value: '+15%', description: 'crecimiento mensual' },
                    { title: 'Tendencia Pagos', value: '+12%', description: 'crecimiento mensual' },
                    { title: 'Correlación', value: '0.87', description: 'reservas-pagos' }
                ],
                chartData: {
                    categories: ['Ene', 'Feb', 'Mar', 'Abr'],
                    series: [
                        { name: 'Reservas', data: [8, 10, 12, 14] },
                        { name: 'Pagos', data: [6, 8, 10, 12] }
                    ]
                },
                secondaryChart: {
                    labels: ['Reservas', 'Pagos', 'Mensajes'],
                    series: [44, 36, 12]
                },
                tableData: [
                    { categoria: 'Enero', valor: '8 reservas, 6 pagos' },
                    { categoria: 'Febrero', valor: '10 reservas, 8 pagos' },
                    { categoria: 'Marzo', valor: '12 reservas, 10 pagos' },
                    { categoria: 'Abril', valor: '14 reservas, 12 pagos' }
                ]
            };

        case 'clientes-reservas':
            return {
                kpis: [
                    { title: 'Total Clientes', value: 8, description: 'con reservas' },
                    { title: 'Cliente Frecuente', value: 'Carlos M.', description: '3 reservas' },
                    { title: 'Nuevos Clientes', value: 2, description: 'este mes' }
                ],
                chartData: {
                    categories: ['Juan L.', 'María F.', 'Carlos M.', 'Ana L.', 'Pedro G.'],
                    series: [
                        { name: 'Reservas', data: [1, 1, 3, 1, 1] }
                    ]
                },
                secondaryChart: {
                    labels: ['1 Reserva', '2 Reservas', '3+ Reservas'],
                    series: [6, 1, 1]
                },
                tableData: [
                    { categoria: 'Juan Loza', valor: '1 reserva' },
                    { categoria: 'María Fernandez', valor: '1 reserva' },
                    { categoria: 'Carlos Mendoza', valor: '3 reservas' },
                    { categoria: 'Ana Lopez', valor: '1 reserva' },
                    { categoria: 'Pedro Gomez', valor: '1 reserva' }
                ]
            };

        case 'promedio-cliente':
            return {
                kpis: [
                    { title: 'Promedio Reservas', value: '1.5', description: 'por cliente' },
                    { title: 'Moda', value: '1', description: 'reservas por cliente' },
                    { title: 'Máx. Reservas', value: '3', description: 'por un cliente' }
                ],
                chartData: {
                    categories: ['1 Reserva', '2 Reservas', '3 Reservas'],
                    series: [
                        { name: 'Clientes', data: [6, 1, 1] }
                    ]
                },
                secondaryChart: {
                    labels: ['1 Reserva', '2 Reservas', '3 Reservas'],
                    series: [6, 1, 1]
                },
                tableData: [
                    { categoria: '1 Reserva', valor: '6 clientes' },
                    { categoria: '2 Reservas', valor: '1 cliente' },
                    { categoria: '3 Reservas', valor: '1 cliente' },
                    { categoria: 'Total', valor: '8 clientes' }
                ]
            };

        case 'pagos-promedio':
            return {
                kpis: [
                    { title: 'Total Pagos', value: 36, description: 'registrados' },
                    { title: 'Monto Promedio', value: '$147', description: 'por pago' },
                    { title: 'Monto Máximo', value: '$1,800', description: 'pago único' }
                ],
                chartData: {
                    categories: ['<$100', '$100-$500', '$500-$1000', '>$1000'],
                    series: [
                        { name: 'Número de Pagos', data: [12, 15, 5, 4] }
                    ]
                },
                secondaryChart: {
                    labels: ['<$100', '$100-$500', '$500-$1000', '>$1000'],
                    series: [12, 15, 5, 4]
                },
                tableData: [
                    { categoria: 'Menos de $100', valor: '12 pagos' },
                    { categoria: '$100 - $500', valor: '15 pagos' },
                    { categoria: '$500 - $1000', valor: '5 pagos' },
                    { categoria: 'Más de $1000', valor: '4 pagos' }
                ]
            };

        case 'bobeda-demanda':
            return {
                kpis: [
                    { title: 'Mayor Demanda', value: 'Torre', description: '5 reservas' },
                    { title: 'Ocupación', value: '45%', description: 'de bóvedas' },
                    { title: 'Precio Promedio', value: '$915', description: 'por bóveda' }
                ],
                chartData: {
                    categories: ['Parte Alta', 'Torre', 'Jardines', 'Parte Baja', 'Capilla'],
                    series: [
                        { name: 'Reservas', data: [1, 5, 2, 1, 1] }
                    ]
                },
                secondaryChart: {
                    labels: ['Parte Alta', 'Torre', 'Jardines', 'Parte Baja', 'Capilla'],
                    series: [1, 5, 2, 1, 1]
                },
                tableData: [
                    { categoria: 'Parte Alta', valor: '1 reserva (10%)' },
                    { categoria: 'Torre', valor: '5 reservas (50%)' },
                    { categoria: 'Jardines', valor: '2 reservas (20%)' },
                    { categoria: 'Parte Baja', valor: '1 reserva (10%)' },
                    { categoria: 'Capilla', valor: '1 reserva (10%)' }
                ]
            };

        case 'meses-actividad':
            return {
                kpis: [
                    { title: 'Mes más Activo', value: 'Marzo', description: '15 reservas' },
                    { title: 'Promedio Mensual', value: '12', description: 'reservas por mes' },
                    { title: 'Mayor Ingreso', value: 'Marzo', description: '$2,200' }
                ],
                chartData: {
                    categories: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
                    series: [
                        { name: 'Reservas', data: [8, 12, 15, 7, 9, 11, 13, 14, 12, 10, 9, 12] },
                        { name: 'Pagos', data: [6, 9, 12, 5, 7, 8, 10, 11, 9, 8, 7, 9] }
                    ]
                },
                secondaryChart: {
                    labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                    series: [35, 27, 39, 29]
                },
                tableData: [
                    { categoria: 'Enero', valor: '8 reservas, $600' },
                    { categoria: 'Febrero', valor: '12 reservas, $850' },
                    { categoria: 'Marzo', valor: '15 reservas, $2,200' },
                    { categoria: 'Abril', valor: '7 reservas, $1,652' }
                ]
            };

        default:
            return {
                kpis: [
                    { title: 'Sin datos', value: '0', description: 'No hay datos disponibles' },
                    { title: 'Sin datos', value: '0', description: 'No hay datos disponibles' },
                    { title: 'Sin datos', value: '0', description: 'No hay datos disponibles' }
                ],
                chartData: {
                    categories: ['Sin datos'],
                    series: [
                        { name: 'Sin datos', data: [0] }
                    ]
                },
                secondaryChart: {
                    labels: ['Sin datos'],
                    series: [0]
                },
                tableData: [
                    { categoria: 'Sin datos', valor: 'No hay datos disponibles' }
                ]
            };
    }
}

// Función para actualizar la interfaz con los datos
function updateUI(question, data) {
    // Actualizar títulos basados en la pregunta seleccionada
    updateTitles(question);

    // Actualizar KPIs
    updateKPIs(data.kpis);

    // Actualizar gráfico principal
    updateMainChart(data.chartData, question);

    // Actualizar gráfico secundario
    updateSecondaryChart(data.secondaryChart, question);

    // Actualizar tabla de datos
    updateTable(data.tableData);
}

// Actualizar títulos según la pregunta seleccionada
function updateTitles(question) {
    let mainChartTitle = '';
    let secondaryChartTitle = '';

    switch (question) {
        case 'reservas-periodo':
            mainChartTitle = 'Reservas por Mes';
            secondaryChartTitle = 'Reservas por Trimestre';
            break;
        case 'estado-reservas':
            mainChartTitle = 'Estado de Reservas';
            secondaryChartTitle = 'Distribución de Estados';
            break;
        case 'clientes-reservas':
            mainChartTitle = 'Reservas por Cliente';
            secondaryChartTitle = 'Distribución de Clientes';
            break;
        case 'ingresos-periodo':
            mainChartTitle = 'Ingresos por Mes';
            secondaryChartTitle = 'Ingresos por Trimestre';
            break;
        case 'bobedas-estado':
            mainChartTitle = 'Estado de Bóvedas';
            secondaryChartTitle = 'Distribución de Estados';
            break;
        case 'precio-promedio':
            mainChartTitle = 'Precio por Tipo de Bóveda';
            secondaryChartTitle = 'Comparativa de Precios';
            break;
        case 'mensajes-estado':
            mainChartTitle = 'Mensajes por Mes';
            secondaryChartTitle = 'Estado de Mensajes';
            break;
        case 'tendencias-tiempo':
            mainChartTitle = 'Tendencias de Reservas y Pagos';
            secondaryChartTitle = 'Distribución de Actividad';
            break;
        case 'bobeda-demanda':
            mainChartTitle = 'Demanda por Tipo de Bóveda';
            secondaryChartTitle = 'Distribución de Demanda';
            break;
        case 'promedio-cliente':
            mainChartTitle = 'Reservas por Cliente';
            secondaryChartTitle = 'Distribución de Reservas';
            break;
        case 'pagos-promedio':
            mainChartTitle = 'Distribución de Pagos';
            secondaryChartTitle = 'Rangos de Pagos';
            break;
        case 'meses-actividad':
            mainChartTitle = 'Actividad Mensual';
            secondaryChartTitle = 'Actividad Trimestral';
            break;
        default:
            mainChartTitle = 'Gráfico Principal';
            secondaryChartTitle = 'Gráfico Secundario';
    }

    // Actualizar los títulos en la UI
    document.getElementById('chartTitle').innerHTML = `<i class="fas fa-chart-bar me-2"></i>${mainChartTitle}`;
    document.getElementById('secondaryChartTitle').innerHTML = `<i class="fas fa-chart-pie me-2"></i>${secondaryChartTitle}`;
}

// Actualizar KPIs
function updateKPIs(kpis) {
    // Código actualizado
    if (!kpis) return;

    // Aplicar animación con countUp.js para los valores numéricos
    for (let i = 0; i < 3; i++) {
        if (kpis[i]) {
            document.getElementById(`kpi${i + 1}Title`).textContent = kpis[i].title;
            document.getElementById(`kpi${i + 1}Desc`).textContent = kpis[i].description;

            // Solo aplicar countUp si el valor es numérico
            const kpiValue = document.getElementById(`kpi${i + 1}Value`);
            kpiValue.textContent = kpis[i].value;

            // Verificar si el valor es número para aplicar countUp
            if (!isNaN(parseFloat(kpis[i].value))) {
                const countUp = new CountUp(kpiValue, parseFloat(kpis[i].value), {
                    startVal: 0,
                    duration: 2,
                    useEasing: true,
                    useGrouping: true,
                    separator: ',',
                    decimal: '.'
                });
                if (!countUp.error) {
                    countUp.start();
                }
            }
        } else {
            document.getElementById(`kpi${i + 1}Title`).textContent = 'KPI';
            document.getElementById(`kpi${i + 1}Value`).textContent = '--';
            document.getElementById(`kpi${i + 1}Desc`).textContent = 'No disponible';
        }
    }

    // Aplicar animación con countUp.js para los valores numéricos
    for (let i = 0; i < 3; i++) {
        if (kpis[i]) {
            document.getElementById(`kpi${i + 1}Title`).textContent = kpis[i].title;
            document.getElementById(`kpi${i + 1}Desc`).textContent = kpis[i].description;

            // Solo aplicar countUp si el valor es numérico
            const kpiValue = document.getElementById(`kpi${i + 1}Value`);
            kpiValue.textContent = kpis[i].value;

            // Verificar si el valor es número para aplicar countUp
            if (!isNaN(parseFloat(kpis[i].value))) {
                const countUp = new CountUp(kpiValue, parseFloat(kpis[i].value), {
                    startVal: 0,
                    duration: 2,
                    useEasing: true,
                    useGrouping: true,
                    separator: ',',
                    decimal: '.'
                });
                if (!countUp.error) {
                    countUp.start();
                }
            }
        } else {
            document.getElementById(`kpi${i + 1}Title`).textContent = 'KPI';
            document.getElementById(`kpi${i + 1}Value`).textContent = '--';
            document.getElementById(`kpi${i + 1}Desc`).textContent = 'No disponible';
        }
    }


    // Actualizar gráfico principal
    function updateMainChart(data) {
        if (!data || !data.chartData) return;

        const chartContainer = document.getElementById('mainChart');
        if (!chartContainer) return;

        try {
            // Validar datos para evitar NaN
            const series = data.chartData.series.map(s => {
                return {
                    name: s.name || 'Serie',
                    data: Array.isArray(s.data) ? s.data.map(val => {
                        const numVal = Number(val);
                        return isNaN(numVal) ? 0 : numVal; // Reemplaza NaN con 0
                    }) : []
                };
            });
            mainChart.updateOptions(options);
        } catch (error) {
            console.error('Error al actualizar el gráfico principal:', error);
        }

        const categories = Array.isArray(data.chartData.categories) ?
            data.chartData.categories : [];

        // Actualizar gráfico principal
        function updateMainChart(chartData, question) {

            if (!chartData) return;

            const chartContainer = document.getElementById('mainChart');
            if (!chartContainer) return;

            try {
                // Determinar el tipo de gráfico según la pregunta
                let chartType = 'bar';
                switch (question) {
                    case 'reservas-periodo':
                    case 'ingresos-periodo':
                    case 'tendencias-tiempo':
                        chartType = 'line';
                        break;
                    case 'mensajes-estado':
                        chartType = 'area';
                        break;
                    default:
                        chartType = 'bar';
                }

                // Configuración para el gráfico principal
                const mainChartOptions = {
                    series: chartData.series,
                    chart: {
                        type: chartType,
                        height: 400,
                        fontFamily: 'Plom, sans-serif',
                        toolbar: {
                            show: true
                        },
                        animations: {
                            enabled: true,
                            easing: 'easeinout',
                            speed: 800
                        }
                    },
                    plotOptions: {
                        bar: {
                            horizontal: false,
                            columnWidth: '55%',
                            borderRadius: 5,
                            dataLabels: {
                                position: 'top'
                            }
                        }
                    },
                    colors: [colors.primary, colors.success, colors.warning, colors.info],
                    dataLabels: {
                        enabled: false
                    },
                    stroke: {
                        curve: 'smooth',
                        width: 2
                    },
                    grid: {
                        borderColor: '#e7e7e7',
                        row: {
                            colors: ['#f3f3f3', 'transparent']
                        }
                    },
                    xaxis: {
                        categories: chartData.categories,
                        labels: {
                            style: {
                                fontFamily: 'Plom, sans-serif'
                            }
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Valor',
                            style: {
                                fontFamily: 'Plom, sans-serif'
                            }
                        },
                        labels: {
                            style: {
                                fontFamily: 'Plom, sans-serif'
                            }
                        }
                    },
                    fill: {
                        opacity: 1,
                        type: chartType === 'area' ? 'gradient' : 'solid',
                        gradient: {
                            shade: 'light',
                            type: "vertical",
                            shadeIntensity: 0.4,
                            opacityFrom: 0.9,
                            opacityTo: 0.6,
                        }
                    },
                    tooltip: {
                        y: {
                            formatter: function (val) {
                                return val
                            }
                        },
                        theme: 'light',
                        style: {
                            fontFamily: 'Plom, sans-serif'
                        }
                    },
                    legend: {
                        position: 'top',
                        horizontalAlign: 'center',
                        fontFamily: 'Plom, sans-serif'
                    }
                };

                // Actualizar el gráfico principal
                mainChart.updateOptions(mainChartOptions);
            } catch (error) {
                console.error('Error updating main chart:', error);
                chartContainer.innerHTML = '<div class="alert alert-danger">Error al cargar el gráfico</div>';
            }
        }


        // Actualizar gráfico secundario
        function updateSecondaryChart(chartData, question, data) {
            if (!data || !data.secondaryChart) return;

            const chartContainer = document.getElementById('secondary-chart');
            if (!chartContainer) return;

            try {
                let chartType = 'donut';

                // Algunos tipos de preguntas pueden usar otro tipo de gráfico secundario
                if (question === 'tendencias-tiempo' || question === 'meses-actividad') {
                    chartType = 'bar';
                }

                // Configuración actualizada para el gráfico secundario
                const secondaryChartOptions = {
                    series: chartType === 'donut' ? chartData.series : [{
                        name: 'Valor',
                        data: chartData.series
                    }],
                    chart: {
                        type: chartType,
                        height: 400,
                        fontFamily: 'Plom, sans-serif',
                        animations: {
                            enabled: true,
                            easing: 'easeinout',
                            speed: 800
                        }
                    },
                    labels: chartData.labels,
                    colors: [colors.primary, colors.success, colors.warning, colors.info, colors.danger],
                    legend: {
                        position: 'bottom',
                        fontFamily: 'Plom, sans-serif'
                    },
                    responsive: [{
                        breakpoint: 480,
                        options: {
                            chart: {
                                width: 200
                            },
                            legend: {
                                position: 'bottom'
                            }
                        }
                    }],
                    plotOptions: {
                        pie: {
                            donut: {
                                size: '50%',
                                labels: {
                                    show: true,
                                    name: {
                                        show: true,
                                        fontFamily: 'Plom, sans-serif'
                                    },
                                    value: {
                                        show: true,
                                        fontFamily: 'Plom, sans-serif',
                                        formatter: function (val) {
                                            return val;
                                        }
                                    },
                                    total: {
                                        show: true,
                                        label: 'Total',
                                        fontFamily: 'Plom, sans-serif'
                                    }
                                }
                            }
                        },
                        bar: {
                            horizontal: chartType === 'bar',
                            borderRadius: 5,
                            dataLabels: {
                                position: 'top'
                            }
                        }
                    },
                    dataLabels: {
                        enabled: chartType === 'donut',
                        style: {
                            fontFamily: 'Plom, sans-serif'
                        }
                    }
                };
                if (window.secondaryChart) {
                    window.secondaryChart.destroy();
                }
                // Validación estricta de datos
                const series = Array.isArray(data.secondaryChart.series) ?
                    data.secondaryChart.series.map(val => {
                        return (val === undefined || val === null || isNaN(Number(val))) ? 0 : Number(val);
                    }) : [];

                const labels = Array.isArray(data.secondaryChart.labels) ?
                    data.secondaryChart.labels : [];

                // Verificar si hay datos para mostrar
                if (series.length === 0 || series.every(val => val === 0)) {
                    chartContainer.innerHTML = '<div class="alert alert-warning">No hay datos suficientes para mostrar el gráfico</div>';
                    return;
                }

                // Resto del código de configuración...
            } catch (error) {
                console.error('Error updating secondary chart:', error);
                chartContainer.innerHTML = '<div class="alert alert-danger">Error al cargar el gráfico</div>';
            }

            // Actualizar el gráfico secundario
            secondaryChart.updateOptions(secondaryChartOptions);
        }

        // Actualizar tabla de datos
        function updateTable(tableData) {
            const tableBody = document.getElementById('tableBody');
            const tableHeader = document.getElementById('tableHeader');

            // Actualizar cabecera de la tabla
            tableHeader.innerHTML = `
        <th>Categoría</th>
        <th>Valor</th>
    `;

            // Limpiar el cuerpo de la tabla
            tableBody.innerHTML = '';

            // Añadir filas de datos
            if (tableData.length > 0) {
                tableData.forEach(row => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                <td>${row.categoria}</td>
                <td>${row.valor}</td>
            `;
                    tableBody.appendChild(tr);
                });
            } else {
                tableBody.innerHTML = `
            <tr>
                <td colspan="2" class="text-center">No hay datos disponibles</td>
            </tr>
        `;
            }
        }

        // Añadimos algunas funciones adicionales para mejorar la experiencia de usuario

        // Función para exportar datos a CSV
        function exportToCSV() {
            const table = document.getElementById('dataTable');
            let csv = [];

            // Obtener cabeceras
            const headers = [];
            const headerCells = table.querySelectorAll('thead th');
            headerCells.forEach(cell => {
                headers.push(cell.textContent);
            });
            csv.push(headers.join(','));

            // Obtener filas de datos
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach(row => {
                const data = [];
                const cells = row.querySelectorAll('td');
                cells.forEach(cell => {
                    // Escapar comillas en los datos
                    data.push(`"${cell.textContent.replace(/"/g, '""')}"`);
                });
                csv.push(data.join(','));
            });

            // Crear archivo CSV
            const csvContent = csv.join('\n');
            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);

            // Crear enlace de descarga
            const link = document.createElement('a');
            link.setAttribute('href', url);
            link.setAttribute('download', 'reporte_dashboard.csv');
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }

        // Función para imprimir el dashboard
        function printDashboard() {
            window.print();
        }

        // Función para actualizar automáticamente los datos cada cierto tiempo
        function setupAutoRefresh(interval = 300000) { // 5 minutos por defecto
            setInterval(() => {
                if (currentQuestion) {
                    fetchDataAndUpdateUI(currentQuestion);
                }
            }, interval);
        }

        // Iniciar actualización automática
        setupAutoRefresh();

        // Agregar botones de exportación y compartir al cargar la página
        document.addEventListener('DOMContentLoaded', function () {
            const chatbotButton = document.getElementById('chatbot-button');
            if (chatbotButton) {
                chatbotButton.addEventListener('click', function () {

                    // Añadir botones de acción adicionales si no existen
                    if (!document.getElementById('exportBtn')) {
                        const actionsDiv = document.createElement('div');
                        actionsDiv.className = 'mt-3 mb-4 text-end';
                        actionsDiv.innerHTML = `
            <button id="exportBtn" class="btn btn-outline-primary me-2">
                <i class="fas fa-file-export me-1"></i> Exportar CSV
            </button>
            <button id="printBtn" class="btn btn-outline-dark">
                <i class="fas fa-print me-1"></i> Imprimir
            </button>
        `;

                        // Insertar después del filtro de fechas
                        const dateFilters = document.querySelector('.date-filters');
                        dateFilters.parentNode.insertBefore(actionsDiv, dateFilters.nextSibling);

                        // Agregar eventos a los botones
                        document.getElementById('exportBtn').addEventListener('click', exportToCSV);
                        document.getElementById('printBtn').addEventListener('click', printDashboard);
                    }
                });
            } else {
                console.warn('Elemento chatbot-button no encontrado');
            }

        });
    }
}