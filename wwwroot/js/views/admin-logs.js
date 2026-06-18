document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('logs-page');
    if (!page) return; // only run on the Logs page

    var tbody = document.getElementById('logs-tbody');
    var filters = document.getElementById('logs-filters');

    var categoryLabels = {
        auth: 'Authentication',
        device: 'Device',
        app: 'Application'
    };

    function loadLogs(category) {
        tbody.innerHTML = '<tr><td colspan="6" class="logs__empty">Loading…</td></tr>';

        var url = '/api/logs' + (category ? '?category=' + encodeURIComponent(category) : '');
        fetch(url)
            .then(function (res) { return res.json(); })
            .then(function (logs) { renderLogs(logs); })
            .catch(function () {
                tbody.innerHTML = '<tr><td colspan="6" class="logs__empty">Failed to load logs.</td></tr>';
                window.showToast('Failed to load logs.', 'error');
            });
    }

    function renderLogs(logs) {
        if (!logs.length) {
            tbody.innerHTML = '<tr><td colspan="6" class="logs__empty">No logs found.</td></tr>';
            return;
        }

        tbody.innerHTML = logs.map(function (log) {
            var categoryLabel = categoryLabels[log.category] || log.category;
            return '<tr>'
                + '<td class="logs__time">' + formatDate(log.createdAt) + '</td>'
                + '<td><span class="log-badge log-badge--' + log.category + '">' + categoryLabel + '</span></td>'
                + '<td class="logs__action">' + escapeHtml(log.actionType) + '</td>'
                + '<td>' + escapeHtml(log.details) + '</td>'
                + '<td>' + escapeHtml(log.username) + '</td>'
                + '<td>' + (log.deviceName ? escapeHtml(log.deviceName) : '<span class="logs__muted">—</span>') + '</td>'
                + '</tr>';
        }).join('');
    }

    function formatDate(iso) {
        var d = new Date(iso);
        if (isNaN(d.getTime())) return iso;
        return d.toLocaleString();
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    // ===== Filter tabs =====
    filters.querySelectorAll('.logs__tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            filters.querySelectorAll('.logs__tab').forEach(function (t) { t.classList.remove('logs__tab--active'); });
            tab.classList.add('logs__tab--active');
            loadLogs(tab.dataset.category);
        });
    });

    // ===== Init =====
    loadLogs('');
});
