/* ==========================================================================
   MediCore HMS — API Wrapper
   File: wwwroot/js/api.js
   Purpose: Centralize all HTTP calls to the future ASP.NET Core Web API.
   During the frontend-only preview stage (no backend yet), MOCK_MODE is
   true and every call resolves against small in-memory fixtures instead
   of hitting the network, so every page works end-to-end before the API
   exists. Once the Web API is live, set MOCK_MODE to false and set
   BASE_URL to the API's root — no other code in this file should need
   to change, because callers only see the same resolved shape either way.
   Depends on: utils.js (loaded before this file)
   ========================================================================== */

const MediCoreAPI = (function () {
    'use strict';

    /* --------------------------------------------------------------------
       Configuration
       -------------------------------------------------------------------- */

    // Toggle this to false once the ASP.NET Core Web API is deployed.
    const MOCK_MODE = false;

    // Root of the future Web API, e.g. "https://api.medicorehospital.com/api"
    const BASE_URL = 'https://localhost:7049/api';

    // Simulated network latency for mock mode, so loading states are visible.
    const MOCK_LATENCY_MS = 350;

    /* --------------------------------------------------------------------
       Low-level request helper (used once MOCK_MODE is false)
       -------------------------------------------------------------------- */

    /**
     * Perform an authenticated HTTP request against the real Web API.
     * @param {string} path - e.g. "/appointments"
     * @param {object} options - { method, body, headers }
     * @returns {Promise<any>}
     */
    async function request(path, options = {}) {
        const token = MediCoreUtils.storageGet('authToken', null);

        const headers = {
            'Content-Type': 'application/json',
            ...(options.headers || {})
        };
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(BASE_URL + path, {
            method: options.method || 'GET',
            headers,
            body: options.body ? JSON.stringify(options.body) : undefined
        });

        if (response.status === 401) {
            // Session expired — clear local session and bounce to login.
            MediCoreUtils.storageRemove('authToken');
            MediCoreUtils.storageRemove('currentUser');
            window.location.href = '/pages/login.html';
            throw new Error('Unauthorized');
        }

        let data = null;
        try {
            data = await response.json();
        } catch (_) {
            // No JSON body (e.g. 204 No Content) — that's fine.
        }

        if (!response.ok) {
            const message = (data && data.message) || `Request failed (${response.status})`;
            throw new Error(message);
        }

        return data;
    }

    /**
     * Resolve a value after a short simulated delay, for mock mode.
     * @param {any} value
     * @returns {Promise<any>}
     */
    function mockResolve(value) {
        return new Promise((resolve) => {
            setTimeout(() => resolve(value), MOCK_LATENCY_MS);
        });
    }

    /* --------------------------------------------------------------------
       Mock fixtures (frontend-only preview stage)
       -------------------------------------------------------------------- */

    const MOCK_USERS = [
        { id: 'DOC-1042', role: 'Doctor', email: 'demo@medicore.com', password: 'demo123', name: 'Dr. Nasrin Karim', department: 'Cardiology' },
        { id: 'PAT-7710', role: 'Patient', email: 'patient@medicore.com', password: 'demo123', name: 'Rafiq Ahmed' },
        { id: 'ADM-0001', role: 'Admin', email: 'admin@medicore.com', password: 'demo123', name: 'Ayesha Khatun' }
    ];

    const MOCK_DEPARTMENTS = [
        { id: 1, name: 'Cardiology', doctors: 18, patients: 312, status: 'Active' },
        { id: 2, name: 'Orthopedics', doctors: 14, patients: 228, status: 'Active' },
        { id: 3, name: 'Pediatrics', doctors: 11, patients: 406, status: 'Active' },
        { id: 4, name: 'Neurology', doctors: 9, patients: 187, status: 'Active' }
    ];

    const MOCK_MEDICINES = [
        { id: 'MED-0001', name: 'Paracetamol 500mg', category: 'Analgesic', price: 2.5, stock: 42 },
        { id: 'MED-0002', name: 'Amoxicillin 250mg', category: 'Antibiotic', price: 6.0, stock: 312 },
        { id: 'MED-0003', name: 'Cetirizine 10mg', category: 'Antihistamine', price: 3.2, stock: 118 },
        { id: 'MED-0004', name: 'Atorvastatin 20mg', category: 'Cardiac', price: 9.75, stock: 256 },
        { id: 'MED-0005', name: 'Insulin Glargine', category: 'Endocrine', price: 420.0, stock: 9 }
    ];

    /* --------------------------------------------------------------------
       Public endpoint groups
       -------------------------------------------------------------------- */

    const Auth = {
        login(email, password, role) {
            if (MOCK_MODE) {
                const user = MOCK_USERS.find(
                    (u) => u.email.toLowerCase() === String(email).toLowerCase() &&
                        u.password === password &&
                        u.role === role
                );
                if (!user) {
                    return mockResolve(Promise.reject(new Error('Invalid email or password.')))
                        .then(() => { throw new Error('Invalid email or password.'); })
                        .catch((err) => { throw err; });
                }
                return mockResolve({ token: 'mock-token-' + user.id, user });
            }
            return request('/auth/login', { method: 'POST', body: { email, password, role } });
        },

        register(payload) {
            if (MOCK_MODE) {
                return mockResolve({ id: 'PAT-' + Math.floor(1000 + Math.random() * 9000), ...payload });
            }
            return request('/auth/register', { method: 'POST', body: payload });
        },

        logout() {
            if (MOCK_MODE) {
                return mockResolve({ success: true });
            }
            return request('/auth/logout', { method: 'POST' });
        }
    };

    const Departments = {
        list() {
            return MOCK_MODE ? mockResolve(MOCK_DEPARTMENTS) : request('/departments');
        },
        create(payload) {
            return MOCK_MODE ? mockResolve({ id: Date.now(), ...payload }) : request('/departments', { method: 'POST', body: payload });
        },
        update(id, payload) {
            return MOCK_MODE ? mockResolve({ id, ...payload }) : request(`/departments/${id}`, { method: 'PUT', body: payload });
        },
        remove(id) {
            return MOCK_MODE ? mockResolve({ success: true }) : request(`/departments/${id}`, { method: 'DELETE' });
        }
    };

    const Medicines = {
        list() {
            return MOCK_MODE ? mockResolve(MOCK_MEDICINES) : request('/medicines');
        },
        create(payload) {
            return MOCK_MODE ? mockResolve({ id: 'MED-' + Date.now(), ...payload }) : request('/medicines', { method: 'POST', body: payload });
        },
        update(id, payload) {
            return MOCK_MODE ? mockResolve({ id, ...payload }) : request(`/medicines/${id}`, { method: 'PUT', body: payload });
        },
        remove(id) {
            return MOCK_MODE ? mockResolve({ success: true }) : request(`/medicines/${id}`, { method: 'DELETE' });
        }
    };

    const Staff = {
        list() {
            return MOCK_MODE ? mockResolve(MOCK_USERS) : request('/staff');
        },
        create(payload) {
            return MOCK_MODE ? mockResolve({ id: Date.now(), ...payload }) : request('/staff', { method: 'POST', body: payload });
        },
        update(id, payload) {
            return MOCK_MODE ? mockResolve({ id, ...payload }) : request(`/staff/${id}`, { method: 'PUT', body: payload });
        },
        remove(id) {
            return MOCK_MODE ? mockResolve({ success: true }) : request(`/staff/${id}`, { method: 'DELETE' });
        }
    };

    const Appointments = {
        list(params = {}) {
            return MOCK_MODE ? mockResolve([]) : request('/appointments?' + new URLSearchParams(params));
        },
        book(payload) {
            return MOCK_MODE ? mockResolve({ id: 'APT-' + Date.now(), ...payload }) : request('/appointments', { method: 'POST', body: payload });
        },
        reschedule(id, payload) {
            return MOCK_MODE ? mockResolve({ id, ...payload }) : request(`/appointments/${id}/reschedule`, { method: 'PUT', body: payload });
        },
        cancel(id) {
            return MOCK_MODE ? mockResolve({ success: true }) : request(`/appointments/${id}/cancel`, { method: 'POST' });
        }
    };

    const Prescriptions = {
        list(patientId) {
            return MOCK_MODE ? mockResolve([]) : request(`/prescriptions?patientId=${patientId}`);
        },
        create(payload) {
            return MOCK_MODE ? mockResolve({ id: 'RX-' + Date.now(), ...payload }) : request('/prescriptions', { method: 'POST', body: payload });
        }
    };

    const Billing = {
        listInvoices(patientId) {
            return MOCK_MODE ? mockResolve([]) : request(`/billing/invoices?patientId=${patientId}`);
        },
        pay(invoiceId, payload) {
            return MOCK_MODE ? mockResolve({ success: true, invoiceId }) : request(`/billing/invoices/${invoiceId}/pay`, { method: 'POST', body: payload });
        }
    };

    const Feedback = {
        submit(payload) {
            return MOCK_MODE ? mockResolve({ id: Date.now(), ...payload }) : request('/feedback', { method: 'POST', body: payload });
        }
    };

    /* --------------------------------------------------------------------
       Public API
       -------------------------------------------------------------------- */

    return {
        MOCK_MODE,
        request,
        Auth,
        Departments,
        Medicines,
        Staff,
        Appointments,
        Prescriptions,
        Billing,
        Feedback
    };
})();