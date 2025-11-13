document.addEventListener('DOMContentLoaded', function() {
    const registerForm = document.getElementById('registerForm');
    const nameInput = document.getElementById('name');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const termsCheckbox = document.getElementById('terms');
    const submitBtn = registerForm.querySelector('.btn');

    // Создаем элементы для сообщений об ошибках
    createErrorElements();

    // Обработчик отправки формы
    registerForm.addEventListener('submit', function(e) {
        e.preventDefault();
        
        if (validateForm()) {
            submitForm();
        }
    });

    // Валидация в реальном времени
    nameInput.addEventListener('blur', validateName);
    emailInput.addEventListener('blur', validateEmail);
    passwordInput.addEventListener('input', validatePassword);
    confirmPasswordInput.addEventListener('blur', validateConfirmPassword);
    termsCheckbox.addEventListener('change', validateTerms);

    // Обработчики для социальных кнопок
    document.querySelectorAll('.social-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const provider = this.textContent.trim();
            socialRegister(provider);
        });
    });

    function createErrorElements() {
        const formGroups = registerForm.querySelectorAll('.form-group');
        
        formGroups.forEach(group => {
            const errorElement = document.createElement('div');
            errorElement.className = 'error-message';
            group.appendChild(errorElement);
        });

        // Добавляем индикатор силы пароля
        const passwordGroup = passwordInput.closest('.form-group');
        const strengthIndicator = document.createElement('div');
        strengthIndicator.className = 'password-strength';
        strengthIndicator.innerHTML = '<div class="strength-bar"></div>';
        passwordGroup.appendChild(strengthIndicator);
    }

    function validateName() {
        const name = nameInput.value.trim();
        const nameError = nameInput.closest('.form-group').querySelector('.error-message');
        const nameGroup = nameInput.closest('.form-group');
        
        if (!name) {
            showError(nameGroup, nameError, 'Имя обязательно для заполнения');
            return false;
        }
        
        if (name.length < 2) {
            showError(nameGroup, nameError, 'Имя должно содержать минимум 2 символа');
            return false;
        }
        
        showSuccess(nameGroup, nameError);
        return true;
    }

    function validateEmail() {
        const email = emailInput.value.trim();
        const emailError = emailInput.closest('.form-group').querySelector('.error-message');
        const emailGroup = emailInput.closest('.form-group');
        
        if (!email) {
            showError(emailGroup, emailError, 'Email обязателен для заполнения');
            return false;
        }
        
        if (!isValidEmail(email)) {
            showError(emailGroup, emailError, 'Введите корректный email адрес');
            return false;
        }
        
        showSuccess(emailGroup, emailError);
        return true;
    }

    function validatePassword() {
        const password = passwordInput.value;
        const passwordError = passwordInput.closest('.form-group').querySelector('.error-message');
        const passwordGroup = passwordInput.closest('.form-group');
        const strengthBar = passwordGroup.querySelector('.strength-bar');
        
        // Сбрасываем классы
        strengthBar.className = 'strength-bar';
        
        if (!password) {
            showError(passwordGroup, passwordError, 'Пароль обязателен для заполнения');
            updatePasswordRequirements([]);
            return false;
        }
        
        const requirements = checkPasswordRequirements(password);
        const validRequirements = requirements.filter(req => req.valid);
        
        updatePasswordRequirements(requirements);
        
        if (validRequirements.length < 3) {
            showError(passwordGroup, passwordError, 'Пароль слишком слабый');
            updateStrengthIndicator(validRequirements.length);
            return false;
        }
        
        if (password.length < 8) {
            showError(passwordGroup, passwordError, 'Пароль должен содержать минимум 8 символов');
            return false;
        }
        
        showSuccess(passwordGroup, passwordError);
        updateStrengthIndicator(validRequirements.length);
        return true;
    }

    function validateConfirmPassword() {
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        const confirmError = confirmPasswordInput.closest('.form-group').querySelector('.error-message');
        const confirmGroup = confirmPasswordInput.closest('.form-group');
        
        if (!confirmPassword) {
            showError(confirmGroup, confirmError, 'Подтверждение пароля обязательно');
            return false;
        }
        
        if (password !== confirmPassword) {
            showError(confirmGroup, confirmError, 'Пароли не совпадают');
            return false;
        }
        
        showSuccess(confirmGroup, confirmError);
        return true;
    }

    function validateTerms() {
        const termsError = document.createElement('div');
        termsError.className = 'error-message';
        termsError.textContent = 'Необходимо принять условия использования';
        
        const termsGroup = termsCheckbox.closest('.checkbox-group');
        const existingError = termsGroup.querySelector('.error-message');
        
        if (existingError) {
            existingError.remove();
        }
        
        if (!termsCheckbox.checked) {
            termsGroup.appendChild(termsError);
            return false;
        }
        
        return true;
    }

    function checkPasswordRequirements(password) {
        return [
            {
                type: 'length',
                valid: password.length >= 8,
                message: 'Минимум 8 символов'
            },
            {
                type: 'uppercase',
                valid: /[A-Z]/.test(password),
                message: 'Заглавная буква'
            },
            {
                type: 'number',
                valid: /[0-9]/.test(password),
                message: 'Цифра'
            },
            {
                type: 'special',
                valid: /[!@#$%^&*(),.?":{}|<>]/.test(password),
                message: 'Специальный символ'
            }
        ];
    }

    function updatePasswordRequirements(requirements) {
        const requirementElements = document.querySelectorAll('.requirement');
        
        requirementElements.forEach(element => {
            const requirementType = element.dataset.requirement;
            const requirement = requirements.find(req => req.type === requirementType);
            
            if (requirement) {
                element.classList.toggle('valid', requirement.valid);
                element.classList.toggle('invalid', !requirement.valid);
            }
        });
    }

    function updateStrengthIndicator(strength) {
        const strengthBar = passwordInput.closest('.form-group').querySelector('.strength-bar');
        const classes = ['strength-weak', 'strength-medium', 'strength-strong', 'strength-very-strong'];
        
        strengthBar.className = 'strength-bar';
        
        if (strength > 0) {
            strengthBar.classList.add(classes[Math.min(strength - 1, 3)]);
        }
    }

    function validateForm() {
        const isNameValid = validateName();
        const isEmailValid = validateEmail();
        const isPasswordValid = validatePassword();
        const isConfirmValid = validateConfirmPassword();
        const isTermsValid = validateTerms();
        
        return isNameValid && isEmailValid && isPasswordValid && isConfirmValid && isTermsValid;
    }

    function showError(inputGroup, errorElement, message) {
        inputGroup.classList.add('error');
        inputGroup.classList.remove('success');
        errorElement.textContent = message;
        errorElement.classList.add('show');
    }

    function showSuccess(inputGroup, errorElement) {
        inputGroup.classList.add('success');
        inputGroup.classList.remove('error');
        errorElement.classList.remove('show');
    }

    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function submitForm() {
        const formData = {
            name: nameInput.value.trim(),
            email: emailInput.value.trim(),
            password: passwordInput.value,
            role: document.querySelector('input[name="role"]:checked').value,
            newsletter: document.getElementById('newsletter').checked
        };
        
        // Показываем состояние загрузки
        submitBtn.classList.add('loading');
        submitBtn.disabled = true;
        
        // Имитация запроса к серверу
        setTimeout(() => {
            console.log('Отправка данных:', formData);
            
            // Сбрасываем состояние загрузки
            submitBtn.classList.remove('loading');
            submitBtn.disabled = false;
            
            // Временное сообщение об успехе
            alert('Регистрация успешна! На ваш email отправлено письмо с подтверждением.');
            
            // В реальном приложении:
            // window.location.href = '/email-verification';
            
        }, 2000);
    }

    function socialRegister(provider) {
        // Показываем уведомление
        alert(`Регистрация через ${provider} - функционал в разработке`);
        
        // В реальном приложении здесь будет OAuth авторизация
        // window.location.href = `/auth/${provider.toLowerCase()}`;
    }

    // Дополнительные улучшения UX
    nameInput.addEventListener('input', function() {
        if (this.value.trim()) {
            this.closest('.form-group').classList.remove('error');
        }
    });

    emailInput.addEventListener('input', function() {
        if (this.value.trim()) {
            this.closest('.form-group').classList.remove('error');
        }
    });

    confirmPasswordInput.addEventListener('input', function() {
        if (this.value) {
            this.closest('.form-group').classList.remove('error');
        }
    });

    // Автофокус на поле имени при загрузке
    nameInput.focus();
});