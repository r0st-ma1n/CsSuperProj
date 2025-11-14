// ========== СИСТЕМА ОТЗЫВОВ ==========
const API_BASE_URL = 'http://localhost:5086/api';
const COURSE_ID = 1;

// Загрузка отзывов для конкретного урока
async function loadReviews(lessonId) {
    try {
        const response = await fetch(`${API_BASE_URL}/reviews/course/${COURSE_ID}?lessonId=${lessonId}`);
        if (response.ok) {
            const reviews = await response.json();
            displayReviews(reviews);
        } else {
            console.error('Ошибка загрузки отзывов:', response.status);
        }
    } catch (error) {
        console.error('Ошибка загрузки отзывов:', error);
    }
}

// Загрузка статистики для конкретного урока
async function loadRatingStats(lessonId) {
    try {
        const response = await fetch(`${API_BASE_URL}/reviews/course/${COURSE_ID}/stats?lessonId=${lessonId}`);
        if (response.ok) {
            const stats = await response.json();
            displayRatingStats(stats);
        } else {
            console.error('Ошибка загрузки статистики:', response.status);
        }
    } catch (error) {
        console.error('Ошибка загрузки статистики:', error);
    }
}

// Отображение отзывов
function displayReviews(reviews) {
    const commentsList = document.getElementById('commentsListLesson');

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

// Отображение статистики
function displayRatingStats(stats) {
    const averageRating = document.getElementById('averageRating');
    const reviewCount = document.getElementById('reviewCount');
    const ratingStats = document.getElementById('ratingStats');

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

// Отправка отзыва для конкретного урока
async function submitReviewForLesson(lessonId) {
    const commentInput = document.getElementById('lessonCommentInput');
    const commentText = commentInput.value.trim();
    const userRating = getCurrentRating('lesson');

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
        const response = await fetch(`${API_BASE_URL}/reviews/course/${COURSE_ID}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                rating: userRating,
                content: commentText,
                lessonId: lessonId
            })
        });

        if (response.ok) {
            const result = await response.json();
            alert('Отзыв успешно добавлен!');
            commentInput.value = '';
            resetStars(document.querySelectorAll('.rating-stars[data-task="lesson"] .star'));
            document.getElementById('currentRatingLesson').textContent = 'Не оценено';
            localStorage.removeItem('rating_lesson');

            // Перезагружаем отзывы и статистику для этого урока
            loadReviews(lessonId);
            loadRatingStats(lessonId);
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