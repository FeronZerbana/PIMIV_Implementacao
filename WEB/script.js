// =============================================================================
//  SHOPNOW — Branch de Controle (controle/web)
//  Versao 100% Estatica do Site Legado (Sem Requisicoes ou Dependencias de API)
// =============================================================================

// ── CATALOGO PADRAO INICIAL (MOCK LOCAL) ──
const PRODUTOS_PADRAO = [
  {
    idProduto: 1,
    tipoRoupa: 'Camiseta Clean Code Dev',
    preco: 79.90,
    tamanho: 'M',
    estoque: { idEstoque: 1, codProduto: 1, quantidadeDisponivel: 25 }
  },
  {
    idProduto: 2,
    tipoRoupa: 'Moletom Algoritmos & Café',
    preco: 189.90,
    tamanho: 'G',
    estoque: { idEstoque: 2, codProduto: 2, quantidadeDisponivel: 15 }
  },
  {
    idProduto: 3,
    tipoRoupa: 'Camiseta Linux Kernel',
    preco: 69.90,
    tamanho: 'GG',
    estoque: { idEstoque: 3, codProduto: 3, quantidadeDisponivel: 30 }
  },
  {
    idProduto: 4,
    tipoRoupa: 'Camiseta Git Commit & Push',
    preco: 64.90,
    tamanho: 'P',
    estoque: { idEstoque: 4, codProduto: 4, quantidadeDisponivel: 20 }
  },
  {
    idProduto: 5,
    tipoRoupa: 'Moletom Cyber Security Hoodie',
    preco: 219.90,
    tamanho: 'M',
    estoque: { idEstoque: 5, codProduto: 5, quantidadeDisponivel: 12 }
  },
  {
    idProduto: 6,
    tipoRoupa: 'Camiseta Artificial Intelligence',
    preco: 84.90,
    tamanho: 'G',
    estoque: { idEstoque: 6, codProduto: 6, quantidadeDisponivel: 18 }
  },
  {
    idProduto: 7,
    tipoRoupa: 'Camiseta Fullstack Developer',
    preco: 74.90,
    tamanho: 'M',
    estoque: { idEstoque: 7, codProduto: 7, quantidadeDisponivel: 22 }
  },
  {
    idProduto: 8,
    tipoRoupa: 'Moletom Python Dark Mode',
    preco: 199.90,
    tamanho: 'GG',
    estoque: { idEstoque: 8, codProduto: 8, quantidadeDisponivel: 10 }
  },
  {
    idProduto: 9,
    tipoRoupa: 'Camiseta Database SQL Hero',
    preco: 69.90,
    tamanho: 'G',
    estoque: { idEstoque: 9, codProduto: 9, quantidadeDisponivel: 16 }
  }
];

// ── ESTADO GLOBAL DA APLICACAO ──
let cartItems = []; // Array de itens: { idProduto, nome, preco, emoji, desc, qty, tamanho }
let usuarioLogado = false;
let descontoAtual = 0;
let usuarioId = null;
let usuarioNome = null;
let isAdmin = false;
let produtosDisponiveis = [];

// ── PERSISTENCIA LOCAL DE PRODUTOS ──
function obterProdutosLocais() {
  const dados = localStorage.getItem('shopnow_produtos_controle');
  if (dados) {
    try {
      const parsed = JSON.parse(dados);
      if (Array.isArray(parsed) && parsed.length > 0) {
        return parsed;
      }
    } catch (e) {
      console.warn('Erro ao ler produtos do localStorage, restaurando catalogo padrao:', e);
    }
  }
  salvarProdutosLocais(PRODUTOS_PADRAO);
  return JSON.parse(JSON.stringify(PRODUTOS_PADRAO));
}

function salvarProdutosLocais(produtos) {
  localStorage.setItem('shopnow_produtos_controle', JSON.stringify(produtos));
}

// ── NAVEGACAO ENTRE TELAS (SPA) ──
function goTo(pageId) {
  document.querySelectorAll('.page').forEach(function(page) {
    page.classList.remove('active');
  });

  const target = document.getElementById(pageId);
  if (target) {
    target.classList.add('active');
  } else {
    console.warn('Pagina destino nao encontrada:', pageId);
  }
  window.scrollTo(0, 0);
}

