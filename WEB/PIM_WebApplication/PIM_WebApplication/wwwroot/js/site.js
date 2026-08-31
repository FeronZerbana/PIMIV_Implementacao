// =========================================
//  SHOPNOW — Scripts de Suporte Client-Side
// =========================================

function showToast(mensagem) {
    const toast = document.getElementById('toast');
    if (!toast) return;
    toast.textContent = mensagem;
    toast.classList.add('show');
    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// Auto-dismiss de mensagens de notificacao passadas via TempData
document.addEventListener('DOMContentLoaded', function () {
    const toastEl = document.getElementById('toast');
    if (toastEl && toastEl.getAttribute('data-msg')) {
        showToast(toastEl.getAttribute('data-msg'));
    }
});
