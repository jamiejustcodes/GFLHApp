(function () {
    'use strict';

    // ── Checkbox confirmation guard for order deletion ──
    var checkbox = document.getElementById('confirmDeleteOrder');
    var button = document.getElementById('deleteOrderButton');
    var form = document.getElementById('orderDeleteForm');

    function syncButton() {
        if (!checkbox || !button) return;
        button.disabled = !checkbox.checked;
        button.classList.toggle('is-ready', checkbox.checked);
    }

    if (checkbox && button) {
        checkbox.addEventListener('change', syncButton);
        syncButton();
    }

    if (form && button) {
        form.addEventListener('submit', function (event) {
            if (button.disabled) {
                event.preventDefault();
                return;
            }

            button.setAttribute('aria-busy', 'true');
        });
    }
}());