// ── NOTIFICACOES (TOAST) ──
function showToast(mensagem) {
  const toast = document.getElementById('toast');
  if (!toast) return;
  toast.textContent = mensagem;
  toast.classList.add('show');
  setTimeout(function() {
    toast.classList.remove('show');
  }, 2800);
}

// ── GESTAO DE SESSAO ESTÁTICA ──
function restaurarSessao() {
  const usuarioArmazenado = localStorage.getItem('usuarioId');

  if (usuarioArmazenado) {
    usuarioId = parseInt(usuarioArmazenado, 10);
    usuarioNome = localStorage.getItem('usuarioNome') || 'Usuario';
    isAdmin = localStorage.getItem('isAdmin') === 'true';
    usuarioLogado = true;
    atualizarVisibilidadeAdmin();
  } else {
    // Por padrao no modo estatico, o usuario inicia diretamente na Home
    usuarioLogado = false;
    usuarioId = null;
    usuarioNome = null;
    isAdmin = false;
    atualizarVisibilidadeAdmin();
  }

  // Abre diretamente na vitrine da loja
  goTo('page-home');
}

// ── CONTROLE DO BOTAO ADMIN NA NAVBAR ──
function atualizarVisibilidadeAdmin() {
  const botoesAdmin = document.querySelectorAll('a[onclick*="irParaAdmin"], a[onclick*="page-admin"]');
  botoesAdmin.forEach(function(btn) {
    if (isAdmin) {
      btn.style.display = 'inline';
      btn.style.visibility = 'visible';
    } else {
      btn.style.display = 'none';
      btn.style.visibility = 'hidden';
    }
  });
}

// ── LOGIN ESTÁTICO (SEM API) ──
function fazerLogin() {
  const emailInput = document.getElementById('login-email');
  const passInput = document.getElementById('login-pass');
  const email = emailInput ? emailInput.value.trim() : '';
  const senha = passInput ? passInput.value : '';

  if (!email || !senha) {
    showToast('Preencha todos os campos!');
    return;
  }

  if (!email.includes('@')) {
    showToast('Informe um e-mail valido!');
    return;
  }

  // Determinacao de perfil com base no e-mail
  const ehAdmin = email.toLowerCase().includes('admin');
  const nomeExibicao = ehAdmin ? 'Administrador' : email.split('@')[0];
  const idGerado = Math.floor(Math.random() * 1000) + 1;

  localStorage.setItem('usuarioId', idGerado);
  localStorage.setItem('usuarioNome', nomeExibicao);
  localStorage.setItem('isAdmin', ehAdmin ? 'true' : 'false');

  usuarioId = idGerado;
  usuarioNome = nomeExibicao;
  isAdmin = ehAdmin;
  usuarioLogado = true;

  atualizarVisibilidadeAdmin();
  showToast(ehAdmin ? 'Login de Administrador realizado!' : 'Login realizado com sucesso!');

  setTimeout(function() {
    goTo('page-home');
  }, 600);
}

// ── CADASTRO ESTÁTICO ──
function fazerCadastro() {
  const nomeInput = document.getElementById('reg-nome');
  const emailInput = document.getElementById('reg-email');
  const passInput = document.getElementById('reg-pass');
  const pass2Input = document.getElementById('reg-pass2');

  const nome = nomeInput ? nomeInput.value.trim() : '';
  const email = emailInput ? emailInput.value.trim() : '';
  const senha = passInput ? passInput.value : '';
  const senha2 = pass2Input ? pass2Input.value : '';

  if (!nome || !email || !senha || !senha2) {
    showToast('Preencha todos os campos!');
    return;
  }

  if (!email.includes('@')) {
    showToast('Informe um e-mail valido!');
    return;
  }

  if (senha.length < 6) {
    showToast('A senha deve conter ao menos 6 caracteres.');
    return;
  }

  if (senha !== senha2) {
    showToast('As senhas nao coincidem!');
    return;
  }

  showToast('Conta criada com sucesso! Faca login.');

  if (nomeInput) nomeInput.value = '';
  if (emailInput) emailInput.value = '';
  if (passInput) passInput.value = '';
  if (pass2Input) pass2Input.value = '';

  setTimeout(function() {
    goTo('page-login');
  }, 900);
}

