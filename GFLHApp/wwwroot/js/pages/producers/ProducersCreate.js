(function () {
    'use strict';

    var form = document.getElementById('producerForm');
    if (window.lucide) lucide.createIcons();

    var nameInput = document.getElementById('producerName');
    var emailInput = document.getElementById('producerEmail');
    var infoInput = document.getElementById('producerInfo');
    var vatToggle = document.getElementById('isVATRegistered');
    var vatNumberGroup = document.getElementById('vatNumberGroup');
    var vatInput = document.getElementById('vatNumber');
    var passwordInput = document.getElementById('accountPassword');
    var confirmPasswordInput = document.getElementById('confirmPassword');
    var uploadInput = document.getElementById('imageUpload');
    var pathInput = document.getElementById('liveImagePathInput');
    var imgCard = document.getElementById('adminImgCard');
    var previewHost = document.getElementById('adminImgEl');
    var pathLabel = document.getElementById('adminImgPath');
    var nameLabel = document.getElementById('adminImgName');
    var previewName = document.getElementById('previewName');
    var previewAvatar = document.getElementById('previewAvatar');
    var previewEmailText = document.getElementById('previewEmailText');
    var previewInfo = document.getElementById('previewInfo');
    var previewVat = document.getElementById('previewVat');

    function setError(id, message) {
        var node = document.getElementById(id);
        if (node) node.textContent = message || '';
    }

    function toggleVATNumber() {
        var isOpen = vatToggle && vatToggle.checked;
        if (vatNumberGroup) vatNumberGroup.classList.toggle('is-open', isOpen);
        if (previewVat) previewVat.classList.toggle('is-visible', isOpen);
        if (!isOpen && vatInput) {
            vatInput.value = '';
            setError('vatError', '');
        }
    }

    function validateName() {
        var val = (nameInput ? nameInput.value : '').trim();
        if (!val) { setError('nameError', 'Producer name is required.'); return false; }
        if (val.length < 3) { setError('nameError', 'Producer name must be at least 3 characters.'); return false; }
        if (val.length > 100) { setError('nameError', 'Producer name must not exceed 100 characters.'); return false; }
        setError('nameError', '');
        return true;
    }

    function validateEmail() {
        var val = (emailInput ? emailInput.value : '').trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!val) { setError('emailError', 'Email address is required.'); return false; }
        if (!emailRegex.test(val)) { setError('emailError', 'Please enter a valid email address.'); return false; }
        if (val.length > 150) { setError('emailError', 'Email address must not exceed 150 characters.'); return false; }
        setError('emailError', '');
        return true;
    }

    function validateInfo() {
        var val = (infoInput ? infoInput.value : '').trim();
        if (!val) { setError('infoError', 'Producer information is required.'); return false; }
        if (val.length < 10) { setError('infoError', 'Producer information must be at least 10 characters.'); return false; }
        if (val.length > 500) { setError('infoError', 'Producer information must not exceed 500 characters.'); return false; }
        setError('infoError', '');
        return true;
    }

    function validateVAT() {
        if (!vatToggle || !vatToggle.checked) {
            setError('vatError', '');
            return true;
        }

        var val = (vatInput ? vatInput.value : '').trim().toUpperCase();
        var vatRegex = /^GB[0-9]{9}$/;
        if (!val) { setError('vatError', 'VAT number is required if VAT registered.'); return false; }
        if (!vatRegex.test(val)) { setError('vatError', 'VAT number must start with GB followed by exactly 9 digits.'); return false; }
        if (vatInput) vatInput.value = val;
        setError('vatError', '');
        return true;
    }

    function validatePassword() {
        var val = passwordInput ? passwordInput.value : '';
        var passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$/;
        if (!val) { setError('passwordError', 'Password is required.'); return false; }
        if (!passwordRegex.test(val)) {
            setError('passwordError', 'Password must be at least 8 characters and include uppercase, lowercase, a number, and a special character.');
            return false;
        }
        setError('passwordError', '');
        return true;
    }

    function validateConfirmPassword() {
        var pass = passwordInput ? passwordInput.value : '';
        var confirm = confirmPasswordInput ? confirmPasswordInput.value : '';
        if (!confirm) { setError('confirmPasswordError', 'Please confirm the password.'); return false; }
        if (pass !== confirm) { setError('confirmPasswordError', 'Passwords do not match.'); return false; }
        setError('confirmPasswordError', '');
        return true;
    }

    function validateImage() {
        var file = uploadInput && uploadInput.files && uploadInput.files.length > 0 ? uploadInput.files[0] : null;
        var allowed = ['image/jpeg', 'image/png', 'image/webp'];
        var allowedExtensions = /\.(jpe?g|png|webp)$/i;
        if (!file) {
            setError('imageError', '');
            return true;
        }

        if (allowed.indexOf(file.type) === -1 && !allowedExtensions.test(file.name)) {
            setError('imageError', 'Upload a JPG, PNG, or WebP image.');
            return false;
        }

        if (file.size > 5 * 1024 * 1024) {
            setError('imageError', 'Image files must be smaller than 5 MB.');
            return false;
        }

        setError('imageError', '');
        return true;
    }

    function setPlaceholder() {
        if (!previewHost) return;
        previewHost.outerHTML = '<div class="pw-admin-img-placeholder" id="adminImgEl"><i data-lucide="sprout" style="width:72px;height:72px;opacity:.45;"></i></div>';
        previewHost = document.getElementById('adminImgEl');
        if (window.lucide) lucide.createIcons();
    }

    function setImage(src, label) {
        if (!previewHost) return;
        previewHost.outerHTML = '<img id="adminImgEl" src="' + src + '" alt="Producer image preview" />';
        previewHost = document.getElementById('adminImgEl');
        if (pathLabel) pathLabel.textContent = label;
    }

    if (vatToggle) {
        vatToggle.addEventListener('change', toggleVATNumber);
        toggleVATNumber();
    }

    if (nameInput && nameLabel) {
        nameLabel.textContent = nameInput.value || 'New producer';
        nameInput.addEventListener('input', function () {
            var value = nameInput.value.trim() || 'Producer name';
            nameLabel.textContent = value === 'Producer name' ? 'New producer' : value;
            if (previewName) previewName.textContent = value;
            if (previewAvatar) previewAvatar.textContent = value.charAt(0).toUpperCase() || '?';
        });
        nameInput.addEventListener('blur', validateName);
    }
    if (emailInput) {
        emailInput.addEventListener('input', function () {
            if (previewEmailText) previewEmailText.textContent = emailInput.value.trim() || '-';
        });
        emailInput.addEventListener('blur', validateEmail);
    }
    if (infoInput) {
        infoInput.addEventListener('input', function () {
            if (previewInfo) previewInfo.textContent = infoInput.value.trim() || 'No bio yet.';
        });
        infoInput.addEventListener('blur', validateInfo);
    }
    if (vatInput) vatInput.addEventListener('blur', validateVAT);
    if (passwordInput) passwordInput.addEventListener('blur', validatePassword);
    if (confirmPasswordInput) confirmPasswordInput.addEventListener('blur', validateConfirmPassword);

    if (pathInput) {
        pathInput.addEventListener('input', function () {
            if (uploadInput && uploadInput.files && uploadInput.files.length > 0) return;
            var src = pathInput.value.trim();
            if (src) setImage(src, src);
            else {
                setPlaceholder();
                if (pathLabel) pathLabel.textContent = 'no image selected';
            }
        });
    }

    if (uploadInput) {
        uploadInput.addEventListener('change', function () {
            if (!validateImage()) return;
            var file = uploadInput.files && uploadInput.files.length > 0 ? uploadInput.files[0] : null;
            if (!file) {
                var src = pathInput ? pathInput.value.trim() : '';
                if (src) setImage(src, src);
                else {
                    setPlaceholder();
                    if (pathLabel) pathLabel.textContent = 'no image selected';
                }
                return;
            }

            setImage(URL.createObjectURL(file), file.name);
        });
    }

    if (form) {
        form.addEventListener('submit', function (e) {
            var valid = validateName() & validateEmail() & validateInfo() & validateVAT() & validatePassword() & validateConfirmPassword() & validateImage();
            if (!valid) e.preventDefault();
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

    var summary = document.querySelector('[data-valmsg-summary]');
    if (summary && summary.querySelectorAll('li').length > 0) {
        summary.removeAttribute('hidden');
    }

    if (pathInput && pathInput.value.trim()) {
        setImage(pathInput.value.trim(), pathInput.value.trim());
    }
}());
