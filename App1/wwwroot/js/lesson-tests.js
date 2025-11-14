// ========== ТЕСТОВЫЕ ЗАДАНИЯ ==========

let testResults = {
    total: 3,
    correct: 0,
    answered: 0
};

function selectOption(optionElement) {
    const question = optionElement.closest('.test-task');
    const options = question.querySelectorAll('.test-option');

    // Снимаем выделение со всех вариантов в этом вопросе
    options.forEach(opt => {
        opt.classList.remove('selected');
        opt.querySelector('input[type="checkbox"]').checked = false;
    });

    // Выделяем выбранный вариант
    optionElement.classList.add('selected');
    optionElement.querySelector('input[type="checkbox"]').checked = true;
}

function checkAnswer(questionId, correctOptionId) {
    const question = document.querySelector(`#q${questionId}_opt1`).closest('.test-task');
    const selectedOption = question.querySelector('.test-option.selected');
    const correctOption = document.getElementById(correctOptionId);

    if (!selectedOption) {
        alert('Пожалуйста, выберите вариант ответа');
        return;
    }

    // Скрываем все фидбэки
    hideAllFeedbacks(questionId);

    if (selectedOption.querySelector('input').id === correctOptionId) {
        // Правильный ответ
        selectedOption.classList.add('correct');
        document.getElementById(`feedback${questionId}_correct`).style.display = 'block';
        testResults.correct++;
    } else {
        // Неправильный ответ
        selectedOption.classList.add('incorrect');
        correctOption.parentElement.classList.add('correct');
        document.getElementById(`feedback${questionId}_incorrect`).style.display = 'block';
    }

    testResults.answered++;
    updateTestResults();

    // Блокируем возможность изменения ответа
    const options = question.querySelectorAll('.test-option');
    options.forEach(opt => {
        opt.style.pointerEvents = 'none';
    });

    question.querySelector('.btn-check').disabled = true;
}

function resetQuestion(questionId) {
    const question = document.querySelector(`#q${questionId}_opt1`).closest('.test-task');
    const options = question.querySelectorAll('.test-option');

    // Сбрасываем стили
    options.forEach(opt => {
        opt.classList.remove('selected', 'correct', 'incorrect');
        opt.querySelector('input[type="checkbox"]').checked = false;
        opt.style.pointerEvents = 'auto';
    });

    // Скрываем фидбэки
    hideAllFeedbacks(questionId);

    // Активируем кнопки
    question.querySelector('.btn-check').disabled = false;
}

function hideAllFeedbacks(questionId) {
    const correctFeedback = document.getElementById(`feedback${questionId}_correct`);
    const incorrectFeedback = document.getElementById(`feedback${questionId}_incorrect`);

    if (correctFeedback) correctFeedback.style.display = 'none';
    if (incorrectFeedback) incorrectFeedback.style.display = 'none';
}

function updateTestResults() {
    if (testResults.answered === testResults.total) {
        const percentage = (testResults.correct / testResults.total) * 100;
        const finalScore = document.getElementById('finalScore');
        const resultsMessage = document.getElementById('resultsMessage');
        const testProgress = document.getElementById('testProgress');
        const resultsDetails = document.getElementById('resultsDetails');

        if (finalScore) finalScore.textContent = `${testResults.correct}/${testResults.total}`;
        if (testProgress) testProgress.style.width = `${percentage}%`;

        // Сообщение в зависимости от результата
        if (resultsMessage) {
            if (percentage >= 80) {
                resultsMessage.textContent = 'Отличный результат! Вы отлично усвоили материал.';
                resultsMessage.style.color = '#27ae60';
            } else if (percentage >= 60) {
                resultsMessage.textContent = 'Хороший результат! Вы хорошо поняли основы.';
                resultsMessage.style.color = '#f39c12';
            } else {
                resultsMessage.textContent = 'Рекомендуется повторить материал и пройти тест заново.';
                resultsMessage.style.color = '#e74c3c';
            }
        }

        // Детали результатов
        if (resultsDetails) {
            resultsDetails.innerHTML = `
                <p>✅ Правильных ответов: ${testResults.correct}</p>
                <p>❌ Неправильных ответов: ${testResults.total - testResults.correct}</p>
                <p>📊 Процент правильных: ${percentage.toFixed(1)}%</p>
            `;
        }

        const testResultsElement = document.getElementById('testResults');
        if (testResultsElement) {
            testResultsElement.style.display = 'block';
            testResultsElement.scrollIntoView({ behavior: 'smooth' });
        }
    }
}

function resetAllQuestions() {
    testResults = {
        total: 3,
        correct: 0,
        answered: 0
    };

    // Сбрасываем все вопросы
    for (let i = 1; i <= testResults.total; i++) {
        resetQuestion(i);
    }

    // Скрываем результаты
    const testResultsElement = document.getElementById('testResults');
    if (testResultsElement) {
        testResultsElement.style.display = 'none';
    }
}