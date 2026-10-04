/* ==========================================================================
   MediCore HMS — Admin Staff Management Page Script
   File: wwwroot/js/admin/staff.js
   Purpose: Page-specific behavior for pages/admin/staff-management.html —
   role tabs, search/department/status filters, add/edit account modal,
   delete confirmation modal, and select-all checkbox behavior.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let rowPendingDelete = null;
    let editingRow = null;

    document.addEventListener('DOMContentLoaded', () => {
        // MediCoreAuth.guardPage('Admin');

        loadStaffTable(); // সবার আগে API থেকে ডেটা এনে টেবিল লোড করবে
        initMobileSidebarToggle();
        initRoleTabs();
        initFilters();
        initSelectAll();
        initAddEditModal();
        initDeleteModal();
        initRowActionButtons();
    });

    /* --------------------------------------------------------------------
       Load Staff Table from API / Mock Data
       -------------------------------------------------------------------- */
    async function loadStaffTable() {
        try {
            const staffList = await MediCoreAPI.Staff.list();
            const tbody = document.getElementById('staffTableBody');
            if (!tbody) return;

            tbody.innerHTML = ''; // পুরোনো কোনো ডেটা থাকলে মুছে ফেলবে

            staffList.forEach((staff) => {
                const tr = document.createElement('tr');
                tr.dataset.staffId = staff.id;
                tr.dataset.role = (staff.role || 'Patient').toLowerCase();

                tr.innerHTML = `
                    <td><input type="checkbox" class="row-check"></td>
                    <td>
                        <div class="cell-primary">${staff.name || (staff.firstName + ' ' + staff.lastName)}</div>
                        <div class="cell-secondary">ID: ${staff.id}</div>
                    </td>
                    <td><span class="role-tag role-${(staff.role || '').toLowerCase()}">${staff.role || 'Staff'}</span></td>
                    <td class="cell-primary">${staff.email || ''}</td>
                    <td>${staff.department || 'General'}</td>
                    <td><span class="status-badge status-active">Active</span></td>
                    <td>
                        <div class="table-actions">
                            <button class="btn-icon edit-row-btn" title="Edit"><i class="fas fa-edit"></i></button>
                            <button class="btn-icon delete-row-btn text-danger" title="Delete"><i class="fas fa-trash"></i></button>
                        </div>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        } catch (err) {
            MediCoreUtils.showToast('Could not load staff members.', 'danger');
        }
    }

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
       Role tabs (All / Doctors / Patients / Staff)
       -------------------------------------------------------------------- */
    function initRoleTabs() {
        const tabs = document.querySelectorAll('#roleTabs .tab-btn');
        if (!tabs.length) return;

        tabs.forEach((tab) => {
            tab.addEventListener('click', () => {
                tabs.forEach((t) => t.classList.remove('is-active'));
                tab.classList.add('is-active');

                const role = tab.dataset.role;
                const rows = document.querySelectorAll('#staffTableBody tr');
                rows.forEach((row) => {
                    const matches = role === 'all' || row.dataset.role === role.toLowerCase();
                    row.style.display = matches ? '' : 'none';
                });
            });
        });
    }

    /* --------------------------------------------------------------------
       Search / department / status filters
       -------------------------------------------------------------------- */
    function initFilters() {
        const searchInput = document.getElementById('staffSearchInput');
        const deptFilter = document.getElementById('departmentFilter');
        const statusFilter = document.getElementById('statusFilter');

        function applyFilters() {
            const query = (searchInput?.value || '').toLowerCase().trim();
            const dept = deptFilter?.value || '';
            const status = statusFilter?.value || '';
            const rows = document.querySelectorAll('#staffTableBody tr');

            rows.forEach((row) => {
                const text = row.textContent.toLowerCase();
                const matchesQuery = !query || text.includes(query);
                const matchesDept = !dept || text.includes(dept.toLowerCase());
                const matchesStatus = !status || text.includes(status.toLowerCase());
                row.style.display = (matchesQuery && matchesDept && matchesStatus) ? '' : 'none';
            });
        }

        if (searchInput) searchInput.addEventListener('input', MediCoreUtils.debounce(applyFilters, 250));
        if (deptFilter) deptFilter.addEventListener('change', applyFilters);
        if (statusFilter) statusFilter.addEventListener('change', applyFilters);
    }

    /* --------------------------------------------------------------------
       Select-all checkbox
       -------------------------------------------------------------------- */
    function initSelectAll() {
        const selectAll = document.getElementById('selectAllRows');
        if (!selectAll) return;

        selectAll.addEventListener('change', () => {
            document.querySelectorAll('.row-check').forEach((cb) => {
                cb.checked = selectAll.checked;
            });
        });
    }

    /* --------------------------------------------------------------------
       Add / Edit account modal
       -------------------------------------------------------------------- */
    function initAddEditModal() {
        const overlay = document.getElementById('staffModalOverlay');
        const form = document.getElementById('staffForm');
        const title = document.getElementById('staffModalTitle');
        const addBtn = document.getElementById('addStaffBtn');
        const roleSelect = document.getElementById('staffRole');
        const deptField = document.getElementById('staffDepartmentField');

        if (!overlay || !form) return;

        MediCoreUtils.initModalCloseHandlers('staffModalOverlay', ['closeStaffModalBtn', 'cancelStaffModalBtn']);

        // Populate department select with real department Ids from the API
        (async function populateDepartmentSelect() {
            const sel = document.getElementById('staffDepartment');
            if (!sel) return;
            try {
                const depts = await MediCoreAPI.Departments.list();
                sel.innerHTML = '<option value="">Select department</option>';
                (depts || []).forEach(d => {
                    const opt = document.createElement('option');
                    // API returns { id, name } in mock and Departments controller uses Id/Name casing
                    opt.value = d.id ?? d.Id ?? d.ID ?? '';
                    opt.textContent = d.name ?? d.Name ?? String(opt.value);
                    sel.appendChild(opt);
                });
            } catch (e) {
                // If we can't load departments, keep the placeholder only.
                console.error('Could not load departments for staff form', e);
            }
        })();

        function toggleDeptField() {
            if (!roleSelect || !deptField) return;
            deptField.style.display = roleSelect.value === 'Doctor' ? '' : 'none';
        }
        if (roleSelect) {
            roleSelect.addEventListener('change', toggleDeptField);
        }

        addBtn?.addEventListener('click', () => {
            editingRow = null;
            title.textContent = 'Add New Account';
            form.reset();
            toggleDeptField();
            MediCoreUtils.openModal('staffModalOverlay');
        });

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                role: document.getElementById('staffRole').value,
                firstName: document.getElementById('staffFirstName').value,
                lastName: document.getElementById('staffLastName').value,
                email: document.getElementById('staffEmail').value,
                phone: document.getElementById('staffPhone').value,
                department: document.getElementById('staffDepartment').value,
                status: document.getElementById('staffStatus').value
            };

            const saveBtn = document.getElementById('saveStaffBtn');
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving…';

            try {
                if (editingRow) {
                    await MediCoreAPI.Staff.update(editingRow.dataset.staffId || Date.now(), payload);
                    MediCoreUtils.showToast('Account updated successfully.', 'success');
                } else {
                    await MediCoreAPI.Staff.create(payload);
                    MediCoreUtils.showToast('Account created successfully.', 'success');
                }
                MediCoreUtils.closeModal('staffModalOverlay');
                loadStaffTable(); // টেবিল রিফ্রেশ করবে
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not save account.', 'danger');
            } finally {
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save Account';
            }
        });
    }

    /* --------------------------------------------------------------------
       Row action buttons (edit / delete)
       -------------------------------------------------------------------- */
    function initRowActionButtons() {
        const tbody = document.getElementById('staffTableBody');
        if (!tbody) return;

        tbody.addEventListener('click', (e) => {
            const editBtn = e.target.closest('.edit-row-btn');
            const deleteBtn = e.target.closest('.delete-row-btn');

            if (editBtn) {
                const row = editBtn.closest('tr');
                openEditModalForRow(row);
            }

            if (deleteBtn) {
                rowPendingDelete = deleteBtn.closest('tr');
                MediCoreUtils.openModal('deleteConfirmOverlay');
            }
        });
    }

    function openEditModalForRow(row) {
        editingRow = row;
        const title = document.getElementById('staffModalTitle');
        const roleSelect = document.getElementById('staffRole');
        const deptField = document.getElementById('staffDepartmentField');

        const roleTag = row.querySelector('.role-tag');
        const role = roleTag ? roleTag.textContent.trim() : 'Patient';
        const nameParts = (row.querySelector('.cell-primary')?.textContent || '').trim().split(' ');

        title.textContent = 'Edit Account';
        roleSelect.value = role;
        document.getElementById('staffFirstName').value = nameParts[0] || '';
        document.getElementById('staffLastName').value = nameParts.slice(1).join(' ') || '';

        const emailCell = row.querySelectorAll('.cell-primary')[1];
        document.getElementById('staffEmail').value = emailCell ? emailCell.textContent.trim() : '';

        deptField.style.display = role === 'Doctor' ? '' : 'none';

        MediCoreUtils.openModal('staffModalOverlay');
    }

    /* --------------------------------------------------------------------
       Delete confirmation modal
       -------------------------------------------------------------------- */
    function initDeleteModal() {
        MediCoreUtils.initModalCloseHandlers('deleteConfirmOverlay', ['cancelDeleteBtn']);

        const confirmBtn = document.getElementById('confirmDeleteBtn');
        if (!confirmBtn) return;

        confirmBtn.addEventListener('click', async () => {
            if (!rowPendingDelete) {
                MediCoreUtils.closeModal('deleteConfirmOverlay');
                return;
            }

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Deleting…';

            try {
                await MediCoreAPI.Staff.remove(rowPendingDelete.dataset.staffId || 'unknown');
                rowPendingDelete.remove();
                MediCoreUtils.showToast('Account deleted.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not delete account.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Delete Account';
                rowPendingDelete = null;
                MediCoreUtils.closeModal('deleteConfirmOverlay');
            }
        });
    }
})();