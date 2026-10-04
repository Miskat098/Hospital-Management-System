/* ==========================================================================
   MediCore HMS — Patient Feedback Page Script
   File: wwwroot/js/patient/feedback.js
   Purpose: Page-specific behavior for pages/patient/feedback.html —
   star rating selection feedback and feedback form submission.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Patient');

        initMobileSidebarToggle();
        initStarRatingFeedback();
        initFeedbackForm();
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
       Star rating — the visual fill is handled entirely by CSS sibling
       selectors on the radio inputs; this just confirms the selection
       with a small toast so the choice feels acknowledged.
       -------------------------------------------------------------------- */
    function initStarRatingFeedback() {
        const rating = document.getElementById('starRating');
        if (!rating) return;

        rating.addEventListener('change', (e) => {
            if (e.target.name !== 'rating') return;
            const value = e.target.value;
            const labels = {
                1: 'Poor',
                2: 'Fair',
                3: 'Good',
                4: 'Very Good',
                5: 'Excellent'
            };
            MediCoreUtils.showToast(`Rating selected: ${value} star${value === '1' ? '' : 's'} — ${labels[value]}`, 'default', 1800);
        });
    }

    /* --------------------------------------------------------------------
       Feedback form submission
       -------------------------------------------------------------------- */
    function initFeedbackForm() {
        const form = document.getElementById('feedbackForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            if (!MediCoreUtils.validateForm(form)) return;

            const ratingInput = form.querySelector('input[name="rating"]:checked');
            if (!ratingInput) {
                MediCoreUtils.showToast('Please select a star rating before submitting.', 'danger');
                return;
            }

            const payload = {
                appointmentId: document.getElementById('feedbackAppointment').value,
                rating: parseInt(ratingInput.value, 10),
                comment: document.getElementById('feedbackComment').value,
                anonymous: document.getElementById('feedbackAnonymous').checked
            };

            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Submitting…';

            try {
                await MediCoreAPI.Feedback.submit(payload);
                MediCoreUtils.showToast('Thank you — your feedback has been submitted!', 'success');
                form.reset();
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not submit feedback. Please try again.', 'danger');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Submit Feedback';
            }
        });
    }
})();