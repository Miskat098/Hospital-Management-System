/* ==========================================================================
   MediCore HMS — Admin Medicine Inventory Page Script
   File: wwwroot/js/admin/inventory.js
   Purpose: Page-specific behavior for pages/admin/medicine-inventory.html —
   search/category/stock filters, add/edit medicine modal, delete confirm.
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let rowPendingDelete = null;
    let editingRow = null;

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Admin');

        initMobileSidebarToggle();
        initFilters();
        initStockChips();
        initAddEditModal();
        initDeleteModal();
        initRowActionButtons();
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
       Search / category filters
       -------------------------------------------------------------------- */
    function initFilters() {
        const searchInput = document.getElementById('medicineSearchInput');
        const categoryFilter = document.getElementById('categoryFilter');
        const rows = document.querySelectorAll('#medicineTableBody tr');

        function applyFilters() {
            const query = (searchInput?.value || '').toLowerCase().trim();
            const category = (categoryFilter?.value || '').toLowerCase();
            const activeChip = document.querySelector('.chip.is-active');
            const stockLevel = activeChip ? activeChip.dataset.stockFilter : 'all';

            rows.forEach((row) => {
                const text = row.textContent.toLowerCase();
                const matchesQuery = !query || text.includes(query);
                const matchesCategory = !category || text.includes(category);
                const matchesStock = stockLevel === 'all' || row.dataset.stock === stockLevel;
                row.style.display = (matchesQuery && matchesCategory && matchesStock) ? '' : 'none';
            });
        }

        if (searchInput) searchInput.addEventListener('input', MediCoreUtils.debounce(applyFilters, 250));
        if (categoryFilter) categoryFilter.addEventListener('change', applyFilters);

        // Expose so the chip handler below can trigger the same filter pass.
        initFilters._apply = applyFilters;
    }

    /* --------------------------------------------------------------------
       Stock level chips (All / Low Stock / Critical)
       -------------------------------------------------------------------- */
    function initStockChips() {
        const chips = document.querySelectorAll('.chip[data-stock-filter]');
        if (!chips.length) return;

        chips.forEach((chip) => {
            chip.addEventListener('click', () => {
                chips.forEach((c) => c.classList.remove('is-active'));
                chip.classList.add('is-active');
                if (typeof initFilters._apply === 'function') {
                    initFilters._apply();
                }
            });
        });
    }

    /* --------------------------------------------------------------------
       Add / Edit medicine modal
       -------------------------------------------------------------------- */
    function initAddEditModal() {
        const overlay = document.getElementById('medicineModalOverlay');
        const form = document.getElementById('medicineForm');
        const title = document.getElementById('medicineModalTitle');
        const addBtn = document.getElementById('addMedicineBtn');

        if (!overlay || !form) return;

        MediCoreUtils.initModalCloseHandlers('medicineModalOverlay', [
            'closeMedicineModalBtn',
            'cancelMedicineModalBtn'
        ]);

        addBtn?.addEventListener('click', () => {
            editingRow = null;
            title.textContent = 'Add Medicine';
            form.reset();
            MediCoreUtils.openModal('medicineModalOverlay');
        });

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (!MediCoreUtils.validateForm(form)) return;

            const payload = {
                name: document.getElementById('medName').value,
                category: document.getElementById('medCategory').value,
                form: document.getElementById('medForm').value,
                price: parseFloat(document.getElementById('medPrice').value),
                stock: parseInt(document.getElementById('medStock').value, 10),
                expiryDate: document.getElementById('medExpiry').value
            };

            const saveBtn = document.getElementById('saveMedicineBtn');
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving…';

            try {
                if (editingRow) {
                    await MediCoreAPI.Medicines.update(editingRow.dataset.medId || Date.now(), payload);
                    MediCoreUtils.showToast('Medicine updated successfully.', 'success');
                } else {
                    await MediCoreAPI.Medicines.create(payload);
                    MediCoreUtils.showToast('Medicine added to inventory.', 'success');
                }
                MediCoreUtils.closeModal('medicineModalOverlay');
                // In a live build, re-fetch and re-render the medicine table here.
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not save medicine.', 'danger');
            } finally {
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save Medicine';
            }
        });
    }

    /* --------------------------------------------------------------------
       Row action buttons (edit / delete)
       -------------------------------------------------------------------- */
    function initRowActionButtons() {
        const tbody = document.getElementById('medicineTableBody');
        if (!tbody) return;

        tbody.addEventListener('click', (e) => {
            const editBtn = e.target.closest('.edit-med-btn');
            const deleteBtn = e.target.closest('.delete-med-btn');

            if (editBtn) {
                openEditModalForRow(editBtn.closest('tr'));
            }

            if (deleteBtn) {
                rowPendingDelete = deleteBtn.closest('tr');
                MediCoreUtils.openModal('deleteMedConfirmOverlay');
            }
        });
    }

    function openEditModalForRow(row) {
        editingRow = row;
        document.getElementById('medicineModalTitle').textContent = 'Edit Medicine';
        document.getElementById('medName').value = row.querySelector('.cell-primary')?.textContent.trim() || '';
        document.getElementById('medPrice').value = (row.children[2]?.textContent || '').replace(/[^\d.]/g, '');
        document.getElementById('medStock').value = row.querySelector('.stock-num')?.textContent.trim() || '';
        MediCoreUtils.openModal('medicineModalOverlay');
    }

    /* --------------------------------------------------------------------
       Delete confirmation modal
       -------------------------------------------------------------------- */
    function initDeleteModal() {
        MediCoreUtils.initModalCloseHandlers('deleteMedConfirmOverlay', ['cancelDeleteMedBtn']);

        const confirmBtn = document.getElementById('confirmDeleteMedBtn');
        if (!confirmBtn) return;

        confirmBtn.addEventListener('click', async () => {
            if (!rowPendingDelete) {
                MediCoreUtils.closeModal('deleteMedConfirmOverlay');
                return;
            }

            confirmBtn.disabled = true;
            confirmBtn.textContent = 'Removing…';

            try {
                await MediCoreAPI.Medicines.remove(rowPendingDelete.dataset.medId || 'unknown');
                rowPendingDelete.remove();
                MediCoreUtils.showToast('Medicine removed from inventory.', 'success');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not remove medicine.', 'danger');
            } finally {
                confirmBtn.disabled = false;
                confirmBtn.textContent = 'Remove Medicine';
                rowPendingDelete = null;
                MediCoreUtils.closeModal('deleteMedConfirmOverlay');
            }
        });
    }
})();