// ── LOGOUT ──
function sair() {
  localStorage.removeItem('usuarioId');
  localStorage.removeItem('usuarioNome');
  localStorage.removeItem('isAdmin');

  usuarioLogado = false;
  usuarioId = null;
  usuarioNome = null;
  isAdmin = false;
  cartItems = [];
  descontoAtual = 0;

  atualizarBadge();
  atualizarVisibilidadeAdmin();
  showToast('Sessao encerrada. Ate breve!');

  setTimeout(function() {
    goTo('page-home');
  }, 500);
}

// ── ACESSO AO PAINEL ADMIN ──
function irParaAdmin() {
  if (!usuarioLogado) {
    // Para facilitar testes na branch de controle, orienta ou autentica
    showToast('Dica: Acesse como admin@shopnow.com para visualizar o painel admin.');
    goTo('page-login');
    return;
  }

  if (!isAdmin) {
    showToast('Acesso restrito a administradores. Faca login com e-mail admin.');
    return;
  }

  goTo('page-admin');
  setTimeout(function() {
    carregarProdutosAdmin();
  }, 100);
}

// ── CARREGAMENTO E RENDERIZACAO DE PRODUTOS ──
function carregarProdutos() {
  produtosDisponiveis = obterProdutosLocais();
  renderizarProdutosDestaque();
  renderizarOfertas();
}

function mapearEmojiProduto(tipoRoupa) {
  const lower = tipoRoupa.toLowerCase();
  if (lower.includes('moletom') || lower.includes('hoodie')) return '🧥';
  if (lower.includes('programação') || lower.includes('dev') || lower.includes('code')) return '💻';
  if (lower.includes('linux')) return '🐧';
  if (lower.includes('ia') || lower.includes('tech') || lower.includes('robot')) return '🤖';
  if (lower.includes('game')) return '🎮';
  return '👕';
}

function renderizarProdutosDestaque() {
  const container = document.getElementById('produtos-destaques');
  if (!container) return;

  container.innerHTML = '';

  produtosDisponiveis.forEach(function(produto) {
    const { idProduto, tipoRoupa, preco, tamanho, estoque } = produto;
    const qtdDisponivel = estoque ? estoque.quantidadeDisponivel : 0;
    const temEstoque = qtdDisponivel > 0;
    const emoji = mapearEmojiProduto(tipoRoupa);

    const card = document.createElement('div');
    card.className = 'product-card';
    card.setAttribute('data-id', idProduto);
    card.setAttribute('data-nome', tipoRoupa);
    card.setAttribute('data-preco', preco);
    card.setAttribute('data-emoji', emoji);
    card.setAttribute('data-desc', `Tamanho: ${tamanho} - Estoque: ${qtdDisponivel}`);
    card.setAttribute('data-tamanho', tamanho);

    card.innerHTML = `
      <div class="product-img">${emoji}</div>
      <div class="product-info">
        <div class="product-name">${tipoRoupa}</div>
        <div class="product-desc">Tamanho: ${tamanho} - Estoque: ${qtdDisponivel} un</div>
        <div class="product-footer">
          <span class="product-price">R$ ${preco.toFixed(2).replace('.', ',')}</span>
          <button class="add-btn" onclick="addCart(this)" ${!temEstoque ? 'disabled style="opacity:0.5;cursor:not-allowed"' : ''}>
            ${temEstoque ? '+ Adicionar' : 'Fora de estoque'}
          </button>
        </div>
      </div>
    `;

    container.appendChild(card);
  });
}

function renderizarOfertas() {
  const container = document.getElementById('produtos-ofertas');
  if (!container) return;

  container.innerHTML = '';

  produtosDisponiveis.forEach(function(produto) {
    const { idProduto, tipoRoupa, preco, tamanho, estoque } = produto;
    const qtdDisponivel = estoque ? estoque.quantidadeDisponivel : 0;
    const temEstoque = qtdDisponivel > 0;
    const emoji = mapearEmojiProduto(tipoRoupa);

    const card = document.createElement('div');
    card.className = 'product-card';
    card.setAttribute('data-id', idProduto);
    card.setAttribute('data-nome', tipoRoupa);
    card.setAttribute('data-preco', preco);
    card.setAttribute('data-emoji', emoji);
    card.setAttribute('data-desc', `Tamanho: ${tamanho} - Estoque: ${qtdDisponivel}`);
    card.setAttribute('data-tamanho', tamanho);

    card.innerHTML = `
      <div class="product-img">${emoji}</div>
      <div class="product-info">
        <div class="product-name">${tipoRoupa}</div>
        <div class="product-desc">Tamanho: ${tamanho} - Estoque: ${qtdDisponivel} un</div>
        <div class="product-footer">
          <span class="product-price">R$ ${preco.toFixed(2).replace('.', ',')}</span>
          <button class="add-btn" onclick="addCart(this)" ${!temEstoque ? 'disabled style="opacity:0.5;cursor:not-allowed"' : ''}>
            ${temEstoque ? '+ Adicionar' : 'Fora de estoque'}
          </button>
        </div>
      </div>
    `;

    container.appendChild(card);
  });
}

