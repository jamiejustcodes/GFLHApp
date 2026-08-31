(function () {
    'use strict';

    if (window.lucide) lucide.createIcons();

    /* ── DOM Element Bindings ── */
    var nameInput = document.getElementById('liveNameInput');
    var emailInput = document.getElementById('liveEmailInput');
    var infoInput = document.getElementById('liveInfoInput');
    var vatInput = document.getElementById('VATNumber');
    var previewName = document.getElementById('previewName');
    var previewAvatar = document.getElementById('previewAvatar');
    var previewEmailText = document.getElementById('previewEmailText');
    var previewInfo = document.getElementById('previewInfo');
    var imgCard = document.getElementById('adminImgCard');
    var nameDisplay = document.getElementById('adminImgName');
    var pathDisplay = document.getElementById('adminImgPath');
    var imageInput = document.getElementById('liveImageInput');
    var uploadInput = document.getElementById('imageUpload');
    var originalPath = pathDisplay && pathDisplay.textContent !== 'no image set' ? pathDisplay.textContent.trim() : '';

    function ensureErrorAfter(input, id) {
        if (!input) return null;
        var existing = document.getElementById(id);
        if (existing) return existing;
        var error = document.createElement('span');
        error.id = id;
        error.className = 'pw-field__error';
        input.insertAdjacentElement('afterend', error);
        return error;
    }

    function setError(input, id, message) {
        var node = ensureErrorAfter(input, id);
        if (node) node.textContent = message || '';
    }

    function validateName() {
        var val = nameInput ? nameInput.value.trim() : '';
        if (!val) { setError(nameInput, 'nameError', 'Producer name is required.'); return false; }
        if (val.length < 3) { setError(nameInput, 'nameError', 'Producer name must be at least 3 characters.'); return false; }
        if (val.length > 100) { setError(nameInput, 'nameError', 'Producer name must not exceed 100 characters.'); return false; }
        setError(nameInput, 'nameError', '');
        return true;
    }

    function validateEmail() {
        var val = emailInput ? emailInput.value.trim() : '';
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!val) { setError(emailInput, 'emailError', 'Email address is required.'); return false; }
        if (!emailRegex.test(val)) { setError(emailInput, 'emailError', 'Please enter a valid email address.'); return false; }
        if (val.length > 150) { setError(emailInput, 'emailError', 'Email address must not exceed 150 characters.'); return false; }
        setError(emailInput, 'emailError', '');
        return true;
    }

    function validateInfo() {
        var val = infoInput ? infoInput.value.trim() : '';
        if (!val) { setError(infoInput, 'infoError', 'Producer information is required.'); return false; }
        if (val.length < 10) { setError(infoInput, 'infoError', 'Producer information must be at least 10 characters.'); return false; }
        if (val.length > 500) { setError(infoInput, 'infoError', 'Producer information must not exceed 500 characters.'); return false; }
        setError(infoInput, 'infoError', '');
        return true;
    }

    function validateVAT() {
        if (!vatToggle || !vatToggle.checked) {
            setError(vatInput, 'vatError', '');
            return true;
        }

        var val = vatInput ? vatInput.value.trim().toUpperCase() : '';
        if (!val) { setError(vatInput, 'vatError', 'VAT number is required if VAT registered.'); return false; }
        if (!/^GB[0-9]{9}$/.test(val)) { setError(vatInput, 'vatError', 'VAT number must start with GB followed by exactly 9 digits.'); return false; }
        if (vatInput) vatInput.value = val;
        setError(vatInput, 'vatError', '');
        return true;
    }

    function validateImage() {
        var file = uploadInput && uploadInput.files && uploadInput.files.length > 0 ? uploadInput.files[0] : null;
        var error = ensureErrorAfter(uploadInput, 'imageError');
        if (!file) {
            if (error) error.textContent = '';
            return true;
        }

        if (!/\.(jpe?g|png|webp)$/i.test(file.name) && ['image/jpeg', 'image/png', 'image/webp'].indexOf(file.type) === -1) {
            if (error) error.textContent = 'Upload a JPG, PNG, or WebP image.';
            return false;
        }

        if (file.size > 5 * 1024 * 1024) {
            if (error) error.textContent = 'Image files must be smaller than 5 MB.';
            return false;
        }

        if (error) error.textContent = '';
        return true;
    }

    // ── Live card synchronization ──
    if (nameInput) {
        nameInput.addEventListener('input', function () {
            var v = nameInput.value.trim() || 'Producer name';
            if (previewName) previewName.textContent = v;
            if (previewAvatar) previewAvatar.textContent = v.charAt(0).toUpperCase() || '?';
            if (nameDisplay) nameDisplay.textContent = v;
        });
        nameInput.addEventListener('blur', validateName);
    }

    if (emailInput && previewEmailText) {
        emailInput.addEventListener('input', function () {
            previewEmailText.textContent = emailInput.value.trim() || '—';
        });
        emailInput.addEventListener('blur', validateEmail);
    }

    if (infoInput && previewInfo) {
        infoInput.addEventListener('input', function () {
            previewInfo.textContent = infoInput.value.trim() || 'No bio yet.';
        });
        infoInput.addEventListener('blur', validateInfo);
    }

    // ── VAT toggle and conditional field visibility ──
    var vatToggle = document.getElementById('vatToggle');
    var vatNumberField = document.getElementById('vatNumberField');
    var previewVat = document.getElementById('previewVat');

    function syncVat() {
        var on = vatToggle && vatToggle.checked;
        if (vatNumberField) vatNumberField.classList.toggle('is-open', on);
        if (previewVat) previewVat.classList.toggle('is-visible', on);
        if (!on && vatInput) {
            vatInput.value = '';
            setError(vatInput, 'vatError', '');
        }
    }

    if (vatToggle) {
        vatToggle.addEventListener('change', syncVat);
        syncVat();
    }
    if (vatInput) vatInput.addEventListener('blur', validateVAT);

    function renderPlaceholder() {
        if (!imgCard) return;
        var existing = imgCard.querySelector('img');
        if (existing) {
            var ph = document.createElement('div');
            ph.className = 'pw-admin-img-placeholder';
            ph.id = 'adminImgEl';
            ph.innerHTML = '<i data-lucide="sprout" style="width:72px;height:72px;opacity:.45;"></i>';
            existing.replaceWith(ph);
            if (window.lucide) lucide.createIcons();
        }
    }

    function renderImage(src) {
        if (!imgCard) return;
        var existing = imgCard.querySelector('img');
        var placeholder = imgCard.querySelector('.pw-admin-img-placeholder');

        if (existing) {
            existing.src = src;
            existing.alt = nameInput ? (nameInput.value || 'Producer image') : 'Producer image';
            return;
        }

        var img = document.createElement('img');
        img.alt = nameInput ? (nameInput.value || 'Producer image') : 'Producer image';
        img.src = src;
        if (placeholder) placeholder.replaceWith(img);
        else imgCard.insertBefore(img, imgCard.querySelector('.pw-admin-img-meta'));
    }

    if (imageInput) {
        imageInput.addEventListener('input', function () {
            if (uploadInput && uploadInput.files && uploadInput.files.length > 0) return;

            var src = imageInput.value.trim();
            if (pathDisplay) pathDisplay.textContent = src || originalPath || 'no image set';

            if (src) renderImage(src);
            else if (originalPath) renderImage(originalPath);
            else renderPlaceholder();
        });
    }

    if (uploadInput) {
        uploadInput.addEventListener('change', function () {
            if (!validateImage()) return;
            var file = uploadInput.files && uploadInput.files.length > 0 ? uploadInput.files[0] : null;

            if (!file) {
                var src = imageInput ? imageInput.value.trim() : '';
                if (src) {
                    renderImage(src);
                    if (pathDisplay) pathDisplay.textContent = src;
                } else if (originalPath) {
                    renderImage(originalPath);
                    if (pathDisplay) pathDisplay.textContent = originalPath;
                } else {
                    renderPlaceholder();
                    if (pathDisplay) pathDisplay.textContent = 'no image set';
                }
                return;
            }

            renderImage(URL.createObjectURL(file));
            if (pathDisplay) pathDisplay.textContent = file.name;
        });
    }

    // ── 3D Card Tilt Interaction ──
    var card = document.getElementById('previewCard');
    if (card) {
        var ticking = false, lastE = {};
        card.addEventListener('mousemove', function (e) {
            lastE = e;
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(function () {
                var r = card.getBoundingClientRect();
                var x = ((lastE.clientX - r.left) / r.width - 0.5) * 2;
                var y = ((lastE.clientY - r.top) / r.height - 0.5) * 2;
                card.style.transform = 'perspective(800px) rotateY(' + (x * 10) + 'deg) rotateX(' + (-y * 8) + 'deg) scale(1.02)';
                card.style.boxShadow = '0 16px 48px rgba(0,0,0,.14)';
                ticking = false;
            });
        }, { passive: true });

        card.addEventListener('mouseleave', function () {
            card.style.transform = '';
            card.style.boxShadow = '';
        });
    }

    if (imgCard) {
        var imgTicking = false, imgLastE = {};
        imgCard.addEventListener('mousemove', function (e) {
            imgLastE = e;
            if (imgTicking) return;
            imgTicking = true;
            requestAnimationFrame(function () {
                var r = imgCard.getBoundingClientRect();
                var x = ((imgLastE.clientX - r.left) / r.width - 0.5) * 2;
                var y = ((imgLastE.clientY - r.top) / r.height - 0.5) * 2;
                imgCard.style.transform = 'perspective(800px) rotateY(' + (x * 10) + 'deg) rotateX(' + (-y * 8) + 'deg) scale(1.02)';
                imgCard.style.boxShadow = '0 16px 48px rgba(0,0,0,.14)';
                imgTicking = false;
            });
        }, { passive: true });

        imgCard.addEventListener('mouseleave', function () {
            imgCard.style.transform = '';
            imgCard.style.boxShadow = '';
        });
    }

    var form = document.getElementById('editProducerForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            var valid = validateName() & validateEmail() & validateInfo() & validateVAT() & validateImage();
            if (!valid) e.preventDefault();
        });
    }

    var summary = document.querySelector('[data-valmsg-summary]');
    if (summary && summary.querySelectorAll('li').length > 0) {
        summary.removeAttribute('hidden');
    }
}());
