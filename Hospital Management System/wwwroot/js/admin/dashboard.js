/* ==========================================================================
   MediCore HMS — Admin Dashboard Page Script
   File: wwwroot/js/admin/dashboard.js
   Purpose: Page-specific behavior for pages/admin/dashboard.html.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Admin');

        initMobileSidebarToggle();
        initTopbarSearch();
    });

    /**
     * Wires the hamburger menu button (visible on small screens) to toggle
     * the sidebar's .is-open class, matching the CSS in dashboard.css.
     */
    function initMobileSidebarToggle() {
        const toggleBtn = document.getElementById('menuToggle');
        const sidebar = document.getElementById('sidebar');
        if (!toggleBtn || !sidebar) return;

        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('is-open');
        });

        // Close the sidebar when clicking outside of it on mobile.
        document.addEventListener('click', (e) => {
            if (!sidebar.classList.contains('is-open')) return;
            if (sidebar.contains(e.target) || toggleBtn.contains(e.target)) return;
            sidebar.classList.remove('is-open');
        });
    }

    /**
     * Simple debounce-driven console hook for the topbar search input.
     * Wire this to a real global search endpoint once the API exists.
     */
    function initTopbarSearch() {
        const input = document.querySelector('.topbar-search input');
        if (!input) return;

        const handleSearch = MediCoreUtils.debounce((value) => {
            if (!value) return;
            // Placeholder for a future global search call, e.g.:
            // MediCoreAPI.Search.query(value).then(renderResults);
            console.log('Admin global search:', value);
        }, 350);

        input.addEventListener('input', (e) => handleSearch(e.target.value.trim()));
    }
})();