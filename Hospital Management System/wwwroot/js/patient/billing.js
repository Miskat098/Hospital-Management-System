/* ==========================================================================
   MediCore HMS — Patient Billing Page Script
   File: wwwroot/js/patient/billing.js
   Purpose: Page-specific behavior for pages/patient/billing.html —
   Pending/Paid tabs, opening the payment modal, toggling card fields
   based on payment method, and simulating a successful payment.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let invoicePendingPayment = null;

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Patient');

        initMobileSidebarToggle();
        initBillingTabs();
        initPaymentModal();
        initPaymentMethodToggle();
        initPaymentSuccessModal();
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
       Pending / Paid tabs
       -------------------------------------------------------------------- */
    function initBillingTabs() {
        const tabs = document.querySelectorAll('#billingTabs .tab-btn');
        if (!tabs.length) return;

        const panels = {
            pending: document.getElementById('pending-invoices'),
            paid: document.getElementById('paid-invoices')
        };

        tabs.forEach((tab) => {
            tab.addEventListener('click', () => {
                tabs.forEach((t) => t.classList.remove('is-active'));
                tab.classList.add('is-active');

                const target = tab.dataset.status;
                Object.keys(panels).forEach((key) => {
                    if (panels[key]) {
                        panels[key].style.display = key === target ? '' : 'none';
                    }
                });
            });
        });
    }

    /* --------------------------------------------------------------------
       Payment modal open/close and "Pay Now" trigger
       -------------------------------------------------------------------- */
    function initPaymentModal() {
        const overlay = document.getElementById('paymentModalOverlay');
        if (!overlay) return;

        MediCoreUtils.initModalCloseHandlers('paymentModalOverlay', [
            'closePaymentModalBtn',
            'cancelPaymentBtn'
        ]);

        document.querySelectorAll('.pay-invoice-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                invoicePendingPayment = btn.dataset.invoice;
                const titleEl = overlay.querySelector('.modal-header h3');
                if (titleEl) titleEl.textContent = `Pay Invoice — ${invoicePendingPayment}`;
                MediCoreUtils.openModal('paymentModalOverlay');
            });
        });

        const confirmBtn = document.getElementById('confirmPaymentBtn');
        confirmBtn?.addEventListener('click', async () => {
            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Processing…';

            try {
                await MediCoreAPI.Billing.pay(invoicePendingPayment || 'unknown', {
                    method: document.getElementById('paymentMethod')?.value
                });
                MediCoreUtils.closeModal('paymentModalOverlay');
                MediCoreUtils.openModal('paymentSuccessOverlay');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Payment failed. Please try again.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Pay ৳3,200';
            }
        });

        // "Details" button — simple informational toast for now.
        document.querySelectorAll('.view-invoice-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                MediCoreUtils.showToast(`Viewing details for ${btn.dataset.invoice}`, 'default', 2000);
            });
        });
    }

    /* --------------------------------------------------------------------
       Payment method select — show/hide card fields
       -------------------------------------------------------------------- */
    function initPaymentMethodToggle() {
        const select = document.getElementById('paymentMethod');
        const cardFields = document.getElementById('cardFields');
        if (!select || !cardFields) return;

        function toggle() {
            cardFields.style.display = select.value === 'card' ? '' : 'none';
        }
        select.addEventListener('change', toggle);
        toggle();
    }

    /* --------------------------------------------------------------------
       Payment success modal close
       -------------------------------------------------------------------- */
    function initPaymentSuccessModal() {
        MediCoreUtils.initModalCloseHandlers('paymentSuccessOverlay', ['closePaymentSuccessBtn']);
    }
})();