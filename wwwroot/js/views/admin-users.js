document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('users-page');
    if (!page) return; // only run on the Users management page

    var tbody = document.getElementById('users-tbody');
    var userModal = document.getElementById('user-modal');
    var deleteModal = document.getElementById('delete-modal');
    var userLogsModal = document.getElementById('user-logs-modal');
    var userLogsList = document.getElementById('user-logs-list');
    var userLogsTitle = document.getElementById('user-logs-modal-title');
    var roleCards = document.getElementById('role-cards');

    var form = {
        id: document.getElementById('user-id'),
        name: document.getElementById('user-name'),
        username: document.getElementById('user-username'),
        password: document.getElementById('user-password'),
        roleId: document.getElementById('user-role-id'),
        passwordHint: document.getElementById('password-hint')
    };

    var modalTitle = document.getElementById('user-modal-title');
    var deleteUsernameLabel = document.getElementById('delete-username');

    var roles = [];
    var pendingDeleteId = null;

    var roleIcons = {
        Admin: '<circle cx="12" cy="8" r="4"/><path d="M5.5 21a8.38 8.38 0 0113 0"/><path d="M12 2l1.5 2.5M12 2l-1.5 2.5"/>',
        IT: '<rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/>',
        Guest: '<circle cx="12" cy="8" r="4"/><path d="M5.5 21a8.38 8.38 0 0113 0"/>'
    };

    // ===== Modal helpers =====
    function openModal(modal) { modal.classList.add('modal--open'); modal.setAttribute('aria-hidden', 'false'); }
    function closeModal(modal) { modal.classList.remove('modal--open'); modal.setAttribute('aria-hidden', 'true'); }

    document.querySelectorAll('[data-close-modal]').forEach(function (el) {
        el.addEventListener('click', function () {
            closeModal(userModal);
            closeModal(deleteModal);
            closeModal(userLogsModal);
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeModal(userModal);
            closeModal(deleteModal);
            closeModal(userLogsModal);
        }
    });

    // ===== Render role selection cards =====
    function renderRoleCards(selectedRoleId) {
        roleCards.innerHTML = roles.map(function (r) {
            var icon = roleIcons[r.roleName] || roleIcons.Guest;
            var isSelected = r.roleId === selectedRoleId;
            return '<div class="role-card' + (isSelected ? ' role-card--selected' : '') + '" data-role-id="' + r.roleId + '">'
                + '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">' + icon + '</svg>'
                + '<span class="role-card__name">' + r.roleName + '</span>'
                + (r.description ? '<span class="role-card__desc">' + r.description + '</span>' : '')
                + '</div>';
        }).join('');

        form.roleId.value = selectedRoleId || '';

        roleCards.querySelectorAll('.role-card').forEach(function (card) {
            card.addEventListener('click', function () {
                roleCards.querySelectorAll('.role-card').forEach(function (c) { c.classList.remove('role-card--selected'); });
                card.classList.add('role-card--selected');
                form.roleId.value = parseInt(card.dataset.roleId);
            });
        });
    }

    // ===== Load data =====
    function loadRoles() {
        return fetch('/api/roles')
            .then(function (res) { return res.json(); })
            .then(function (data) { roles = data; });
    }

    function loadUsers() {
        tbody.innerHTML = '<tr><td colspan="4" class="users__empty">Loading…</td></tr>';
        fetch('/api/users')
            .then(function (res) { return res.json(); })
            .then(function (users) { renderUsers(users); })
            .catch(function () {
                tbody.innerHTML = '<tr><td colspan="4" class="users__empty">Failed to load users.</td></tr>';
                window.showToast('Failed to load users.', 'error');
            });
    }

    function renderUsers(users) {
        if (!users.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="users__empty">No users found.</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(function (u) {
            return '<tr>'
                + '<td>' + escapeHtml(u.name) + '</td>'
                + '<td>' + escapeHtml(u.username) + '</td>'
                + '<td><span class="role-badge role-badge--' + u.roleName.toLowerCase() + '">' + escapeHtml(u.roleName) + '</span></td>'
                + '<td class="users__row-actions">'
                + '<button class="icon-btn" data-logs="' + u.userId + '" data-username="' + escapeHtml(u.username) + '" title="View logs">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6"/><path d="M16 13H8M16 17H8M10 9H8"/></svg>'
                + '</button>'
                + '<button class="icon-btn" data-edit="' + u.userId + '" title="Edit">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>'
                + '</button>'
                + '<button class="icon-btn icon-btn--danger" data-delete="' + u.userId + '" data-username="' + escapeHtml(u.username) + '" title="Delete">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>'
                + '</button>'
                + '</td>'
                + '</tr>';
        }).join('');

        tbody.querySelectorAll('[data-edit]').forEach(function (btn) {
            btn.addEventListener('click', function () { openEditModal(parseInt(btn.dataset.edit)); });
        });
        tbody.querySelectorAll('[data-delete]').forEach(function (btn) {
            btn.addEventListener('click', function () { openDeleteModal(parseInt(btn.dataset.delete), btn.dataset.username); });
        });
        tbody.querySelectorAll('[data-logs]').forEach(function (btn) {
            btn.addEventListener('click', function () { openUserLogsModal(parseInt(btn.dataset.logs), btn.dataset.username); });
        });
    }

    // ===== Add =====
    document.getElementById('btn-add-user').addEventListener('click', function () {
        form.id.value = '';
        form.name.value = '';
        form.username.value = '';
        form.password.value = '';
        form.username.disabled = false;
        form.password.required = true;
        form.passwordHint.textContent = 'Minimum 8 characters. Required.';
        modalTitle.textContent = 'Add User';
        renderRoleCards(roles.length ? roles[0].roleId : null);
        openModal(userModal);
    });

    // ===== Edit =====
    function openEditModal(userId) {
        fetch('/api/users/' + userId)
            .then(function (res) { return res.json(); })
            .then(function (u) {
                form.id.value = u.userId;
                form.name.value = u.name;
                form.username.value = u.username;
                form.username.disabled = true; // username is the unique key, not editable
                form.password.value = '';
                form.password.required = false;
                form.passwordHint.textContent = 'Leave blank to keep current password.';
                modalTitle.textContent = 'Edit User';

                var role = roles.find(function (r) { return r.roleName === u.roleName; });
                renderRoleCards(role ? role.roleId : null);
                openModal(userModal);
            })
            .catch(function () { window.showToast('Failed to load user.', 'error'); });
    }

    // ===== Save (create or update) =====
    document.getElementById('btn-save-user').addEventListener('click', function () {
        if (!validateForm()) return;

        var id = form.id.value;
        var isEdit = !!id;

        var payload = {
            name: form.name.value.trim(),
            roleId: parseInt(form.roleId.value)
        };

        if (isEdit) {
            if (form.password.value) payload.password = form.password.value;
        } else {
            payload.username = form.username.value.trim();
            payload.password = form.password.value;
        }

        var url = isEdit ? '/api/users/' + id : '/api/users';
        var method = isEdit ? 'PUT' : 'POST';

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (res.status === 409) {
                return res.json().then(function (body) { throw new Error(body.message || 'Username already exists.'); });
            }
            if (!res.ok) throw new Error('Request failed.');
            return res.status === 204 ? null : res.json();
        }).then(function () {
            closeModal(userModal);
            window.showToast(isEdit ? 'User updated successfully.' : 'User created successfully.', 'success');
            loadUsers();
        }).catch(function (err) {
            window.showToast(err.message || 'Operation failed.', 'error');
        });
    });

    function validateForm() {
        var isEdit = !!form.id.value;

        if (!form.name.value.trim()) { window.showToast('Name is required.', 'error'); return false; }
        if (!isEdit && !form.username.value.trim()) { window.showToast('Username is required.', 'error'); return false; }
        if (!isEdit && form.password.value.length < 8) { window.showToast('Password must be at least 8 characters.', 'error'); return false; }
        if (isEdit && form.password.value && form.password.value.length < 8) { window.showToast('Password must be at least 8 characters.', 'error'); return false; }
        if (!form.roleId.value) { window.showToast('Please select a role.', 'error'); return false; }

        return true;
    }

    // ===== Delete =====
    function openDeleteModal(userId, username) {
        pendingDeleteId = userId;
        deleteUsernameLabel.textContent = username;
        openModal(deleteModal);
    }

    document.getElementById('btn-confirm-delete').addEventListener('click', function () {
        if (!pendingDeleteId) return;

        fetch('/api/users/' + pendingDeleteId, { method: 'DELETE' })
            .then(function (res) {
                if (!res.ok) throw new Error('Delete failed.');
                closeModal(deleteModal);
                window.showToast('User deleted successfully.', 'success');
                loadUsers();
            })
            .catch(function () { window.showToast('Failed to delete user.', 'error'); })
            .finally(function () { pendingDeleteId = null; });
    });

    // ===== View user logs =====
    function openUserLogsModal(userId, username) {
        userLogsTitle.textContent = 'Activity Logs — ' + username;
        userLogsList.innerHTML = '<p class="user-logs__empty">Loading…</p>';
        openModal(userLogsModal);

        fetch('/api/logs/by-user/' + userId)
            .then(function (res) {
                if (!res.ok) throw new Error('Request failed.');
                return res.json();
            })
            .then(function (logs) { renderUserLogs(logs); })
            .catch(function () {
                userLogsList.innerHTML = '<p class="user-logs__empty">Failed to load logs.</p>';
                window.showToast('Failed to load user logs.', 'error');
            });
    }

    function renderUserLogs(logs) {
        if (!logs.length) {
            userLogsList.innerHTML = '<p class="user-logs__empty">No activity recorded for this user.</p>';
            return;
        }

        userLogsList.innerHTML = logs.map(function (log) {
            return '<div class="user-logs__item">'
                + '<div class="user-logs__item-head">'
                + '<span class="user-logs__action">' + escapeHtml(log.actionType) + '</span>'
                + '<span class="user-logs__time">' + formatDate(log.createdAt) + '</span>'
                + '</div>'
                + '<div class="user-logs__details">' + escapeHtml(log.details) + '</div>'
                + '</div>';
        }).join('');
    }

    function formatDate(iso) {
        var d = new Date(iso);
        return isNaN(d.getTime()) ? iso : d.toLocaleString();
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str == null ? '' : str;
        return div.innerHTML;
    }

    // ===== Init =====
    loadRoles().then(loadUsers).catch(function () {
        window.showToast('Failed to load roles.', 'error');
        loadUsers();
    });
});
