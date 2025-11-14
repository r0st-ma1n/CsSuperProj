// ========== СИСТЕМА ОТЗЫВОВ ДЛЯ ML КУРСА ==========
const API_BASE_URL = 'http://localhost:5086/api';
const ML_COURSE_ID = 2; // ID для ML курса

// Загрузка отзывов для ML курса
async function loadMLCourseReviews() {
    try {
        const response = await fetch(`${API_BASE_URL}/reviews/course/${ML_COURSE_ID}`);
        if (response.ok) {
            const reviews = await response.json();
            displayMLCourseReviews(reviews);
        } else {
            console.error('Ошибка загрузки отзывов ML курса:', response.status);
        }
    } catch (error) {
        console.error('Ошибка загрузки отзывов ML курса:', error);
    }
}

// Загрузка статистики для ML курса
async function loadMLCourseRatingStats() {
    try {
        const response = await fetch(`${API_BASE_URL}/reviews/course/${ML_COURSE_ID}/stats`);
        if (response.ok) {
            const stats = await response.json();
            displayMLCourseRatingStats(stats);
        } else {
            console.error('Ошибка загрузки статистики ML курса:', response.status);
        }
    } catch (error) {
        console.error('Ошибка загрузки статистики ML курса:', error);
    }
}

// Отображение отзывов ML курса
function displayMLCourseReviews(reviews) {
    const commentsList = document.getElementById('commentsListCourse');

    if (!commentsList) return;

    if (reviews.length === 0) {
        commentsList.innerHTML = '<p class="no-reviews">Пока нет отзывов. Будьте первым!</p>';
        return;
    }

    commentsList.innerHTML = reviews.map(review => `
        <div class="comment-item">
            <div class="comment-header">
                <span class="comment-author">${review.userFullName}</span>
                <span class="comment-date">${formatDate(review.createdAt)}</span>
            </div>
            <div class="comment-text">${escapeHtml(review.content)}</div>
            <div class="comment-rating">
                ${'⭐'.repeat(review.rating)}${'☆'.repeat(5 - review.rating)} (${review.rating}/5)
            </div>
        </div>
    `).join('');
}

// Отображение статистики ML курса
function displayMLCourseRatingStats(stats) {
    const averageRating = document.getElementById('courseAverageRating');
    const reviewCount = document.getElementById('courseReviewCount');
    const ratingStats = document.getElementById('courseRatingStats');

    if (averageRating) averageRating.textContent = stats.averageRating.toFixed(1);
    if (reviewCount) reviewCount.textContent = `(${stats.reviewCount} ${getReviewWord(stats.reviewCount)})`;

    const stars = ratingStats?.querySelectorAll('.star');
    if (stars) {
        resetStars(stars);

        const fullStars = Math.floor(stats.averageRating);
        const hasHalfStar = stats.averageRating % 1 >= 0.5;

        stars.forEach((star, index) => {
            if (index < fullStars) {
                star.textContent = '★';
                star.classList.add('active');
            } else if (index === fullStars && hasHalfStar) {
                star.textContent = '½';
                star.classList.add('active');
            }
        });
    }
}

// Отправка отзыва для ML курса
async function submitReviewForMLCourse() {
    const commentInput = document.getElementById('courseCommentInput');
    const commentText = commentInput.value.trim();
    const userRating = getCurrentRating('course');

    if (!commentText) {
        alert('Пожалуйста, введите текст отзыва');
        return;
    }

    if (userRating === 0) {
        alert('Пожалуйста, поставьте оценку');
        return;
    }

    if (commentText.length < 10) {
        alert('Отзыв должен содержать минимум 10 символов');
        return;
    }

    const token = getToken();
    if (!token) {
        alert('Пожалуйста, войдите в систему чтобы оставить отзыв');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/reviews/course/${ML_COURSE_ID}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                rating: userRating,
                content: commentText
                // lessonId не передаем - это отзыв на весь курс
            })
        });

        if (response.ok) {
            const result = await response.json();
            alert('Отзыв на курс успешно добавлен!');
            commentInput.value = '';
            resetStars(document.querySelectorAll('.rating-stars[data-task="course"] .star'));
            document.getElementById('currentRatingCourse').textContent = 'Не оценено';
            localStorage.removeItem('rating_course');

            // Перезагружаем отзывы и статистику для курса
            loadMLCourseReviews();
            loadMLCourseRatingStats();
        } else {
            const errorText = await response.text();
            console.error('Error response:', errorText);

            if (response.status === 401) {
                alert('Ошибка авторизации. Пожалуйста, войдите снова.');
                removeTokens();
            } else {
                alert(`Ошибка: ${errorText || 'Не удалось добавить отзыв'}`);
            }
        }
    } catch (error) {
        console.error('Ошибка отправки отзыва:', error);
        alert('Ошибка сети. Попробуйте еще раз.');
    }
}

// Вспомогательные функции
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU') + ' ' + date.toLocaleTimeString('ru-RU', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function getReviewWord(count) {
    if (count % 10 === 1 && count % 100 !== 11) return 'отзыв';
    if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return 'отзыва';
    return 'отзывов';
}

// Делаем функции глобальными для доступа из HTML
window.submitReviewForCourse = submitReviewForMLCourse;
window.loadCourseReviews = loadMLCourseReviews;
window.loadCourseRatingStats = loadMLCourseRatingStats;