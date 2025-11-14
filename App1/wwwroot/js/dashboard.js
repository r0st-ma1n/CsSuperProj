// dashboard.js
const API_BASE_URL = 'http://localhost:5086/api';

// Загрузить данные дашборда
async function loadDashboard() {
    const token = getToken();
    if (!token) {
        window.location.href = '/login';
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/User/dashboard`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            displayDashboard(data);
        } else {
            console.error('Ошибка загрузки дашборда');
        }
    } catch (error) {
        console.error('Ошибка:', error);
    }
}

// Отобразить данные дашборда
function displayDashboard(data) {
    // Общая статистика
    document.getElementById('totalCourses').textContent = data.statistics.totalCourses;
    document.getElementById('completedLessons').textContent = data.statistics.totalLessonsCompleted;
    document.getElementById('overallProgress').textContent = data.statistics.overallProgress + '%';

    // Список курсов
    displayMyCourses(data.enrollments);
}

// Отобразить список курсов
function displayMyCourses(enrollments) {
    const container = document.getElementById('myCoursesList');

    if (enrollments.length === 0) {
        container.innerHTML = '<p>Вы еще не записались на курсы</p>';
        return;
    }

    container.innerHTML = enrollments.map(enrollment => `
        <div class="course-card">
            <div class="course-header">
                <h4>${enrollment.course.title}</h4>
                <span class="progress-badge">${Math.round(enrollment.progress)}%</span>
            </div>
            <div class="course-progress">
                <div class="progress-bar">
                    <div class="progress-fill" style="width: ${enrollment.progress}%"></div>
                </div>
            </div>
            <div class="course-stats">
                <span>Пройдено: ${enrollment.completedLessons}/${enrollment.totalLessons} уроков</span>
            </div>
            <div class="course-actions">
                <a href="/courses/python/lesson/1" class="btn btn-outline">Продолжить</a>
            </div>
        </div>
    `).join('');
}

// Инициализация при загрузке
document.addEventListener('DOMContentLoaded', loadDashboard);