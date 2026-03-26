document.addEventListener('DOMContentLoaded', function () {
    var overlay = document.getElementById('login-overlay');
    var openBtn = document.getElementById('btn-open-login');
    var closeBtn = document.getElementById('btn-close-login');

    if (!overlay) return;

    function openModal() {
        overlay.classList.add('login-overlay--visible');
        var firstInput = overlay.querySelector('input[type="text"]');
        if (firstInput) firstInput.focus();
    }

    function closeModal() {
        overlay.classList.remove('login-overlay--visible');
    }

    if (openBtn) {
        openBtn.addEventListener('click', openModal);
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', closeModal);
    }

    overlay.addEventListener('click', function (e) {
        if (e.target === overlay) closeModal();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && overlay.classList.contains('login-overlay--visible')) {
            closeModal();
        }
    });
});
