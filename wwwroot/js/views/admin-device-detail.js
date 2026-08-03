document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('device-detail-page');
    if (!page) return; // only run on the device detail page

    var deviceId = parseInt(page.dataset.deviceId);
    var isAdmin = page.dataset.isAdmin === 'true';

    var titleEl = document.getElementById('device-title');
    var subtitleEl = document.getElementById('device-subtitle');
    var statusBadge = document.getElementById('device-status-badge');
    var body = document.getElementById('device-detail-body');
    var actionsList = document.getElementById('device-actions-list');

    var opModal = document.getElementById('op-modal');
    var opTitle = document.getElementById('op-modal-title');
    var opBody = document.getElementById('op-modal-body');
    var opConfirm = document.getElementById('op-modal-confirm');

    var device = null;
    var vlans = [];
    var locations = [];
    var confirmHandler = null;

    var statusClasses = { Online: 'online', Offline: 'offline' };

    // ===== Helpers =====
    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    function isValidIpv4(ip) {
        if (!ip || ip.indexOf('/') !== -1) return false;
        var m = /^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/.exec(ip);
        if (!m) return false;
        for (var i = 1; i <= 4; i++) { if (parseInt(m[i], 10) > 255) return false; }
        return true;
    }

    function openOp(title, bodyHtml, confirmLabel, onConfirm, confirmClass) {
        opTitle.textContent = title;
        opBody.innerHTML = bodyHtml;
        opConfirm.textContent = confirmLabel || 'Confirm';
        opConfirm.className = 'btn ' + (confirmClass || 'btn--primary');
        opConfirm.hidden = !onConfirm;
        confirmHandler = onConfirm;
        opModal.classList.add('modal--open');
        opModal.setAttribute('aria-hidden', 'false');
        var firstInput = opBody.querySelector('input, select');
        if (firstInput) firstInput.focus();
    }

    function closeOp() {
        opModal.classList.remove('modal--open');
        opModal.setAttribute('aria-hidden', 'true');
        confirmHandler = null;
    }

    opConfirm.addEventListener('click', function () { if (confirmHandler) confirmHandler(); });
    document.querySelectorAll('[data-close-op]').forEach(function (el) {
        el.addEventListener('click', closeOp);
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && opModal.classList.contains('modal--open')) closeOp();
    });

    function api(method, url, payload) {
        var opts = { method: method, headers: {} };
        if (payload !== undefined) {
            opts.headers['Content-Type'] = 'application/json';
            opts.body = JSON.stringify(payload);
        }
        return fetch(url, opts).then(function (res) {
            if (res.status === 204) return null;
            return res.json().catch(function () { return null; }).then(function (bodyJson) {
                if (!res.ok) {
                    var msg = bodyJson && bodyJson.message ? bodyJson.message : 'The operation could not be completed.';
                    throw new Error(msg);
                }
                return bodyJson;
            });
        });
    }

    // ===== Load device =====
    function loadDevice() {
        return fetch('/api/devices/' + deviceId)
            .then(function (res) {
                if (!res.ok) throw new Error('not found');
                return res.json();
            })
            .then(function (d) {
                device = d;
                renderHeader();
                renderOverview();
                renderActions();
            });
    }

    function renderHeader() {
        titleEl.textContent = device.deviceName || 'Unnamed device';
        subtitleEl.textContent = device.deviceType + '  -  ' + device.deviceLocation;
        var cls = statusClasses[device.status] || 'unknown';
        statusBadge.className = 'status-badge status-badge--' + cls;
        statusBadge.textContent = device.status;
    }

    function infoRow(label, value, icon) {
        return '<div class="device-info__row">'
            + '<span class="device-info__label">' + icon + escapeHtml(label) + '</span>'
            + '<span class="device-info__value">' + value + '</span>'
            + '</div>';
    }

    function renderOverview() {
        var ipIcon = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/><line x1="6" y1="6" x2="6.01" y2="6"/><line x1="6" y1="18" x2="6.01" y2="18"/></svg>';
        var locIcon = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>';
        var typeIcon = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/></svg>';
        var vlanIcon = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16v6H4zM4 14h16v6H4z"/></svg>';

        var vlanText = '<span class="device-info__muted">Not assigned</span>';
        var macIcon = '<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="4" y="4" width="16" height="16" rx="2"/><path d="M9 9h.01M15 9h.01M9 15h6"/></svg>';
        var macText = device.macAddress
            ? '<code>' + escapeHtml(device.macAddress) + '</code>'
            : '<span class="device-info__muted">Unknown</span>';
        var html = '<div class="device-info card">'
            + '<h3 class="card__title">Overview</h3>'
            + infoRow('IP Address', '<code>' + escapeHtml(device.ipAddress) + '</code>', ipIcon)
            + infoRow('MAC Address', macText, macIcon)
            + infoRow('Type', escapeHtml(device.deviceType), typeIcon)
            + infoRow('Location', escapeHtml(device.deviceLocation), locIcon)
            + infoRow('VLAN', '<span id="overview-vlan">' + vlanText + '</span>', vlanIcon)
            + '</div>'
            + '<div class="device-status-panel card" id="device-status-panel">'
            + '<h3 class="card__title">Live status</h3>'
            + '<div class="ui-state"><p class="device-info__muted">Use the "Get status" action to refresh the live details.</p></div>'
            + '</div>';

        body.innerHTML = html;
        refreshOverviewVlan();
    }

    function refreshOverviewVlan() {
        var el = document.getElementById('overview-vlan');
        if (!el) return;
        var v = vlans.find(function (x) { return x.vlanId === device.vlanId; });
        el.innerHTML = v
            ? 'VLAN ' + v.vlanNumber + ' - ' + escapeHtml(v.name)
            : '<span class="device-info__muted">Not assigned</span>';
    }

    // ===== Action sidebar =====
    // available to IT + Admin unless adminOnly is set.
    function actionDefs() {
        return [
            { key: 'probe', label: 'Probe connection', desc: 'Check if the device responds.', color: 'cyan',
              icon: '<path d="M5 12.55a11 11 0 0114.08 0"/><path d="M1.42 9a16 16 0 0121.16 0"/><path d="M8.53 16.11a6 6 0 016.95 0"/><line x1="12" y1="20" x2="12.01" y2="20"/>' },
            { key: 'status', label: 'Get device status', desc: 'Refresh live details.', color: 'green',
              icon: '<path d="M22 12h-4l-3 9L9 3l-3 9H2"/>' },
            { key: 'ip', label: 'Change IP address', desc: 'Assign a different IP.', color: 'accent',
              icon: '<rect x="2" y="2" width="20" height="8" rx="2"/><rect x="2" y="14" width="20" height="8" rx="2"/><line x1="6" y1="6" x2="6.01" y2="6"/><line x1="6" y1="18" x2="6.01" y2="18"/>' },
            { key: 'vlan', label: 'Attach / detach VLAN', desc: 'Connect to a VLAN group.', color: 'purple',
              icon: '<path d="M4 4h16v6H4zM4 14h16v6H4z"/>' },
            { key: 'move', label: 'Move location', desc: 'Reassign the device.', color: 'orange',
              icon: '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/>' },
            { key: 'ports', label: 'Configure ports', desc: 'Enable, disable, assign VLAN.', color: 'cyan',
              icon: '<rect x="3" y="8" width="4" height="8"/><rect x="10" y="8" width="4" height="8"/><rect x="17" y="8" width="4" height="8"/>' },
            { key: 'routing', label: 'Routing', desc: 'Basic static routes.', color: 'pink',
              icon: '<circle cx="6" cy="19" r="3"/><circle cx="18" cy="5" r="3"/><path d="M9 19h6a3 3 0 003-3V8"/>' }
        ];
    }

    function renderActions() {
        actionsList.innerHTML = actionDefs().map(function (a) {
            return '<button type="button" class="action-tile action-tile--' + a.color + '" data-action="' + a.key + '">'
                + '<span class="action-tile__icon"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">' + a.icon + '</svg></span>'
                + '<span class="action-tile__text"><span class="action-tile__label">' + a.label + '</span>'
                + '<span class="action-tile__desc">' + a.desc + '</span></span>'
                + '</button>';
        }).join('');

        actionsList.querySelectorAll('[data-action]').forEach(function (btn) {
            btn.addEventListener('click', function () { handleAction(btn.dataset.action); });
        });
    }

    function handleAction(key) {
        switch (key) {
            case 'probe': return doProbe();
            case 'status': return doStatus();
            case 'ip': return openIpModal();
            case 'vlan': return openVlanModal();
            case 'move': return openMoveModal();
            case 'ports': return openPortsModal();
            case 'routing': return openRoutingModal();
        }
    }

    // ===== Probe =====
    function doProbe() {
        openOp('Probe connection',
            '<p class="op-text">Checking whether <strong>' + escapeHtml(device.deviceName || device.ipAddress)
            + '</strong> responds on the network...</p>'
            + '<div class="op-probe" id="op-probe">' + window.uiSpinner('Probing') + '</div>',
            null, null);

        api('POST', '/api/devices/' + deviceId + '/probe')
            .then(function (r) {
                var box = document.getElementById('op-probe');
                if (!box) return;
                var cls = r.success ? 'op-result--ok' : 'op-result--fail';
                var icon = r.success
                    ? '<path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><path d="M22 4L12 14.01l-3-3"/>'
                    : '<circle cx="12" cy="12" r="10"/><path d="M15 9l-6 6M9 9l6 6"/>';
                box.innerHTML = '<div class="op-result ' + cls + '">'
                    + '<svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">' + icon + '</svg>'
                    + '<p>' + escapeHtml(r.message) + '</p>'
                    + (r.success && r.latencyMs != null ? '<span class="op-result__meta">Response time: ' + r.latencyMs + ' ms</span>' : '')
                    + '</div>'
                    + (!r.success ? '<p class="op-hint">Tip: configure the device (IP, VLAN, ports) and use the "Connect" button on the devices page to bring it online.</p>' : '');
            })
            .catch(function (err) { window.showToast(err.message, 'error'); closeOp(); });
    }

    // ===== Get status =====
    function doStatus() {
        var panel = document.getElementById('device-status-panel');
        if (panel) {
            var existing = panel.querySelector('.ui-state, .device-status__grid');
            if (existing) existing.replaceWith(spinnerNode('Loading status'));
        }

        api('GET', '/api/devices/' + deviceId + '/status')
            .then(function (s) {
                if (!panel) return;
                var vlanText = s.vlanNumber ? ('VLAN ' + s.vlanNumber + ' - ' + escapeHtml(s.vlanName)) : 'Not assigned';
                panel.innerHTML = '<h3 class="card__title">Live status</h3>'
                    + '<div class="device-status__grid">'
                    + statusItem('Reachability', s.status, statusClasses[s.status] || 'unknown')
                    + statusItem('IP address', s.ipAddress, null)
                    + statusItem('VLAN', vlanText, null)
                    + statusItem('Ports up', s.portsUp + ' / ' + s.portCount, null)
                    + '</div>';
                window.showToast('Status refreshed.', 'success');
            })
            .catch(function (err) { window.showToast(err.message, 'error'); });
    }

    function spinnerNode(label) {
        var wrap = document.createElement('div');
        wrap.innerHTML = window.uiSpinner(label);
        return wrap.firstChild;
    }

    function statusItem(label, value, cls) {
        var val = cls
            ? '<span class="status-badge status-badge--' + cls + '">' + escapeHtml(value) + '</span>'
            : '<span class="device-status__value">' + escapeHtml(value) + '</span>';
        return '<div class="device-status__item"><span class="device-status__label">' + escapeHtml(label) + '</span>' + val + '</div>';
    }

    // ===== Change IP =====
    function openIpModal() {
        openOp('Change IP address',
            '<div class="form-group"><label for="op-ip">New IP address</label>'
            + '<input type="text" id="op-ip" maxlength="45" value="' + escapeHtml(device.ipAddress) + '" />'
            + '<small class="form-hint">Must be a valid IPv4 address (0.0.0.0) within the assignable range.</small></div>',
            'Apply', function () {
                var ip = document.getElementById('op-ip').value.trim();
                if (!isValidIpv4(ip)) { window.showToast('Enter a valid IPv4 address (0.0.0.0), no subnet mask.', 'error'); return; }
                api('PUT', '/api/devices/' + deviceId + '/ip', { ipAddress: ip })
                    .then(function (d) { device = d; renderHeader(); renderOverview(); closeOp(); window.showToast('IP address updated.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });
    }

    // ===== VLAN attach/detach =====
    function openVlanModal() {
        var options = '<option value="">Not assigned (detach)</option>'
            + vlans.map(function (v) {
                return '<option value="' + v.vlanId + '"' + (v.vlanId === device.vlanId ? ' selected' : '') + '>VLAN '
                    + v.vlanNumber + ' - ' + escapeHtml(v.name) + '</option>';
            }).join('');

        // VLAN management (create/delete) is available to IT and Admin.
        openOp('Attach / detach VLAN',
            '<p class="op-text">A VLAN groups devices together as if they were on their own small network.</p>'
            + '<div class="form-group"><label for="op-vlan">VLAN</label><select id="op-vlan">' + options + '</select></div>'
            + '<button type="button" class="op-link" id="op-manage-vlans">Create or manage VLANs</button>',
            'Apply', function () {
                var val = document.getElementById('op-vlan').value;
                var vlanId = val ? parseInt(val) : null;
                api('PUT', '/api/devices/' + deviceId + '/vlan', { vlanId: vlanId })
                    .then(function (d) { device = d; renderOverview(); closeOp(); window.showToast('VLAN assignment updated.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });

        var manageBtn = document.getElementById('op-manage-vlans');
        if (manageBtn) manageBtn.addEventListener('click', openManageVlans);
    }

    function openManageVlans() {
        var listHtml = vlans.length
            ? '<ul class="op-list">' + vlans.map(function (v) {
                return '<li class="op-list__item"><span><strong>VLAN ' + v.vlanNumber + '</strong> - ' + escapeHtml(v.name)
                    + ' <span class="device-info__muted">(' + v.deviceCount + ' device' + (v.deviceCount === 1 ? '' : 's') + ')</span></span>'
                    + (isAdmin ? '<button type="button" class="op-list__del" data-del-vlan="' + v.vlanId + '" title="Delete VLAN">&times;</button>' : '')
                    + '</li>';
            }).join('') + '</ul>'
            : '<p class="device-info__muted">No VLANs yet.</p>';

        openOp('Manage VLANs',
            listHtml
            + '<div class="op-divider"></div>'
            + '<div class="op-grid">'
            + '<div class="form-group"><label for="op-vnum">VLAN number</label><input type="number" id="op-vnum" min="1" max="4094" placeholder="e.g. 10" /></div>'
            + '<div class="form-group"><label for="op-vname">Name</label><input type="text" id="op-vname" maxlength="60" placeholder="e.g. Medical Devices" /></div>'
            + '</div>',
            'Create VLAN', function () {
                var num = parseInt(document.getElementById('op-vnum').value);
                var name = document.getElementById('op-vname').value.trim();
                if (!num || num < 1 || num > 4094) { window.showToast('VLAN number must be between 1 and 4094.', 'error'); return; }
                if (!name) { window.showToast('VLAN name is required.', 'error'); return; }
                api('POST', '/api/vlans', { vlanNumber: num, name: name })
                    .then(function () { return reloadVlans(); })
                    .then(function () { openManageVlans(); window.showToast('VLAN created.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });

        opBody.querySelectorAll('[data-del-vlan]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                api('DELETE', '/api/vlans/' + btn.dataset.delVlan)
                    .then(function () { return reloadVlans(); })
                    .then(function () { if (device.vlanId && !vlans.find(function (v) { return v.vlanId === device.vlanId; })) device.vlanId = null; openManageVlans(); window.showToast('VLAN deleted.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });
        });
    }

    function reloadVlans() {
        return fetch('/api/vlans').then(function (r) { return r.json(); }).then(function (data) { vlans = data; });
    }

    // ===== Move location =====
    function openMoveModal() {
        var options = locations.map(function (l) {
            return '<option value="' + l.id + '"' + (l.id === device.deviceLocationId ? ' selected' : '') + '>'
                + escapeHtml(l.locationName) + ' (' + escapeHtml(l.locationFloor) + ')</option>';
        }).join('');

        openOp('Move device',
            '<p class="op-text">Reassign this device to a different location.</p>'
            + '<div class="form-group"><label for="op-loc">Location</label><select id="op-loc">' + options + '</select></div>',
            'Move', function () {
                var locId = parseInt(document.getElementById('op-loc').value);
                api('PUT', '/api/devices/' + deviceId + '/location', { deviceLocationId: locId })
                    .then(function (d) { device = d; renderHeader(); renderOverview(); closeOp(); window.showToast('Device moved.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });
    }

    // ===== Ports =====
    function openPortsModal() {
        openOp('Configure ports', window.uiSpinner('Loading ports'), null, null);

        fetch('/api/devices/' + deviceId + '/ports')
            .then(function (r) { return r.json(); })
            .then(function (ports) { renderPortsModal(ports); })
            .catch(function () { window.showToast('Failed to load ports.', 'error'); closeOp(); });
    }

    function renderPortsModal(ports) {
        if (!ports.length) {
            opBody.innerHTML = '<p class="device-info__muted">This device has no ports registered.</p>';
            opConfirm.hidden = true;
            return;
        }

        var vlanOptions = function (selected) {
            return '<option value="">No VLAN</option>' + vlans.map(function (v) {
                return '<option value="' + v.vlanId + '"' + (v.vlanId === selected ? ' selected' : '') + '>VLAN ' + v.vlanNumber + '</option>';
            }).join('');
        };

        opTitle.textContent = 'Configure ports';
        opConfirm.hidden = true;
        opBody.innerHTML = '<div class="ports-table">' + ports.map(function (p) {
            return '<div class="ports-row" data-port="' + p.portId + '">'
                + '<div class="ports-row__name"><span class="ports-dot ' + (p.isUp ? 'up' : 'down') + '"></span>' + escapeHtml(p.name) + '</div>'
                + '<label class="ports-row__toggle"><input type="checkbox" data-enabled ' + (p.isEnabled ? 'checked' : '') + ' /> Enabled</label>'
                + '<select data-vlan>' + vlanOptions(p.vlanId) + '</select>'
                + '<button type="button" class="btn btn--ghost btn--sm" data-save-port>Save</button>'
                + '</div>';
        }).join('') + '</div>';

        opBody.querySelectorAll('[data-save-port]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var row = btn.closest('.ports-row');
                var portId = parseInt(row.dataset.port);
                var enabled = row.querySelector('[data-enabled]').checked;
                var vlanVal = row.querySelector('[data-vlan]').value;
                api('PUT', '/api/devices/' + deviceId + '/ports/' + portId, {
                    isEnabled: enabled, vlanId: vlanVal ? parseInt(vlanVal) : null
                }).then(function (p) {
                    var dot = row.querySelector('.ports-dot');
                    dot.className = 'ports-dot ' + (p.isUp ? 'up' : 'down');
                    window.showToast('Port "' + p.name + '" updated.', 'success');
                }).catch(function (err) { window.showToast(err.message, 'error'); });
            });
        });
    }

    // ===== Routing =====
    function openRoutingModal() {
        openOp('Routing', window.uiSpinner('Loading routes'), null, null);

        fetch('/api/devices/' + deviceId + '/routes')
            .then(function (r) { return r.json(); })
            .then(function (routes) { renderRoutingModal(routes); })
            .catch(function () { window.showToast('Failed to load routes.', 'error'); closeOp(); });
    }

    function renderRoutingModal(routes) {
        opTitle.textContent = 'Routing';
        opConfirm.hidden = true;

        var listHtml = routes.length
            ? '<ul class="op-list">' + routes.map(function (r) {
                return '<li class="op-list__item"><span>Traffic to <strong>' + escapeHtml(r.destination)
                    + '</strong> via <strong>' + escapeHtml(r.gateway) + '</strong></span>'
                    + '<button type="button" class="op-list__del" data-del-route="' + r.routeId + '" title="Delete route">&times;</button></li>';
            }).join('') + '</ul>'
            : '<p class="device-info__muted">No routes configured. Traffic uses the default gateway.</p>';

        opBody.innerHTML = '<p class="op-text">A route tells this device where to send traffic for a given destination network.</p>'
            + listHtml
            + '<div class="op-divider"></div>'
            + '<div class="op-grid">'
            + '<div class="form-group"><label for="op-dest">Destination network</label><input type="text" id="op-dest" maxlength="45" placeholder="e.g. 10.0.0.0" /></div>'
            + '<div class="form-group"><label for="op-gw">Gateway</label><input type="text" id="op-gw" maxlength="45" placeholder="e.g. 192.168.1.1" /></div>'
            + '</div>'
            + '<button type="button" class="btn btn--primary btn--sm" id="op-add-route">Add route</button>';

        document.getElementById('op-add-route').addEventListener('click', function () {
            var dest = document.getElementById('op-dest').value.trim();
            var gw = document.getElementById('op-gw').value.trim();
            if (!isValidIpv4(dest)) { window.showToast('Destination must be a valid IPv4 address.', 'error'); return; }
            if (!isValidIpv4(gw)) { window.showToast('Gateway must be a valid IPv4 address.', 'error'); return; }
            api('POST', '/api/devices/' + deviceId + '/routes', { destination: dest, gateway: gw })
                .then(function () { return fetch('/api/devices/' + deviceId + '/routes').then(function (r) { return r.json(); }); })
                .then(function (rs) { renderRoutingModal(rs); window.showToast('Route added.', 'success'); })
                .catch(function (err) { window.showToast(err.message, 'error'); });
        });

        opBody.querySelectorAll('[data-del-route]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                api('DELETE', '/api/devices/' + deviceId + '/routes/' + btn.dataset.delRoute)
                    .then(function () { return fetch('/api/devices/' + deviceId + '/routes').then(function (r) { return r.json(); }); })
                    .then(function (rs) { renderRoutingModal(rs); window.showToast('Route removed.', 'success'); })
                    .catch(function (err) { window.showToast(err.message, 'error'); });
            });
        });
    }

    // ===== Init =====
    Promise.all([
        fetch('/api/vlans').then(function (r) { return r.json(); }),
        fetch('/api/locations').then(function (r) { return r.json(); })
    ]).then(function (results) {
        vlans = results[0] || [];
        locations = results[1] || [];
    }).then(loadDevice).then(function () {
        // Keep the status badge in sync with the background monitor while the page is open.
        setInterval(refreshStatusBadge, 30000);
    }).catch(function () {
        body.innerHTML = window.uiEmptyState('This device could not be found.', 'error');
        actionsList.innerHTML = '';
        titleEl.textContent = 'Device not found';
    });

    function refreshStatusBadge() {
        if (document.hidden) return; // skip when the tab is in the background
        fetch('/api/devices/' + deviceId)
            .then(function (res) { return res.ok ? res.json() : null; })
            .then(function (d) {
                if (!d || d.status === device.status) return;
                device.status = d.status;
                var cls = statusClasses[d.status] || 'unknown';
                statusBadge.className = 'status-badge status-badge--' + cls;
                statusBadge.textContent = d.status;
                window.showToast('Device is now ' + d.status + '.', d.status === 'Online' ? 'success' : 'info');
            })
            .catch(function () { /* ignore transient errors */ });
    }
});
