using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ModuleManager : MonoBehaviour
{
    // === ПЕРЕМЕННЫЕ ДЛЯ СИСТЕМЫ МОДУЛЕЙ ===
    private int currentModule = 1;
    private List<int> learningWords = new List<int>(); // Список слов для изучения (индексы)
    private Dictionary<int, int> wordCorrectCount = new Dictionary<int, int>(); // Счетчик правильных ответов
    private List<int> learnedWords = new List<int>(); // Изученные слова (5+ правильных ответов)
    private const int LEARNING_POOL_SIZE = 20;
    private const int CORRECT_ANSWERS_TO_LEARN = 5;

    public int CurrentModule => currentModule;

    void Start()
    {
        InitializeModuleSystem();
    }

    // Инициализация системы модулей
    void InitializeModuleSystem()
    {
        currentModule = PlayerPrefs.GetInt("currentModule", 1);
        LoadWordProgress();
        InitializeLearningPool();
    }

    // Загрузка прогресса по словам
    void LoadWordProgress()
    {
        // Загружаем изученные слова для текущего модуля
        string learnedWordsKey = $"module_{currentModule}_learned";
        string learnedWordsStr = PlayerPrefs.GetString(learnedWordsKey, "");

        if (!string.IsNullOrEmpty(learnedWordsStr))
        {
            learnedWords = learnedWordsStr.Split(',').Select(int.Parse).ToList();
        }
        else
        {
            learnedWords = new List<int>();
        }

        // Загружаем счетчики правильных ответов
        wordCorrectCount.Clear();
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions != null)
        {
            for (int i = 0; i < currentQuestions.Length; i++)
            {
                string key = $"module_{currentModule}_word_{i}";
                int correctCount = PlayerPrefs.GetInt(key, 0);
                if (correctCount > 0)
                {
                    wordCorrectCount[i] = correctCount;
                }
            }
        }
    }

    // Инициализация пула слов для изучения
    void InitializeLearningPool()
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null) return;

        learningWords.Clear();

        // Добавляем не изученные слова в пул
        List<int> allWords = Enumerable.Range(0, currentQuestions.Length).ToList();
        List<int> availableWords = allWords.Where(w => !learnedWords.Contains(w)).ToList();

        // Берем первые 20 доступных слов
        int wordsToTake = Mathf.Min(LEARNING_POOL_SIZE, availableWords.Count);
        learningWords = availableWords.Take(wordsToTake).ToList();

        Debug.Log($"Инициализирован пул изучения для модуля {currentModule}: {learningWords.Count} слов");
    }

    // Добавление нового слова в пул изучения
    void AddNewWordToPool()
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null) return;

        // Находим следующее не изученное слово
        List<int> allWords = Enumerable.Range(0, currentQuestions.Length).ToList();
        List<int> availableWords = allWords.Where(w =>
            !learnedWords.Contains(w) && !learningWords.Contains(w)).ToList();

        if (availableWords.Count > 0)
        {
            learningWords.Add(availableWords[0]);
            Debug.Log($"Добавлено новое слово в пул изучения: {availableWords[0]}");
        }
    }

    // Обновление счетчика правильных ответов для слова
    public void UpdateWordProgress(int wordIndex, bool isCorrect)
    {
        if (isCorrect)
        {
            if (!wordCorrectCount.ContainsKey(wordIndex))
            {
                wordCorrectCount[wordIndex] = 1;
            }
            else
            {
                wordCorrectCount[wordIndex]++;
            }

            // Сохраняем прогресс
            string key = $"module_{currentModule}_word_{wordIndex}";
            PlayerPrefs.SetInt(key, wordCorrectCount[wordIndex]);

            // Если слово правильно ответили 5 раз, помечаем его как изученное
            if (wordCorrectCount[wordIndex] >= CORRECT_ANSWERS_TO_LEARN)
            {
                if (!learnedWords.Contains(wordIndex))
                {
                    learnedWords.Add(wordIndex);

                    // Сохраняем изученные слова
                    string learnedKey = $"module_{currentModule}_learned";
                    PlayerPrefs.SetString(learnedKey, string.Join(",", learnedWords));

                    // Удаляем из пула изучения
                    learningWords.Remove(wordIndex);

                    // Добавляем новое слово в пул
                    AddNewWordToPool();

                    Debug.Log($"Слово {wordIndex} изучено! Добавлено новое слово в пул.");

                    // Проверяем, завершен ли модуль
                    CheckModuleCompletion();
                }
            }
        }
    }

    // Проверка завершения модуля
    void CheckModuleCompletion()
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null) return;

        // Если все слова модуля изучены
        if (learnedWords.Count >= currentQuestions.Length)
        {
            Debug.Log($"Модуль {currentModule} полностью изучен!");

            // Переключаемся на следующий модуль
            currentModule++;
            if (currentModule > 3) currentModule = 3;

            PlayerPrefs.SetInt("currentModule", currentModule);

            // Сбрасываем прогресс слов для нового модуля
            wordCorrectCount.Clear();
            learnedWords.Clear();
            learningWords.Clear();

            // Инициализируем новый модуль
            InitializeLearningPool();
        }
    }

    // Получение доступных вопросов для уровня
    public List<int> GetAvailableQuestionsForLevel(int totalQuestionsInLevel)
    {
        List<int> available = new List<int>();

        if (totalQuestionsInLevel <= 0) return available;

        // 1. Добавляем слова из пула изучения (до 80% вопросов)
        int learningWordsCount = Mathf.Min(learningWords.Count, Mathf.FloorToInt(totalQuestionsInLevel * 0.8f));
        for (int i = 0; i < learningWordsCount; i++)
        {
            if (i < learningWords.Count)
            {
                available.Add(learningWords[i]);
            }
        }

        // 2. Добавляем изученные слова (не более 5)
        int remainingSlots = totalQuestionsInLevel - available.Count;
        int learnedToAdd = Mathf.Min(5, remainingSlots, learnedWords.Count);

        if (learnedWords.Count > 0 && learnedToAdd > 0)
        {
            List<int> shuffledLearned = new List<int>(learnedWords);
            ShuffleList(shuffledLearned);

            for (int i = 0; i < learnedToAdd && i < shuffledLearned.Count; i++)
            {
                available.Add(shuffledLearned[i]);
            }
        }

        // 3. Если все еще не хватает вопросов, добавляем случайные слова
        if (available.Count < totalQuestionsInLevel)
        {
            string[] currentQuestions = GetCurrentVopros();
            if (currentQuestions != null)
            {
                List<int> allWords = Enumerable.Range(0, currentQuestions.Length).ToList();
                List<int> remainingWords = allWords.Where(w => !available.Contains(w)).ToList();

                ShuffleList(remainingWords);

                int needed = totalQuestionsInLevel - available.Count;
                for (int i = 0; i < needed && i < remainingWords.Count; i++)
                {
                    available.Add(remainingWords[i]);
                }
            }
        }

        ShuffleList(available);

        Debug.Log($"Для уровня выбрано {available.Count} вопросов из модуля {currentModule}");

        return available;
    }

    // Загрузка ячеек уровня
    public void LoadLevelCells(SaverTest saver)
    {
        string cellsString = PlayerPrefs.GetString($"level_{saver.currentLevel}_cells", "");

        if (!string.IsNullOrEmpty(cellsString))
        {
            string[] cellsArray = cellsString.Split(',');
            int[] currentLevelCells = new int[20];

            for (int i = 0; i < 20 && i < cellsArray.Length; i++)
            {
                if (int.TryParse(cellsArray[i], out int cellState))
                {
                    currentLevelCells[i] = cellState;
                }
            }

            saver.SetCurrentLevelCells(currentLevelCells);

            int answeredQuestions = 0;
            int correctAnswersInLevel = 0;

            for (int i = 0; i < 20; i++)
            {
                if (currentLevelCells[i] == 2 || currentLevelCells[i] == 3)
                {
                    answeredQuestions++;
                }
                if (currentLevelCells[i] == 2)
                {
                    correctAnswersInLevel++;
                }
            }

            string questionsString = PlayerPrefs.GetString($"level_{saver.currentLevel}_questions", "");
            List<int> levelQuestionIndices = new List<int>();

            if (!string.IsNullOrEmpty(questionsString))
            {
                string[] questionArray = questionsString.Split(',');
                foreach (string id in questionArray)
                {
                    if (int.TryParse(id, out int questionId))
                    {
                        levelQuestionIndices.Add(questionId);
                    }
                }
            }

            saver.SetLevelQuestionIndices(levelQuestionIndices);
            saver.SetLevelVariables(levelQuestionIndices.Count, answeredQuestions, correctAnswersInLevel);
        }
        else
        {
            CreateNewLevel(saver);
        }
    }

    // Создание нового уровня
    void CreateNewLevel(SaverTest saver)
    {
        int totalQuestionsInLevel;

        if (saver.currentLevel <= 3)
        {
            totalQuestionsInLevel = saver.currentLevel == 1 ? 3 :
                                   saver.currentLevel == 2 ? 5 : 7;
        }
        else
        {
            totalQuestionsInLevel = UnityEngine.Random.Range(9, 16);
        }

        int[] currentLevelCells = new int[20];
        for (int i = 0; i < 20; i++)
        {
            currentLevelCells[i] = 0;
        }

        List<int> availablePositions = Enumerable.Range(0, 20).ToList();
        ShuffleList(availablePositions);

        List<int> availableQuestions = GetAvailableQuestionsForLevel(totalQuestionsInLevel);

        for (int i = 0; i < totalQuestionsInLevel && i < availablePositions.Count; i++)
        {
            int position = availablePositions[i];

            if (i < availableQuestions.Count)
            {
                int questionIndex = availableQuestions[i];
                currentLevelCells[position] = 1;
            }
        }

        saver.SetCurrentLevelCells(currentLevelCells);
        saver.SetLevelQuestionIndices(availableQuestions);
        saver.SetLevelVariables(availableQuestions.Count, 0, 0);
    }

    // Сохранение ячеек уровня
    public void SaveLevelCells(int currentLevel, int[] currentLevelCells, List<int> levelQuestionIndices)
    {
        if (currentLevelCells == null || currentLevelCells.Length != 20) return;

        string cellsString = string.Join(",", currentLevelCells);
        PlayerPrefs.SetString($"level_{currentLevel}_cells", cellsString);

        if (levelQuestionIndices != null && levelQuestionIndices.Count > 0)
        {
            string questionsString = string.Join(",", levelQuestionIndices);
            PlayerPrefs.SetString($"level_{currentLevel}_questions", questionsString);
        }

        PlayerPrefs.Save();
    }

    // Создание данных вопроса
    public QuestionData CreateQuestionData(int questionIndex)
    {
        return new QuestionData(questionIndex, currentModule);
    }

    // Получение текущих данных модуля
    string[] GetCurrentVopros()
    {
        switch (currentModule)
        {
            case 1: return GameData1.vopros;
            case 2: return GameData2.vopros;
            case 3: return GameData3.vopros;
            default: return GameData1.vopros;
        }
    }

    // Сброс прогресса модулей
    public void ResetModuleProgress()
    {
        currentModule = 1;
        PlayerPrefs.SetInt("currentModule", currentModule);

        // Удаляем все сохранения для модулей
        for (int module = 1; module <= 3; module++)
        {
            PlayerPrefs.DeleteKey($"module_{module}_learned");

            string[] questions = module == 1 ? GameData1.vopros :
                                module == 2 ? GameData2.vopros :
                                GameData3.vopros;

            if (questions != null)
            {
                for (int i = 0; i < questions.Length; i++)
                {
                    PlayerPrefs.DeleteKey($"module_{module}_word_{i}");
                }
            }
        }

        wordCorrectCount.Clear();
        learnedWords.Clear();
        learningWords.Clear();

        InitializeLearningPool();
    }

    // Перемешивание списка
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    public List<int> GetLearnedWords()
    {
        return learnedWords;
    }
    // Получить словарь с количеством правильных ответов
    public Dictionary<int, int> GetWordCorrectCount()
    {
        return wordCorrectCount;
    }

    // Получить список слов в процессе изучения
    public List<int> GetLearningWords()
    {
        return learningWords;
    }

    // Получить прогресс изучения конкретного слова
    public int GetWordProgress(int wordIndex)
    {
        if (wordCorrectCount.ContainsKey(wordIndex))
        {
            return wordCorrectCount[wordIndex];
        }
        return 0;
    }

    // Получить общий прогресс модуля
    public float GetModuleProgress()
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null || currentQuestions.Length == 0)
            return 0f;

        return (float)learnedWords.Count / currentQuestions.Length;
    }

    // Получить статистику по модулю
    public (int totalWords, int learnedWords, int inProgressWords, float progressPercentage) GetModuleStatistics()
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null)
            return (0, 0, 0, 0f);

        int total = currentQuestions.Length;
        int learned = learnedWords.Count;
        int inProgress = learningWords.Count;
        float progress = total > 0 ? ((float)learned / total) * 100f : 0f;

        return (total, learned, inProgress, progress);
    }

    // Получить детальную информацию о слове
    public string GetWordDetails(int wordIndex)
    {
        string[] currentQuestions = GetCurrentVopros();
        if (currentQuestions == null || wordIndex < 0 || wordIndex >= currentQuestions.Length)
            return "Слово не найдено";

        string question = currentQuestions[wordIndex];
        int progress = GetWordProgress(wordIndex);
        bool isLearned = learnedWords.Contains(wordIndex);

        return $"Слово {wordIndex + 1}: {question}\n" +
               $"Прогресс: {progress}/5\n" +
               $"Статус: {(isLearned ? "ИЗУЧЕНО" : "В ПРОЦЕССЕ")}";
    }














}