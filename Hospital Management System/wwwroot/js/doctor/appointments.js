/* ==========================================================================
   MediCore HMS — Doctor Appointments & Patients Page Script
   File: wwwroot/js/doctor/appointments.js
   Purpose: Shared page-specific behavior for both
   pages/doctor/appointments.html (status tabs, reschedule/cancel modals)
   and pages/doctor/patients.html (patient list selection). Each page only
   contains the DOM elements relevant to it, so every init function below
   safely no-ops when its elements are absent.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let appointmentPendingAction = null; // <div class="timeline-slot"> queued for reschedule/cancel

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Doctor');

        initMobileSidebarToggle();
        initAppointmentTabs();
        initRescheduleModal();
        initCancelModal();
        initPatientListSelection();
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
       appointments.html: Upcoming / Completed / Cancelled tabs
       -------------------------------------------------------------------- */
    function initAppointmentTabs() {
        const tabs = document.querySelectorAll('#appointmentTabs .tab-btn');
        const slots = document.querySelectorAll('#appointmentsTimeline .timeline-slot');
        if (!tabs.length) return;

        tabs.forEach((tab) => {
            tab.addEventListener('click', () => {
                tabs.forEach((t) => t.classList.remove('is-active'));
                tab.classList.add('is-active');

                const status = tab.dataset.status;
                slots.forEach((slot) => {
                    const slotStatus = slot.dataset.status;
                    const matches = status === 'upcoming'
                        ? (slotStatus === 'upcoming' || slotStatus === 'in-progress')
                        : slotStatus === status;
                    slot.style.display = matches ? '' : 'none';
                });
            });
        });
    }

    /* --------------------------------------------------------------------
       appointments.html: Reschedule modal
       -------------------------------------------------------------------- */
    function initRescheduleModal() {
        const overlay = document.getElementById('rescheduleModalOverlay');
        const form = document.getElementById('rescheduleForm');
        if (!overlay) return;

        MediCoreUtils.initModalCloseHandlers('rescheduleModalOverlay', [
            'closeRescheduleModalBtn',
            'cancelRescheduleBtn'
        ]);

        document.querySelectorAll('.reschedule-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                appointmentPendingAction = btn.closest('.timeline-slot');
                MediCoreUtils.openModal('rescheduleModalOverlay');
            });
        });

        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                newDate: document.getElementById('rescheduleDate').value,
                newTime: document.getElementById('rescheduleTime').value,
                reason: document.getElementById('rescheduleReason').value
            };

            try {
                const apptId = appointmentPendingAction?.querySelector('.tc-reason')?.textContent.match(/#APT-\d+/)?.[0] || 'unknown';
                await MediCoreAPI.Appointments.reschedule(apptId, payload);
                MediCoreUtils.showToast('Appointment rescheduled successfully.', 'success');
                MediCoreUtils.closeModal('rescheduleModalOverlay');
                form.reset();
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not reschedule appointment.', 'danger');
            }
        });
    }

    /* --------------------------------------------------------------------
       appointments.html: Cancel confirmation modal
       -------------------------------------------------------------------- */
    function initCancelModal() {
        const overlay = document.getElementById('cancelApptOverlay');
        if (!overlay) return;

        MediCoreUtils.initModalCloseHandlers('cancelApptOverlay', ['keepApptBtn']);

        document.querySelectorAll('.cancel-appt-btn').forEach((btn) => {
            btn.addEventListener('click', () => {
                appointmentPendingAction = btn.closest('.timeline-slot');
                MediCoreUtils.openModal('cancelApptOverlay');
            });
        });

        const confirmBtn = document.getElementById('confirmCancelApptBtn');
        confirmBtn?.addEventListener('click', async () => {
            if (!appointmentPendingAction) {
                MediCoreUtils.closeModal('cancelApptOverlay');
                return;
            }

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Cancelling…';

            try {
                const apptId = appointmentPendingAction.querySelector('.tc-reason')?.textContent.match(/#APT-\d+/)?.[0] || 'unknown';
                await MediCoreAPI.Appointments.cancel(apptId);

                const card = appointmentPendingAction.querySelector('.timeline-card');
                card?.classList.add('status-cancelled');
                const actionsDiv = card?.querySelector('.tc-actions');
                if (actionsDiv) {
                    actionsDiv.outerHTML = '<span class="badge badge-danger">Cancelled</span>';
                }

                MediCoreUtils.showToast('Appointment cancelled.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not cancel appointment.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Cancel Appointment';
                appointmentPendingAction = null;
                MediCoreUtils.closeModal('cancelApptOverlay');
            }
        });
    }

    /* --------------------------------------------------------------------
       patients.html: Selecting a patient from the list updates the
       detail panel's highlighted state (full record swap would be wired
       to a real API call once the backend exists).
       -------------------------------------------------------------------- */
    function initPatientListSelection() {
        const container = document.getElementById('patientListContainer');
        if (!container) return;

        container.addEventListener('click', (e) => {
            const item = e.target.closest('.patient-list-item');
            if (!item) return;

            container.querySelectorAll('.patient-list-item').forEach((el) => {
                el.classList.remove('is-selected');
                el.style.borderColor = '';
                el.style.background = '';
            });
            item.classList.add('is-selected');
            item.style.borderColor = 'var(--teal-500)';
            item.style.background = 'var(--teal-50)';

            // In a live build, this would fetch and render the selected
            // patient's full medical history:
            // MediCoreAPI.Patients.getHistory(item.dataset.patientId).then(renderDetail);
        });
    }
})();