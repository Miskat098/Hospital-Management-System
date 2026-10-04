/* ==========================================================================
   MediCore HMS — Patient Dashboard Page Script
   File: wwwroot/js/patient/dashboard.js
   Purpose: Page-specific behavior shared by pages/patient/dashboard.html
   and pages/patient/profile.html — sidebar toggle, and (on profile.html)
   the Personal/Medical/Security tab switcher and form submissions.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Patient');

        initMobileSidebarToggle();
        initProfileTabs();
        initPersonalInfoForm();
        initMedicalInfoForm();
        initSecurityForm();
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
       profile.html: Personal Info / Medical Info / Security tabs
       (no-ops safely on dashboard.html, which has no #profileTabs)
       -------------------------------------------------------------------- */
    function initProfileTabs() {
        const tabs = document.querySelectorAll('#profileTabs .tab-btn');
        if (!tabs.length) return;

        const panels = {
            personal: document.getElementById('panel-personal'),
            medical: document.getElementById('panel-medical'),
            security: document.getElementById('panel-security')
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
       profile.html: Personal Information form
       -------------------------------------------------------------------- */
    function initPersonalInfoForm() {
        const form = document.getElementById('personalInfoForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                firstName: document.getElementById('pFirstName').value,
                lastName: document.getElementById('pLastName').value,
                email: document.getElementById('pEmail').value,
                phone: document.getElementById('pPhone').value,
                dob: document.getElementById('pDob').value,
                gender: document.getElementById('pGender').value,
                nationalId: document.getElementById('pNid').value,
                address: document.getElementById('pAddress').value,
                emergencyContact: document.getElementById('pEmergencyContact').value
            };

            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Saving…';

            try {
                // In a live build: await MediCoreAPI.Patients.updateProfile(payload);
                await new Promise((resolve) => setTimeout(resolve, 400));
                MediCoreUtils.showToast('Personal information updated.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not save changes.', 'danger');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Save Changes';
            }
        });
    }

    /* --------------------------------------------------------------------
       profile.html: Medical Information form
       -------------------------------------------------------------------- */
    function initMedicalInfoForm() {
        const form = document.getElementById('medicalInfoForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                bloodGroup: document.getElementById('mBloodGroup').value,
                heightCm: document.getElementById('mHeight').value,
                allergies: document.getElementById('mAllergies').value,
                chronicConditions: document.getElementById('mConditions').value,
                currentMedications: document.getElementById('mMedications').value
            };

            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Saving…';

            try {
                // In a live build: await MediCoreAPI.Patients.updateMedicalInfo(payload);
                await new Promise((resolve) => setTimeout(resolve, 400));
                MediCoreUtils.showToast('Medical information updated.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not save changes.', 'danger');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Save Changes';
            }
        });
    }

    /* --------------------------------------------------------------------
       profile.html: Security / password form
       -------------------------------------------------------------------- */
    function initSecurityForm() {
        const form = document.getElementById('securityForm');
        if (!form) return;

        const newPw = document.getElementById('newPassword');
        const confirmPw = document.getElementById('confirmNewPassword');

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const confirmField = confirmPw.closest('.field');
            if (newPw.value !== confirmPw.value) {
                confirmField.classList.add('has-error');
                return;
            }
            confirmField.classList.remove('has-error');

            const submitBtn = form.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Updating…';

            try {
                // In a live build: await MediCoreAPI.Patients.changePassword({...});
                await new Promise((resolve) => setTimeout(resolve, 400));
                MediCoreUtils.showToast('Password updated successfully.', 'success');
                form.reset();
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not update password.', 'danger');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Update Password';
            }
        });
    }
})();