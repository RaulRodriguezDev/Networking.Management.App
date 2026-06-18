document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('logs-page');
    if (!page) return; // only run on the Logs page

    var tbody = document.getElementById('logs-tbody');
    var filters = document.getElementById('logs-filters');

    var filterInputs = {
        user: document.getElementById('filter-user'),
        device: document.getElementById('filter-device'),
        action: document.getElementById('filter-action'),
        from: document.getElementById('filter-from'),
        to: document.getElementById('filter-to')
    };
    var clearBtn = document.getElementById('filter-clear');

    var categoryLabels = {
        auth: 'Authentication',
        device: 'Device',
        app: 'Application'
    };

    var allLogs = [];          // logs for the current category, unfiltered
    var currentCategory = '';

    var paginator = window.createPaginator({
        container: document.getElementById('logs-pagination'),
        pageSize: 15,
        onRender: renderLogs
    });

    function loadLogs(category) {
        currentCategory = category || '';
        tbody.innerHTML = '<tr><td colspan="6" class="logs__empty">Loading…</td></tr>';

        var url = '/api/logs' + (category ? '?category=' + encodeURIComponent(category) : '');
        fetch(url)
            .then(function (res) { return res.json(); })
            .then(function (logs) {
                allLogs = logs;
                applyFilters();
            })
            .catch(function () {
                tbody.innerHTML = '<tr><td colspan="6" class="logs__empty">Failed to load logs.</td></tr>';
                window.showToast('Failed to load logs.', 'error');
            });
    }

    function applyFilters() {
        var userTerm = filterInputs.user.value.trim().toLowerCase();
        var deviceTerm = filterInputs.device.value.trim().toLowerCase();
        var actionTerm = filterInputs.action.value.trim().toLowerCase();
        var fromDate = filterInputs.from.value ? new Date(filterInputs.from.value + 'T00:00:00') : null;
        var toDate = filterInputs.to.value ? new Date(filterInputs.to.value + 'T23:59:59') : null;

        var filtered = allLogs.filter(function (log) {
            if (userTerm && (log.username || '').toLowerCase().indexOf(userTerm) === -1) return false;
            if (deviceTerm && (log.deviceName || '').toLowerCase().indexOf(deviceTerm) === -1) return false;
            if (actionTerm && (log.actionType || '').toLowerCase().indexOf(actionTerm) === -1) return false;

            if (fromDate || toDate) {
                var created = new Date(log.createdAt);
                if (fromDate && created < fromDate) return false;
                if (toDate && created > toDate) return false;
            }
            return true;
        });

        paginator.setItems(filtered);
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

    // ===== Category tabs =====
    filters.querySelectorAll('.logs__tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            filters.querySelectorAll('.logs__tab').forEach(function (t) { t.classList.remove('logs__tab--active'); });
            tab.classList.add('logs__tab--active');
            loadLogs(tab.dataset.category);
        });
    });

    // ===== Filter inputs =====
    Object.keys(filterInputs).forEach(function (key) {
        filterInputs[key].addEventListener('input', applyFilters);
    });

    clearBtn.addEventListener('click', function () {
        Object.keys(filterInputs).forEach(function (key) { filterInputs[key].value = ''; });
        applyFilters();
    });

    // ===== Init: pre-apply filters passed via URL (redirects from Users/Devices) =====
    function applyUrlFilters() {
        var params = new URLSearchParams(window.location.search);
        var username = params.get('username');
        var device = params.get('device');
        if (username) filterInputs.user.value = username;
        if (device) filterInputs.device.value = device;
    }

    applyUrlFilters();
    loadLogs('');
});
