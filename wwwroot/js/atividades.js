// Tela "Minhas atividades" (Views/Atividades/Index.cshtml): filtros, quadro Kanban e "Mover para".
// Tudo por delegação de eventos no document, porque #painel-resumo e #area-atividades são
// substituídos após cada mudança de status (atualizarRegioes).
(function () {
    var NOMES_STATUS = {
        Pendente: 'Pendente',
        EmDesenvolvimento: 'Em Desenvolvimento',
        EmTeste: 'Em Teste',
        Concluido: 'Concluído'
    };

    // ---------- Filtros: selects enviam o formulário ao mudar ----------
    document.addEventListener('change', function (e) {
        if (e.target.matches('[data-filtros] [data-auto-envio]')) {
            e.target.form.requestSubmit();
        }
    });

    // ---------- Chamada ao servidor ----------
    function alterarStatus(id, status, token) {
        var dados = new FormData();
        dados.append('status', status);
        dados.append('__RequestVerificationToken', token);
        return fetch('/Atividades/AlterarStatus/' + encodeURIComponent(id), {
            method: 'POST',
            body: dados,
            headers: { 'X-Requested-With': 'fetch' },
            credentials: 'same-origin'
        }).then(function (resposta) {
            if (!resposta.ok) throw new Error('HTTP ' + resposta.status);
        });
    }

    // Recarrega o HTML da própria página (mesmos filtros) e troca só os indicadores e as atividades.
    function atualizarRegioes() {
        return fetch(location.href, { credentials: 'same-origin' })
            .then(function (r) { return r.text(); })
            .then(function (html) {
                var novo = new DOMParser().parseFromString(html, 'text/html');
                ['painel-resumo', 'area-atividades'].forEach(function (id) {
                    var atual = document.getElementById(id);
                    var substituto = novo.getElementById(id);
                    if (atual && substituto) atual.innerHTML = substituto.innerHTML;
                });
            });
    }

    function tokenDaPagina() {
        var campo = document.querySelector('#token-atividades input[name="__RequestVerificationToken"]');
        return campo ? campo.value : '';
    }

    function concluirMudanca(promessa, titulo, status) {
        return promessa
            .then(function () {
                window.mostrarToast('"' + titulo + '" movida para ' + NOMES_STATUS[status] + '.', 'sucesso');
            })
            .catch(function () {
                window.mostrarToast('Não foi possível mover a atividade. Tente novamente.', 'erro');
            })
            .then(atualizarRegioes);
    }

    // ---------- Menu "Mover para" (funciona sem JS como POST normal) ----------
    document.addEventListener('submit', function (e) {
        var form = e.target.closest('form[data-mover]');
        if (!form) return;
        e.preventDefault();

        var card = form.closest('.card-atividade');
        var status = form.querySelector('[name="status"]').value;
        var token = form.querySelector('[name="__RequestVerificationToken"]').value;
        concluirMudanca(alterarStatus(card.dataset.id, status, token), card.dataset.titulo, status);
    });

    // ---------- Arrastar e soltar no quadro ----------
    var arrastando = null;

    document.addEventListener('dragstart', function (e) {
        var card = e.target.closest && e.target.closest('.card-atividade[draggable="true"]');
        if (!card) return;
        arrastando = card;
        card.classList.add('arrastando');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', card.dataset.id);
        document.querySelector('.quadro').classList.add('arrastando-ativo');
    });

    document.addEventListener('dragend', function () {
        if (arrastando) arrastando.classList.remove('arrastando');
        arrastando = null;
        document.querySelectorAll('.coluna.alvo').forEach(function (c) { c.classList.remove('alvo'); });
        var quadro = document.querySelector('.quadro');
        if (quadro) quadro.classList.remove('arrastando-ativo');
    });

    document.addEventListener('dragover', function (e) {
        var zona = arrastando && e.target.closest('[data-soltar]');
        if (!zona) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        document.querySelectorAll('.coluna.alvo').forEach(function (c) {
            if (c !== zona.parentElement) c.classList.remove('alvo');
        });
        zona.parentElement.classList.add('alvo');
    });

    document.addEventListener('drop', function (e) {
        var zona = arrastando && e.target.closest('[data-soltar]');
        if (!zona) return;
        e.preventDefault();

        var card = arrastando;
        var novoStatus = zona.dataset.soltar;
        if (card.dataset.status === novoStatus) return;

        // Move já na tela; a resposta do servidor depois traz contagens e prazos corretos.
        zona.insertBefore(card, zona.querySelector('.coluna-vazia'));
        concluirMudanca(alterarStatus(card.dataset.id, novoStatus, tokenDaPagina()), card.dataset.titulo, novoStatus);
    });
})();
