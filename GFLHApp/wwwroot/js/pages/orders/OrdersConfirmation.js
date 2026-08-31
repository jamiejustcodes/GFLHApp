(function () {
    'use strict';

    // ── Mouse-driven parallax effect on confirmation hero icons ──
    var hero = document.querySelector('.oc-hero');
    var floats = Array.prototype.slice.call(document.querySelectorAll('.oc-hero__float'));
    var depths = [18, 12, 24];
    var targetX = 0;
    var targetY = 0;
    var currentX = 0;
    var currentY = 0;
    var raf = null;

    function lerp(a, b, t) {
        return a + (b - a) * t;
    }

    function draw() {
        currentX = lerp(currentX, targetX, 0.08);
        currentY = lerp(currentY, targetY, 0.08);

        floats.forEach(function (el, index) {
            var depth = depths[index] || 14;
            el.style.setProperty('--oc-x', (currentX * depth).toFixed(2) + 'px');
            el.style.setProperty('--oc-y', (currentY * depth).toFixed(2) + 'px');
        });

        if (Math.abs(currentX - targetX) > 0.002 || Math.abs(currentY - targetY) > 0.002) {
            raf = requestAnimationFrame(draw);
        } else {
            raf = null;
        }
    }

    function requestDraw() {
        if (!raf) raf = requestAnimationFrame(draw);
    }

    if (hero && floats.length) {
        hero.addEventListener('mousemove', function (event) {
            var rect = hero.getBoundingClientRect();
            targetX = (event.clientX - rect.left) / rect.width - 0.5;
            targetY = (event.clientY - rect.top) / rect.height - 0.5;
            requestDraw();
        }, { passive: true });

        hero.addEventListener('mouseleave', function () {
            targetX = 0;
            targetY = 0;
            requestDraw();
        }, { passive: true });
    }

    // ── Order confirmation checkmark pop animation ──
    var confirmMark = document.querySelector('.oc-confirm-mark');
    if (confirmMark) {
        setTimeout(function () {
            confirmMark.classList.add('oc-pop');
        }, 250);
    }

    // ── Grower card hover elevation ──
    document.querySelectorAll('.oc-grower-card').forEach(function (card) {
        card.addEventListener('mouseenter', function () {
            card.style.transform = 'translateY(-2px)';
            card.style.boxShadow = 'var(--shadow-md)';
        });

        card.addEventListener('mouseleave', function () {
            card.style.transform = '';
            card.style.boxShadow = '';
        });
    });
}());

