document.getElementById('loginForm').addEventListener('submit', function(e) {
    e.preventDefault();
    
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    
    // Здесь будет логика авторизации
    console.log('Вход:', { email, password });
    
    // Временное уведомление
    alert('Функция входа в разработке');
});

// Добавляем обработчики для социальных кнопок
document.querySelectorAll('.social-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        const provider = this.textContent;
        alert(`Вход через ${provider} в разработке`);
    });
});