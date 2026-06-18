document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('locations-page');
    if (!page) return; // only run on the Locations management page

    var tbody = document.getElementById('locations-tbody');
    var locationModal = document.getElementById('location-modal');
    var deleteModal = document.getElementById('delete-modal');

    var form = {
        id: document.getElementById('location-id'),
        name: document.getElementById('location-name'),
        floor: document.getElementById('location-floor')
    };

    var modalTitle = document.getElementById('location-modal-title');
    var deleteNameLabel = document.getElementById('delete-location-name');

    var pendingDeleteId = null;

    var statusLabels = {
        online: 'Online',
        offline: 'Offline',
        unknown: 'No devices'
    };

    // ===== Modal helpers =====
    function openModal(modal) { modal.classList.add('modal--open'); modal.setAttribute('aria-hidden', 'false'); }
    function closeModal(modal) { modal.classList.remove('modal--open'); modal.setAttribute('aria-hidden', 'true'); }

    document.querySelectorAll('[data-close-modal]').forEach(function (el) {
        el.addEventListener('click', function () {
            closeModal(locationModal);
            closeModal(deleteModal);
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeModal(locationModal);
            closeModal(deleteModal);
        }
    });

    // ===== Load data =====
    function loadLocations() {
        tbody.innerHTML = '<tr><td colspan="5" class="locations__empty">Loading…</td></tr>';
        fetch('/api/locations')
            .then(function (res) { return res.json(); })
            .then(function (locations) { renderLocations(locations); })
            .catch(function () {
                tbody.innerHTML = '<tr><td colspan="5" class="locations__empty">Failed to load locations.</td></tr>';
                window.showToast('Failed to load locations.', 'error');
            });
    }

    function renderLocations(locations) {
        if (!locations.length) {
            tbody.innerHTML = '<tr><td colspan="5" class="locations__empty">No locations found.</td></tr>';
            return;
        }

        tbody.innerHTML = locations.map(function (l) {
            var statusLabel = statusLabels[l.status] || l.status;
            return '<tr>'
                + '<td>' + escapeHtml(l.locationName) + '</td>'
                + '<td>' + escapeHtml(l.locationFloor) + '</td>'
                + '<td>' + l.totalDevices + ' (' + l.onlineDevices + ' online, ' + l.offlineDevices + ' offline)</td>'
                + '<td><span class="status-badge status-badge--' + l.status + '">' + statusLabel + '</span></td>'
                + '<td class="locations__row-actions">'
                + '<button class="icon-btn" data-edit="' + l.id + '" title="Edit">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>'
                + '</button>'
                + '<button class="icon-btn icon-btn--danger" data-delete="' + l.id + '" data-name="' + escapeHtml(l.locationName) + '" title="Delete">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>'
                + '</button>'
                + '</td>'
                + '</tr>';
        }).join('');

        tbody.querySelectorAll('[data-edit]').forEach(function (btn) {
            btn.addEventListener('click', function () { openEditModal(parseInt(btn.dataset.edit)); });
        });
        tbody.querySelectorAll('[data-delete]').forEach(function (btn) {
            btn.addEventListener('click', function () { openDeleteModal(parseInt(btn.dataset.delete), btn.dataset.name); });
        });
    }

    // ===== Add =====
    document.getElementById('btn-add-location').addEventListener('click', function () {
        form.id.value = '';
        form.name.value = '';
        form.floor.value = '';
        modalTitle.textContent = 'Add Location';
        openModal(locationModal);
    });

    // ===== Edit =====
    function openEditModal(locationId) {
        fetch('/api/locations/' + locationId)
            .then(function (res) { return res.json(); })
            .then(function (l) {
                form.id.value = l.id;
                form.name.value = l.locationName;
                form.floor.value = l.locationFloor;
                modalTitle.textContent = 'Edit Location';
                openModal(locationModal);
            })
            .catch(function () { window.showToast('Failed to load location.', 'error'); });
    }

    // ===== Save (create or update) =====
    document.getElementById('btn-save-location').addEventListener('click', function () {
        if (!validateForm()) return;

        var id = form.id.value;
        var isEdit = !!id;

        var payload = {
            locationName: form.name.value.trim(),
            locationFloor: form.floor.value.trim()
        };

        var url = isEdit ? '/api/locations/' + id : '/api/locations';
        var method = isEdit ? 'PUT' : 'POST';

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (res.status === 409) {
                return res.json().then(function (body) { throw new Error(body.message || 'Location already exists.'); });
            }
            if (!res.ok) throw new Error('Request failed.');
            return res.status === 204 ? null : res.json();
        }).then(function () {
            closeModal(locationModal);
            window.showToast(isEdit ? 'Location updated successfully.' : 'Location created successfully.', 'success');
            loadLocations();
            document.dispatchEvent(new CustomEvent('locationsChanged'));
        }).catch(function (err) {
            window.showToast(err.message || 'Operation failed.', 'error');
        });
    });

    function validateForm() {
        if (!form.name.value.trim()) { window.showToast('Location name is required.', 'error'); return false; }
        if (!form.floor.value.trim()) { window.showToast('Floor is required.', 'error'); return false; }
        return true;
    }

    // ===== Delete =====
    function openDeleteModal(locationId, name) {
        pendingDeleteId = locationId;
        deleteNameLabel.textContent = name;
        openModal(deleteModal);
    }

    document.getElementById('btn-confirm-delete').addEventListener('click', function () {
        if (!pendingDeleteId) return;

        fetch('/api/locations/' + pendingDeleteId, { method: 'DELETE' })
            .then(function (res) {
                if (res.status === 409) {
                    return res.json().then(function (body) { throw new Error(body.message || 'Cannot delete location.'); });
                }
                if (!res.ok) throw new Error('Delete failed.');
                closeModal(deleteModal);
                window.showToast('Location deleted successfully.', 'success');
                loadLocations();
                document.dispatchEvent(new CustomEvent('locationsChanged'));
            })
            .catch(function (err) { window.showToast(err.message || 'Failed to delete location.', 'error'); })
            .finally(function () { pendingDeleteId = null; });
    });

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    // ===== Init =====
    loadLocations();
});