// ── ADICIONAR AO CARRINHO ──
function addCart(btn) {
  const card = btn.closest('.product-card');
  if (!card) return;

  const idProduto = parseInt(card.getAttribute('data-id'), 10);
  const nome = card.getAttribute('data-nome');
  const preco = parseFloat(card.getAttribute('data-preco'));
  const emoji = card.getAttribute('data-emoji') || '👕';
  const desc = card.getAttribute('data-desc');
  const tamanho = card.getAttribute('data-tamanho') || 'M';

  const produtoLocal = produtosDisponiveis.find(function(p) {
    return p.idProduto === idProduto;
  });

  const qtdDisponivel = (produtoLocal && produtoLocal.estoque) ? produtoLocal.estoque.quantidadeDisponivel : 0;

  if (qtdDisponivel <= 0) {
    showToast('Produto esgotado no estoque!');
    return;
  }

  const existente = cartItems.find(function(i) {
    return i.idProduto === idProduto;
  });

  if (existente) {
    if (existente.qty >= qtdDisponivel) {
      showToast('Limite de estoque atingido para este item!');
      return;
    }
    existente.qty++;
  } else {
    cartItems.push({
      idProduto: idProduto,
      nome: nome,
      preco: preco,
      emoji: emoji,
      desc: desc,
      qty: 1,
      tamanho: tamanho
    });
  }

  atualizarBadge();
  showToast('Produto adicionado ao carrinho!');
}

function atualizarBadge() {
  const total = cartItems.reduce(function(acc, item) {
    return acc + item.qty;
  }, 0);

  const badge = document.getElementById('cart-count');
  if (badge) {
    badge.textContent = total;
  }
}

function irParaCarrinho() {
  goTo('page-cart');
  renderizarCarrinho();
}

// ── RENDERIZACAO DO CARRINHO ──
function renderizarCarrinho() {
  const lista = document.getElementById('cart-items-list');
  const empty = document.getElementById('cart-empty');
  const couponArea = document.getElementById('cart-coupon-area');

  if (!lista) return;
  lista.innerHTML = '';

  if (cartItems.length === 0) {
    if (empty) empty.style.display = 'flex';
    if (couponArea) couponArea.style.display = 'none';
  } else {
    if (empty) empty.style.display = 'none';
    if (couponArea) couponArea.style.display = 'flex';

    cartItems.forEach(function(item, index) {
      const subtotal = item.preco * item.qty;
      const div = document.createElement('div');
      div.className = 'cart-item';

      div.innerHTML = `
        <div class="cart-item-produto">
          <div class="cart-item-thumb">${item.emoji}</div>
          <div class="cart-item-info">
            <div class="cart-item-nome">${item.nome}</div>
            <div class="cart-item-desc">${item.desc}</div>
            <button class="cart-item-remover" onclick="removerItem(${index})">
              Remover
            </button>
          </div>
        </div>
        <div class="cart-item-preco">
          R$ ${item.preco.toFixed(2).replace('.', ',')}
        </div>
        <div class="cart-item-qty">
          <button class="qty-btn" onclick="alterarQty(${index}, -1)">−</button>
          <span class="qty-num">${item.qty}</span>
          <button class="qty-btn" onclick="alterarQty(${index}, 1)">+</button>
        </div>
        <div class="cart-item-subtotal">
          R$ ${subtotal.toFixed(2).replace('.', ',')}
        </div>
      `;

      lista.appendChild(div);
    });
  }

  atualizarResumo();
}

function alterarQty(index, delta) {
  if (!cartItems[index]) return;

  const item = cartItems[index];
  const prod = produtosDisponiveis.find(p => p.idProduto === item.idProduto);
  const maxEstoque = prod && prod.estoque ? prod.estoque.quantidadeDisponivel : 999;

  item.qty += delta;

  if (item.qty > maxEstoque) {
    item.qty = maxEstoque;
    showToast('Quantidade maxima disponivel em estoque atingida.');
  }

  if (item.qty <= 0) {
    cartItems.splice(index, 1);
  }

  atualizarBadge();
  renderizarCarrinho();
}

