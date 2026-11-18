// Editor de descrição (Quill) do formulário de atividade (Views/Atividades/_AtividadeForm.cshtml).
// O HTML inicial vem do campo oculto #Descricao (já sanitizado no servidor) e volta para ele no submit.
(function () {
    var form = document.getElementById('atividade-form');
    var campo = document.getElementById('Descricao');
    if (!form || !campo || typeof Quill === 'undefined') return;

    var quill = new Quill('#editor', {
        theme: 'snow',
        placeholder: 'Descreva sua atividade...',
        modules: {
            toolbar: [
                [{ 'header': [3, 4, false] }],
                ['bold', 'italic', 'underline'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                ['link'],
                ['clean']
            ]
        }
    });

    if (campo.value) {
        quill.clipboard.dangerouslyPasteHTML(campo.value);
    }

    form.addEventListener('submit', function () {
        campo.value = quill.getText().trim().length === 0 ? '' : quill.root.innerHTML;
    });
})();
