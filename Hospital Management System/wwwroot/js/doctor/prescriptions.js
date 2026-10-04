/* ==========================================================================
   MediCore HMS — Doctor Write Prescription Page Script
   File: wwwroot/js/doctor/prescriptions.js
   Purpose: Page-specific behavior for pages/doctor/write-prescription.html —
   dynamically add/remove PRESCRIPTION_ITEMS rows, switch patient, and
   submit the prescription (creates PRESCRIPTION + PRESCRIPTION_ITEMS).
   Depends on: utils.js, api.js, auth.js (all loaded before this file)
   ========================================================================== */

(function () {
    'use strict';

    let rowCounter = 0;

    document.addEventListener('DOMContentLoaded', () => {
        // Uncomment once the backend issues real sessions:
        // MediCoreAuth.guardPage('Doctor');

        initMobileSidebarToggle();
        initAddRemoveRxItems();
        initPrescriptionForm();
        initPatientSwitch();
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
       Add / remove medicine rows in the prescription items table
       -------------------------------------------------------------------- */
    function initAddRemoveRxItems() {
        const addBtn = document.getElementById('addRxItemBtn');
        const tbody = document.getElementById('rxItemsBody');
        if (!addBtn || !tbody) return;

        const medicineOptions = [
            { value: 'MED-0001', label: 'Paracetamol 500mg' },
            { value: 'MED-0002', label: 'Amoxicillin 250mg' },
            { value: 'MED-0003', label: 'Cetirizine 10mg' },
            { value: 'MED-0004', label: 'Atorvastatin 20mg' },
            { value: 'MED-0005', label: 'Insulin Glargine' }
        ];

        function buildRow() {
            rowCounter++;
            const tr = document.createElement('tr');
            tr.className = 'rx-item-row';
            tr.dataset.rowId = rowCounter;

            const medOptionsHtml = medicineOptions
                .map((m) => `<option value="${m.value}">${m.label}</option>`)
                .join('');

            tr.innerHTML = `
        <td class="med-select-cell">
          <select class="input" name="medicine[]" required>
            <option value="">Select medicine</option>
            ${medOptionsHtml}
          </select>
        </td>
        <td class="qty-cell">
          <input class="input" type="text" name="dosage[]" placeholder="e.g. 1 tab" required />
        </td>
        <td class="freq-cell">
          <select class="input" name="frequency[]" required>
            <option value="">Frequency</option>
            <option value="Once daily">Once daily</option>
            <option value="Twice daily">Twice daily</option>
            <option value="Three times daily">Three times daily</option>
            <option value="Every 6 hours">Every 6 hours</option>
            <option value="As needed">As needed</option>
          </select>
        </td>
        <td class="dur-cell">
          <input class="input" type="text" name="duration[]" placeholder="e.g. 7 days" required />
        </td>
        <td class="remove-cell">
          <button type="button" class="icon-btn danger remove-rx-item-btn" aria-label="Remove">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
          </button>
        </td>
      `;
            return tr;
        }

        addBtn.addEventListener('click', () => {
            tbody.appendChild(buildRow());
        });

        tbody.addEventListener('click', (e) => {
            const removeBtn = e.target.closest('.remove-rx-item-btn');
            if (!removeBtn) return;

            const allRows = tbody.querySelectorAll('.rx-item-row');
            if (allRows.length <= 1) {
                MediCoreUtils.showToast('A prescription needs at least one medicine.', 'danger');
                return;
            }
            removeBtn.closest('.rx-item-row').remove();
        });
    }

    /* --------------------------------------------------------------------
       Patient selector (change-patient dropdown in the rx header)
       -------------------------------------------------------------------- */
    function initPatientSwitch() {
        const select = document.getElementById('patientSelect');
        if (!select) return;

        select.addEventListener('change', () => {
            const label = select.options[select.selectedIndex].text;
            MediCoreUtils.showToast(`Switched to patient: ${label}`, 'default', 2000);
            // In a live build, this would fetch the selected patient's
            // allergies/history and update the rx-header panel accordingly.
        });
    }

    /* --------------------------------------------------------------------
       Prescription form submission
       -------------------------------------------------------------------- */
    function initPrescriptionForm() {
        const form = document.getElementById('prescriptionForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            if (!MediCoreUtils.validateForm(form)) {
                MediCoreUtils.showToast('Please complete every medicine row before issuing.', 'danger');
                return;
            }

            const medicines = Array.from(form.querySelectorAll('select[name="medicine[]"]')).map((el) => el.value);
            const dosages = Array.from(form.querySelectorAll('input[name="dosage[]"]')).map((el) => el.value);
            const frequencies = Array.from(form.querySelectorAll('select[name="frequency[]"]')).map((el) => el.value);
            const durations = Array.from(form.querySelectorAll('input[name="duration[]"]')).map((el) => el.value);

            const items = medicines.map((medicineId, i) => ({
                medicineId,
                dosage: dosages[i],
                frequency: frequencies[i],
                duration: durations[i]
            }));

            const payload = {
                patientId: document.getElementById('patientSelect')?.value || null,
                notes: document.getElementById('rxNotes')?.value || '',
                items
            };

            const issueBtn = document.getElementById('issueRxBtn');
            issueBtn.disabled = true;
            issueBtn.textContent = 'Issuing…';

            try {
                await MediCoreAPI.Prescriptions.create(payload);
                MediCoreUtils.openModal('rxSuccessOverlay');
            } catch (err) {
                MediCoreUtils.showToast(err.message || 'Could not issue prescription.', 'danger');
            } finally {
                issueBtn.disabled = false;
                issueBtn.innerHTML = `
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg>
          Issue Prescription
        `;
            }
        });

        // "Write Another" button inside the success modal resets the form in place.
        const writeAnotherBtn = document.getElementById('writeAnotherBtn');
        writeAnotherBtn?.addEventListener('click', () => {
            form.reset();
            MediCoreUtils.closeModal('rxSuccessOverlay');
        });
    }
})();