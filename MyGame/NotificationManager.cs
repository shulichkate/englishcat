using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("Настройки уведомлений")]
    public string notificationChannelId = "daily_reminders";
    public string notificationChannelName = "Новые вопросы ждут Вас";

    [Header("Тексты уведомлений")]
    [TextArea(1, 3)]
    public string[] notificationTexts = new string[]
    {
        "Вопросы ждут",
        "Кто самый умный?",
        "Викторина себя не разгадает",
        "Ждем только тебя",
        "А вот и новый вопрос"
    };

    private bool isInitialized = false;
    private int currentTextIndex = 0;
    private int scheduledNotificationId = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNotifications();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeNotifications()
    {
        if (isInitialized) return;

        Debug.Log("Инициализация системы уведомлений...");

        // Создаем канал уведомлений
        var channel = new AndroidNotificationChannel()
        {
            Id = notificationChannelId,
            Name = notificationChannelName,
            Importance = Importance.High,
            Description = "Ежедневные напоминания",
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        isInitialized = true;

        // Очищаем все предыдущие уведомления перед созданием новых
        CancelAllNotifications();

        // Планируем ежедневное уведомление
        ScheduleDailyNotification();
    }

    void ScheduleDailyNotification()
    {
        if (!isInitialized) return;

        // Отменяем предыдущее уведомление, если оно было
        CancelAllNotifications();

        try
        {
            // Устанавливаем время на 15:35
            DateTime today = DateTime.Today;
            DateTime notificationTime = today.AddHours(15).AddMinutes(35);

            // Если время уже прошло сегодня, ставим на завтра
            if (notificationTime <= DateTime.Now)
            {
                notificationTime = notificationTime.AddDays(1);
            }

            // Получаем следующий текст из массива
            string notificationText = GetNextNotificationText();

            var notification = new AndroidNotification();
            notification.Title = "Ежедневное напоминание";
            notification.Text = notificationText;
            notification.FireTime = notificationTime;
            notification.LargeIcon = "icon_large";
            notification.SmallIcon = "icon_small";

            // Устанавливаем ежедневное повторение
            notification.RepeatInterval = TimeSpan.FromDays(1);

            // Создаем уникальный ID для уведомления
            scheduledNotificationId = Mathf.Abs(notificationChannelId.GetHashCode());

            // Отправляем уведомление с указанным ID
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, notificationChannelId, scheduledNotificationId);

            Debug.Log($"Ежедневное уведомление запланировано на {notificationTime.ToString("HH:mm")} с текстом: '{notificationText}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка планирования уведомления: {e.Message}");
        }
    }

    string GetNextNotificationText()
    {
        if (notificationTexts == null || notificationTexts.Length == 0)
        {
            return "Не забудьте выполнить свои задачи!";
        }

        // Получаем текущий текст
        string text = notificationTexts[currentTextIndex];

        // Увеличиваем индекс для следующего раза
        currentTextIndex++;

        // Если дошли до конца массива, начинаем сначала
        if (currentTextIndex >= notificationTexts.Length)
        {
            currentTextIndex = 0;
        }

        return text;
    }

    public void CancelAllNotifications()
    {
        Debug.Log("Очистка всех уведомлений...");
        AndroidNotificationCenter.CancelAllNotifications();
        scheduledNotificationId = -1;
    }

    public void RescheduleNotification()
    {
        Debug.Log("Перепланирование уведомления...");
        ScheduleDailyNotification();
    }

    // Тестовые методы
    public void TestInstantNotification()
    {
        string testText = GetNextNotificationText();

        var notification = new AndroidNotification();
        notification.Title = "Тестовое уведомление";
        notification.Text = testText;
        notification.FireTime = DateTime.Now.AddSeconds(5);
        notification.LargeIcon = "icon_large";
        notification.SmallIcon = "icon_small";

        AndroidNotificationCenter.SendNotification(notification, notificationChannelId);
        Debug.Log($"Тестовое уведомление отправлено: '{testText}'");
    }

    public void TestSpecificTime()
    {
        string testText = GetNextNotificationText();

        var notification = new AndroidNotification();
        notification.Title = "Тест в конкретное время";
        notification.Text = testText;
        notification.FireTime = DateTime.Now.AddMinutes(2);
        notification.LargeIcon = "icon_large";
        notification.SmallIcon = "icon_small";

        AndroidNotificationCenter.SendNotification(notification, notificationChannelId);
        Debug.Log($"Уведомление на 2 минуты вперед: '{testText}'");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("Приложение свернуто, проверяем уведомления...");
            ScheduleDailyNotification();
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Приложение закрывается, планируем уведомление...");
        ScheduleDailyNotification();
    }

    // Метод для ручного обновления текста
    public void ForceNextText()
    {
        string nextText = GetNextNotificationText();
        Debug.Log($"Следующий текст уведомления: '{nextText}'");
    }

    // Метод для проверки статуса
    public void CheckNotificationStatus()
    {
        if (scheduledNotificationId != -1)
        {
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(scheduledNotificationId);
            Debug.Log($"Статус уведомления ID {scheduledNotificationId}: {notificationStatus}");
        }
        else
        {
            Debug.Log("Уведомление не запланировано");
        }
    }
}