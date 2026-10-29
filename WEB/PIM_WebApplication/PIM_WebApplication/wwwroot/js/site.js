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

    // Gerenciamento de Filtros Colapsaveis nas Telas de Catalogo
    const headersFiltro = document.querySelectorAll('.filtro-secao-header');
    headersFiltro.forEach(header => {
        header.setAttribute('role', 'button');
        header.setAttribute('tabindex', '0');
        header.setAttribute('aria-expanded', 'true');

        function alternarColapso() {
            const secao = header.closest('.filtro-secao');
            if (secao) {
                const foiColapsado = secao.classList.toggle('colapsado');
                header.setAttribute('aria-expanded', (!foiColapsado).toString());
            }
        }

        header.addEventListener('click', alternarColapso);

        header.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                alternarColapso();
            }
        });
    });
});
