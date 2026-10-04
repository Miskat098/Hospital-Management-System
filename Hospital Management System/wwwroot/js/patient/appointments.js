/* ==========================================================================
   MediCore HMS — Patient Appointments Page Script
   File: wwwroot/js/patient/appointments.js
   Purpose: Page-specific behavior for pages/patient/appointments.html —
   Book New / Upcoming / Past tabs, doctor selection, slot picker,
   booking confirmation, and cancel confirmation for upcoming visits.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let appointmentPendingCancel = null;

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Patient');

        initMobileSidebarToggle();
        initPageTabs();
        initDoctorSelection();
        initSlotPicker();
        initBookingConfirmation();
        initCancelModal();
        initRescheduleButtons();
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
       Book New / Upcoming / Past Visits tabs
       -------------------------------------------------------------------- */
    function initPageTabs() {
        const tabs = document.querySelectorAll('#apptPageTabs .tab-btn');
        if (!tabs.length) return;

        const panels = {
            book: document.getElementById('panel-book'),
            upcoming: document.getElementById('panel-upcoming'),
            past: document.getElementById('panel-past')
        };

        tabs.forEach((tab) => {
            tab.addEventListener('click', () => {
                tabs.forEach((t) => t.classList.remove('is-active'));
                tab.classList.add('is-active');

                const target = tab.dataset.panel;
                Object.keys(panels).forEach((key) => {
                    if (panels[key]) {
                        panels[key].style.display = key === target ? '' : 'none';
                    }
                });
            });
        });
    }

    /* --------------------------------------------------------------------
       Doctor selection ("Select Slot" buttons on each doctor-select-card)
       -------------------------------------------------------------------- */
    function initDoctorSelection() {
        const list = document.getElementById('doctorResultsList');
        const slotPanel = document.getElementById('slotSelectionPanel');
        if (!list || !slotPanel) return;

        list.addEventListener('click', (e) => {
            const btn = e.target.closest('.select-doctor-btn');
            if (!btn) return;

            const card = btn.closest('.doctor-select-card');
            const doctorName = card?.querySelector('.dsc-name')?.textContent.trim() || 'the doctor';

            const heading = slotPanel.querySelector('h3');
            if (heading) {
                heading.textContent = `Available Slots — ${doctorName}, 21 Sep 2026`;
            }

            slotPanel.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    }

    /* --------------------------------------------------------------------
       Slot picker (single-select time slot buttons)
       -------------------------------------------------------------------- */
    function initSlotPicker() {
        const picker = document.querySelector('.slot-picker');
        if (!picker) return;

        picker.addEventListener('click', (e) => {
            const btn = e.target.closest('.slot-btn');
            if (!btn || btn.disabled) return;

            picker.querySelectorAll('.slot-btn').forEach((b) => b.classList.remove('is-selected'));
            btn.classList.add('is-selected');
        });
    }

    /* --------------------------------------------------------------------
       Confirm Booking button
       -------------------------------------------------------------------- */
    function initBookingConfirmation() {
        const confirmBtn = document.getElementById('confirmBookingBtn');
        if (!confirmBtn) return;

        MediCoreUtils.initModalCloseHandlers('bookingSuccessOverlay', ['closeBookingSuccessBtn']);

        confirmBtn.addEventListener('click', async () => {
            const selectedSlot = document.querySelector('.slot-btn.is-selected');
            if (!selectedSlot) {
                MediCoreUtils.showToast('Please select a time slot first.', 'danger');
                return;
            }

            const reason = document.getElementById('visitReason')?.value || '';

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Booking…';

            try {
                await MediCoreAPI.Appointments.book({
                    time: selectedSlot.textContent.trim(),
                    reason
                });
                MediCoreUtils.openModal('bookingSuccessOverlay');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not book appointment.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Confirm Booking — ৳1,500';
            }
        });
    }

    /* --------------------------------------------------------------------
       Cancel confirmation modal (Upcoming tab)
       -------------------------------------------------------------------- */
    function initCancelModal() {
        const overlay = document.getElementById('cancelPatientApptOverlay');
        if (!overlay) return;

        MediCoreUtils.initModalCloseHandlers('cancelPatientApptOverlay', ['keepPatientApptBtn']);

        document.querySelectorAll('.cancel-patient-appt-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                appointmentPendingCancel = btn.closest('.appointment-card');
                MediCoreUtils.openModal('cancelPatientApptOverlay');
            });
        });

        const confirmBtn = document.getElementById('confirmCancelPatientApptBtn');
        confirmBtn?.addEventListener('click', async () => {
            if (!appointmentPendingCancel) {
                MediCoreUtils.closeModal('cancelPatientApptOverlay');
                return;
            }

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Cancelling…';

            try {
                await MediCoreAPI.Appointments.cancel('unknown');
                appointmentPendingCancel.style.opacity = '0.5';
                appointmentPendingCancel.querySelector('.ac-actions').innerHTML =
                    '<span class="badge badge-danger">Cancelled</span>';
                MediCoreUtils.showToast('Appointment cancelled.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not cancel appointment.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Yes, Cancel';
                appointmentPendingCancel = null;
                MediCoreUtils.closeModal('cancelPatientApptOverlay');
            }
        });
    }

    /* --------------------------------------------------------------------
       Reschedule buttons (Upcoming tab) — placeholder toast until a
       dedicated reschedule modal is wired up for patients.
       -------------------------------------------------------------------- */
    function initRescheduleButtons() {
        document.querySelectorAll('.reschedule-patient-appt-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                MediCoreUtils.showToast('Pick a new date and time on the Book New tab, then cancel this slot.', 'default', 4000);
            });
        });
    }
})();