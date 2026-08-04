// ===== Global Toast Notifications =====
window.showToast = function (message, type) {
    type = type || 'info';
    var container = document.getElementById('toast-container');
    if (!container) return;

    var icons = {
        success: '<path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><path d="M22 4L12 14.01l-3-3"/>',
        error: '<circle cx="12" cy="12" r="10"/><path d="M15 9l-6 6M9 9l6 6"/>',
        info: '<circle cx="12" cy="12" r="10"/><path d="M12 16v-4M12 8h.01"/>'
    };

    var toast = document.createElement('div');
    toast.className = 'toast toast--' + type;
    toast.setAttribute('role', 'alert');
    toast.innerHTML =
        '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">'
        + (icons[type] || icons.info) + '</svg>'
        + '<span class="toast__message"></span>';
    toast.querySelector('.toast__message').textContent = message;

    container.appendChild(toast);

    requestAnimationFrame(function () { toast.classList.add('toast--visible'); });

    setTimeout(function () {
        toast.classList.remove('toast--visible');
        toast.addEventListener('transitionend', function () { toast.remove(); }, { once: true });
    }, 4000);
};

// ===== Reusable loading spinner markup =====
// Returns an animated SVG spinner with an optional label. Avoids text ellipsis characters
// that can be corrupted by bundling, and gives a consistent loading look across the app.
window.uiSpinner = function (label) {
    return '<div class="ui-state ui-state--loading" role="status" aria-live="polite">'
        + '<svg class="ui-spinner" width="34" height="34" viewBox="0 0 50 50" aria-hidden="true">'
        + '<circle class="ui-spinner__track" cx="25" cy="25" r="20" fill="none" stroke-width="5"/>'
        + '<circle class="ui-spinner__head" cx="25" cy="25" r="20" fill="none" stroke-width="5"/>'
        + '</svg>'
        + (label ? '<span class="ui-state__text">' + label + '</span>' : '')
        + '</div>';
};

// ===== Reusable empty / not-found state markup =====
// kind: 'empty' (default) or 'error'.
window.uiEmptyState = function (message, kind) {
    var icons = {
        empty: '<circle cx="11" cy="11" r="8"/><path d="M21 21l-4.35-4.35"/><path d="M8 11h6"/>',
        error: '<path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>'
    };
    var icon = icons[kind] || icons.empty;
    return '<div class="ui-state ui-state--' + (kind === 'error' ? 'error' : 'empty') + '">'
        + '<svg class="ui-state__icon" width="44" height="44" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">'
        + icon + '</svg>'
        + '<span class="ui-state__text">' + message + '</span>'
        + '</div>';
};

