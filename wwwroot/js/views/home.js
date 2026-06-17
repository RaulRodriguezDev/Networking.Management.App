document.addEventListener('DOMContentLoaded', function () {
    var topbarTitle = document.getElementById('topbar-title');
    var equipmentCount = document.getElementById('equipment-count');
    var onlineCount = document.getElementById('online-count');
    var offlineCount = document.getElementById('offline-count');
    var mainContent = document.querySelector('.main__content');

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
                return '<div class="device-card">'
                    + '<div class="device-card__header">'
                    + '<span class="device-card__type">' + d.deviceType + '</span>'
                    + '<span class="device-card__status ' + statusClass + '">' + d.status + '</span>'
                    + '</div>'
                    + '<div class="device-card__name">' + (d.deviceName || 'Unnamed device') + '</div>'
                    + '<div class="device-card__ip">' + d.ipAddress + '</div>'
                    + '</div>';
            }).join('')
            + '</div>'
            + '</section>';
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