function removerItem(index) {
  if (cartItems[index]) {
    cartItems.splice(index, 1);
    atualizarBadge();
    renderizarCarrinho();
    showToast('Item removido do carrinho.');
  }
}

function atualizarResumo() {
  const subtotal = cartItems.reduce(function(sum, i) {
    return sum + (i.preco * i.qty);
  }, 0);

  const totalQtd = cartItems.reduce(function(sum, i) {
    return sum + i.qty;
  }, 0);

  let frete = (subtotal >= 99 || subtotal === 0) ? 0 : 20.00;
  let desconto = descontoAtual;

  if (desconto > subtotal) {
    desconto = subtotal;
  }

  const total = Math.max(0, subtotal - desconto + frete);

  const elSubtotal = document.getElementById('subtotal');
  const elDesconto = document.getElementById('desconto');
  const elFrete = document.getElementById('frete');
  const elTotal = document.getElementById('total');
  const elParcelas = document.getElementById('parcelamento');

  if (elSubtotal) elSubtotal.textContent = `R$ ${subtotal.toFixed(2).replace('.', ',')}`;
  if (elDesconto) elDesconto.textContent = `– R$ ${desconto.toFixed(2).replace('.', ',')}`;
  if (elFrete) elFrete.textContent = frete === 0 ? 'Grátis' : `R$ ${frete.toFixed(2).replace('.', ',')}`;
  if (elTotal) elTotal.textContent = `R$ ${total.toFixed(2).replace('.', ',')}`;

  if (elParcelas) {
    if (total > 0) {
      const valorParcela = (total / 4).toFixed(2).replace('.', ',');
      elParcelas.textContent = `em até 4x de R$ ${valorParcela} sem juros`;
    } else {
      elParcelas.textContent = '';
    }
  }
}

// ── CUPONS DE DESCONTO ──
function aplicarCupom() {
  const input = document.getElementById('cupom-input');
  const codigo = input ? input.value.trim().toUpperCase() : '';

  if (!codigo) {
    showToast('Digite um código de cupom!');
    return;
  }

  const subtotal = cartItems.reduce((s, i) => s + (i.preco * i.qty), 0);

  if (codigo === 'FRETE22') {
    descontoAtual = 20.00;
    showToast('Cupom FRETE22 aplicado: Frete Grátis garantido!');
  } else if (codigo === 'DEV10') {
    descontoAtual = subtotal * 0.10;
    showToast(`Cupom DEV10 aplicado: R$ ${descontoAtual.toFixed(2).replace('.', ',')} de desconto!`);
  } else if (codigo === 'SHOPNOW15') {
    descontoAtual = subtotal * 0.15;
    showToast(`Cupom SHOPNOW15 aplicado: R$ ${descontoAtual.toFixed(2).replace('.', ',')} de desconto!`);
  } else if (codigo === 'PROMO50') {
    descontoAtual = subtotal * 0.50;
    showToast(`Cupom PROMO50 aplicado: 50% de desconto!`);
  } else {
    descontoAtual = 0;
    showToast('Cupom inválido ou expirado.');
  }

  atualizarResumo();
}

// ── FINALIZACAO DE COMPRA ESTÁTICA ──
function finalizarCompra() {
  if (cartItems.length === 0) {
    showToast('Seu carrinho está vazio.');
    return;
  }

  // Abate de estoque local
  cartItems.forEach(function(item) {
    const prod = produtosDisponiveis.find(p => p.idProduto === item.idProduto);
    if (prod && prod.estoque) {
      prod.estoque.quantidadeDisponivel = Math.max(0, prod.estoque.quantidadeDisponivel - item.qty);
    }
  });

  salvarProdutosLocais(produtosDisponiveis);

  const numPedido = Math.floor(1000 + Math.random() * 9000);
  cartItems = [];
  descontoAtual = 0;

  atualizarBadge();
  renderizarCarrinho();

  showToast(`Compra finalizada com sucesso! Pedido #${numPedido}`);

  setTimeout(function() {
    carregarProdutos();
    goTo('page-home');
  }, 1400);
}

