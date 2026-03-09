using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SaverTest : MonoBehaviour
{
    // === ССЫЛКИ НА ДРУГИЕ КОМПОНЕНТЫ ===
    private ModuleManager moduleManager;
//private LifeTimerManager lifeTimer;
    private BadgeSystem badgeSystem;
    private QuestionSystem questionSystem;

    [Header("Аудио для вопросов")]
    public AudioSource questionAudioSource; // Отдельный AudioSource для озвучки вопросов
    public string audioFolderPath = "audio/"; // Папка с аудиофайлами
    public bool autoPlayQuestionAudio = true; // Автоматически воспроизводить при открытии вопроса

    [Header("UI для изученных слов")]
    public GameObject learnedWordsPanel; // Панель с изученными словами
    public Transform learnedWordsGrid; // Grid для отображения изученных слов
    public GameObject learnedWordPrefab; // Префаб для отображения слова
    public Button showLearnedWordsButton; // Кнопка показа изученных слов
    public Button closeLearnedWordsButton; // Кнопка закрытия панели
    public TextMeshProUGUI learnedWordsTitle; // Заголовок "Изученные слова"
    public TextMeshProUGUI moduleProgressText; // Текст прогресса модуля

    // === UI ЭЛЕМЕНТЫ (ВСЕ ОСТАЮТСЯ ЗДЕСЬ) ===
    [Header("Основные UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI starsText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI lifeTimerText;
    public TextMeshProUGUI moduleText; // Новое поле для номера модуля

    [Header("Кнопки")]
    public Button playButton;
    public Button resetButton;
    public Button exchangeCoinsToLivesButton;
    public TextMeshProUGUI exchangeButtonText;
    public int exchangeRate = 100;

    [Header("Экраны")]
    public GameObject startGameScreen;
    public GameObject levelScreen;
    public GameObject gameScreen;
    public GameObject noLivesPopup;

    [Header("Компоненты уровня")]
    public Transform questionsGrid;
    public GameObject questionCellPrefab;
    public GameObject emptyCellPrefab;
    public Image progressBar;

    [Header("Компоненты игры")]
    public Image questionImage;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;

    [Header("Система значков")]
    public GameObject znachokplashka;
    public Image znachokImg;
    public Button znachokBtn;
    public GameObject zngrid;
    public GameObject pobedaZnblock;
    public Image pobedaZnVpered;
    public TextMeshProUGUI pobedaznzvvpered;

    [Header("Остальные UI")]
    public GameObject pobedablock;
    public Button pobedaBtn;
    public TextMeshProUGUI pobedaUroven;
    public GameObject proigrblock;
    public Button proigrBtn;
    public GameObject zv1;
    public GameObject zv2;
    public GameObject zv3;

    [Header("Аудио")]
    public AudioSource zvklick;
    public AudioSource audioigra;
    public AudioClip klickaudio;
    public AudioClip noaudio;
    public AudioClip pobedaaudio;
    public int audVkl = 0;
    public GameObject knopVikl;
    public GameObject knopVkl;

    [Header("Анимации")]
    public AnimationClip pobedaAnim;
    public AnimationClip pobedaSborAnim;
    public GameObject pobedaplash;
    public GameObject correctAnswerAnimObject;
    public AnimationClip correctAnswerAnim;
    public ParticleSystem coinParticleSystem;
    public GameObject coinAnimObject;
    public Transform coinStartPosition;
    public Transform coinTargetPosition;

    [Header("Реклама")]
    public bool noRek = false;
    public GameObject banner;

    // === ОСНОВНЫЕ ПЕРЕМЕННЫЕ ===
    public int currentLevel = 1;
    public int coins = 0;
    public int stars = 0;
    public int lives = 5;
    public int correctAnswersInLevel = 0;
    public int totalQuestionsInLevel = 0;
    public int answeredQuestions = 0;

    // === СИСТЕМА ЯЧЕЕК ===
    private int[] currentLevelCells = new int[20];
    private List<int> levelQuestionIndices = new List<int>();
    private int currentQuestionIndex = -1;
    private int currentCellIndex = -1;

    void Start()
    {
        // Автоматически добавляем недостающие компоненты
        if (GetComponent<ModuleManager>() == null)
            gameObject.AddComponent<ModuleManager>();
       // if (GetComponent<LifeTimerManager>() == null)
      //      gameObject.AddComponent<LifeTimerManager>();
        if (GetComponent<BadgeSystem>() == null)
            gameObject.AddComponent<BadgeSystem>();
        if (GetComponent<QuestionSystem>() == null)
            gameObject.AddComponent<QuestionSystem>();

        // Получаем ссылки на компоненты
        moduleManager = GetComponent<ModuleManager>();
     //   lifeTimer = GetComponent<LifeTimerManager>();
        badgeSystem = GetComponent<BadgeSystem>();
        questionSystem = GetComponent<QuestionSystem>();

        // Проверяем GameData текущего модуля
        string[] currentVopros = GetCurrentVopros();
        if (currentVopros == null || currentVopros.Length == 0)
        {
            Debug.LogError($"GameData для модуля {moduleManager?.CurrentModule} не инициализирован!");
        }
        else
        {
            Debug.Log($"GameData модуля {moduleManager?.CurrentModule} загружен: {currentVopros.Length} вопросов");
        }

        // Настройка кнопок
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetProgress);

        if (pobedaBtn != null)
            pobedaBtn.onClick.AddListener(PobedaSborVoid);

        if (znachokBtn != null)
            znachokBtn.onClick.AddListener(ZnachkiBtnVoid);

        if (proigrBtn != null)
            proigrBtn.onClick.AddListener(ProigrVoid);

        if (exchangeCoinsToLivesButton != null)
            exchangeCoinsToLivesButton.onClick.AddListener(ExchangeCoinsToLives);

        // Добавьте обработчики кнопок изученных слов
        if (showLearnedWordsButton != null)
            showLearnedWordsButton.onClick.AddListener(ShowLearnedWords);

        if (closeLearnedWordsButton != null)
            closeLearnedWordsButton.onClick.AddListener(CloseLearnedWords);

        LoadGameData();
        UpdateMainScreen();
        UpdateZnachkiGrid();

        // Начальное состояние экранов
        startGameScreen.SetActive(true);
        levelScreen.SetActive(false);
        gameScreen.SetActive(false);
        noLivesPopup.SetActive(false);

        if (pobedablock != null) pobedablock.SetActive(false);
        if (proigrblock != null) proigrblock.SetActive(false);
        if (znachokplashka != null) znachokplashka.SetActive(false);

        // Проверяем бесконечные жизни при старте
     //   if (lifeTimer != null)
      //      lifeTimer.CheckUnlimitedLives(this);

        // Обновляем состояние кнопки
        UpdateExchangeButton();
    }




    // Новый метод для воспроизведения аудио вопроса
    public void PlayQuestionAudio(string imageName)
    {
        if (questionAudioSource == null)
        {
            Debug.LogWarning(" questionAudioSource не назначен!");
            return;
        }

        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogWarning(" Имя аудиофайла пустое");
            return;
        }

        // Пробуем разные пути загрузки
        string[] paths = {
        audioFolderPath + imageName,
        "Audio/" + imageName,
        "Sounds/" + imageName,
        imageName
    };

        AudioClip clip = null;

        foreach (string path in paths)
        {
            clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                Debug.Log($" Загружено аудио: {path}");
                break;
            }
        }

        if (clip != null)
        {
            questionAudioSource.clip = clip;
            questionAudioSource.Play();
            Debug.Log($" Воспроизводится аудио для картинки: {imageName}");
        }
        else
        {
            Debug.LogWarning($" Не найдено аудио: {imageName}");
        }
    }








    // Показать панель изученных слов
    public void ShowLearnedWords()
    {
        if (learnedWordsPanel != null)
        {
            learnedWordsPanel.SetActive(true);
            UpdateLearnedWordsDisplay();
        }
    }
    // Закрыть панель изученных слов
    public void CloseLearnedWords()
    {
        if (learnedWordsPanel != null)
        {
            learnedWordsPanel.SetActive(false);
        }
    }

    // Обновить отображение изученных слов
    void UpdateLearnedWordsDisplay()
    {
        if (learnedWordsGrid == null || learnedWordPrefab == null || moduleManager == null)
        {
            Debug.LogWarning("Не все компоненты для отображения изученных слов инициализированы");
            return;
        }

        // Очищаем сетку
        foreach (Transform child in learnedWordsGrid)
        {
            Destroy(child.gameObject);
        }

        // Получаем изученные слова через ModuleManager
        var learnedWords = moduleManager.GetLearnedWords();
        var wordCorrectCount = moduleManager.GetWordCorrectCount();

        if (learnedWords == null || wordCorrectCount == null)
        {
            Debug.LogWarning("Данные изученных слов не загружены");
            return;
        }

        // Получаем вопросы текущего модуля для отображения текста
        string[] currentQuestions = GetCurrentVopros();

        if (currentQuestions == null || currentQuestions.Length == 0)
        {
            Debug.LogWarning("Нет данных вопросов для текущего модуля");
            return;
        }

        // Обновляем заголовок и прогресс
        if (learnedWordsTitle != null)
            learnedWordsTitle.text = $"Изученные слова (модуль {moduleManager.CurrentModule})";

        if (moduleProgressText != null)
        {
            float progressPercentage = currentQuestions.Length > 0 ?
                ((float)learnedWords.Count / currentQuestions.Length) * 100f : 0f;
            moduleProgressText.text = $"Прогресс: {learnedWords.Count}/{currentQuestions.Length} слов ({progressPercentage:F1}%)";
        }

        // Сортируем слова по индексу
        List<int> sortedWords = new List<int>(learnedWords);
        sortedWords.Sort();

        // Отображаем каждое изученное слово
        foreach (int wordIndex in sortedWords)
        {
            if (wordIndex >= 0 && wordIndex < currentQuestions.Length)
            {
                GameObject wordItem = Instantiate(learnedWordPrefab, learnedWordsGrid);
                LearnedWordItem itemScript = wordItem.GetComponent<LearnedWordItem>();

                if (itemScript != null)
                {
                    // Получаем правильный счет для этого слова
                    int correctCount = wordCorrectCount.ContainsKey(wordIndex) ?
                        wordCorrectCount[wordIndex] : 5; // По умолчанию 5, так как слово изучено

                    // Формируем текст вопроса (первые 30 символов)
                    string questionText = currentQuestions[wordIndex];
                    if (questionText.Length > 30)
                    {
                        questionText = questionText.Substring(0, 30) + "...";
                    }

                    itemScript.Initialize(wordIndex + 1, questionText, correctCount);
                }
            }
        }

        // Если нет изученных слов
        if (sortedWords.Count == 0)
        {
            GameObject emptyItem = Instantiate(learnedWordPrefab, learnedWordsGrid);
            LearnedWordItem itemScript = emptyItem.GetComponent<LearnedWordItem>();
            if (itemScript != null)
            {
                itemScript.Initialize(0, "Нет изученных слов в этом модуле", 0);
            }
        }

        Debug.Log($"Отображено {sortedWords.Count} изученных слов модуля {moduleManager.CurrentModule}");
    }

    // Обновить прогресс модуля на главном экране
    void UpdateModuleProgressUI()
    {
        if (moduleManager == null) return;

        // Вы можете добавить TextMeshProUGUI поле для отображения прогресса на главном экране
        // Например: public TextMeshProUGUI progressText;
        // if (progressText != null)
        // {
        //     string[] questions = GetCurrentVopros();
        //     var learnedWords = moduleManager.GetLearnedWords();
        //     if (questions != null && learnedWords != null)
        //     {
        //         float progress = questions.Length > 0 ? 
        //             ((float)learnedWords.Count / questions.Length) * 100f : 0f;
        //         progressText.text = $"Прогресс: {progress:F1}%";
        //     }
        // }
    }

    // Обновить отображение после изучения слова
    public void RefreshLearnedWordsDisplay()
    {
        UpdateModuleProgressUI();

        // Если панель открыта - обновить ее содержимое
        if (learnedWordsPanel != null && learnedWordsPanel.activeSelf)
        {
            UpdateLearnedWordsDisplay();
        }
    }
    // Метод для получения текущих вопросов
    private string[] GetCurrentVopros()
    {
        if (moduleManager == null) return null;

        switch (moduleManager.CurrentModule)
        {
            case 1: return GameData1.vopros;
            case 2: return GameData2.vopros;
            case 3: return GameData3.vopros;
            default: return GameData1.vopros;
        }
    }

    public void UpdateZnachkiGrid()
    {
        if (badgeSystem != null && zngrid != null)
        {
            badgeSystem.UpdateZnachkiGrid(stars, zngrid);
        }
        else
        {
            Debug.LogWarning("BadgeSystem или zngrid не инициализированы");
        }
    }

    void Update()
    {
       // if (lifeTimer != null)
          //  lifeTimer.UpdateTimer(this);
    }

    // === ОСНОВНЫЕ МЕТОДЫ ===

    public void OnPlayButtonClicked()
    {
        startGameScreen.SetActive(false);
        levelScreen.SetActive(true);

        if (moduleManager != null)
        {
            moduleManager.LoadLevelCells(this);
            CreateGridCells();
            UpdateLevelScreen();
        }
        else
        {
            Debug.LogError("ModuleManager не найден!");
        }
    }

    void UpdateLevelScreen()
    {
        if (progressBar != null && totalQuestionsInLevel > 0)
        {
            progressBar.fillAmount = (float)answeredQuestions / totalQuestionsInLevel;
        }

        UpdateMainScreen();
    }
    // Обновите метод UpdateMainScreen, добавив вызов UpdateCoinsInShops()
    public void UpdateMainScreen()
    {
        if (levelText != null) levelText.text = currentLevel.ToString();
        if (coinsText != null) coinsText.text = coins.ToString();
        if (starsText != null) starsText.text = stars.ToString();
        if (livesText != null) livesText.text = lives.ToString();

        if (moduleManager != null && moduleText != null)
        {
            moduleText.text = $"Модуль {moduleManager.CurrentModule}";
        }

        UpdateExchangeButton();

        
    }

    public void ExchangeCoinsToLives()
    {
        if (coins >= exchangeRate && lives < 5)
        {
            coins -= exchangeRate;
            lives++;
            UpdateMainScreen();
            SaveGameData();
            Debug.Log($"Обмен произведен: 100 монет -> 1 жизнь. Осталось монет: {coins}, жизней: {lives}");
        }
        else if (lives >= 5)
        {
            Debug.Log("Нельзя обменять: максимальное количество жизней уже достигнуто");
        }
    }

    void UpdateExchangeButton()
    {
        /*
        if (exchangeCoinsToLivesButton != null && exchangeButtonText != null)
        {
            bool canExchange = (coins >= exchangeRate && lives < 5 &&
                               (lifeTimer == null || !lifeTimer.IsUnlimitedLivesActive));

            exchangeCoinsToLivesButton.interactable = canExchange;

            if (!canExchange)
            {
                if (lifeTimer != null && lifeTimer.IsUnlimitedLivesActive)
                {
                    exchangeButtonText.text = "Беск. жизни";
                }
                else if (lives >= 5)
                {
                    exchangeButtonText.text = "MAX жизни";
                }
                else if (coins < exchangeRate)
                {
                    exchangeButtonText.text = $"Не хватает\n({exchangeRate} монет)";
                }
                else
                {
                    exchangeButtonText.text = "Обменять";
                }
            }
            else
            {
                exchangeButtonText.text = "Обменять\n100 монет → 1 жизнь";
            }

            exchangeCoinsToLivesButton.image.color = exchangeCoinsToLivesButton.interactable ?
                Color.white : Color.gray;
        }
        */
    }

    public void OnAnswerSelected(int selectedAnswer, int correctAnswer, int questionWordIndex)
    {
        foreach (var button in answerButtons)
        {
            button.interactable = false;
        }

        bool isCorrect = (selectedAnswer == correctAnswer);
        int oldStars = stars;

        // Обновляем прогресс слова
        if (moduleManager != null)
        {
            moduleManager.UpdateWordProgress(questionWordIndex, isCorrect);
        }

        if (isCorrect)
        {
            if (audioigra != null && klickaudio != null)
                audioigra.PlayOneShot(klickaudio);

            coins += 5;
            stars += 1;
            correctAnswersInLevel++;

            // Анимация правильного ответа
            PlayCorrectAnswerAnimation();

            // Анимация монетки
            PlayCoinAnimation();

            UpdateMainScreen();

            if (correctAnswer < answerButtons.Length)
                answerButtons[correctAnswer].image.color = Color.green;

            // ИЗМЕНЕНО: Отмечаем ячейку как правильно отвеченную только при правильном ответе
            if (currentCellIndex != -1)
            {
                currentLevelCells[currentCellIndex] = 2; // Только при правильном ответе
                answeredQuestions++;

                SaveGameData();
                if (moduleManager != null)
                    moduleManager.SaveLevelCells(currentLevel, currentLevelCells, levelQuestionIndices);
            }
        }
        else
        {
            if (audioigra != null && noaudio != null)
                audioigra.PlayOneShot(noaudio);

            /*  if (lifeTimer != null && !lifeTimer.IsUnlimitedLivesActive)
                {
                    lives--;
                    if (lifeTimer != null)
                        lifeTimer.ResetTimer();
                }
            */

            if (correctAnswer < answerButtons.Length)
                answerButtons[correctAnswer].image.color = Color.green;

            if (selectedAnswer < answerButtons.Length)
                answerButtons[selectedAnswer].image.color = Color.red;

            UpdateExchangeButton();

            // УДАЛЕНО: Не сохраняем состояние ячейки при неправильном ответе
            // Ячейка останется доступной (состояние 1)
        }

        // Проверяем новый значок
        if (badgeSystem != null)
            badgeSystem.CheckForNewBadgeDuringGame(oldStars, stars, this);

        if (!znachokplashka.activeSelf)
        {
            Invoke("ReturnToLevelScreen", 1.5f);
        }
    }
    void ReturnToLevelScreen()
    {
        gameScreen.SetActive(false);
        levelScreen.SetActive(true);

        CreateGridCells();
        UpdateLevelScreen();

        UpdateZnachkiGrid();

        if (answeredQuestions >= totalQuestionsInLevel)
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        int starsEarned = CalculateStarsEarned();
        int oldStars = stars;

        stars += starsEarned * 10;

        if (badgeSystem != null)
            badgeSystem.CheckAndShowNewBadge(oldStars, stars, znachokplashka, znachokImg, ref lives);

        if (!znachokplashka.activeSelf)
        {
            ShowVictoryScreen(starsEarned);
        }

        currentLevel++;
        SaveGameData();
    }

    int CalculateStarsEarned()
    {
        int starsEarned = 0;

        if (totalQuestionsInLevel > 0)
        {
            float correctPercentage = (float)correctAnswersInLevel / totalQuestionsInLevel;

            if (correctPercentage >= 0.7f)
            {
                starsEarned = 3;
            }
            else if (correctPercentage >= 0.3f)
            {
                starsEarned = 2;
            }
            else if (correctPercentage > 0f)
            {
                starsEarned = 1;
            }
        }

        return starsEarned;
    }

    void ShowVictoryScreen(int starsEarned)
    {
        if (pobedablock != null && pobedablock.activeSelf) return;

        levelScreen.SetActive(false);

        if (pobedablock != null)
        {
            pobedablock.SetActive(true);
            pobedaUroven.text = currentLevel.ToString();
            ShowStarsOnVictoryScreen(starsEarned);

            if (badgeSystem != null)
                badgeSystem.UpdateNextBadgeDisplay(stars, pobedaZnblock, pobedaZnVpered, pobedaznzvvpered);

            if (audioigra != null && pobedaaudio != null)
                audioigra.PlayOneShot(pobedaaudio);
        }
    }

    void ShowStarsOnVictoryScreen(int starsEarned)
    {
        if (zv1 == null || zv2 == null || zv3 == null) return;

        switch (starsEarned)
        {
            case 3:
                zv1.SetActive(true); zv2.SetActive(true); zv3.SetActive(true); break;
            case 2:
                zv1.SetActive(true); zv2.SetActive(true); zv3.SetActive(false); break;
            case 1:
                zv1.SetActive(true); zv2.SetActive(false); zv3.SetActive(false); break;
            case 0:
                zv1.SetActive(false); zv2.SetActive(false); zv3.SetActive(false); break;
        }
    }

    void CreateGridCells()
    {
        if (questionsGrid == null) return;

        foreach (Transform child in questionsGrid)
        {
            Destroy(child.gameObject);
        }

        if (currentLevelCells == null || currentLevelCells.Length != 20) return;

        int questionCounter = 0;

        for (int i = 0; i < 20; i++)
        {
            int cellState = currentLevelCells[i];

            if (cellState == 0)
            {
                if (emptyCellPrefab != null)
                {
                    Instantiate(emptyCellPrefab, questionsGrid);
                }
            }
            else
            {
                if (questionCellPrefab != null)
                {
                    GameObject cell = Instantiate(questionCellPrefab, questionsGrid);
                    QuestionCell cellScript = cell.GetComponent<QuestionCell>();

                    if (cellScript != null)
                    {
                        int questionListIndex = questionCounter;
                        int questionGlobalIndex = -1;

                        if (questionCounter < levelQuestionIndices.Count)
                        {
                            questionGlobalIndex = levelQuestionIndices[questionCounter];
                        }

                        if (questionGlobalIndex != -1 && moduleManager != null)
                        {
                            try
                            {
                                QuestionData questionData =
                                    moduleManager.CreateQuestionData(questionGlobalIndex);

                                if (cellState == 1)
                                {
                                    cellScript.Initialize(questionListIndex, questionData, OnQuestionCellClicked);
                                }
                                else if (cellState == 2)
                                {
                                    cellScript.SetState(true, true);
                                    cellScript.MakeNonClickable();
                                }
                                else if (cellState == 3)
                                {
                                    cellScript.SetState(true, false);
                                    cellScript.MakeNonClickable();
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"Error creating question: {e.Message}");
                                Destroy(cell);
                                Instantiate(emptyCellPrefab, questionsGrid);
                            }
                        }

                        questionCounter++;
                    }
                }
            }
        }
    }

    void OnQuestionCellClicked(int questionListIndex)
    {
        /* if (lives <= 0 && (lifeTimer == null || !lifeTimer.IsUnlimitedLivesActive))
        {
            ShowNoLivesPopup();
            return;
        }
        */

        currentCellIndex = -1;
        currentQuestionIndex = -1;

        int questionCounter = 0;
        for (int i = 0; i < 20; i++)
        {
            if (currentLevelCells[i] != 0)
            {
                if (questionCounter == questionListIndex)
                {
                    currentCellIndex = i;
                    currentQuestionIndex = questionListIndex;
                    break;
                }
                questionCounter++;
            }
        }

        if (currentQuestionIndex >= 0 && currentQuestionIndex < levelQuestionIndices.Count)
        {
            int questionGlobalIndex = levelQuestionIndices[currentQuestionIndex];

            try
            {
                if (moduleManager != null)
                {
                    QuestionData questionData =
                        moduleManager.CreateQuestionData(questionGlobalIndex);

                    levelScreen.SetActive(false);
                    gameScreen.SetActive(true);

                    if (questionSystem != null)
                        questionSystem.LoadQuestion(questionData, questionText, questionImage, answerButtons, answerTexts, this);

                    // НОВОЕ: Воспроизводим аудио для вопроса
                    if (autoPlayQuestionAudio && questionData != null)
                    {
                        PlayQuestionAudio(questionData.imageName);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading question: {e.Message}");
                gameScreen.SetActive(false);
                levelScreen.SetActive(true);
            }
        }
    }



  
    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    public void PlayCorrectAnswerAnimation()
    {
        /*
          if (correctAnswerAnimObject != null)
          {
              Animator anim = correctAnswerAnimObject.GetComponent<Animator>();
              if (anim != null)
              {
                  anim.SetTrigger("PlayCorrect");
              }
              else
              {
                  correctAnswerAnimObject.SetActive(true);
                  Invoke("HideCorrectAnswerAnimation", 1f);
              }
          }
        */
        correctAnswerAnimObject.SetActive(true);



    }

    void HideCorrectAnswerAnimation()
    {
        if (correctAnswerAnimObject != null)
        {
            correctAnswerAnimObject.SetActive(false);
        }
    }

    void PlayCoinAnimation()
    {
        if (coinParticleSystem != null)
        {
            coinParticleSystem.Play();
        }

        if (coinAnimObject != null && coinStartPosition != null && coinTargetPosition != null)
        {
            StartCoroutine(CoinFlyAnimation());
        }

        StartCoroutine(CoinCountAnimation());
    }

    System.Collections.IEnumerator CoinFlyAnimation()
    {
        GameObject coin = Instantiate(coinAnimObject, coinStartPosition.position, Quaternion.identity);
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            coin.transform.position = Vector3.Lerp(coinStartPosition.position,
                                                  coinTargetPosition.position,
                                                  t);
            coin.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        Destroy(coin);
    }

    System.Collections.IEnumerator CoinCountAnimation()
    {
        if (coinsText != null)
        {
            Vector3 originalScale = coinsText.transform.localScale;
            coinsText.transform.localScale = originalScale * 1.3f;
            yield return new WaitForSeconds(0.1f);
            coinsText.transform.localScale = originalScale;
        }
    }

    public void LoadGameData()
    {
        currentLevel = PlayerPrefs.GetInt("money", 1);
        stars = PlayerPrefs.GetInt("monetki", 0);
        lives = PlayerPrefs.GetInt("lives", 5);
        coins = PlayerPrefs.GetInt("coins", 0);
        audVkl = PlayerPrefs.GetInt("audio", 0);

        if (lives > 5) lives = 5;

        UpdateAudioSettings();
        UpdateMainScreen();
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("money", currentLevel);
        PlayerPrefs.SetInt("monetki", stars);
        PlayerPrefs.SetInt("lives", lives);
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.SetInt("audio", audVkl);
        PlayerPrefs.Save();
    }

    public void UpdateAudioSettings()
    {
        if (audVkl == 1)
        {
            AudioListener.volume = 0;
            if (knopVikl != null) knopVikl.SetActive(false);
            if (knopVkl != null) knopVkl.SetActive(true);
        }
        else
        {
            AudioListener.volume = 1;
            if (knopVikl != null) knopVikl.SetActive(true);
            if (knopVkl != null) knopVkl.SetActive(false);
        }
    }

    public void ShowNoLivesPopup()
    {
        if (noLivesPopup != null)
            noLivesPopup.SetActive(true);
    }

    public void OnNoLivesPopupClose()
    {
        if (noLivesPopup != null)
            noLivesPopup.SetActive(false);
    }

    public void PobedaSborVoid()
    {
        Invoke("CloseVictoryScreen", 1.5f);
    }

    void CloseVictoryScreen()
    {
        if (pobedablock != null)
            pobedablock.SetActive(false);

        levelScreen.SetActive(false);
        startGameScreen.SetActive(true);
        UpdateMainScreen();
    }

    public void ProigrVoid()
    {
       /* if (lifeTimer != null && !lifeTimer.IsUnlimitedLivesActive)
        {
            lives++;
            if (lifeTimer != null)
                lifeTimer.ResetTimer();
            SaveGameData();
            UpdateMainScreen();
        }
       */
        if (proigrblock != null) proigrblock.SetActive(false);
    }

    public void ZnachkiBtnVoid()
    {
        if (znachokplashka != null && znachokplashka.activeSelf)
        {
            znachokplashka.SetActive(false);
            Time.timeScale = 1f;

            UpdateMainScreen();

            if (answeredQuestions >= totalQuestionsInLevel)
            {
                int starsEarned = CalculateStarsEarned();
                ShowVictoryScreen(starsEarned);
            }
            else
            {
                ReturnToLevelScreen();
            }
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();

        currentLevel = 1;
        stars = 0;
        coins = 0;
        lives = 5;
        currentLevelCells = new int[20];
        levelQuestionIndices.Clear();

        if (moduleManager != null)
            moduleManager.ResetModuleProgress();

        UpdateMainScreen();
        UpdateZnachkiGrid();

        SaveGameData();
    }

    public void audViklVoid()
    {
        audVkl = 1;
        SaveGameData();
        UpdateAudioSettings();
    }

    public void audVklVoid()
    {
        audVkl = 0;
        SaveGameData();
        UpdateAudioSettings();
    }

    public void OnBackFromLevelButtonClicked()
    {
        levelScreen.SetActive(false);
        startGameScreen.SetActive(true);
        UpdateMainScreen();
        SaveGameData();
    }

    public void OnExitFromQuestionButtonClicked()
    {
        gameScreen.SetActive(false);
        levelScreen.SetActive(true);
        UpdateLevelScreen();
    }

    void OnApplicationPause(bool pauseStatus)
    {
       /* if (pauseStatus)
        {
            if (lifeTimer != null)
                lifeTimer.SaveLifeTimerData(lives);
        }
        else
        {
            if (lifeTimer != null)
            {
                int loadedLives = lives;
                lifeTimer.LoadLifeTimerData(ref loadedLives, ref lifeTimerText, this);
                lives = loadedLives;
                lifeTimer.CheckUnlimitedLives(this);
            }
        }
       */
    }

    void OnApplicationQuit()
    {
       // if (lifeTimer != null)
       //     lifeTimer.SaveLifeTimerData(lives);
    }

    // === ДЛЯ ДОСТУПА ИЗ ДРУГИХ СКРИПТОВ ===

    public void SetCurrentLevelCells(int[] cells)
    {
        currentLevelCells = cells;
    }

    public void SetLevelQuestionIndices(List<int> indices)
    {
        levelQuestionIndices = indices;
        totalQuestionsInLevel = indices.Count;
    }

    public int[] GetCurrentLevelCells()
    {
        return currentLevelCells;
    }

    public List<int> GetLevelQuestionIndices()
    {
        return levelQuestionIndices;
    }

    public void SetLevelVariables(int total, int answered, int correct)
    {
        totalQuestionsInLevel = total;
        answeredQuestions = answered;
        correctAnswersInLevel = correct;
    }

    public void SetLives(int newLives)
    {
        lives = newLives;
        UpdateMainScreen();
    }

    public void SetStars(int newStars)
    {
        stars = newStars;
        UpdateMainScreen();
    }

    // === МЕТОДЫ ДЛЯ СОВМЕСТИМОСТИ СО СТАРЫМ КОДОМ ===

    // Метод для награды за просмотр рекламы (используется в YandexMobileAdsRewardedAdDemoScript)
    public void Reward()
    {
      /*  if (lifeTimer != null && !lifeTimer.IsUnlimitedLivesActive)
        {
            lives += 1;
            UpdateMainScreen();
            UpdateExchangeButton();
        }
        else
        {
            Debug.Log("Бесконечные жизни активны - реклама за жизнь не нужна");
        }
      */
    }

    // Метод для отключения рекламы (norek1)
    public void DisableAds()
    {
        PlayerPrefs.SetInt("ads_disabled", 1);
        PlayerPrefs.Save();

        Debug.Log("Реклама отключена (покупка norek1)");
        noRek = true;
    }

    // Метод для активации бесконечных жизней на 3 часа
    public void ActivateUnlimitedLives3Hours()
    {
       /* if (lifeTimer != null)
        {
            lifeTimer.ActivateUnlimitedLives(3, this);
            Debug.Log("Активированы бесконечные жизни на 3 часа");
            lives += 5;
            UpdateMainScreen();
            UpdateExchangeButton();
        }
       */
    }

    // Метод для активации бесконечных жизней на 24 часа
    public void ActivateUnlimitedLives24Hours()
    {
       /* if (lifeTimer != null)
        {
            lifeTimer.ActivateUnlimitedLives(24, this);
            Debug.Log("Активированы бесконечные жизни на 24 часа");
            lives += 5;
            UpdateMainScreen();
            UpdateExchangeButton();
        }
       */
    }

    // Метод для проверки, отключена ли реклама
    public bool IsAdsDisabled()
    {
        return PlayerPrefs.GetInt("ads_disabled", 0) == 1;
    }

    // Метод для активации бесконечных жизней (общий, используется другими скриптами)
    public void ActivateUnlimitedLives(int hours)
    {
      /*  if (lifeTimer != null)
        {
            lifeTimer.ActivateUnlimitedLives(hours, this);
            Debug.Log($"Активированы бесконечные жизни на {hours} часов");
            lives += 5;
            UpdateMainScreen();
            UpdateExchangeButton();
        }
      */
    }
}