/* ==========================================================================
   MediCore HMS — Admin Billing & Revenue Page Script
   File: wwwroot/js/admin/billing.js
   Purpose: Page-specific behavior for pages/admin/billing-overview.html —
   period selector, invoice status filter, and CSV export trigger.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Admin');

        initMobileSidebarToggle();
        initPeriodSelect();
        initInvoiceStatusFilter();
        initExportButton();
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
       Period selector (7d / 30d / 90d / 12m)
       -------------------------------------------------------------------- */
    function initPeriodSelect() {
        const select = document.getElementById('periodSelect');
        if (!select) return;

        select.addEventListener('change', () => {
            // In a live build, this would re-fetch revenue trend + KPI data
            // for the selected period and re-render the chart and stat cards:
            // fetchRevenueSummary(select.value).then(renderDashboard);
            MediCoreUtils.showToast(`Showing data for: ${select.options[select.selectedIndex].text}`, 'default', 2000);
        });
    }

    /* --------------------------------------------------------------------
       Invoice status filter on the Recent Invoices table
       -------------------------------------------------------------------- */
    function initInvoiceStatusFilter() {
        const select = document.getElementById('invoiceStatusFilter');
        if (!select) return;

        const rows = document.querySelectorAll('.data-table tbody tr');

        select.addEventListener('change', () => {
            const status = select.value.toLowerCase();
            rows.forEach((row) => {
                const matches = !status || row.textContent.toLowerCase().includes(status);
                row.style.display = matches ? '' : 'none';
            });
        });
    }

    /* --------------------------------------------------------------------
       Export button (placeholder — wires to a real CSV/PDF export once
       the Web API exposes an export endpoint)
       -------------------------------------------------------------------- */
    function initExportButton() {
        const buttons = document.querySelectorAll('.panel-toolbar .btn-secondary');
        buttons.forEach((btn) => {
            if (!btn.textContent.includes('Export')) return;
            btn.addEventListener('click', () => {
                MediCoreUtils.showToast('Export will be available once the reporting API is connected.', 'default');
            });
        });
    }
})();