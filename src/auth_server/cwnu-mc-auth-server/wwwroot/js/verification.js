/*
*   cwnu-mc-auth-server
*   Copyright (C) 2024 Coppermine-SP
*/

(() => {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    const forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault()
                event.stopPropagation()
            }

            form.classList.add('was-validated')
        }, false)
    })
})()

document.addEventListener("DOMContentLoaded", function (event) {
    document.getElementById("student-id").addEventListener('input', (e) =>
    {
        e.target.value = e.target.value.replace(/[^0-9]/, '');
    });
});