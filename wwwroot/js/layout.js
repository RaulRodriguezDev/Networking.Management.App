document.addEventListener('DOMContentLoaded', function () {
    var sidebar = document.getElementById('location-sidebar');
    var toggleBtn = document.getElementById('btn-toggle-left');
    var searchInput = document.getElementById('location-search');
    var locationList = document.getElementById('location-list');

    // Sample hospital locations (replace with API calls later)
    var locations = [
        { id: 1, name: 'Emergency Room',   floor: 'Ground Floor', status: 'online' },
        { id: 2, name: 'Radiology',        floor: '1st Floor',    status: 'online' },
        { id: 3, name: 'ICU',              floor: '2nd Floor',    status: 'warning' },
        { id: 4, name: 'Operating Rooms',  floor: '2nd Floor',    status: 'online' },
        { id: 5, name: 'Pharmacy',         floor: 'Ground Floor', status: 'online' },
        { id: 6, name: 'Laboratory',       floor: '1st Floor',    status: 'offline' },
        { id: 7, name: 'Administration',   floor: '3rd Floor',    status: 'online' },
        { id: 8, name: 'Pediatrics',       floor: '3rd Floor',    status: 'online' }
    ];

    function renderLocations(filter) {
        if (!locationList) return;

        var filtered = filter
            ? locations.filter(function (l) { return l.name.toLowerCase().indexOf(filter.toLowerCase()) !== -1; })
            : locations;

        locationList.innerHTML = filtered.map(function (l) {
            return '<div class="sidebar__nav-item" data-id="' + l.id + '">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">'
                + '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>'
                + '<div class="location-info">'
                + '<div class="location-name">' + l.name + '</div>'
                + '<div class="location-floor">' + l.floor + '</div>'
                + '</div>'
                + '<span class="status-dot ' + l.status + '"></span>'
                + '</div>';
        }).join('');
    }

    renderLocations();

    // Toggle sidebar
    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
        });
    }

    // Filter locations
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            renderLocations(this.value);
        });
    }
});
