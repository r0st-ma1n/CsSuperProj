// ========== СИСТЕМА АВТОРИЗАЦИИ ==========

function getToken() {
    const token = localStorage.getItem('authToken') || localStorage.getItem('token');
    if (token && token.split('.').length === 3) {
        return token;
    }
    return null;
}

function removeTokens() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('token');
}

function showUserInfo(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const payload = JSON.parse(decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join('')));

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
        updateAuthUI('Пользователь');
    }
}

function updateAuthUI(userName) {
    const userInfo = document.getElementById('userInfo');
    const authButtons = document.querySelector('.auth-buttons');

    if (userInfo && authButtons) {
        document.getElementById('userName').textContent = userName;
        userInfo.style.display = 'flex';

        const loginBtn = authButtons.querySelector('a[href="/session/new"]');
        const registerBtn = authButtons.querySelector('a[href="/users/new"]');
        if (loginBtn) loginBtn.parentElement.style.display = 'none';
        if (registerBtn) registerBtn.parentElement.style.display = 'none';

        // Добавляем обработчик для кнопки выхода
        const logoutBtn = userInfo.querySelector('.logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', function () {
                removeTokens();
                location.reload();
            });
        }
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

// Инициализация авторизации при загрузке
document.addEventListener('DOMContentLoaded', initializeAuth);