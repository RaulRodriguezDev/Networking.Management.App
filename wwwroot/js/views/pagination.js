// ===== Reusable client-side paginator =====
// Usage:
//   var paginator = window.createPaginator({
//       container: document.getElementById('users-pagination'),
//       pageSize: 10,
//       onRender: function (pageItems) { /* render rows */ }
//   });
//   paginator.setItems(allUsers);
window.createPaginator = function (options) {
    var container = options.container;
    var pageSize = options.pageSize || 10;
    var onRender = options.onRender;

    var items = [];
    var currentPage = 1;

    function totalPages() {
        return Math.max(1, Math.ceil(items.length / pageSize));
    }

    function pageItems() {
        var start = (currentPage - 1) * pageSize;
        return items.slice(start, start + pageSize);
    }

    function goTo(page) {
        var total = totalPages();
        currentPage = Math.min(Math.max(1, page), total);
        onRender(pageItems());
        renderControls();
    }

    function renderControls() {
        if (!container) return;

        var total = totalPages();

        if (items.length <= pageSize) {
            container.innerHTML = '';
            return;
        }

        var from = (currentPage - 1) * pageSize + 1;
        var to = Math.min(currentPage * pageSize, items.length);

        var html = '<div class="pagination__info">Showing ' + from + ' - ' + to + ' of ' + items.length + '</div>'
            + '<div class="pagination__controls">';

        html += '<button type="button" class="pagination__btn" data-page="prev"' + (currentPage === 1 ? ' disabled' : '') + '>'
            + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M15 18l-6-6 6-6"/></svg>'
            + '</button>';

        pageNumbers(total).forEach(function (p) {
            if (p === '...') {
                html += '<span class="pagination__ellipsis">...</span>';
            } else {
                html += '<button type="button" class="pagination__btn pagination__page' + (p === currentPage ? ' pagination__page--active' : '') + '" data-page="' + p + '">' + p + '</button>';
            }
        });

        html += '<button type="button" class="pagination__btn" data-page="next"' + (currentPage === total ? ' disabled' : '') + '>'
            + '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 18l6-6-6-6"/></svg>'
            + '</button>';

        html += '</div>';
        container.innerHTML = html;

        container.querySelectorAll('[data-page]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var target = btn.dataset.page;
                if (target === 'prev') goTo(currentPage - 1);
                else if (target === 'next') goTo(currentPage + 1);
                else goTo(parseInt(target));
            });
        });
    }

    // Build a compact list of page numbers with ellipses for large sets.
    function pageNumbers(total) {
        var pages = [];
        if (total <= 7) {
            for (var i = 1; i <= total; i++) pages.push(i);
            return pages;
        }

        pages.push(1);
        var start = Math.max(2, currentPage - 1);
        var end = Math.min(total - 1, currentPage + 1);

        if (start > 2) pages.push('...');
        for (var j = start; j <= end; j++) pages.push(j);
        if (end < total - 1) pages.push('...');

        pages.push(total);
        return pages;
    }

    return {
        setItems: function (newItems, keepPage) {
            items = newItems || [];
            if (!keepPage) {
                currentPage = 1;
            } else {
                // Clamp in case the item count shrank below the current page.
                currentPage = Math.min(currentPage, totalPages());
            }
            onRender(pageItems());
            renderControls();
        },
        reset: function () {
            currentPage = 1;
        }
    };
};