// ── PAINEL DE ADMINISTRACAO ESTÁTICO ──
function carregarProdutosAdmin() {
  produtosDisponiveis = obterProdutosLocais();

  const select = document.getElementById('admin-select-produto');
  const tabela = document.getElementById('admin-produtos-lista');

  if (select) {
    select.innerHTML = '<option value="" disabled selected>Selecione um produto</option>';
    produtosDisponiveis.forEach(function(p) {
      const opt = document.createElement('option');
      opt.value = p.idProduto;
      opt.textContent = `${p.tipoRoupa} (Tam: ${p.tamanho}) — Estoque: ${p.estoque.quantidadeDisponivel} un`;
      select.appendChild(opt);
    });
  }

  if (tabela) {
    tabela.innerHTML = '';
    produtosDisponiveis.forEach(function(p) {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td style="padding:0.7rem; border-bottom:1px solid #333;">${p.idProduto}</td>
        <td style="padding:0.7rem; border-bottom:1px solid #333;"><strong>${p.tipoRoupa}</strong></td>
        <td style="padding:0.7rem; border-bottom:1px solid #333;">${p.tamanho}</td>
        <td style="padding:0.7rem; border-bottom:1px solid #333;">R$ ${p.preco.toFixed(2).replace('.', ',')}</td>
        <td style="padding:0.7rem; border-bottom:1px solid #333;">${p.estoque.quantidadeDisponivel} un</td>
      `;
      tabela.appendChild(row);
    });
  }
}

function cadastrarProduto() {
  const nomeInput = document.getElementById('admin-nome-produto');
  const precoInput = document.getElementById('admin-preco-produto');
  const tamanhoInput = document.getElementById('admin-tamanho-produto');

  const nome = nomeInput ? nomeInput.value.trim() : '';
  const precoStr = precoInput ? precoInput.value.trim().replace(',', '.') : '';
  const tamanho = tamanhoInput ? tamanhoInput.value : '';

  if (!nome || !precoStr || !tamanho) {
    showToast('Preencha todos os campos do produto!');
    return;
  }

  const preco = parseFloat(precoStr);
  if (isNaN(preco) || preco <= 0) {
    showToast('Informe um preço válido.');
    return;
  }

  const maxId = produtosDisponiveis.reduce((max, p) => p.idProduto > max ? p.idProduto : max, 0);
  const novoId = maxId + 1;

  const novoProduto = {
    idProduto: novoId,
    tipoRoupa: nome,
    preco: preco,
    tamanho: tamanho,
    estoque: {
      idEstoque: novoId,
      codProduto: novoId,
      quantidadeDisponivel: 10 // Saldo inicial padrão
    }
  };

  produtosDisponiveis.push(novoProduto);
  salvarProdutosLocais(produtosDisponiveis);

  if (nomeInput) nomeInput.value = '';
  if (precoInput) precoInput.value = '';
  if (tamanhoInput) tamanhoInput.value = '';

  showToast(`Produto '${nome}' cadastrado com sucesso!`);

  carregarProdutosAdmin();
  carregarProdutos();
}

function adicionarEstoque() {
  const select = document.getElementById('admin-select-produto');
  const qtdInput = document.getElementById('admin-quantidade-estoque');

  const idProduto = select ? parseInt(select.value, 10) : 0;
  const quantidade = qtdInput ? parseInt(qtdInput.value, 10) : 0;

  if (!idProduto || isNaN(idProduto) || !quantidade || quantidade <= 0) {
    showToast('Selecione um produto e informe uma quantidade válida.');
    return;
  }

  const produto = produtosDisponiveis.find(p => p.idProduto === idProduto);
  if (produto && produto.estoque) {
    produto.estoque.quantidadeDisponivel += quantidade;
    salvarProdutosLocais(produtosDisponiveis);

    if (qtdInput) qtdInput.value = '';
    if (select) select.value = '';

    showToast(`Estoque atualizado: +${quantidade} unidades adicionadas!`);

    carregarProdutosAdmin();
    carregarProdutos();
  } else {
    showToast('Produto não encontrado.');
  }
}

// ── INICIALIZACAO AUTOMATICA NO CARREGAMENTO DA PAGINA ──
document.addEventListener('DOMContentLoaded', function() {
  restaurarSessao();
  carregarProdutos();
  console.log('ShopNow Controle Web: Modo estático inicializado com sucesso.');
});
