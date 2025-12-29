(function () {
    const modalEl = document.getElementById('categoryAccountModal');
    const inputEl = document.getElementById('txtCategory');

    if (!modalEl || !inputEl) return;

    modalEl.addEventListener('categoryModal:select', function (e) {
        const name = (e && e.detail && e.detail.name) ? String(e.detail.name).trim() : '';

        if (!name) {
            alert('Please select a category first.');
            return;
        }

        inputEl.value = name;

        try {
            let bsModal = (window.bootstrap && window.bootstrap.Modal.getInstance(modalEl)) || null;
            if (!bsModal && window.bootstrap && window.bootstrap.Modal) {
                bsModal = new window.bootstrap.Modal(modalEl);
            }
            if (bsModal) {
                bsModal.hide();
            } else {
                modalEl.classList.remove('show');
                modalEl.setAttribute('aria-hidden', 'true');
                modalEl.style.display = 'none';
                document.body.classList.remove('modal-open');
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
            }
        } catch (_) {
            modalEl.classList.remove('show');
            modalEl.setAttribute('aria-hidden', 'true');
            modalEl.style.display = 'none';
            document.body.classList.remove('modal-open');
            document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
        }
    });
})();
