document.addEventListener('DOMContentLoaded', function () {
    var topbarTitle = document.getElementById('topbar-title');
    var equipmentCount = document.getElementById('equipment-count');
    var onlineCount = document.getElementById('online-count');
    var offlineCount = document.getElementById('offline-count');
    var mainContent = document.querySelector('.main__content');

    // The admin sidebar section only renders for Admin/IT, who may view the logs page.
    var canViewLogs = !!document.querySelector('.sidebar__admin');

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    function updateTopbar(location, devices) {
        if (topbarTitle) topbarTitle.textContent = location.locationName + ' — ' + location.locationFloor;

        var online = devices.filter(function (d) { return d.status === 'Online'; }).length;
        var offline = devices.filter(function (d) { return d.status === 'Offline'; }).length;

        if (equipmentCount) equipmentCount.textContent = devices.length + ' device' + (devices.length !== 1 ? 's' : '');
        if (onlineCount) onlineCount.textContent = online + ' online';
        if (offlineCount) offlineCount.textContent = offline + ' offline';
    }

    function renderDevices(location, devices) {
        updateTopbar(location, devices);

        if (!mainContent) return;

        if (devices.length === 0) {
            mainContent.innerHTML = '<section class="devices"><p class="devices__empty">No devices registered for this location.</p></section>';
            return;
        }

        mainContent.innerHTML = '<section class="devices">'
            + '<div class="devices__grid">'
            + devices.map(function (d) {
                var statusClass = d.status === 'Online' ? 'online' : 'offline';
                var logsBtn = canViewLogs && d.deviceName
                    ? '<button type="button" class="device-card__logs" data-device="' + escapeHtml(d.deviceName) + '" title="View logs">'
                        + '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6"/><path d="M16 13H8M16 17H8M10 9H8"/></svg>'
                        + '<span>Logs</span>'
                        + '</button>'
                    : '';
                return '<div class="device-card">'
                    + '<div class="device-card__header">'
                    + '<span class="device-card__type">' + escapeHtml(d.deviceType) + '</span>'
                    + '<span class="device-card__status ' + statusClass + '">' + escapeHtml(d.status) + '</span>'
                    + '</div>'
                    + '<div class="device-card__name">' + escapeHtml(d.deviceName || 'Unnamed device') + '</div>'
                    + '<div class="device-card__ip">' + escapeHtml(d.ipAddress) + '</div>'
                    + logsBtn
                    + '</div>';
            }).join('')
            + '</div>'
            + '</section>';

        mainContent.querySelectorAll('.device-card__logs').forEach(function (btn) {
            btn.addEventListener('click', function () {
                window.location.href = '/Admin/Logs?device=' + encodeURIComponent(btn.dataset.device);
            });
        });
    }

    function loadDevices(location) {
        if (!mainContent) return;
        mainContent.innerHTML = '<section class="devices"><p class="devices__empty">Loading…</p></section>';

        fetch('/api/devices/by-location/' + location.id)
            .then(function (res) { return res.json(); })
            .then(function (devices) { renderDevices(location, devices); })
            .catch(function () {
                mainContent.innerHTML = '<section class="devices"><p class="devices__empty">Failed to load devices.</p></section>';
            });
    }

    document.addEventListener('locationSelected', function (e) {
        loadDevices(e.detail);
    });
});
