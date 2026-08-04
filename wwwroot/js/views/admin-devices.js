document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('devices-page');
    if (!page) return; // only run on the Devices management page

    var grid = document.getElementById('devices-grid');
    var deviceModal = document.getElementById('device-modal');
    var deviceTypeModal = document.getElementById('device-type-modal');
    var deleteModal = document.getElementById('delete-modal');
    var ipRangeModal = document.getElementById('ip-range-modal');
    var discoverModal = document.getElementById('discover-modal');

    var deviceModalTitle = document.getElementById('device-modal-title');
    var deleteDeviceLabel = document.getElementById('delete-device-name');

    var isAdmin = page.dataset.isAdmin === 'true';

    var deviceForm = {
        id: document.getElementById('device-id'),
        name: document.getElementById('device-name'),
        ip: document.getElementById('device-ip'),
        mac: document.getElementById('device-mac'),
        addressing: document.getElementById('device-addressing'),
        type: document.getElementById('device-type'),
        location: document.getElementById('device-location')
    };

    var ipHint = document.getElementById('device-ip-hint');
    var ipPattern = /^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/;

    var typeForm = {
        name: document.getElementById('device-type-name'),
        description: document.getElementById('device-type-description')
    };

    var pendingDeleteId = null;

    var statusClasses = {
        Online: 'online',
        Offline: 'offline'
    };

    var paginator = window.createPaginator({
        container: document.getElementById('devices-pagination'),
        pageSize: 8,
        onRender: renderDevices
    });

    // ===== Modal helpers =====
    function openModal(modal) { modal.classList.add('modal--open'); modal.setAttribute('aria-hidden', 'false'); }
    function closeModal(modal) { modal.classList.remove('modal--open'); modal.setAttribute('aria-hidden', 'true'); }

    document.querySelectorAll('[data-close-modal]').forEach(function (el) {
        el.addEventListener('click', function () {
            closeModal(deviceModal);
            closeModal(deviceTypeModal);
            closeModal(deleteModal);
            closeModal(ipRangeModal);
            closeModal(discoverModal);
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeModal(deviceModal);
            closeModal(deviceTypeModal);
            closeModal(deleteModal);
            closeModal(ipRangeModal);
            closeModal(discoverModal);
        }
    });

    // ===== Load devices =====
    function loadDevices() {
        grid.innerHTML = window.uiSpinner('Loading devices');
        fetch('/api/devices')
            .then(function (res) { return res.json(); })
            .then(function (devices) { paginator.setItems(devices); })
            .catch(function () {
                grid.innerHTML = window.uiEmptyState('Failed to load devices.', 'error');
                window.showToast('Failed to load devices.', 'error');
            });
    }

    // Quietly refresh the list (no spinner, keeps the current page) so the background
    // monitor's status changes appear without disrupting the view.
    function refreshDevicesQuietly() {
        if (document.hidden) return;
        fetch('/api/devices')
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (devices) { if (devices) paginator.setItems(devices, true); })
            .catch(function () { /* ignore transient errors */ });
    }

    function renderDevices(devices) {
        if (!devices.length) {
            grid.innerHTML = window.uiEmptyState('No devices yet. Use �Add Device� to register one.');
            return;
        }

        grid.innerHTML = devices.map(function (d) {
            var statusClass = statusClasses[d.status] || 'unknown';
            return '<article class="device-tile" data-open="' + d.deviceId + '" role="button" tabindex="0" title="Open device">'
                + '<div class="device-tile__top">'
                + '<span class="device-tile__type">' + escapeHtml(d.deviceType) + '</span>'
                + '<span class="status-badge status-badge--' + statusClass + '">' + escapeHtml(d.status) + '</span>'
                + '</div>'
                + '<h3 class="device-tile__name">' + escapeHtml(d.deviceName || 'Unnamed device') + '</h3>'
                + '<div class="device-tile__ip">'
                + '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/><line x1="6" y1="6" x2="6.01" y2="6"/><line x1="6" y1="18" x2="6.01" y2="18"/></svg>'
                + escapeHtml(d.ipAddress)
                + '</div>'
                + '<div class="device-tile__location">'
                + '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>'
                + escapeHtml(d.deviceLocation)
                + '</div>'
                + '<div class="device-tile__actions">'
                + (d.status === 'Online' ? '' :
                    '<button class="icon-btn icon-btn--connect" data-connect="' + d.deviceId + '" data-name="' + escapeHtml(d.deviceName || d.ipAddress) + '" title="Connect to network">'
                    + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12.55a11 11 0 0114.08 0"/><path d="M1.42 9a16 16 0 0121.16 0"/><path d="M8.53 16.11a6 6 0 016.95 0"/><line x1="12" y1="20" x2="12.01" y2="20"/></svg>'
                    + '</button>')
                + '<button class="icon-btn icon-btn--edit" data-edit="' + d.deviceId + '" title="Edit">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>'
                + '</button>'
                + '<button class="icon-btn icon-btn--danger" data-delete="' + d.deviceId + '" data-name="' + escapeHtml(d.deviceName || d.ipAddress) + '" title="Delete">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>'
                + '</button>'
                + '</div>'
                + '</article>';
        }).join('');

        grid.querySelectorAll('[data-edit]').forEach(function (btn) {
            btn.addEventListener('click', function (e) { e.stopPropagation(); openEditModal(parseInt(btn.dataset.edit)); });
        });
        grid.querySelectorAll('[data-delete]').forEach(function (btn) {
            btn.addEventListener('click', function (e) { e.stopPropagation(); openDeleteModal(parseInt(btn.dataset.delete), btn.dataset.name); });
        });
        grid.querySelectorAll('[data-connect]').forEach(function (btn) {
            btn.addEventListener('click', function (e) { e.stopPropagation(); connectDevice(parseInt(btn.dataset.connect), btn.dataset.name, btn); });
        });
        grid.querySelectorAll('[data-open]').forEach(function (card) {
            var go = function () { window.location.href = '/Admin/Device/' + card.dataset.open; };
            card.addEventListener('click', go);
            card.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); go(); }
            });
        });
    }

    // ===== Populate dropdowns =====
    function loadDeviceTypes() {
        return fetch('/api/devicetypes')
            .then(function (res) { return res.json(); })
            .then(function (types) {
                deviceForm.type.innerHTML = '<option value="">Select a type�</option>'
                    + types.map(function (t) {
                        return '<option value="' + t.id + '">' + escapeHtml(t.typeName) + '</option>';
                    }).join('');
            });
    }

    function loadLocations() {
        return fetch('/api/locations')
            .then(function (res) { return res.json(); })
            .then(function (locations) {
                deviceForm.location.innerHTML = '<option value="">Select a location</option>'
                    + locations.map(function (l) {
                        return '<option value="' + l.id + '">' + escapeHtml(l.locationName) + '</option>';
                    }).join('');
            });
    }

    // ===== Add device =====
    function openAddDeviceModal(prefillIp, prefillMac, prefillName) {
        deviceForm.id.value = '';
        deviceForm.name.value = prefillName || '';
        deviceForm.ip.value = prefillIp || '';
        deviceForm.mac.value = prefillMac || '';
        // A device found on the network almost certainly leases its IP from the router, so
        // default those to DHCP; manually registered devices default to Static.
        deviceForm.addressing.value = prefillIp ? 'Dhcp' : 'Static';
        deviceForm.type.value = '';
        deviceForm.location.value = '';
        deviceModalTitle.textContent = 'Add Device';
        refreshIpHint();

        return Promise.all([loadDeviceTypes(), loadLocations()])
            .then(function () { openModal(deviceModal); })
            .catch(function () { window.showToast('Failed to load form data.', 'error'); });
    }

    document.getElementById('btn-add-device').addEventListener('click', function () {
        openAddDeviceModal();
    });

    // Show the assignable range (boundaries reserved) in the IP hint.
    function refreshIpHint() {
        ipHint.textContent = 'Must be a unique IPv4 within the assignable range.';
        fetch('/api/networksettings')
            .then(function (res) { return res.json(); })
            .then(function (range) {
                ipHint.textContent = 'Must be a unique IPv4 between ' + range.rangeStart + ' and ' + range.rangeEnd
                    + ' (first and last addresses are reserved).';
            })
            .catch(function () { /* keep the generic hint */ });
    }

    // ===== Suggest an available IP =====
    document.getElementById('btn-suggest-ip').addEventListener('click', function () {
        var btn = this;
        btn.disabled = true;
        fetch('/api/devices/suggest-ip')
            .then(function (res) {
                if (res.status === 409) {
                    return res.json().then(function (body) { throw new Error(body.message || 'No free IP addresses available.'); });
                }
                if (!res.ok) throw new Error('Request failed.');
                return res.json();
            })
            .then(function (data) {
                deviceForm.ip.value = data.ipAddress;
                window.showToast('Suggested an available IP address.', 'success');
            })
            .catch(function (err) { window.showToast(err.message || 'Failed to suggest an IP.', 'error'); })
            .finally(function () { btn.disabled = false; });
    });

    // ===== Edit device =====
    function openEditModal(deviceId) {
        refreshIpHint();
        Promise.all([loadDeviceTypes(), loadLocations()])
            .then(function () { return fetch('/api/devices/' + deviceId); })
            .then(function (res) { return res.json(); })
            .then(function (d) {
                deviceForm.id.value = d.deviceId;
                deviceForm.name.value = d.deviceName || '';
                deviceForm.ip.value = d.ipAddress;
                deviceForm.mac.value = d.macAddress || '';
                deviceForm.addressing.value = d.addressingMode || 'Static';
                deviceForm.type.value = d.deviceTypeId;
                deviceForm.location.value = d.deviceLocationId;
                deviceModalTitle.textContent = 'Edit Device';
                openModal(deviceModal);
            })
            .catch(function () { window.showToast('Failed to load device.', 'error'); });
    }

    document.getElementById('btn-save-device').addEventListener('click', function () {
        if (!validateDeviceForm()) return;

        var id = deviceForm.id.value;
        var isEdit = !!id;

        var payload = {
            deviceName: deviceForm.name.value.trim() || null,
            ipAddress: deviceForm.ip.value.trim(),
            macAddress: deviceForm.mac.value.trim() || null,
            addressingMode: deviceForm.addressing.value,
            deviceTypeId: parseInt(deviceForm.type.value),
            deviceLocationId: parseInt(deviceForm.location.value)
        };

        var url = isEdit ? '/api/devices/' + id : '/api/devices';
        var method = isEdit ? 'PUT' : 'POST';

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (res.status === 409) {
                return res.json().then(function (body) { throw new Error(body.message || 'Device already exists.'); });
            }
            if (res.status === 400) {
                return res.json().then(function (body) { throw new Error(body.message || 'Invalid device data.'); });
            }
            if (!res.ok) throw new Error('Request failed.');
            return res.json();
        }).then(function () {
            closeModal(deviceModal);
            window.showToast(isEdit ? 'Device updated successfully.' : 'Device created successfully.', 'success');
            loadDevices();
        }).catch(function (err) {
            window.showToast(err.message || 'Operation failed.', 'error');
        });
    });

    function validateDeviceForm() {
        var ip = deviceForm.ip.value.trim();
        if (!ip) { window.showToast('IP address is required.', 'error'); return false; }
        if (!isValidIpv4(ip)) { window.showToast('IP address must be a valid IPv4 address (0.0.0.0), without a subnet mask.', 'error'); return false; }
        if (!deviceForm.type.value) { window.showToast('Device type is required.', 'error'); return false; }
        if (!deviceForm.location.value) { window.showToast('Location is required.', 'error'); return false; }
        return true;
    }

    function isValidIpv4(ip) {
        if (ip.indexOf('/') !== -1) return false; // reject CIDR / subnet mask
        var m = ipPattern.exec(ip);
        if (!m) return false;
        for (var i = 1; i <= 4; i++) {
            var octet = parseInt(m[i], 10);
            if (octet < 0 || octet > 255) return false;
        }
        return true;
    }

    // ===== Delete device =====
    function openDeleteModal(deviceId, name) {
        pendingDeleteId = deviceId;
        deleteDeviceLabel.textContent = name;
        openModal(deleteModal);
    }

    document.getElementById('btn-confirm-delete').addEventListener('click', function () {
        if (!pendingDeleteId) return;

        fetch('/api/devices/' + pendingDeleteId, { method: 'DELETE' })
            .then(function (res) {
                if (!res.ok) throw new Error('Delete failed.');
                closeModal(deleteModal);
                window.showToast('Device deleted successfully.', 'success');
                loadDevices();
            })
            .catch(function () { window.showToast('Failed to delete device.', 'error'); })
            .finally(function () { pendingDeleteId = null; });
    });

    // ===== Connect device (provisions over SSH in the future) =====
    function connectDevice(deviceId, name, btn) {
        btn.disabled = true;
        btn.classList.add('icon-btn--loading');

        fetch('/api/devices/' + deviceId + '/connect', { method: 'POST' })
            .then(function (res) {
                if (!res.ok) throw new Error('Connect failed.');
                return res.json();
            })
            .then(function () {
                window.showToast('Device �' + name + '� connected to the network.', 'success');
                loadDevices();
            })
            .catch(function () {
                btn.disabled = false;
                btn.classList.remove('icon-btn--loading');
                window.showToast('Failed to connect device.', 'error');
            });
    }

    // ===== Add device type =====
    document.getElementById('btn-add-device-type').addEventListener('click', function () {
        typeForm.name.value = '';
        typeForm.description.value = '';
        openModal(deviceTypeModal);
    });

    document.getElementById('btn-save-device-type').addEventListener('click', function () {
        if (!typeForm.name.value.trim()) { window.showToast('Type name is required.', 'error'); return; }

        var payload = {
            typeName: typeForm.name.value.trim(),
            description: typeForm.description.value.trim() || null
        };

        fetch('/api/devicetypes', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (res.status === 409) {
                return res.json().then(function (body) { throw new Error(body.message || 'Device type already exists.'); });
            }
            if (!res.ok) throw new Error('Request failed.');
            return res.json();
        }).then(function () {
            closeModal(deviceTypeModal);
            window.showToast('Device type created successfully.', 'success');
        }).catch(function (err) {
            window.showToast(err.message || 'Operation failed.', 'error');
        });
    });

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    // ===== IP Range =====
    var rangeForm = {
        start: document.getElementById('range-start'),
        end: document.getElementById('range-end')
    };
    var rangeDisplay = document.getElementById('ip-range-display');
    var rangeFormEl = document.getElementById('ip-range-form');
    var btnEditRange = document.getElementById('btn-edit-range');
    var btnSaveRange = document.getElementById('btn-save-range');

    var currentRange = { rangeStart: '', rangeEnd: '' };

    function setRangeEditing(editing) {
        rangeFormEl.hidden = !editing;
        btnSaveRange.hidden = !editing;
        btnEditRange.hidden = editing;
    }

    document.getElementById('btn-ip-range').addEventListener('click', function () {
        setRangeEditing(false);
        rangeDisplay.textContent = 'Loading...';
        // Only admins can change the range.
        btnEditRange.style.display = isAdmin ? '' : 'none';
        openModal(ipRangeModal);

        fetch('/api/networksettings')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                currentRange = data;
                rangeDisplay.textContent = data.rangeStart + '  -  ' + data.rangeEnd;
                rangeForm.start.value = data.rangeStart;
                rangeForm.end.value = data.rangeEnd;
            })
            .catch(function () {
                rangeDisplay.textContent = 'Failed to load range.';
                window.showToast('Failed to load IP range.', 'error');
            });
    });

    btnEditRange.addEventListener('click', function () {
        setRangeEditing(true);
    });

    btnSaveRange.addEventListener('click', function () {
        var ipPattern = /^(\d{1,3}\.){3}\d{1,3}$/;
        var start = rangeForm.start.value.trim();
        var end = rangeForm.end.value.trim();

        if (!ipPattern.test(start) || !ipPattern.test(end)) {
            window.showToast('Please enter valid IPv4 addresses for both ends of the range.', 'error');
            return;
        }

        btnSaveRange.disabled = true;

        fetch('/api/networksettings', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ rangeStart: start, rangeEnd: end })
        }).then(function (res) {
            if (res.status === 403) throw new Error('Only administrators can change the IP range.');
            if (res.status === 400) {
                return res.json().then(function (body) {
                    var msg = body && body.errors ? 'Invalid IP range.' : (body.message || 'Invalid IP range.');
                    throw new Error(msg);
                });
            }
            if (!res.ok) throw new Error('Request failed.');
            return res.json();
        }).then(function (data) {
            currentRange = data;
            rangeDisplay.textContent = data.rangeStart + '  �  ' + data.rangeEnd;
            setRangeEditing(false);
            window.showToast('Network IP range updated. The network is being reconfigured.', 'success');
        }).catch(function (err) {
            window.showToast(err.message || 'Failed to update IP range.', 'error');
        }).finally(function () {
            btnSaveRange.disabled = false;
        });
    });

    // ===== Discover devices =====
    var discoverResults = document.getElementById('discover-results');
    var discoverRange = document.getElementById('discover-range');
    var btnRunScan = document.getElementById('btn-run-scan');

    document.getElementById('btn-discover').addEventListener('click', function () {
        discoverResults.innerHTML = '<p class="discover__hint">Press &ldquo;Scan network&rdquo; to begin. Larger ranges may take a moment.</p>';
        discoverRange.textContent = 'Range: loading...';
        openModal(discoverModal);

        fetch('/api/networksettings')
            .then(function (res) { return res.json(); })
            .then(function (range) { discoverRange.textContent = 'Range: ' + range.rangeStart + ' \u2013 ' + range.rangeEnd; })
            .catch(function () { discoverRange.textContent = 'Range: unavailable'; });
    });

    btnRunScan.addEventListener('click', function () {
        btnRunScan.disabled = true;
        discoverResults.innerHTML = window.uiSpinner('Scanning the network for live hosts');

        fetch('/api/devices/discover', { method: 'POST' })
            .then(function (res) {
                if (res.status === 400) {
                    return res.json().then(function (body) { throw new Error(body.message || 'The network range is invalid.'); });
                }
                if (res.status === 403) throw new Error('You are not allowed to scan the network.');
                if (!res.ok) throw new Error('Scan failed.');
                return res.json();
            })
            .then(renderDiscovery)
            .catch(function (err) {
                discoverResults.innerHTML = window.uiEmptyState(err.message || 'Network scan failed.', 'error');
                window.showToast(err.message || 'Network scan failed.', 'error');
            })
            .finally(function () { btnRunScan.disabled = false; });
    });

    function renderDiscovery(result) {
        discoverRange.textContent = 'Range: ' + result.rangeStart + ' \u2013 ' + result.rangeEnd;

        var html = '<p class="discover__summary">Scanned ' + result.scannedCount + ' address(es): '
            + '<strong>' + result.discovered.length + '</strong> live host(s), '
            + '<strong>' + result.missing.length + '</strong> registered device(s) not responding.</p>';

        html += '<h4 class="discover__section-title">Live hosts</h4>';
        if (!result.discovered.length) {
            html += '<p class="discover__hint">No hosts responded in the configured range.</p>';
        } else {
            html += '<table class="discover__table"><thead><tr>'
                + '<th>IP Address</th><th>Name</th><th>MAC Address</th><th>Latency</th><th>Status</th><th></th>'
                + '</tr></thead><tbody>';
            html += result.discovered.map(function (h) {
                var latency = (h.latencyMs === null || h.latencyMs === undefined) ? '\u2013' : h.latencyMs + ' ms';
                var name = h.isRegistered ? (h.deviceName || '') : (h.hostName || '');
                var nameCell = name ? escapeHtml(name) : '<span class="discover__muted">Unknown</span>';
                var statusCell = h.isRegistered
                    ? '<span class="discover__tag discover__tag--known">Registered' + (h.deviceName ? ': ' + escapeHtml(h.deviceName) : '') + '</span>'
                    : '<span class="discover__tag discover__tag--new">New</span>';
                var actionCell = h.isRegistered
                    ? '<a class="btn btn--ghost btn--sm" href="/Admin/Device/' + h.deviceId + '">Open</a>'
                    : '<button type="button" class="btn btn--primary btn--sm" data-add-ip="' + escapeHtml(h.ipAddress) + '" data-add-mac="' + escapeHtml(h.macAddress || '') + '" data-add-name="' + escapeHtml(h.hostName || '') + '">Add</button>';
                return '<tr>'
                    + '<td>' + escapeHtml(h.ipAddress) + '</td>'
                    + '<td>' + nameCell + '</td>'
                    + '<td>' + (h.macAddress ? escapeHtml(h.macAddress) : '<span class="discover__muted">Unknown</span>') + '</td>'
                    + '<td>' + latency + '</td>'
                    + '<td>' + statusCell + '</td>'
                    + '<td class="discover__action-cell">' + actionCell + '</td>'
                    + '</tr>';
            }).join('');
            html += '</tbody></table>';
        }

        if (result.missing.length) {
            html += '<h4 class="discover__section-title">Registered but not responding</h4>';
            html += '<table class="discover__table"><thead><tr>'
                + '<th>Device</th><th>IP Address</th><th>Status</th><th></th>'
                + '</tr></thead><tbody>';
            html += result.missing.map(function (d) {
                return '<tr>'
                    + '<td>' + escapeHtml(d.deviceName || 'Unnamed device') + '</td>'
                    + '<td>' + escapeHtml(d.ipAddress) + '</td>'
                    + '<td>' + escapeHtml(d.status) + '</td>'
                    + '<td class="discover__action-cell">'
                    + '<button type="button" class="btn btn--danger btn--sm" data-remove-id="' + d.deviceId + '" data-name="' + escapeHtml(d.deviceName || d.ipAddress) + '">Remove</button>'
                    + '</td>'
                    + '</tr>';
            }).join('');
            html += '</tbody></table>';
        }

        discoverResults.innerHTML = html;

        discoverResults.querySelectorAll('[data-add-ip]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                closeModal(discoverModal);
                openAddDeviceModal(btn.dataset.addIp, btn.dataset.addMac, btn.dataset.addName);
            });
        });
        discoverResults.querySelectorAll('[data-remove-id]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                closeModal(discoverModal);
                openDeleteModal(parseInt(btn.dataset.removeId), btn.dataset.name);
            });
        });
    }

    // ===== Init =====
    loadDevices();
    setInterval(refreshDevicesQuietly, 30000);
});
