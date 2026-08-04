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

    function updateTitle(location) {
        if (topbarTitle) topbarTitle.textContent = location.locationName + ' - ' + location.locationFloor;
    }

    // The three topbar badges are a live, network-wide summary (all locations combined),
    // not the counts for the single location currently being viewed.
    function renderStats(devices) {
        var online = devices.filter(function (d) { return d.status === 'Online'; }).length;
        var offline = devices.filter(function (d) { return d.status === 'Offline'; }).length;

        if (equipmentCount) equipmentCount.textContent = devices.length + ' device' + (devices.length !== 1 ? 's' : '');
        if (onlineCount) onlineCount.textContent = online + ' online';
        if (offlineCount) offlineCount.textContent = offline + ' offline';
    }

    function loadNetworkStats() {
        fetch('/api/devices')
            .then(function (res) { return res.json(); })
            .then(function (devices) { renderStats(Array.isArray(devices) ? devices : []); })
            .catch(function () { /* keep the last known counts on a transient failure */ });
    }

    function deviceHeader(location) {
        return '<div class="devices__header">'
            + '<button type="button" class="devices__back" id="btn-back-home" title="Back to home" aria-label="Back to home">'
            + '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 12H5"/><path d="M12 19l-7-7 7-7"/></svg>'
            + '</button>'
            + '<div class="devices__heading">'
            + '<h3 class="devices__title">' + escapeHtml(location.locationName) + '</h3>'
            + '<span class="devices__subtitle">' + escapeHtml(location.locationFloor || '') + '</span>'
            + '</div>'
            + '</div>';
    }

    function renderDevices(location, devices) {
        updateTitle(location);

        if (!mainContent) return;

        if (devices.length === 0) {
            mainContent.innerHTML = '<section class="devices">' + deviceHeader(location) + window.uiEmptyState('No devices registered for this location.') + '</section>';
            bindDevicesEvents();
            return;
        }

        mainContent.innerHTML = '<section class="devices">'
            + deviceHeader(location)
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

        bindDevicesEvents();
    }

    function bindDevicesEvents() {
        var backBtn = document.getElementById('btn-back-home');
        if (backBtn) backBtn.addEventListener('click', showWelcome);

        if (!mainContent) return;
        mainContent.querySelectorAll('.device-card__logs').forEach(function (btn) {
            btn.addEventListener('click', function () {
                window.location.href = '/Admin/Logs?device=' + encodeURIComponent(btn.dataset.device);
            });
        });
    }

    function showWelcome() {
        // Navigate to the Home route so the welcome screen and sidebar reset cleanly.
        window.location.href = '/';
    }

    function loadDevices(location) {
        if (!mainContent) return;
        mainContent.innerHTML = '<section class="devices">' + window.uiSpinner('Loading devices') + '</section>';

        fetch('/api/devices/by-location/' + location.id)
            .then(function (res) { return res.json(); })
            .then(function (devices) { renderDevices(location, devices); })
            .catch(function () {
                mainContent.innerHTML = '<section class="devices">' + window.uiEmptyState('Failed to load devices.', 'error') + '</section>';
            });
    }

    document.addEventListener('locationSelected', function (e) {
        loadDevices(e.detail);
    });

    loadNetworkStats();
    setInterval(loadNetworkStats, 30000);
});
