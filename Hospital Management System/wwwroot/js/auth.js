/* ==========================================================================
   MediCore HMS — Auth & RBAC
   File: wwwroot/js/auth.js
   Purpose: Handle login/register form submission, store the session,
   guard pages by role, wire up the logout button, and dynamically
   hydrate all user info placeholders across pages.
   Depends on: utils.js, api.js (both loaded before this file)
   ========================================================================== */

(function() {
    'use strict';

    /* --------------------------------------------------------------------
       Session helpers
       -------------------------------------------------------------------- */

    function getCurrentUser() {
        return MediCoreUtils.storageGet('currentUser', null);
    }

    function isLoggedIn() {
        return !!MediCoreUtils.storageGet('authToken', null);
    }

    function setSession(token, user) {
        MediCoreUtils.storageSet('authToken', token);
        MediCoreUtils.storageSet('currentUser', user);
    }

    function clearSession() {
        MediCoreUtils.storageRemove('authToken');
        MediCoreUtils.storageRemove('currentUser');
    }

    /**
     * Redirect to the correct dashboard home for a given role.
     * @param {'Admin'|'Doctor'|'Patient'} role
     */
    function redirectToDashboard(role) {
        const routes = {
            Admin: '/pages/admin/dashboard.html',
            Doctor: '/pages/doctor/dashboard.html',
            Patient: '/pages/patient/dashboard.html'
        };
        window.location.href = routes[role] || '/index.html';
    }

    /**
     * Call this at the top of any protected dashboard page to enforce RBAC.
     * If the user is not logged in, or is logged in as the wrong role,
     * they are redirected away.
     * @param {'Admin'|'Doctor'|'Patient'} requiredRole
     */
    function guardPage(requiredRole) {
        if (!isLoggedIn()) {
            window.location.href = '/pages/login.html';
            return;
        }
        const user = getCurrentUser();
        if (requiredRole && user && user.role !== requiredRole) {
            redirectToDashboard(user.role);
        }
    }

    /* --------------------------------------------------------------------
       Login form
       -------------------------------------------------------------------- */

    function initLoginForm() {
        const form = document.getElementById('loginForm');
        if (!form) return;

        const alertBox = document.getElementById('loginAlert');
        const alertText = document.getElementById('loginAlertText');
        const submitBtn = document.getElementById('loginSubmitBtn');

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            if (!MediCoreUtils.validateForm(form)) return;

            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const roleInput = form.querySelector('input[name="role"]:checked');
            const role = roleInput ? roleInput.value : 'Patient';

            if (alertBox) alertBox.style.display = 'none';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Logging in…';
            }

            try {
                const result = await MediCoreAPI.Auth.login(email, password, role);
                setSession(result.token, result.user);
                MediCoreUtils.showToast(`Welcome back, ${result.user.name.split(' ')[0]}!`, 'success');
                redirectToDashboard(result.user.role);
            } catch (err) {
                if (alertBox && alertText) {
                    alertText.textContent = err.message || 'Invalid email or password. Please try again.';
                    alertBox.style.display = 'flex';
                }
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = 'Log In';
                }
            }
        });
    }

    /* --------------------------------------------------------------------
       Register form
       -------------------------------------------------------------------- */

    function initRegisterForm() {
        const form = document.getElementById('registerForm');
        if (!form) return;

        const alertBox = document.getElementById('registerAlert');
        const alertText = document.getElementById('registerAlertText');
        const submitBtn = document.getElementById('registerSubmitBtn');
        const pwInput = document.getElementById('regPassword');
        const confirmInput = document.getElementById('confirmPassword');
        const strengthMeter = document.getElementById('pwStrengthMeter');
        const strengthLabel = document.getElementById('pwStrengthLabel');

        if (pwInput && strengthMeter) {
            pwInput.addEventListener('input', () => {
                const strength = MediCoreUtils.passwordStrength(pwInput.value);
                strengthMeter.className = 'pw-strength ' + strength;
                if (strengthLabel) {
                    const labels = {
                        weak: 'Weak — add a number, a symbol, and more length.',
                        medium: 'Medium — getting there, try adding a symbol.',
                        strong: 'Strong password.'
                    };
                    strengthLabel.textContent = labels[strength];
                }
            });
        }

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            if (!MediCoreUtils.validateForm(form)) return;

            const confirmField = confirmInput.closest('.field');
            if (pwInput.value !== confirmInput.value) {
                confirmField.classList.add('has-error');
                return;
            }
            confirmField.classList.remove('has-error');

            const payload = {
                firstName: document.getElementById('firstName').value,
                lastName: document.getElementById('lastName').value,
                email: document.getElementById('regEmail').value,
                phone: document.getElementById('phone').value,
                dob: document.getElementById('dob').value,
                gender: document.getElementById('gender').value,
                password: pwInput.value,
                role: 'Patient'
            };

            if (alertBox) alertBox.style.display = 'none';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Creating account…';
            }

            try {
                await MediCoreAPI.Auth.register(payload);
                MediCoreUtils.showToast('Account created! Please log in.', 'success');
                setTimeout(() => { window.location.href = '/pages/login.html'; }, 1200);
            } catch (err) {
                if (alertBox && alertText) {
                    alertText.textContent = err.message || 'Something went wrong. Please check the form and try again.';
                    alertBox.style.display = 'flex';
                }
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = 'Create Account';
                }
            }
        });
    }

    /* --------------------------------------------------------------------
       Logout button (present in every dashboard sidebar)
       -------------------------------------------------------------------- */

    function initLogoutButton() {
        const btn = document.getElementById('logoutBtn');
        if (!btn) return;

        btn.addEventListener('click', async () => {
            try {
                await MediCoreAPI.Auth.logout();
            } catch (_) {
                // Even if the API call fails, clear the local session and leave.
            }
            clearSession();
            window.location.href = '/pages/login.html';
        });
    }

    /* --------------------------------------------------------------------
       Sidebar and Page user info hydration (replaces hardcoded data)
       -------------------------------------------------------------------- */

    function hydrateSidebarUser() {
        const user = getCurrentUser();
        if (!user) return;

        const userName = user.name || `${user.firstName || ''} ${user.lastName || ''}`.trim();
        const userRole = user.department || user.role;
        const userInitials = MediCoreUtils.getInitials(userName);

        // Update sidebar user name elements
        document.querySelectorAll('.u-name').forEach(el => {
            el.textContent = userName;
        });

        // Update sidebar user role / ID elements
        document.querySelectorAll('.u-role').forEach(el => {
            el.textContent = user.patientId ? `Patient ID: ${user.patientId}` : (userRole || '');
        });

        // Update avatars across the UI
        document.querySelectorAll('.avatar').forEach(el => {
            if (el.textContent.trim().length <= 2 && !el.querySelector('svg')) {
                el.textContent = userInitials;
            }
        });

        // Update topbar welcome message if present
        const welcomeTitle = document.querySelector('.topbar-title h1');
        if (welcomeTitle && user.firstName) {
            welcomeTitle.textContent = `Welcome back, ${user.firstName}`;
        }
    }

    /* --------------------------------------------------------------------
       Init on DOM ready
       -------------------------------------------------------------------- */

    document.addEventListener('DOMContentLoaded', () => {
        initLoginForm();
        initRegisterForm();
        initLogoutButton();
        hydrateSidebarUser();
    });

    /* --------------------------------------------------------------------
       Public API (exposed for guardPage() calls at the top of dashboard pages)
       -------------------------------------------------------------------- */

    window.MediCoreAuth = {
        getCurrentUser,
        isLoggedIn,
        setSession,
        clearSession,
        redirectToDashboard,
        guardPage
    };
})();