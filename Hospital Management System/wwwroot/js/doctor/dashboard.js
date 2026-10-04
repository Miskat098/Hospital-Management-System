/* ==========================================================================
   MediCore HMS — Doctor Dashboard Page Script
   File: wwwroot/js/doctor/dashboard.js
   Purpose: Page-specific behavior for pages/doctor/dashboard.html.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Doctor');

        initMobileSidebarToggle();
        initStartVisitButtons();
        initTopbarSearch();
    });

    /* --------------------------------------------------------------------
       Shared: mobile sidebar toggle
       -------------------------------------------------------------------- */
    function initMobileSidebarToggle() {
        const toggleBtn = document.getElementById('menuToggle');
        const sidebar = document.getElementById('sidebar');
        if (!toggleBtn || !sidebar) return;
        toggleBtn.addEventListener('click', () => sidebar.classList.toggle('is-open'));
    }

    /* --------------------------------------------------------------------
       "Start Visit" buttons on the today's schedule timeline
       -------------------------------------------------------------------- */
    function initStartVisitButtons() {
        document.querySelectorAll('.timeline-card .btn-primary').forEach((btn) => {
            if (!btn.textContent.includes('Start Visit')) return;
            btn.addEventListener('click', () => {
                const card = btn.closest('.timeline-card');
                const patientName = card?.querySelector('.tc-name')?.textContent.trim() || 'the patient';
                MediCoreUtils.showToast(`Visit started with ${patientName}.`, 'success');
                // In a live build, this would navigate to a visit/consultation
                // workspace or open a modal to record vitals and notes.
            });
        });
    }

    /* --------------------------------------------------------------------
       Topbar patient search
       -------------------------------------------------------------------- */
    function initTopbarSearch() {
        const input = document.querySelector('.topbar-search input');
        if (!input) return;

        const handleSearch = MediCoreUtils.debounce((value) => {
            if (!value) return;
            // Placeholder for a future patient search call, e.g.:
            // MediCoreAPI.Patients.search(value).then(renderResults);
            console.log('Doctor patient search:', value);
        }, 350);

        input.addEventListener('input', (e) => handleSearch(e.target.value.trim()));
    }
})();