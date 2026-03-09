/*
using UnityEngine;
using System;
using NotificationSamples;
using TMPro;
using UnityEngine.Android; // Добавь это вверху
public class Notif : MonoBehaviour
{

    [SerializeField] private GameNotificationsManager notificationsManager;
    private int notificationDelay;
    public TextMeshProUGUI blbl;

    private void InitializeNotifications()
    {
        GameNotificationChannel channel = new GameNotificationChannel("mntutorial", "первое туториал", "втторое тутриал");
        notificationsManager.Initialize(channel);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Запрос разрешения для Android 13+
#if UNITY_ANDROID && !UNITY_EDITOR
    if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
    {
        Debug.Log("Requesting notification permission...");
        Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
    }
#endif

    }


    public void OnTimeInput(string text)
    {
        if (int.TryParse(text, out int sec))
            notificationDelay = sec;
    }


    public void CreateNotificationbt(int nn) {
        blbl.text = nn.ToString();
        CreateNotification("заголов", "dskjfhksdj dsfsdf sd fs df sd", DateTime.Now.AddSeconds(nn));
    }
    public void CreateNotificationbtdd(int nn)
    {
        blbl.text = nn.ToString();
        CreateNotification("aaaaaaaa", "rrrrrrrrrdskjfhksdj dsfsdf sd fs df sd", DateTime.Now.AddSeconds(nn));
    }
    public void CreateNotificationbtddd(int nn)
    {
        blbl.text = nn.ToString();
        CreateNotification("bbbbbbbb", "fffffffdskjfhksdj dsfsdf sd fs df sd", DateTime.Now.AddSeconds(nn));
    }

    private void CreateNotification(string title, string body, DateTime time)
    {
        IGameNotification notification = notificationsManager.CreateNotification();
        if(notification != null )
        {
            notification.Title = title;
            notification.Body = body;
            notification.DeliveryTime = time;
            notificationsManager.ScheduleNotification(notification);



        }
    }








    // Update is called once per frame
    void Update()
    {
        
    }
}
*/