document.addEventListener('DOMContentLoaded', function () {
var sidebar = document.getElementById('location-sidebar');
var toggleBtn = document.getElementById('btn-toggle-left');
var searchInput = document.getElementById('location-search');
var locationList = document.getElementById('location-list');

    var allLocations = [];
    var selectedLocationId = null;
    var collapsedFloors = {};

    function groupByFloor(locations) {
        var groups = {};
        locations.forEach(function (l) {
            var floor = l.locationFloor || 'Unassigned';
            if (!groups[floor]) groups[floor] = [];
            groups[floor].push(l);
        });
        return groups;
    }

    function renderLocations(filter) {
        if (!locationList) return;

        var filtered = filter
            ? allLocations.filter(function (l) { return l.locationName.toLowerCase().indexOf(filter.toLowerCase()) !== -1; })
            : allLocations;

        if (filtered.length === 0) {
            locationList.innerHTML = '<p class="sidebar__empty">No locations found</p>';
            return;
        }

        var groups = groupByFloor(filtered);
        var floors = Object.keys(groups).sort();

        locationList.innerHTML = floors.map(function (floor) {
            var items = groups[floor];
            // When searching, force-expand so matches are visible.
            var isCollapsed = !filter && collapsedFloors[floor];

            var itemsHtml = items.map(function (l) {
                var isActive = l.id === selectedLocationId ? ' active' : '';
                return '<div class="sidebar__location" data-id="' + l.id + '">'
                    + '<div class="sidebar__nav-item' + isActive + '" data-id="' + l.id + '">'
                    + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">'
                    + '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>'
                    + '<div class="location-info">'
                    + '<div class="location-name">' + escapeHtml(l.locationName) + '</div>'
                    + '<div class="location-floor">' + (l.totalDevices || 0) + ' device' + (l.totalDevices === 1 ? '' : 's') + '</div>'
                    + '</div>'
                    + '<span class="status-dot ' + l.status + '"></span>'
                    + '</div>'
                    + '<div class="sidebar__accordion"><div class="sidebar__accordion-inner"></div></div>'
                    + '</div>';
            }).join('');

            return '<div class="sidebar__floor' + (isCollapsed ? ' collapsed' : '') + '" data-floor="' + escapeHtml(floor) + '">'
                + '<button type="button" class="sidebar__floor-header">'
                + '<svg class="sidebar__floor-chevron" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18l6-6-6-6"/></svg>'
                + '<span class="sidebar__floor-name">' + escapeHtml(floor) + '</span>'
                + '<span class="sidebar__floor-count">' + items.length + '</span>'
                + '</button>'
                + '<div class="sidebar__floor-items">' + itemsHtml + '</div>'
                + '</div>';
        }).join('');

        bindFloorHeaders();
        bindLocationItems();
        bindLocationHover();
    }

    function bindFloorHeaders() {
        locationList.querySelectorAll('.sidebar__floor-header').forEach(function (header) {
            header.addEventListener('click', function () {
                var floorEl = header.closest('.sidebar__floor');
                var floor = floorEl.dataset.floor;
                floorEl.classList.toggle('collapsed');
                collapsedFloors[floor] = floorEl.classList.contains('collapsed');
            });
        });
    }

    function bindLocationItems() {
        locationList.querySelectorAll('.sidebar__nav-item').forEach(function (item) {
            item.addEventListener('click', function () {
                locationList.querySelectorAll('.sidebar__nav-item').forEach(function (i) { i.classList.remove('active'); });
                item.classList.add('active');

                selectedLocationId = parseInt(item.dataset.id);
                var location = allLocations.find(function (l) { return l.id === selectedLocationId; });
                if (location) {
                    document.dispatchEvent(new CustomEvent('locationSelected', { detail: location }));
                }
            });
        });
    }

    // Hovering a location expands an inline accordion that previews its devices.
    // The device list is fetched lazily on first hover and cached on the element.
    function bindLocationHover() {
        locationList.querySelectorAll('.sidebar__location').forEach(function (loc) {
            loc.addEventListener('mouseenter', function () { openAccordion(loc); });
            loc.addEventListener('mouseleave', function () { closeAccordion(loc); });
        });
    }

    function openAccordion(loc) {
        var panel = loc.querySelector('.sidebar__accordion');
        var inner = loc.querySelector('.sidebar__accordion-inner');
        if (!panel || !inner) return;

        panel.classList.add('expanded');
        if (loc.dataset.loaded === '1') return;
        loc.dataset.loaded = '1';

        inner.innerHTML = '<div class="sidebar__accordion-empty">Loading</div>';
        fetch('/api/devices/by-location/' + parseInt(loc.dataset.id))
            .then(function (res) { return res.json(); })
            .then(function (devices) { inner.innerHTML = renderAccordionDevices(devices); })
            .catch(function () {
                inner.innerHTML = '<div class="sidebar__accordion-empty">Failed to load</div>';
                loc.dataset.loaded = '';
            });
    }

    function closeAccordion(loc) {
        var panel = loc.querySelector('.sidebar__accordion');
        if (panel) panel.classList.remove('expanded');
    }

    function renderAccordionDevices(devices) {
        if (!devices || devices.length === 0) {
            return '<div class="sidebar__accordion-empty">No devices</div>';
        }
        return devices.map(function (d) {
            var st = d.status === 'Online' ? 'online' : (d.status === 'Offline' ? 'offline' : 'warning');
            return '<div class="sidebar__accordion-item">'
                + '<span class="status-dot ' + st + '"></span>'
                + '<span class="sidebar__accordion-name">' + escapeHtml(d.deviceName || d.ipAddress) + '</span>'
                + '<span class="sidebar__accordion-ip">' + escapeHtml(d.ipAddress) + '</span>'
                + '</div>';
        }).join('');
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    function loadLocations() {
        fetch('/api/locations')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                allLocations = data;
                renderLocations(searchInput ? searchInput.value : '');
            })
            .catch(function () {
                if (locationList) locationList.innerHTML = '<p class="sidebar__empty">Failed to load locations</p>';
            });
    }

    loadLocations();

    // Refresh the sidebar in real time when locations change elsewhere in the app.
    document.addEventListener('locationsChanged', loadLocations);

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            renderLocations(this.value);
        });
    }
});
