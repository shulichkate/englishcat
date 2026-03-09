using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CatRoomManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemId;              // Уникальный ID (например "cushion")
        public int price;                   // Цена в монетах
        public GameObject shopObject;       // ГОТОВЫЙ объект в магазине (уже лежит на сцене)
        public GameObject roomObject;       // Объект в комнате (который будет включаться)
        public bool isPurchased = false;    // Куплено или нет
    }

    [Header("Все предметы магазина")]
    public List<ShopItem> items = new List<ShopItem>();

    [Header("Ссылки")]
    public SaverTest saver;
    public TextMeshProUGUI coinsText;       // Текст с монетами в магазине
    public Button openShopButton;           // Кнопка открытия магазина
    public Button closeShopButton;          // Кнопка закрытия магазина
    public GameObject shopPanel;            // Панель магазина

    private string saveKey = "CatRoomData";

    void Start()
    {
        LoadData();
        UpdateShopAndRoom();

        // Настраиваем кнопки
        if (openShopButton != null)
            openShopButton.onClick.AddListener(OpenShop);

        if (closeShopButton != null)
            closeShopButton.onClick.AddListener(CloseShop);

        // Изначально магазин закрыт
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    void OpenShop()
    {
        if (shopPanel != null)
        {
            // При открытии обновляем цены и кнопки
            UpdateShopItems();
            shopPanel.SetActive(true);
            UpdateCoinsDisplay();
            Time.timeScale = 0f;
        }
    }

    void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // Обновляем ВСЕ предметы в магазине и комнате
    void UpdateShopAndRoom()
    {
        foreach (var item in items)
        {
            // Обновляем магазин
            if (item.shopObject != null)
            {
                // Если куплено - отключаем в магазине
                item.shopObject.SetActive(!item.isPurchased);

                // Если НЕ куплено - обновляем цену и кнопку
                if (!item.isPurchased)
                {
                    // Обновляем текст цены
                    TextMeshProUGUI priceText = item.shopObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (priceText != null)
                    {
                        priceText.text = item.price.ToString();
                    }

                    // Настраиваем кнопку
                    Button buyButton = item.shopObject.GetComponentInChildren<Button>();
                    if (buyButton != null)
                    {
                        buyButton.onClick.RemoveAllListeners();
                        string currentId = item.itemId;
                        buyButton.onClick.AddListener(() => BuyItem(currentId));
                    }
                }
            }

            // Обновляем комнату
            if (item.roomObject != null)
            {
                item.roomObject.SetActive(item.isPurchased);
            }
        }
    }

    // Обновляем только цены и кнопки (при открытии магазина)
    void UpdateShopItems()
    {
        foreach (var item in items)
        {
            if (item.shopObject != null && !item.isPurchased)
            {
                // Обновляем текст цены
                TextMeshProUGUI priceText = item.shopObject.GetComponentInChildren<TextMeshProUGUI>();
                if (priceText != null)
                {
                    priceText.text = item.price.ToString();
                }

                // Настраиваем кнопку
                Button buyButton = item.shopObject.GetComponentInChildren<Button>();
                if (buyButton != null)
                {
                    buyButton.onClick.RemoveAllListeners();
                    string currentId = item.itemId;
                    buyButton.onClick.AddListener(() => BuyItem(currentId));
                }
            }
        }
    }

    void LoadData()
    {
        string savedData = PlayerPrefs.GetString(saveKey, "");

        if (!string.IsNullOrEmpty(savedData))
        {
            string[] purchasedIds = savedData.Split(',');

            foreach (string id in purchasedIds)
            {
                if (string.IsNullOrEmpty(id)) continue;

                foreach (var item in items)
                {
                    if (item.itemId == id)
                    {
                        item.isPurchased = true;
                        break;
                    }
                }
            }

            Debug.Log($"Загружено {purchasedIds.Length} купленных предметов");
        }
    }

    void SaveData()
    {
        List<string> purchasedIds = new List<string>();

        foreach (var item in items)
        {
            if (item.isPurchased)
            {
                purchasedIds.Add(item.itemId);
            }
        }

        string savedData = string.Join(",", purchasedIds);
        PlayerPrefs.SetString(saveKey, savedData);
        PlayerPrefs.Save();

        Debug.Log($"Сохранено {purchasedIds.Count} купленных предметов");
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null && saver != null)
        {
            coinsText.text = $"Монет: {saver.coins}";
        }
    }

    void BuyItem(string itemId)
    {
        ShopItem targetItem = null;
        foreach (var item in items)
        {
            if (item.itemId == itemId)
            {
                targetItem = item;
                break;
            }
        }

        if (targetItem == null)
        {
            Debug.LogError($"Предмет с ID {itemId} не найден");
            return;
        }

        if (targetItem.isPurchased)
        {
            Debug.Log("Предмет уже куплен");
            return;
        }

        if (saver != null && saver.coins >= targetItem.price)
        {
            saver.coins -= targetItem.price;
            saver.SaveGameData();

            targetItem.isPurchased = true;
            SaveData();

            // Отключаем в магазине
            if (targetItem.shopObject != null)
            {
                targetItem.shopObject.SetActive(false);
            }

            // Включаем в комнате
            if (targetItem.roomObject != null)
            {
                targetItem.roomObject.SetActive(true);
            }

            UpdateCoinsDisplay();

            Debug.Log($" Куплен {targetItem.itemId} за {targetItem.price} монет");
        }
        else
        {
            Debug.Log($" Не хватает монет! Нужно {targetItem.price}, есть {saver?.coins}");
        }
    }

    public void ResetAllPurchases()
    {
        PlayerPrefs.DeleteKey(saveKey);

        foreach (var item in items)
        {
            item.isPurchased = false;
        }

        UpdateShopAndRoom();
        Debug.Log("Все покупки сброшены");
    }
}