(function () {
    'use strict';

    // ── Delivery vs Collection conditional panel toggling ──
    var deliveryToggle = document.querySelector('[data-fulfilment-toggle="delivery"]');
    var collectionToggle = document.querySelector('[data-fulfilment-toggle="collection"]');
    var panels = Array.prototype.slice.call(document.querySelectorAll('[data-conditional-panel]'));
    var form = document.getElementById('orderEditForm');

    function setPanelState() {
        var deliveryOn = deliveryToggle && deliveryToggle.checked;
        var collectionOn = collectionToggle && collectionToggle.checked;

        panels.forEach(function (panel) {
            var type = panel.getAttribute('data-conditional-panel');
            var active = type === 'delivery' ? deliveryOn : collectionOn;
            panel.classList.toggle('is-muted', !active);
            panel.setAttribute('aria-hidden', active ? 'false' : 'true');
        });
    }

    function bindExclusive(source, target) {
        if (!source || !target) return;
        source.addEventListener('change', function () {
            if (source.checked) target.checked = false;
            if (!source.checked && !target.checked) source.checked = true;
            setPanelState();
        });
    }

    bindExclusive(deliveryToggle, collectionToggle);
    bindExclusive(collectionToggle, deliveryToggle);
    setPanelState();

    if (form) {
        form.addEventListener('submit', function () {
            var submit = form.querySelector('button[type="submit"]');
            if (!submit) return;
            submit.classList.add('is-saving');
            submit.setAttribute('aria-busy', 'true');
        });
    }
}());

