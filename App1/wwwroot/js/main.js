document.addEventListener('DOMContentLoaded', function() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    document.querySelectorAll('.feature-card, .lesson').forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });

    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

// ========== СИСТЕМА АВТОРИЗАЦИИ ==========

function getToken() {
    return localStorage.getItem('authToken') || localStorage.getItem('token');
}

function removeTokens() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('token');
}

function showUserInfo(token) {
    try {
        // Пытаемся декодировать токен
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const payload = JSON.parse(decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join('')));

        // Пробуем разные варианты получения имени
        let userName = 'Пользователь';

        if (payload.FirstName && payload.LastName) {
            userName = `${payload.FirstName} ${payload.LastName}`;
        } else if (payload.name) {
            userName = payload.name;
        } else if (payload.unique_name) {
            userName = payload.unique_name;
        } else if (payload.email) {
            userName = payload.email.split('@')[0];
        }

        updateAuthUI(userName);
    } catch (e) {
        console.log('Ошибка декодирования токена:', e);
        // Если не получается, просто показываем "Пользователь"
        updateAuthUI('Пользователь');
    }
}

function updateAuthUI(userName) {
    const userInfo = document.getElementById('userInfo');
    const authButtons = document.querySelector('.auth-buttons');

    if (userInfo && authButtons) {
        document.getElementById('userName').textContent = userName;
        userInfo.style.display = 'block';

        // Скрываем кнопки входа/регистрации
        const loginBtn = authButtons.querySelector('a[href="/session/new"]');
        const registerBtn = authButtons.querySelector('a[href="/users/new"]');
        if (loginBtn) loginBtn.parentElement.style.display = 'none';
        if (registerBtn) registerBtn.parentElement.style.display = 'none';
    }
}

function initializeAuth() {
    const token = getToken();

    if (token) {
        console.log('Пользователь авторизован');
        showUserInfo(token);
    } else {
        console.log('Пользователь не авторизован');
    }
}

function logout() {
    removeTokens();
    location.reload();
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    initializeAuth();
});

// Альтернативная инициализация для старых браузеров
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeAuth);
} else {
    initializeAuth();
}