(function () {
    var colors = ['#16A34A', '#4ade80', '#86efac', '#d9f99d', '#6ee7b7', '#a7f3d0', '#bbf7d0', '#34d399', '#10b981'];

    /* ── Particle Animation Helpers ── */
    function spawnParticle(cx, cy) {
        var el = document.createElement('div');
        var color = colors[Math.floor(Math.random() * colors.length)];
        var size = 7 + Math.random() * 11;
        var angle = Math.random() * Math.PI * 2;
        var speed = 80 + Math.random() * 160;
        var dx = Math.cos(angle) * speed;
        var dy = Math.sin(angle) * speed - 55;
        var dur = 600 + Math.random() * 450;

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

    function burst(cx, cy, n) {
        for (var i = 0; i < n; i++) {
            (function (i) {
                setTimeout(function () {
                    spawnParticle(cx + (Math.random() - .5) * 70, cy + (Math.random() - .5) * 70);
                }, i * 18);
            })(i);
        }
    }

    /* ── Mouse Parallax on Hero Floating Icons ── */
    var floats = document.querySelectorAll('.pd-hero__float');
    var depths = [14, 20, 9, 17];
    document.addEventListener('mousemove', function (e) {
        var cx = window.innerWidth / 2;
        var cy = window.innerHeight / 2;
        var dx = (e.clientX - cx) / cx;
        var dy = (e.clientY - cy) / cy;
        floats.forEach(function (el, i) {
            var d = depths[i] || 12;
            el.style.transform = 'translate(' + (dx * d) + 'px,' + (dy * d) + 'px)';
        });
    });

    /* ── Interactive Avatar Burst ── */
    var avatar = document.querySelector('.pd-hero__avatar');
    if (avatar) {
        avatar.addEventListener('click', function () {
            var r = avatar.getBoundingClientRect();
            burst(r.left + r.width / 2, r.top + r.height / 2, 28);
        });
    }

    /* ── 3D Tilt and Particle Burst on Product Cards ── */
    var lastBurst = 0;
    document.querySelectorAll('.pd-product-card').forEach(function (card) {
        card.style.transition = 'transform .25s ease, box-shadow .25s ease';

        card.addEventListener('mouseenter', function () {
            var now = Date.now();
            if (now - lastBurst < 300) return; // Throttle hover bursts
            lastBurst = now;
            var r = card.getBoundingClientRect();
            burst(r.left + r.width / 2, r.top + 30, 14);
        });

        card.addEventListener('mousemove', function (e) {
            var r = card.getBoundingClientRect();
            var x = (e.clientX - r.left) / r.width - 0.5;
            var y = (e.clientY - r.top) / r.height - 0.5;
            card.style.transform = 'translateY(-6px) rotateY(' + (x * 10) + 'deg) rotateX(' + (-y * 7) + 'deg)';
        });

        card.addEventListener('mouseleave', function () {
            card.style.transform = '';
        });
    });

    /* ── Click Ripple Effects ── */
    document.querySelectorAll('.pd-fact-card, .pd-stat-tile').forEach(function (card) {
        card.style.position = 'relative';
        card.style.overflow = 'hidden';

        card.addEventListener('click', function (e) {
            var r = card.getBoundingClientRect();
            var size = Math.max(r.width, r.height) * 1.4;
            var el = document.createElement('span');
            el.className = 'pd-ripple';
            el.style.cssText =
                'width:' + size + 'px;height:' + size + 'px;' +
                'left:' + (e.clientX - r.left - size / 2) + 'px;' +
                'top:' + (e.clientY - r.top - size / 2) + 'px;';
            card.appendChild(el);
            setTimeout(function () { el.remove(); }, 600);
        });
    });

    /* ── Animated Stat Counter Numbers ── */
    var statEls = document.querySelectorAll('.pd-stat-tile__value[data-target]');
    var counted = false;

    function runCounters() {
        if (counted) return;
        counted = true;

        statEls.forEach(function (el) {
            var target = parseInt(el.dataset.target, 10);
            if (isNaN(target) || target === 0) return;
            var startTime = null, dur = 900;

            (function step(ts) {
                if (!startTime) startTime = ts;
                var p = Math.min((ts - startTime) / dur, 1);
                var ease = 1 - Math.pow(1 - p, 3); // Cubic ease out
                el.textContent = Math.round(ease * target);
                if (p < 1) requestAnimationFrame(step);
                else el.textContent = target;
            })(performance.now());
        });
    }

    new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            if (e.isIntersecting) runCounters();
        });
    }, { threshold: 0.3 }).observe(
        document.querySelector('.pd-stats-strip') || document.body
    );
})();