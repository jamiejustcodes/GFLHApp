'use strict';

(function () {
    var root = document.documentElement;
    var storage = window.localStorage;
    var fontSizeKey = 'gflh-font-size';
    var darkModeKey = 'gflh-dark-mode';
    var contrastKey = 'gflh-high-contrast';
    var defaultFontSize = 16;
    var minFontSize = 14;
    var maxFontSize = 22;
    var fontStep = 2;
    var isSpeaking = false;

    function setStoredValue(key, value) {
        try {
            storage.setItem(key, value);
        } catch (e) { }
    }

    function getStoredValue(key) {
        try {
            return storage.getItem(key);
        } catch (e) {
            return null;
        }
    }

    function setPressed(selector, pressed) {
        document.querySelectorAll(selector).forEach(function (el) {
            el.setAttribute('aria-pressed', String(pressed));
        });
    }

    function setLabel(name, value) {
        document.querySelectorAll('[data-accessibility-label="' + name + '"]').forEach(function (el) {
            el.textContent = value;
        });
    }

    function refreshUi() {
        var isDark = document.body.classList.contains('dark-mode');
        var isHighContrast = document.body.classList.contains('high-contrast');

        setPressed('[data-accessibility-action="dark-mode"]', isDark);
        setPressed('[data-accessibility-action="high-contrast"]', isHighContrast);
        setPressed('[data-accessibility-action="speak"]', isSpeaking);

        setLabel('dark-mode', isDark ? 'Light mode' : 'Dark mode');
        setLabel('high-contrast', isHighContrast ? 'Normal contrast' : 'High contrast');
        setLabel('speak', isSpeaking ? 'Stop reading' : 'Read page aloud');
    }

    function applyStoredPreferences() {
        if (getStoredValue(darkModeKey) === 'true' || getStoredValue('theme') === 'dark') {
            document.body.classList.add('dark-mode');
        }

        if (getStoredValue(contrastKey) === 'true' || getStoredValue('contrastMode') === 'high-contrast') {
            document.body.classList.add('high-contrast');
        }

        var savedFontSize = parseInt(getStoredValue(fontSizeKey) || getStoredValue('fontSize'), 10);
        if (!Number.isNaN(savedFontSize)) {
            setFontSize(savedFontSize, false);
        }

        refreshUi();
    }

    function toggleDarkMode() {
        document.body.classList.toggle('dark-mode');
        setStoredValue(darkModeKey, String(document.body.classList.contains('dark-mode')));
        refreshUi();
    }

    function toggleHighContrast() {
        document.body.classList.toggle('high-contrast');
        setStoredValue(contrastKey, String(document.body.classList.contains('high-contrast')));
        refreshUi();
    }

    function setFontSize(size, persist) {
        var nextSize = Math.max(minFontSize, Math.min(maxFontSize, size));

        root.style.fontSize = nextSize === defaultFontSize ? '' : nextSize + 'px';

        if (persist !== false) {
            if (nextSize === defaultFontSize) {
                try {
                    storage.removeItem(fontSizeKey);
                } catch (e) { }
            } else {
                setStoredValue(fontSizeKey, String(nextSize));
            }
        }
    }

    function changeFontSize(action) {
        var currentSize = parseFloat(window.getComputedStyle(root).fontSize) || defaultFontSize;

        if (action === 'increase') {
            setFontSize(currentSize + fontStep);
        } else if (action === 'decrease') {
            setFontSize(currentSize - fontStep);
        } else {
            setFontSize(defaultFontSize);
        }
    }

    function getReadableText() {
        var main = document.getElementById('main-content') || document.querySelector('main') || document.body;
        return main.innerText.replace(/\s+/g, ' ').trim();
    }

    function speakText() {
        if (!('speechSynthesis' in window) || !('SpeechSynthesisUtterance' in window)) {
            return;
        }

        if (isSpeaking) {
            window.speechSynthesis.cancel();
            isSpeaking = false;
            refreshUi();
            return;
        }

        var text = getReadableText();
        if (!text) return;

        var speech = new SpeechSynthesisUtterance(text);
        speech.lang = 'en-GB';
        speech.rate = 0.95;
        speech.onend = speech.onerror = function () {
            isSpeaking = false;
            refreshUi();
        };

        window.speechSynthesis.cancel();
        isSpeaking = true;
        refreshUi();
        window.speechSynthesis.speak(speech);
    }

    function closeAccessibilityMenu() {
        var menu = document.getElementById('accessibilityMenu');
        var toggle = document.getElementById('accessibilityToggle');
        var dropdown = document.getElementById('accessibilityDropdown');

        if (!menu || !toggle) return;

        menu.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
        if (dropdown) dropdown.hidden = true;
    }

    function openAccessibilityMenu() {
        var menu = document.getElementById('accessibilityMenu');
        var toggle = document.getElementById('accessibilityToggle');
        var dropdown = document.getElementById('accessibilityDropdown');

        if (!menu || !toggle || !dropdown) return;

        menu.classList.add('is-open');
        toggle.setAttribute('aria-expanded', 'true');
        dropdown.hidden = false;
    }

    function initAccessibilityMenu() {
        var menu = document.getElementById('accessibilityMenu');
        var toggle = document.getElementById('accessibilityToggle');
        var dropdown = document.getElementById('accessibilityDropdown');

        if (!menu || !toggle || !dropdown) return;

        toggle.addEventListener('click', function (event) {
            event.stopPropagation();
            menu.classList.contains('is-open') ? closeAccessibilityMenu() : openAccessibilityMenu();
        });

        dropdown.addEventListener('click', function (event) {
            var actionButton = event.target.closest('[data-accessibility-action]');
            if (!actionButton) return;

            var action = actionButton.getAttribute('data-accessibility-action');

            if (action === 'dark-mode') toggleDarkMode();
            if (action === 'increase-font') changeFontSize('increase');
            if (action === 'decrease-font') changeFontSize('decrease');
            if (action === 'reset-font') changeFontSize('reset');
            if (action === 'high-contrast') toggleHighContrast();
            if (action === 'speak') speakText();

            if (action !== 'speak') closeAccessibilityMenu();
        });

        document.addEventListener('click', function (event) {
            if (!menu.contains(event.target)) closeAccessibilityMenu();
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') closeAccessibilityMenu();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        applyStoredPreferences();
        initAccessibilityMenu();
    });

    window.toggleDarkMode = toggleDarkMode;
    window.toggleHighContrast = toggleHighContrast;
    window.changeFontSize = changeFontSize;
    window.speakText = speakText;
}());
