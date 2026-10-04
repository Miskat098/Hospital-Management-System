/* ==========================================================================
   MediCore HMS — Patient Prescriptions Page Script
   File: wwwroot/js/patient/prescriptions.js
   Purpose: Page-specific behavior for pages/patient/prescriptions.html —
   doctor/year filters and the download-as-PDF button on each prescription.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Patient');

        initMobileSidebarToggle();
        initFilters();
        initDownloadButtons();
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
       Doctor / year filters
       -------------------------------------------------------------------- */
    function initFilters() {
        const doctorFilter = document.getElementById('prescriptionDoctorFilter');
        const yearFilter = document.getElementById('prescriptionYearFilter');
        const cards = document.querySelectorAll('.prescription-card');
        if (!cards.length) return;

        function applyFilters() {
            const doctorQuery = (doctorFilter?.selectedOptions[0]?.text || '').toLowerCase();
            const doctorSelected = doctorFilter?.value || '';
            const year = yearFilter?.value || '';

            cards.forEach((card) => {
                const text = card.textContent.toLowerCase();
                const matchesDoctor = !doctorSelected || text.includes(doctorQuery);
                const matchesYear = !year || text.includes(year);
                card.style.display = (matchesDoctor && matchesYear) ? '' : 'none';
            });
        }

        doctorFilter?.addEventListener('change', applyFilters);
        yearFilter?.addEventListener('change', applyFilters);
    }

    /* --------------------------------------------------------------------
       Download buttons (placeholder — wires to a real PDF export once
       the Web API can generate prescription PDFs)
       -------------------------------------------------------------------- */
    function initDownloadButtons() {
        document.querySelectorAll('.download-rx-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                const card = btn.closest('.prescription-card');
                const doctor = card?.querySelector('.rxh-doctor')?.textContent.trim() || 'this prescription';
                MediCoreUtils.showToast(`Preparing PDF for ${doctor}…`, 'default', 2500);
                // In a live build:
                // MediCoreAPI.Prescriptions.downloadPdf(rxId).then(triggerBrowserDownload);
            });
        });
    }
})();