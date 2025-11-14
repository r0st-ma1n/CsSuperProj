// lesson-progress.js - ПОЛНАЯ РЕАЛИЗАЦИЯ
const PROGRESS_API_URL = 'http://localhost:5086/api'; // УНИКАЛЬНОЕ ИМЯ

// Функция для получения токена
function getToken() {
    return localStorage.getItem('token') || sessionStorage.getItem('token');
}

// Отметить урок как пройденный
async function completeLesson(lessonId, courseId) {
    const token = getToken();
    if (!token) {
        console.log('Пользователь не авторизован');
        alert('Для сохранения прогресса необходимо авторизоваться');
        return false;
    }

    try {
        // Сначала проверяем/создаем запись в Enrollments
        const enrollmentResponse = await fetch(`${PROGRESS_API_URL}/UserProgress/ensure-enrollment`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                courseId: courseId
            })
        });

        if (!enrollmentResponse.ok) {
            console.error('❌ Ошибка при создании записи на курс:', enrollmentResponse.status);
        }

        // Затем отмечаем урок как завершенный
        const response = await fetch(`${PROGRESS_API_URL}/UserProgress/complete-lesson`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                lessonId: lessonId,
                courseId: courseId
            })
        });

        if (response.ok) {
            console.log('✅ Урок отмечен как пройденный');
            showSuccessMessage('Урок успешно завершен! Прогресс сохранен.');
            updateProgressUI(lessonId, true);
            return true;
        } else {
            console.error('❌ Ошибка при отметке урока:', response.status);
            alert('Ошибка сохранения прогресса. Попробуйте еще раз.');
            return false;
        }
    } catch (error) {
        console.error('❌ Ошибка сети:', error);
        alert('Ошибка сети. Проверьте подключение к интернету.');
        return false;
    }
}

// Показать сообщение об успехе
function showSuccessMessage(message) {
    const toast = document.createElement('div');
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #27ae60;
        color: white;
        padding: 15px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 10000;
        font-weight: 500;
    `;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Обновить UI после завершения урока
function updateProgressUI(lessonId, isCompleted) {
    const lessonElement = document.querySelector(`[data-lesson-id="${lessonId}"]`);
    if (lessonElement && isCompleted) {
        lessonElement.classList.add('completed');
    }
    updateProgressBar();
}

// Обновить прогресс-бар курса
async function updateProgressBar() {
    const progressBar = document.querySelector('.progress-fill');
    if (!progressBar) return;
}

// Добавить кнопку завершения урока на страницу
function addCompleteLessonButton(lessonId, courseId) {
    console.log('🔄 Добавляем кнопку завершения урока...', lessonId, courseId);

    if (document.getElementById('completeLessonBtn')) {
        console.log('⚠️ Кнопка уже существует');
        return;
    }

    const completeBtn = document.createElement('button');
    completeBtn.id = 'completeLessonBtn';
    completeBtn.innerHTML = '✅ Завершить урок';
    completeBtn.style.cssText = `
        display: block;
        margin: 30px auto;
        padding: 12px 24px;
        font-size: 16px;
        background: #27ae60;
        color: white;
        border: none;
        border-radius: 8px;
        cursor: pointer;
        transition: all 0.3s ease;
    `;

    completeBtn.onmouseover = function () {
        this.style.background = '#219653';
        this.style.transform = 'translateY(-2px)';
    };

    completeBtn.onmouseout = function () {
        this.style.background = '#27ae60';
        this.style.transform = 'translateY(0)';
    };

    completeBtn.onclick = async function () {
        completeBtn.disabled = true;
        completeBtn.innerHTML = '⏳ Сохраняем прогресс...';

        const success = await completeLesson(lessonId, courseId);

        if (success) {
            completeBtn.innerHTML = '✅ Урок завершен';
            completeBtn.style.background = '#95a5a6';
            completeBtn.style.cursor = 'default';
            showNextLessonLink();
        } else {
            completeBtn.disabled = false;
            completeBtn.innerHTML = '✅ Завершить урок';
        }
    };

    // Добавляем кнопку на страницу
    const testResults = document.getElementById('testResults');
    if (testResults) {
        console.log('✅ Добавляем кнопку в результаты теста');
        testResults.appendChild(completeBtn);
    } else {
        const lastQuestion = document.querySelector('.test-task:last-child');
        if (lastQuestion) {
            console.log('✅ Добавляем кнопку после последнего вопроса');
            lastQuestion.parentNode.insertBefore(completeBtn, lastQuestion.nextSibling);
        } else {
            console.log('❌ Не найден контейнер для кнопки');
            const mainContent = document.querySelector('.main-content');
            if (mainContent) {
                mainContent.appendChild(completeBtn);
            }
        }
    }
}

// Показать ссылку на следующий урок
function showNextLessonLink() {
    const nextLessonBtn = document.createElement('a');
    nextLessonBtn.className = 'btn btn-primary';
    nextLessonBtn.innerHTML = '➡️ Следующий урок';
    nextLessonBtn.href = '/courses/python/lesson/2';
    nextLessonBtn.style.cssText = `
        display: block;
        margin: 20px auto;
        padding: 12px 24px;
        text-align: center;
        text-decoration: none;
        background: #3498db;
        color: white;
        border-radius: 8px;
        transition: all 0.3s ease;
        max-width: 200px;
    `;

    const completeBtn = document.getElementById('completeLessonBtn');
    if (completeBtn) {
        completeBtn.parentNode.insertBefore(nextLessonBtn, completeBtn.nextSibling);
    }
}

// Инициализация системы прогресса для урока
function initLessonProgress(lessonId, courseId) {
    console.log(`🎯 Инициализация прогресса для урока ${lessonId}, курс ${courseId}`);

    // Добавляем кнопку завершения
    setTimeout(() => {
        addCompleteLessonButton(lessonId, courseId);
    }, 500);
}