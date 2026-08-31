(function () {
    // Multi-colour palette for interactive particle effects
    var colors = ['#16A34A', '#F59E0B', '#F97316', '#EC4899', '#8B5CF6', '#4ADE80', '#FEF08A', '#FB923C', '#A78BFA', '#6EE7B7'];
    var lastSpawn = 0;

    /**
     * Spawns an animated confetti particle that disperses from (cx, cy).
     */
    function spawnParticle(cx, cy) {
        var el = document.createElement('div');
        var color = colors[Math.floor(Math.random() * colors.length)];
        var size = 7 + Math.random() * 11;
        var angle = Math.random() * Math.PI * 2;
        var speed = 80 + Math.random() * 180;
        var dx = Math.cos(angle) * speed;
        var dy = Math.sin(angle) * speed - 60;
        var dur = 650 + Math.random() * 500;

        el.style.cssText =
            'position:fixed;left:' + (cx - size / 2) + 'px;top:' + (cy - size / 2) + 'px;' +
            'width:' + size + 'px;height:' + size + 'px;border-radius:50%;background:' + color + ';' +
            'pointer-events:none;z-index:9999;';
        document.body.appendChild(el);

        el.animate(
            [{ transform: 'translate(0,0) scale(1)', opacity: 1 },
            { transform: 'translate(' + dx + 'px,' + dy + 'px) scale(0.05)', opacity: 0 }],
            { duration: dur, easing: 'ease-out', fill: 'forwards' }
        ).onfinish = function () { el.remove(); };
    }

    /**
     * Triggers a staggered cluster burst of particles around a coordinate.
     */
    function burst(cx, cy, n) {
        for (var i = 0; i < n; i++) {
            (function (i) {
                setTimeout(function () {
                    spawnParticle(
                        cx + (Math.random() - 0.5) * 80,
                        cy + (Math.random() - 0.5) * 80
                    );
                }, i * 18);
            })(i);
        }
    }

    // ── Particle burst triggers on polaroids and cards ──
    document.querySelectorAll('.gw-polaroid[data-burst]').forEach(function (el) {
        el.addEventListener('mouseenter', function () {
            var r = el.getBoundingClientRect();
            burst(r.left + r.width / 2, r.top + r.height / 2, 18);
        });
    });

    document.querySelectorAll('.pw-card__burst-target').forEach(function (el) {
        el.addEventListener('mouseenter', function () {
            var r = el.getBoundingClientRect();
            burst(r.left + r.width / 2, r.top + r.height / 2, 22);
        });

        el.addEventListener('mousemove', function (e) {
            var now = Date.now();
            if (now - lastSpawn < 80) return; // Throttle to prevent DOM flooding
            lastSpawn = now;
            for (var i = 0; i < 2; i++) {
                spawnParticle(
                    e.clientX + (Math.random() - 0.5) * 16,
                    e.clientY + (Math.random() - 0.5) * 16
                );
            }
        });
    });

    // ── Interactive 3D tilt effect on producer cards ──
    document.querySelectorAll('.pw-card--producer').forEach(function (card) {
        card.addEventListener('mousemove', function (e) {
            var rect = card.getBoundingClientRect();
            var x = (e.clientX - rect.left) / rect.width - 0.5;
            var y = (e.clientY - rect.top) / rect.height - 0.5;
            card.style.transform = 'translateY(-6px) rotateY(' + (x * 8) + 'deg) rotateX(' + (-y * 5) + 'deg)';
        });

        card.addEventListener('mouseleave', function () {
            card.style.transform = '';
        });
    });
})();