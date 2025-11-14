// ========== СИСТЕМА ОЦЕНОК ==========

function highlightStars(stars, rating) {
    resetStars(stars);
    stars.forEach((star, index) => {
        if (index < rating) {
            star.textContent = '★';
            star.classList.add('active');
        }
    });
}

function resetStars(stars) {
    stars.forEach(star => {
        star.textContent = '☆';
        star.classList.remove('active');
    });
}

function setRating(taskId, rating) {
    localStorage.setItem(`rating_${taskId}`, rating);
    const stars = document.querySelector(`.rating-stars[data-task="${taskId}"]`)?.querySelectorAll('.star');
    if (stars) {
        highlightStars(stars, rating);
    }
    const ratingElement = document.getElementById(`currentRating${taskId.charAt(0).toUpperCase() + taskId.slice(1)}`);
    if (ratingElement) {
        ratingElement.textContent = `${rating}/5`;
    }
}

function getCurrentRating(taskId) {
    return parseInt(localStorage.getItem(`rating_${taskId}`)) || 0;
}

// Инициализация звезд рейтинга
function initializeRatingStars() {
    document.querySelectorAll('.rating-stars .star').forEach(star => {
        star.addEventListener('click', function () {
            const rating = parseInt(this.getAttribute('data-value'));
            const taskId = this.closest('.rating-stars').getAttribute('data-task');
            setRating(taskId, rating);
        });

        star.addEventListener('mouseover', function () {
            const rating = parseInt(this.getAttribute('data-value'));
            const stars = this.closest('.rating-stars').querySelectorAll('.star');
            highlightStars(stars, rating);
        });
    });

    document.querySelectorAll('.rating-stars').forEach(starsContainer => {
        starsContainer.addEventListener('mouseleave', function () {
            const currentRating = getCurrentRating(this.getAttribute('data-task'));
            const stars = this.querySelectorAll('.star');
            if (currentRating > 0) {
                highlightStars(stars, currentRating);
            } else {
                resetStars(stars);
            }
        });
    });

    // Восстанавливаем оценки из localStorage
    document.querySelectorAll('.rating-stars').forEach(starsContainer => {
        const taskId = starsContainer.getAttribute('data-task');
        const savedRating = getCurrentRating(taskId);
        if (savedRating > 0) {
            const stars = starsContainer.querySelectorAll('.star');
            highlightStars(stars, savedRating);
            const ratingElement = document.getElementById(`currentRating${taskId.charAt(0).toUpperCase() + taskId.slice(1)}`);
            if (ratingElement) {
                ratingElement.textContent = `${savedRating}/5`;
            }
        }
    });
}

// Инициализация при загрузке
document.addEventListener('DOMContentLoaded', initializeRatingStars);