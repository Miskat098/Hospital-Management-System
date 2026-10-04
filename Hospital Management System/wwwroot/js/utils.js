/* ==========================================================================
   MediCore HMS — Shared Utilities
   File: wwwroot/js/utils.js
   Contains: formatting helpers, validation helpers, toast notifications,
             modal open/close helpers, small DOM helpers, local storage
             helpers for the frontend-only preview stage.
   Loaded on every page BEFORE api.js and auth.js.
   ========================================================================== */

const MediCoreUtils = (function () {
    'use strict';

    /* --------------------------------------------------------------------
       Formatting helpers
       -------------------------------------------------------------------- */

    /**
     * Format a number as Bangladeshi Taka currency string.
     * @param {number} amount
     * @param {boolean} withSymbol
     * @returns {string}
     */
    function formatCurrency(amount, withSymbol = true) {
        const num = Number(amount) || 0;
        const formatted = num.toLocaleString('en-IN', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        });
        return withSymbol ? `৳${formatted}` : formatted;
    }

    /**
     * Format an ISO date string or Date object into "21 Sep 2026" style.
     * @param {string|Date} date
     * @returns {string}
     */
    function formatDate(date) {
        const d = date instanceof Date ? date : new Date(date);
        if (isNaN(d.getTime())) return '';
        const day = d.getDate();
        const month = d.toLocaleString('en-US', { month: 'short' });
        const year = d.getFullYear();
        return `${day} ${month} ${year}`;
    }

    /**
     * Format a time string (HH:MM, 24h) into "10:30 AM" style.
     * @param {string} time24
     * @returns {string}
     */
    function formatTime(time24) {
        if (!time24) return '';
        const [hStr, mStr] = time24.split(':');
        let h = parseInt(hStr, 10);
        const m = mStr ? mStr.padStart(2, '0') : '00';
        const meridiem = h >= 12 ? 'PM' : 'AM';
        h = h % 12;
        if (h === 0) h = 12;
        return `${h}:${m} ${meridiem}`;
    }

    /**
     * Format a full name into two-letter initials for avatars.
     * @param {string} fullName
     * @returns {string}
     */
    function getInitials(fullName) {
        if (!fullName) return '';
        const parts = fullName.trim().split(/\s+/).filter(Boolean);
        if (parts.length === 0) return '';
        if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
        return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }

    /**
     * Truncate text to a max length with an ellipsis.
     * @param {string} text
     * @param {number} maxLength
     * @returns {string}
     */
    function truncateText(text, maxLength = 60) {
        if (!text) return '';
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength).trim() + '…';
    }

    /* --------------------------------------------------------------------
       Validation helpers
       -------------------------------------------------------------------- */

    const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const BD_PHONE_REGEX = /^01[0-9]{9}$/;

    function isValidEmail(value) {
        return EMAIL_REGEX.test(String(value || '').trim());
    }

    function isValidBdPhone(value) {
        return BD_PHONE_REGEX.test(String(value || '').trim());
    }

    function isNotEmpty(value) {
        return String(value || '').trim().length > 0;
    }

    function passwordStrength(password) {
        if (!password) return 'weak';
        let score = 0;
        if (password.length >= 8) score++;
        if (/[A-Z]/.test(password)) score++;
        if (/[0-9]/.test(password)) score++;
        if (/[^A-Za-z0-9]/.test(password)) score++;
        if (score <= 1) return 'weak';
        if (score <= 3) return 'medium';
        return 'strong';
    }

    /**
     * Validate a single form field element based on its native HTML5
     * validity state, and toggle the .has-error class on its .field wrapper.
     * @param {HTMLElement} inputEl
     * @returns {boolean} true if valid
     */
    function validateField(inputEl) {
        const wrapper = inputEl.closest('.field');
        const valid = inputEl.checkValidity();
        if (wrapper) {
            wrapper.classList.toggle('has-error', !valid);
        }
        return valid;
    }

    /**
     * Validate every input/select/textarea within a form element.
     * @param {HTMLFormElement} formEl
     * @returns {boolean} true if the whole form is valid
     */
    function validateForm(formEl) {
        let allValid = true;
        const fields = formEl.querySelectorAll('input, select, textarea');
        fields.forEach((field) => {
            if (!validateField(field)) {
                allValid = false;
            }
        });
        return allValid;
    }

    /* --------------------------------------------------------------------
       Toast notifications
       -------------------------------------------------------------------- */

    /**
     * Show a toast notification. Requires a <div class="toast-stack" id="toastStack">
     * element to be present on the page.
     * @param {string} message
     * @param {'default'|'success'|'danger'} type
     * @param {number} durationMs
     */
    function showToast(message, type = 'default', durationMs = 3500) {
        const stack = document.getElementById('toastStack');
        if (!stack) {
            console.warn('MediCoreUtils.showToast: #toastStack not found on page');
            return;
        }

        const toast = document.createElement('div');
        toast.className = 'toast' + (type !== 'default' ? ` toast-${type}` : '');
        toast.setAttribute('role', 'status');

        const icon = document.createElement('span');
        icon.innerHTML = type === 'success'
            ? '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg>'
            : type === 'danger'
                ? '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18"><circle cx="12" cy="12" r="10"/><path d="M12 8v5M12 16h.01"/></svg>'
                : '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4M12 8h.01"/></svg>';

        const text = document.createElement('span');
        text.textContent = message;

        toast.appendChild(icon);
        toast.appendChild(text);
        stack.appendChild(toast);

        setTimeout(() => {
            toast.style.transition = 'opacity 200ms ease, transform 200ms ease';
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(16px)';
            setTimeout(() => toast.remove(), 220);
        }, durationMs);
    }

    /* --------------------------------------------------------------------
       Modal helpers
       -------------------------------------------------------------------- */

    /**
     * Open a modal overlay by element id.
     * @param {string} overlayId
     */
    function openModal(overlayId) {
        const overlay = document.getElementById(overlayId);
        if (!overlay) return;
        overlay.classList.add('is-open');
        document.body.style.overflow = 'hidden';
    }

    /**
     * Close a modal overlay by element id.
     * @param {string} overlayId
     */
    function closeModal(overlayId) {
        const overlay = document.getElementById(overlayId);
        if (!overlay) return;
        overlay.classList.remove('is-open');
        document.body.style.overflow = '';
    }

    /**
     * Wire up standard modal close behavior: close button(s), overlay
     * background click, and Escape key.
     * @param {string} overlayId
     * @param {string[]} closeButtonIds
     */
    function initModalCloseHandlers(overlayId, closeButtonIds = []) {
        const overlay = document.getElementById(overlayId);
        if (!overlay) return;

        closeButtonIds.forEach((btnId) => {
            const btn = document.getElementById(btnId);
            if (btn) btn.addEventListener('click', () => closeModal(overlayId));
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) closeModal(overlayId);
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && overlay.classList.contains('is-open')) {
                closeModal(overlayId);
            }
        });
    }

    /* --------------------------------------------------------------------
       Small DOM helpers
       -------------------------------------------------------------------- */

    function qs(selector, scope = document) {
        return scope.querySelector(selector);
    }

    function qsa(selector, scope = document) {
        return Array.from(scope.querySelectorAll(selector));
    }

    /**
     * Debounce a function call (useful for search inputs).
     * @param {Function} fn
     * @param {number} delayMs
     * @returns {Function}
     */
    function debounce(fn, delayMs = 300) {
        let timer = null;
        return function debounced(...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delayMs);
        };
    }

    /* --------------------------------------------------------------------
       Local storage helpers (frontend-only preview stage)
       These will be replaced by real API/session calls once the
       ASP.NET Core Web API backend is wired up.
       -------------------------------------------------------------------- */

    const STORAGE_PREFIX = 'medicore_';

    function storageSet(key, value) {
        try {
            localStorage.setItem(STORAGE_PREFIX + key, JSON.stringify(value));
        } catch (err) {
            console.warn('MediCoreUtils.storageSet failed:', err);
        }
    }

    function storageGet(key, fallback = null) {
        try {
            const raw = localStorage.getItem(STORAGE_PREFIX + key);
            return raw === null ? fallback : JSON.parse(raw);
        } catch (err) {
            console.warn('MediCoreUtils.storageGet failed:', err);
            return fallback;
        }
    }

    function storageRemove(key) {
        try {
            localStorage.removeItem(STORAGE_PREFIX + key);
        } catch (err) {
            console.warn('MediCoreUtils.storageRemove failed:', err);
        }
    }

    /* --------------------------------------------------------------------
       Public API
       -------------------------------------------------------------------- */

    return {
        // formatting
        formatCurrency,
        formatDate,
        formatTime,
        getInitials,
        truncateText,
        // validation
        isValidEmail,
        isValidBdPhone,
        isNotEmpty,
        passwordStrength,
        validateField,
        validateForm,
        // toast
        showToast,
        // modal
        openModal,
        closeModal,
        initModalCloseHandlers,
        // dom
        qs,
        qsa,
        debounce,
        // storage
        storageSet,
        storageGet,
        storageRemove
    };
})();