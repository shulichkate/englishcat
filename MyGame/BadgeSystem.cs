using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BadgeSystem : MonoBehaviour
{
    private Sprite[] znachok;
    private int[] zvezdZnach;

    void Start()
    {
        LoadBadgesFromResources();
    }

    public void LoadBadgesFromResources()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("znachki");

        if (loadedSprites != null && loadedSprites.Length > 0)
        {
            znachok = loadedSprites;
            Debug.Log($"Загружено {znachok.Length} значков из Resources/znachki");

            // Создаем массив звезд для значков
            zvezdZnach = new int[znachok.Length];
            for (int i = 0; i < znachok.Length; i++)
            {
                int[] fixedValues = { 15, 50, 100, 200, 350, 550, 800, 1100, 1500, 2000,
                                     2600, 3300, 4100, 5000, 6000, 7200, 8600, 10200,
                                     12000, 14000, 16500, 19500, 23000, 27000, 31500,
                                     36500, 42000, 48000, 55000, 65000 };

                if (i < fixedValues.Length)
                    zvezdZnach[i] = fixedValues[i];
                else
                    zvezdZnach[i] = fixedValues[fixedValues.Length - 1] + (i - fixedValues.Length + 1) * 5000;
            }
        }
        else
        {
            Debug.LogWarning("Не найдены значки в Resources/znachki. Создаем тестовые значки.");

            // Создаем тестовые значки, если в Resources нет
            CreateTestBadges();
        }
    }

    void CreateTestBadges()
    {
        // Создаем 10 тестовых значков
        znachok = new Sprite[10];
        zvezdZnach = new int[10];

        int[] testValues = { 15, 50, 100, 200, 350, 550, 800, 1100, 1500, 2000 };

        for (int i = 0; i < 10; i++)
        {
            // Создаем тестовую текстуру
            Texture2D texture = new Texture2D(100, 100);
            Color[] colors = new Color[100 * 100];

            // Заполняем разными цветами для теста
            for (int j = 0; j < colors.Length; j++)
            {
                float r = (float)i / 10f;
                float g = 0.5f;
                float b = 1.0f - (float)i / 10f;
                colors[j] = new Color(r, g, b);
            }
            texture.SetPixels(colors);
            texture.Apply();

            // Создаем спрайт
            znachok[i] = Sprite.Create(texture, new Rect(0, 0, 100, 100), Vector2.one * 0.5f);
            zvezdZnach[i] = testValues[i];
        }

        Debug.Log($"Создано {znachok.Length} тестовых значков");
    }

    public void UpdateZnachkiGrid(int stars, GameObject zngrid)
    {
        if (zngrid == null || znachok == null || zvezdZnach == null)
        {
            Debug.LogWarning("Не инициализированы данные для отображения значков");
            return;
        }

        // Очищаем сетку
        foreach (Transform child in zngrid.transform)
        {
            Destroy(child.gameObject);
        }

        int badgeCount = Mathf.Min(znachok.Length, zvezdZnach.Length);

        for (int i = 0; i < badgeCount; i++)
        {
            GameObject badgeObj = new GameObject($"Badge_{i}");
            RectTransform rt = badgeObj.AddComponent<RectTransform>();
            badgeObj.transform.SetParent(zngrid.transform);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(100, 100);

            Image badgeImage = badgeObj.AddComponent<Image>();
            badgeImage.sprite = znachok[i];
            badgeImage.preserveAspect = true;

            bool isUnlocked = stars >= zvezdZnach[i];

            if (isUnlocked)
            {
                badgeImage.color = Color.white;
            }
            else
            {
                badgeImage.color = new Color(1f, 1f, 1f, 0.3f);

                GameObject textObj = new GameObject("StarsText");
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textObj.transform.SetParent(badgeObj.transform);
                textRt.localPosition = Vector3.zero;
                textRt.sizeDelta = new Vector2(100, 100);

                TextMeshProUGUI starsText = textObj.AddComponent<TextMeshProUGUI>();
                starsText.text = zvezdZnach[i].ToString();
                starsText.alignment = TextAlignmentOptions.Center;
                starsText.verticalAlignment = VerticalAlignmentOptions.Middle;
                starsText.fontSize = 50;
                starsText.color = Color.white;
                starsText.fontStyle = FontStyles.Bold;
            }
        }

        Debug.Log($"Обновлена сетка значков: {badgeCount} значков, звезд: {stars}");
    }

    public void CheckAndShowNewBadge(int oldStars, int stars, GameObject znachokplashka, Image znachokImg, ref int lives)
    {
        if (znachok == null || zvezdZnach == null || znachokplashka == null || znachokImg == null)
        {
            Debug.LogWarning("Не инициализированы данные для проверки значков");
            return;
        }

        for (int i = 0; i < zvezdZnach.Length && i < znachok.Length; i++)
        {
            if (stars >= zvezdZnach[i] && oldStars < zvezdZnach[i])
            {
                ShowNewBadgeWithReward(i, znachokplashka, znachokImg, ref lives);
                break;
            }
        }
    }

    void ShowNewBadgeWithReward(int badgeIndex, GameObject znachokplashka, Image znachokImg, ref int lives)
    {
        if (badgeIndex < 0 || badgeIndex >= znachok.Length) return;

        Time.timeScale = 0f;
        znachokplashka.SetActive(true);
        znachokImg.sprite = znachok[badgeIndex];

        lives += 10;
        Debug.Log($"?? Новый значок {badgeIndex}! +10 жизней. Всего жизней: {lives}");
    }

    public void CheckForNewBadgeDuringGame(int oldStars, int stars, SaverTest saver)
    {
        if (znachok == null || zvezdZnach == null ||
            saver.znachokplashka == null || saver.znachokImg == null)
        {
            Debug.LogWarning("Не инициализированы данные для проверки значков в игре");
            return;
        }

        for (int i = 0; i < zvezdZnach.Length && i < znachok.Length; i++)
        {
            if (stars >= zvezdZnach[i] && oldStars < zvezdZnach[i])
            {
                ShowBadgePopupImmediately(i, saver);
                break;
            }
        }
    }

    void ShowBadgePopupImmediately(int badgeIndex, SaverTest saver)
    {
        Time.timeScale = 0f;
        saver.znachokplashka.SetActive(true);
        saver.znachokImg.sprite = znachok[badgeIndex];

        saver.lives += 10;

        saver.UpdateMainScreen();
        saver.SaveGameData();

        Debug.Log($"Новый значок получен во время игры! Требовалось: {zvezdZnach[badgeIndex]} звезд");
    }

    public void UpdateNextBadgeDisplay(int stars, GameObject pobedaZnblock, Image pobedaZnVpered, TextMeshProUGUI pobedaznzvvpered)
    {
        if (pobedaZnblock == null || pobedaZnVpered == null || pobedaznzvvpered == null)
        {
            Debug.LogWarning("Не привязаны UI элементы для отображения следующего значка!");
            return;
        }

        if (zvezdZnach == null || znachok == null)
        {
            Debug.LogWarning("Массивы значков не инициализированы!");
            return;
        }

        int nextBadgeIndex = -1;
        for (int i = 0; i < zvezdZnach.Length; i++)
        {
            if (stars < zvezdZnach[i])
            {
                nextBadgeIndex = i;
                break;
            }
        }

        if (nextBadgeIndex >= 0 && nextBadgeIndex < znachok.Length)
        {
            pobedaZnblock.SetActive(true);
            pobedaZnVpered.sprite = znachok[nextBadgeIndex];
            int starsNeeded = zvezdZnach[nextBadgeIndex] - stars;
            pobedaznzvvpered.text = $"До следующего значка: {starsNeeded} звезд";
        }
        else
        {
            pobedaZnblock.SetActive(false);
        }
    }

    // Методы для получения данных
    public Sprite[] GetZnachok()
    {
        return znachok;
    }

    public int[] GetZvezdZnach()
    {
        return zvezdZnach;
    }
}