document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('reports-page');
    if (!page) return; // only run on the Reports page

    var summary = document.getElementById('reports-summary');
    var statusTbody = document.getElementById('reports-status-tbody');
    var recentCount = document.getElementById('reports-recent-count');
    var generated = document.getElementById('reports-generated');
    var refreshBtn = document.getElementById('reports-refresh');

    var bars = {
        type: document.getElementById('reports-by-type'),
        location: document.getElementById('reports-by-location'),
        vlan: document.getElementById('reports-by-vlan'),
        category: document.getElementById('reports-by-category'),
        actions: document.getElementById('reports-top-actions')
    };

    function load() {
        summary.innerHTML = window.uiSpinner('Loading reports');
        statusTbody.innerHTML = '<tr><td colspan="3">' + window.uiSpinner('Loading') + '</td></tr>';

        fetch('/api/reports/overview')
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (data) {
                if (!data) throw new Error('bad response');
                render(data);
            })
            .catch(function () {
                summary.innerHTML = window.uiEmptyState('Failed to load reports.', 'error');
                statusTbody.innerHTML = '<tr><td colspan="3">' + window.uiEmptyState('Failed to load data.', 'error') + '</td></tr>';
                window.showToast('Failed to load reports.', 'error');
            });
    }

    function render(data) {
        renderSummary(data.deviceSummary);
        renderBars(bars.type, data.devicesByType);
        renderBars(bars.location, data.devicesByLocation);
        renderBars(bars.vlan, data.devicesByVlan);
        renderBars(bars.category, data.activityByCategory);
        renderBars(bars.actions, data.topActions);
        renderStatusTable(data.recentStatusChanges);

        recentCount.textContent = data.recentStatusChangeCount + ' in the last 7 days';
        generated.textContent = 'Generated ' + formatDate(data.generatedAtUtc);
    }

    function renderSummary(s) {
        if (!s) { summary.innerHTML = ''; return; }
        var cards = [
            { label: 'Total devices', value: s.total, kind: '' },
            { label: 'Online', value: s.online, kind: 'is-online' },
            { label: 'Offline', value: s.offline, kind: 'is-offline' },
            { label: 'Unregistered', value: s.unregistered, kind: 'is-warn' },
            { label: 'Availability', value: s.availabilityPercent + '%', kind: 'is-accent' }
        ];
        summary.innerHTML = cards.map(function (c) {
            return '<div class="reports__stat ' + c.kind + '">'
                + '<span class="reports__stat-value">' + escapeHtml(String(c.value)) + '</span>'
                + '<span class="reports__stat-label">' + escapeHtml(c.label) + '</span>'
                + '</div>';
        }).join('');
    }

    function renderBars(container, items) {
        if (!container) return;
        if (!items || !items.length) {
            container.innerHTML = window.uiEmptyState('No data available.');
            return;
        }
        var max = items.reduce(function (m, i) { return i.count > m ? i.count : m; }, 0) || 1;
        container.innerHTML = items.map(function (i) {
            var pct = Math.round(i.count * 100 / max);
            return '<div class="reports__bar-row">'
                + '<span class="reports__bar-label" title="' + escapeHtml(i.label) + '">' + escapeHtml(i.label) + '</span>'
                + '<span class="reports__bar-track"><span class="reports__bar-fill" style="width:' + pct + '%"></span></span>'
                + '<span class="reports__bar-count">' + i.count + '</span>'
                + '</div>';
        }).join('');
    }

    function renderStatusTable(rows) {
        if (!rows || !rows.length) {
            statusTbody.innerHTML = '<tr><td colspan="3">' + window.uiEmptyState('No recent status changes.') + '</td></tr>';
            return;
        }
        statusTbody.innerHTML = rows.map(function (r) {
            return '<tr>'
                + '<td class="reports__time">' + formatDate(r.createdAt) + '</td>'
                + '<td>' + (r.deviceName ? escapeHtml(r.deviceName) : '<span class="reports__muted">N/A</span>') + '</td>'
                + '<td>' + escapeHtml(r.details) + '</td>'
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

    refreshBtn.addEventListener('click', load);
    load();
});
