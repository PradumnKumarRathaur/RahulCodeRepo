// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showToast(type, message, time) {
    Swal.fire({
        toast: true,
        position: "top-right",
        showConfirmButton: false,
        timer: time ?? 2000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        },
        customClass: {
            popup: `swal2-toast-${type} custom-top-toast`
        },
        icon: type,
        title: message
    });
}
