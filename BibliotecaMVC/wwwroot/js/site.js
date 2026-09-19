// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('submit', function (event) {
    const form = event.target;

    if (!(form instanceof HTMLFormElement)) {
        return;
    }

    let alertOptions;

    if (form.classList.contains('swal-delete-form')) {
        alertOptions = {
            title: '¿Eliminar libro?',
            text: 'Esta acción no se puede deshacer.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        };
    } else if (form.classList.contains('swal-save-form')) {
        alertOptions = {
            title: '¿Guardar cambios?',
            text: 'Se actualizará la información del libro.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, guardar',
            cancelButtonText: 'Cancelar'
        };
    } else {
        return;
    }

    event.preventDefault();

    Swal.fire(alertOptions).then(function (result) {
        if (result.isConfirmed) {
            HTMLFormElement.prototype.submit.call(form);
        }
    });
});
