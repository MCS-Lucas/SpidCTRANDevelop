// dashboard.js — Chart.js interop helpers for the Spid Dashboard

const _charts = {};

window.renderDashboardChart = function (canvasId, labels, datasets) {
    // Destroy previous instance if it exists
    if (_charts[canvasId]) {
        _charts[canvasId].destroy();
        delete _charts[canvasId];
    }

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Colour palette for different operators
    const palette = [
        '#1a73e8', '#34a853', '#fbbc04', '#ea4335',
        '#9c27b0', '#00bcd4', '#ff5722', '#607d8b'
    ];

    const styledDatasets = datasets.map((ds, i) => ({
        label: ds.label,
        data: ds.data,
        borderColor: palette[i % palette.length],
        backgroundColor: palette[i % palette.length] + '22',
        pointBackgroundColor: palette[i % palette.length],
        pointRadius: 4,
        pointHoverRadius: 6,
        borderWidth: 2,
        tension: 0.3,
        fill: false
    }));

    _charts[canvasId] = new Chart(ctx, {
        type: 'line',
        data: { labels, datasets: styledDatasets },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { usePointStyle: true, boxWidth: 8 }
                },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                x: {
                    title: { display: true, text: 'Dia', font: { size: 11 } },
                    grid: { color: '#f0f0f0' }
                },
                y: {
                    title: { display: true, text: 'Nº de Viagens', font: { size: 11 } },
                    beginAtZero: true,
                    ticks: { stepSize: 1, precision: 0 },
                    grid: { color: '#f0f0f0' }
                }
            }
        }
    });
};

window.destroyDashboardChart = function (canvasId) {
    if (_charts[canvasId]) {
        _charts[canvasId].destroy();
        delete _charts[canvasId];
    }
};
