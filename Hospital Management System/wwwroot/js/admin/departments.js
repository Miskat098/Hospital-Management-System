/* ==========================================================================
   MediCore HMS — Admin Department Management Page Script
   File: wwwroot/js/admin/departments.js
   Purpose: Page-specific behavior for pages/admin/department-management.html —
   add/edit department modal and delete confirmation modal.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let cardPendingDelete = null;
    let editingCard = null;

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Admin');

        initMobileSidebarToggle();
        initAddEditModal();
        initDeleteModal();
        initCardActionButtons();
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
       Add / Edit department modal
       -------------------------------------------------------------------- */
    function initAddEditModal() {
        const overlay = document.getElementById('departmentModalOverlay');
        const form = document.getElementById('departmentForm');
        const title = document.getElementById('departmentModalTitle');
        const addBtn = document.getElementById('addDepartmentBtn');

        if (!overlay || !form) return;

        MediCoreUtils.initModalCloseHandlers('departmentModalOverlay', [
            'closeDepartmentModalBtn',
            'cancelDepartmentModalBtn'
        ]);

        addBtn?.addEventListener('click', () => {
            editingCard = null;
            title.textContent = 'Add Department';
            form.reset();
            MediCoreUtils.openModal('departmentModalOverlay');
        });

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                name: document.getElementById('deptName').value,
                description: document.getElementById('deptDescription').value,
                headDoctorId: document.getElementById('deptHead').value,
                status: document.getElementById('deptStatus').value
            };

            const saveBtn = document.getElementById('saveDepartmentBtn');
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving…';

            try {
                if (editingCard) {
                    await MediCoreAPI.Departments.update(editingCard.dataset.deptId, payload);
                    MediCoreUtils.showToast('Department updated successfully.', 'success');
                } else {
                    await MediCoreAPI.Departments.create(payload);
                    MediCoreUtils.showToast('Department created successfully.', 'success');
                }
                MediCoreUtils.closeModal('departmentModalOverlay');
                // In a live build, re-fetch and re-render the department grid here.
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not save department.', 'danger');
            } finally {
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save Department';
            }
        });
    }

    /* --------------------------------------------------------------------
       Card action buttons (edit / delete) — wired via event delegation
       -------------------------------------------------------------------- */
    function initCardActionButtons() {
        const grid = document.getElementById('departmentGrid');
        if (!grid) return;

        grid.addEventListener('click', (e) => {
            const editBtn = e.target.closest('.edit-dept-btn');
            const deleteBtn = e.target.closest('.delete-dept-btn');

            if (editBtn) {
                openEditModalForCard(editBtn.closest('.department-card'));
            }

            if (deleteBtn) {
                cardPendingDelete = deleteBtn.closest('.department-card');
                MediCoreUtils.openModal('deleteDeptConfirmOverlay');
            }
        });
    }

    function openEditModalForCard(card) {
        editingCard = card;
        const title = document.getElementById('departmentModalTitle');
        title.textContent = 'Edit Department';

        document.getElementById('deptName').value = card.querySelector('h4')?.textContent.trim() || '';
        document.getElementById('deptDescription').value = card.querySelector('p')?.textContent.trim() || '';

        const statusBadge = card.querySelector('.badge');
        document.getElementById('deptStatus').value = statusBadge ? statusBadge.textContent.trim() : 'Active';

        MediCoreUtils.openModal('departmentModalOverlay');
    }

    /* --------------------------------------------------------------------
       Delete confirmation modal
       -------------------------------------------------------------------- */
    function initDeleteModal() {
        MediCoreUtils.initModalCloseHandlers('deleteDeptConfirmOverlay', ['cancelDeleteDeptBtn']);

        const confirmBtn = document.getElementById('confirmDeleteDeptBtn');
        if (!confirmBtn) return;

        confirmBtn.addEventListener('click', async () => {
            if (!cardPendingDelete) {
                MediCoreUtils.closeModal('deleteDeptConfirmOverlay');
                return;
            }

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Deleting…';

            try {
                await MediCoreAPI.Departments.remove(cardPendingDelete.dataset.deptId);
                cardPendingDelete.remove();
                MediCoreUtils.showToast('Department deleted.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not delete department.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Delete Department';
                cardPendingDelete = null;
                MediCoreUtils.closeModal('deleteDeptConfirmOverlay');
            }
        });
    }
})();