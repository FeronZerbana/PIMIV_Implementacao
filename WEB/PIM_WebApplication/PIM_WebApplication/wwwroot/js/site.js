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

// -----------------------------------------------------
//  ORDENACAO DE PRODUTOS (CLIENT-SIDE E URL SYNC)
// -----------------------------------------------------
function tratarAlteracaoOrdenacao(criterio) {
    const grid = document.querySelector('.categoria-produtos-grid');
    if (!grid) return;

    const cards = Array.from(grid.querySelectorAll('.instrumento-card'));
    if (!cards || cards.length === 0) return;

    // Garantir índice original para restauração do modo "populares"
    cards.forEach((card, idx) => {
        if (!card.hasAttribute('data-ordem-original')) {
            card.setAttribute('data-ordem-original', idx.toString());
        }
    });

    if (criterio === 'menor-preco') {
        cards.sort((a, b) => {
            const precoA = parseFloat(a.getAttribute('data-preco-raw') || '0');
            const precoB = parseFloat(b.getAttribute('data-preco-raw') || '0');
            return precoA - precoB;
        });
    } else if (criterio === 'maior-preco') {
        cards.sort((a, b) => {
            const precoA = parseFloat(a.getAttribute('data-preco-raw') || '0');
            const precoB = parseFloat(b.getAttribute('data-preco-raw') || '0');
            return precoB - precoA;
        });
    } else {
        cards.sort((a, b) => {
            const idxA = parseInt(a.getAttribute('data-ordem-original') || '0', 10);
            const idxB = parseInt(b.getAttribute('data-ordem-original') || '0', 10);
            return idxA - idxB;
        });
    }

    // Readicionar os cards na nova ordem
    cards.forEach(card => grid.appendChild(card));

    // Atualizar URL sem recarregar a pagina
    try {
        const url = new URL(window.location.href);
        if (criterio && criterio !== 'populares') {
            url.searchParams.set('ordem', criterio);
        } else {
            url.searchParams.delete('ordem');
        }
        window.history.replaceState({}, '', url.toString());
    } catch (e) {
        // Fallback silencioso para navegadores mais antigos
    }
}

// -----------------------------------------------------
//  MODAL DE DETALHES DO PRODUTO
// -----------------------------------------------------
function abrirModalDetalhesPorCard(card, event) {
    if (event) {
        // Ignora clique se originado de formulario ou botao de compra direta
        if (event.target.closest('form') || event.target.closest('button')) {
            return;
        }
    }

    const modal = document.getElementById('modalDetalhesProduto');
    if (!modal || !card) return;

    const id = card.getAttribute('data-id') || '';
    const nome = card.getAttribute('data-nome') || '';
    const categoria = card.getAttribute('data-categoria') || '';
    const preco = card.getAttribute('data-preco') || '';
    const precoRaw = card.getAttribute('data-preco-raw') || '';
    const descricao = card.getAttribute('data-descricao') || '';
    const estoque = parseInt(card.getAttribute('data-estoque') || '0', 10);
    const imagem = card.getAttribute('data-imagem') || '';

    // Preenchimento dos campos visuais
    const imgEl = document.getElementById('modalProdutoImg');
    if (imgEl) {
        imgEl.src = imagem;
        imgEl.alt = nome;
    }

    const catEl = document.getElementById('modalProdutoCategoria');
    if (catEl) catEl.textContent = categoria;

    const nomeEl = document.getElementById('modalProdutoNome');
    if (nomeEl) nomeEl.textContent = nome;

    const precoEl = document.getElementById('modalProdutoPreco');
    if (precoEl) precoEl.textContent = 'R$ ' + preco;

    const descEl = document.getElementById('modalProdutoDescricao');
    if (descEl) descEl.textContent = descricao;

    const estoqueBadge = document.getElementById('modalProdutoEstoqueBadge');
    const btnComprar = document.getElementById('modalBtnAdicionarCarrinho');
    const btnTexto = document.getElementById('modalBtnTexto');

    if (estoque > 0) {
        if (estoqueBadge) {
            estoqueBadge.textContent = 'Em estoque (' + estoque + ' un disponíveis)';
            estoqueBadge.className = 'modal-estoque-badge';
        }
        if (btnComprar) {
            btnComprar.disabled = false;
            btnComprar.style.opacity = '1';
            btnComprar.style.cursor = 'pointer';
        }
        if (btnTexto) btnTexto.textContent = 'Adicionar ao carrinho';
    } else {
        if (estoqueBadge) {
            estoqueBadge.textContent = 'Produto fora de estoque';
            estoqueBadge.className = 'modal-estoque-badge esgotado';
        }
        if (btnComprar) {
            btnComprar.disabled = true;
            btnComprar.style.opacity = '0.5';
            btnComprar.style.cursor = 'not-allowed';
        }
        if (btnTexto) btnTexto.textContent = 'Fora de estoque';
    }

    // Preenchimento dos campos ocultos para submissao ao carrinho
    const inputId = document.getElementById('modalInputIdProduto');
    if (inputId) inputId.value = id;

    const inputNome = document.getElementById('modalInputNome');
    if (inputNome) inputNome.value = categoria;

    const inputPreco = document.getElementById('modalInputPreco');
    if (inputPreco) inputPreco.value = precoRaw;

    const inputModelo = document.getElementById('modalInputModelo');
    if (inputModelo) inputModelo.value = nome;

    const inputTamanho = document.getElementById('modalInputTamanho');
    if (inputTamanho) inputTamanho.value = nome;

    const inputImagem = document.getElementById('modalInputImagemUrl');
    if (inputImagem) inputImagem.value = imagem;

    modal.style.display = 'flex';
    document.body.style.overflow = 'hidden';
}

function fecharModalDetalhes() {
    const modal = document.getElementById('modalDetalhesProduto');
    if (!modal) return;
    modal.style.display = 'none';
    document.body.style.overflow = '';
}

function tratarCliqueBackdropModal(event) {
    if (event.target && event.target.id === 'modalDetalhesProduto') {
        fecharModalDetalhes();
    }
}

// Fechamento de modais com a tecla ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        fecharModalDetalhes();
    }
});

// Auto-dismiss de mensagens de notificacao passadas via TempData e inicializacoes
document.addEventListener('DOMContentLoaded', function () {
    const toastEl = document.getElementById('toast');
    if (toastEl && toastEl.getAttribute('data-msg')) {
        showToast(toastEl.getAttribute('data-msg'));
    }

    // Inicializacao da ordenacao caso exista parametro na URL
    const selectOrdenar = document.getElementById('ordenarPor');
    if (selectOrdenar) {
        const urlParams = new URLSearchParams(window.location.search);
        const ordemParam = urlParams.get('ordem');
        if (ordemParam && (ordemParam === 'menor-preco' || ordemParam === 'maior-preco')) {
            selectOrdenar.value = ordemParam;
            tratarAlteracaoOrdenacao(ordemParam);
        }
    }
});
