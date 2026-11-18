// Comportamentos globais. Carregado em todas as páginas depois do bootstrap.bundle.

(function () {
    // ---------- Tema claro/escuro ----------
    // O tema inicial é aplicado por um script inline no <head> do _Layout (evita piscar).
    var botaoTema = document.getElementById('alternar-tema');

    function atualizarIconeTema() {
        if (!botaoTema) return;
        var escuro = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        var icone = botaoTema.querySelector('i');
        icone.className = escuro ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
        botaoTema.setAttribute('aria-pressed', escuro ? 'true' : 'false');
    }

    if (botaoTema) {
        botaoTema.addEventListener('click', function () {
            var novo = document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-bs-theme', novo);
            localStorage.setItem('tema', novo);
            atualizarIconeTema();
        });
        atualizarIconeTema();
    }

    // ---------- Toasts (TempData, ver _Messages.cshtml) ----------
    document.querySelectorAll('.toast').forEach(function (el) {
        bootstrap.Toast.getOrCreateInstance(el, { delay: 5000 }).show();
    });

    // Toast criado pelo JavaScript (ex.: após mover uma atividade no quadro). tipo: 'sucesso' | 'erro'.
    window.mostrarToast = function (texto, tipo) {
        var container = document.querySelector('.toasts-app');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed end-0 p-3 toasts-app';
            document.body.appendChild(container);
        }
        var erro = tipo === 'erro';
        var toast = document.createElement('div');
        toast.className = 'toast align-items-center border-0 ' + (erro ? 'text-bg-danger' : 'text-bg-success');
        toast.setAttribute('role', erro ? 'alert' : 'status');
        toast.setAttribute('aria-live', erro ? 'assertive' : 'polite');
        toast.innerHTML = '<div class="d-flex"><div class="toast-body"><i class="fa-solid ' +
            (erro ? 'fa-circle-exclamation' : 'fa-circle-check') + ' me-2" aria-hidden="true"></i><span></span></div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fechar"></button></div>';
        toast.querySelector('.toast-body span').textContent = texto;
        container.appendChild(toast);
        toast.addEventListener('hidden.bs.toast', function () { toast.remove(); });
        bootstrap.Toast.getOrCreateInstance(toast, { delay: 4000 }).show();
    };

    // ---------- Modal de confirmação de exclusão (_ConfirmarExclusao.cshtml) ----------
    // O botão que abre o modal informa a URL (data-acao) e o título da atividade (data-titulo).
    var modalExclusao = document.getElementById('confirmar-exclusao');
    if (modalExclusao) {
        modalExclusao.addEventListener('show.bs.modal', function (evento) {
            var botao = evento.relatedTarget;
            if (!botao) return;
            modalExclusao.querySelector('form').action = botao.getAttribute('data-acao');
            modalExclusao.querySelector('[data-titulo-atividade]').textContent = botao.getAttribute('data-titulo');
        });
    }

    // ---------- Painéis de login / cadastro / recuperação de senha (Home) ----------
    // Botões com data-painel="login|register|forgot" alternam os painéis [data-painel-conteudo].
    var auth = document.querySelector('[data-auth]');
    if (auth) {
        var paineis = auth.querySelectorAll('[data-painel-conteudo]');
        var abas = auth.querySelectorAll('[role="tab"][data-painel]');

        function mostrarPainel(nome, focar) {
            paineis.forEach(function (p) {
                p.hidden = p.getAttribute('data-painel-conteudo') !== nome;
            });
            abas.forEach(function (aba) {
                var ativa = aba.getAttribute('data-painel') === nome;
                aba.classList.toggle('active', ativa);
                aba.setAttribute('aria-selected', ativa ? 'true' : 'false');
                aba.tabIndex = ativa ? 0 : -1;
            });
            if (focar) {
                var primeiro = auth.querySelector('[data-painel-conteudo="' + nome + '"] input:not([type=hidden])');
                if (primeiro) primeiro.focus();
            }
        }

        auth.querySelectorAll('[data-painel]').forEach(function (botao) {
            botao.addEventListener('click', function () {
                mostrarPainel(botao.getAttribute('data-painel'), true);
            });
        });

        mostrarPainel(auth.getAttribute('data-auth') || 'login', false);
    }
})();
