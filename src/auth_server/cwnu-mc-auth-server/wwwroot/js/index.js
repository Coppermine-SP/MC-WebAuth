/*
*   cwnu-mc-auth-server
*   Copyright (C) 2024 Coppermine-SP
*/

var hiddenForm;
var authCodeInput;
var authCodeContainer;

function ConfigureInputBox() {
    const inputs = authCodeContainer.querySelectorAll('input[type="text"]');
    inputs.forEach((input, index) => {
        input.addEventListener('input', (e) => {
            // 입력된 값이 숫자나 대문자가 아니라면 제거
            input.value = input.value.toUpperCase().replace(/[^A-Z0-9]/, '');

            // 다음 input 필드로 포커스 이동
            if (input.value && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }

            // 모든 필드가 채워졌는지 확인
            if (Array.from(inputs).every(i => i.value)) {
                const code = Array.from(inputs).map(i => i.value).join('');
                authCodeInput.value = code;
                hiddenForm.submit();
            }
        });

        // 백스페이스 키 입력 감지 및 이전 입력 필드로 포커스 이동
        input.addEventListener('keydown', (e) => {
            if (e.key === 'Backspace' && input.value === '' && index > 0) {
                inputs[index - 1].focus();
            }
        });
    });
}

function Init() {
    hiddenForm = document.getElementById("auth-hidden-form");
    authCodeInput = document.getElementById("auth-hidden-form-authcode");
    authCodeContainer = document.getElementById("authcode-container");
    ConfigureInputBox();
}

document.addEventListener("DOMContentLoaded", function(event) {
    Init();
});