document.addEventListener('DOMContentLoaded', function () {
    var page = document.getElementById('users-page');
    if (!page) return; // only run on the Users management page

    var tbody = document.getElementById('users-tbody');
    var userModal = document.getElementById('user-modal');
    var deleteModal = document.getElementById('delete-modal');
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

    var paginator = window.createPaginator({
        container: document.getElementById('users-pagination'),
        pageSize: 10,
        onRender: renderUsers
    });

    var roleIcons = {
        Admin: '<path d="M5 16L3 5l5.5 5L12 4l3.5 6L21 5l-2 11H5zm0 3h14v2H5v-2z"/>',
        IT: '<path d="M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z"/>',
        Guest: '<path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>'
    };

    // ===== Modal helpers =====
    function openModal(modal) { modal.classList.add('modal--open'); modal.setAttribute('aria-hidden', 'false'); }
    function closeModal(modal) { modal.classList.remove('modal--open'); modal.setAttribute('aria-hidden', 'true'); }

    document.querySelectorAll('[data-close-modal]').forEach(function (el) {
        el.addEventListener('click', function () {
            closeModal(userModal);
            closeModal(deleteModal);
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeModal(userModal);
            closeModal(deleteModal);
        }
    });

    // ===== Render role selection cards =====
    function renderRoleCards(selectedRoleId) {
        roleCards.innerHTML = roles.map(function (r) {
            var icon = roleIcons[r.roleName] || roleIcons.Guest;
            var isSelected = r.roleId === selectedRoleId;
            return '<div class="role-card' + (isSelected ? ' role-card--selected' : '') + '" data-role-id="' + r.roleId + '">'
                + '<svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">' + icon + '</svg>'
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
            .then(function (users) { paginator.setItems(users); })
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
                + '<button class="icon-btn icon-btn--logs" data-logs="' + u.userId + '" data-username="' + escapeHtml(u.username) + '" title="View logs">'
                + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><path d="M14 2v6h6"/><path d="M16 13H8M16 17H8M10 9H8"/></svg>'
                + '</button>'
                + '<button class="icon-btn icon-btn--edit" data-edit="' + u.userId + '" title="Edit">'
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
            btn.addEventListener('click', function () {
                // Redirect to the Logs section pre-filtered by this user.
                window.location.href = '/Admin/Logs?username=' + encodeURIComponent(btn.dataset.username);
            });
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
