using RuStore.CoreClient;
using RuStore.PayClient;
using RuStore.PayExample.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RuStore.PayExample {

    public class ExampleController : MonoBehaviour {

        public SaverTest saver;

        public GameObject h3h;
        public GameObject h24h;
        public GameObject rekNo;

        [SerializeField]
        private string logTag;

        [SerializeField]
        private string[] productsId;

        [SerializeField]
        private CardsView productsView;

        [SerializeField]
        private CardsView purchasesView;

        [SerializeField]
        private PurchaseMethodBox purchaseMethodBox;

        [SerializeField]
        private MessageBox messageBox;

        [SerializeField]
        private LoadingIndicator loadingIndicator;

        [SerializeField]
        private Text isPurchaseAvailability;

        [SerializeField]
        private Text isRuStoreInstalledLabel;

        [SerializeField]
        private ProductTypeView productTypeView;

        [SerializeField]
        private PurchaseStatusView purchaseStatusView;

        private void Start() {
            ProductCardView.OnBuyProduct += ProductCardView_OnBuyProduct;

            PurchaseCardView.OnConfirmPurchase += PurchaseCardView_OnConfirmPurchase;
            PurchaseCardView.OnCancelPurchase += PurchaseCardView_OnCancelPurchase;
            PurchaseCardView.OnGetPurchase += PurchaseCardView_OnGetPurchase;

            productTypeView.onValueChangedEvent += ProductTypeView_onValueChangedEvent;
            purchaseStatusView.onValueChangedEvent += PurchaseStatusView_onValueChangedEvent;

            RuStorePayClient.Instance.GetPurchaseAvailability(
                onFailure: (error) => {
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    var message = "Purchases are available: " + result.isAvailable.ToString();
                    isPurchaseAvailability.text = message;
                });

            var isRuStoreInstalled = RuStoreCoreClient.Instance.IsRuStoreInstalled();
            var message = isRuStoreInstalled ? "RuStore is installed [v]" : "RuStore is not installed [x]";
            isRuStoreInstalledLabel.text = message;

          
            CheckPurchasesOnStart();
            GetProducts();
        }

        private void PurchaseStatusView_onValueChangedEvent(object sender, Enum e) => LoadPurchases();

        private void ProductTypeView_onValueChangedEvent(object sender, ProductType? e) => LoadPurchases();

        public void GetUserAuthorizationStatus() {
            loadingIndicator.Show();

            RuStorePayClient.Instance.GetUserAuthorizationStatus(
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    loadingIndicator.Hide();
                    messageBox.Show("UserAuthorizationStatus", result.ToString());
                });
        }

        public void GetPurchaseAvailability() {
            loadingIndicator.Show();

            RuStorePayClient.Instance.GetPurchaseAvailability(
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    loadingIndicator.Hide();

                    if (result.isAvailable) {
                        messageBox.Show("Availability", "True");
                    }
                    else {
                        OnRuStorePaymentException(result.cause);
                    }
                });
        }

        public void GetProducts() {
            loadingIndicator.Show();

            var ids = Array.ConvertAll(productsId, p => new ProductId(p));

            RuStorePayClient.Instance.GetProducts(
                productsId: ids,
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    loadingIndicator.Hide();
                    productsView.SetData(result);

                    var jsonResult = DataSerializer.SerializeToJson(result, true);
                    Logcat.LogWarning(logTag, jsonResult);
                });
        }

        private void ProductCardView_OnBuyProduct(object sender, EventArgs e)
        {
            var product = (sender as ICardView<Product>).GetData();

            var parameters = new ProductPurchaseParams(
                    productId: product.productId
                );

            Action<RuStoreError> onError = (error) => {
                loadingIndicator.Hide();
                OnRuStorePaymentException(error);
            };

            // ИЗМЕНИТЕ ЭТОТ БЛОК:
            Action<ProductPurchaseResult> onSuccess = (result) => {
                loadingIndicator.Hide();

                // Вызываем обработчик по ID товара
                HandlePurchaseByProductId(result, product);

                var jsonResult = DataSerializer.SerializeToJson(result, true);
                Logcat.LogWarning(logTag, jsonResult);
            };

            Action onPreferredOneStep = () => {
                loadingIndicator.Show();
                RuStorePayClient.Instance.Purchase(parameters, PreferredPurchaseType.ONE_STEP, onError, onSuccess);
            };

            Action onPreferredOTwoStep = () => {
                loadingIndicator.Show();
                RuStorePayClient.Instance.Purchase(parameters, PreferredPurchaseType.TWO_STEP, onError, onSuccess);
            };

            Action onTwoStep = () => {
                loadingIndicator.Show();
                RuStorePayClient.Instance.PurchaseTwoStep(parameters, onError, onSuccess);
            };

            purchaseMethodBox?.Show(product.title.value, onPreferredOneStep, onPreferredOTwoStep, onTwoStep);
        }

        public void LoadPurchases() {
            loadingIndicator.Show();
            RuStorePayClient.Instance.GetPurchases(
                productType: productTypeView.GetState(),
                purchaseStatus: purchaseStatusView.GetState(),
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    loadingIndicator.Hide();
                    purchasesView.SetData(result);

                    var jsonResult = DataSerializer.SerializeToJson(result, true);
                    Logcat.LogWarning(logTag, jsonResult);
                });
        }

        private void PurchaseCardView_OnGetPurchase(object sender, EventArgs e) {
            loadingIndicator.Show();

            var purchase = (sender as ICardView<IPurchase>).GetData();

            RuStorePayClient.Instance.GetPurchase(
                purchaseId: purchase.purchaseId,
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: (result) => {
                    loadingIndicator.Hide();
                    messageBox.Show("Purchase", string.Format("Purchase id: {0}", result.purchaseId));

                    var jsonResult = DataSerializer.SerializeToJson(result, true);
                    Logcat.LogWarning(logTag, jsonResult);
                });
        }

        private void PurchaseCardView_OnConfirmPurchase(object sender, EventArgs e) {
            loadingIndicator.Show();

            var purchase = (sender as ICardView<IPurchase>).GetData();
            RuStorePayClient.Instance.ConfirmTwoStepPurchase(
                purchaseId: purchase.purchaseId,
                developerPayload: null,
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: () => {
                    loadingIndicator.Hide();
                    LoadPurchases();
                });
        }

        private void PurchaseCardView_OnCancelPurchase(object sender, EventArgs e) {
            loadingIndicator.Show();

            var purchase = (sender as ICardView<IPurchase>).GetData();
            RuStorePayClient.Instance.CancelTwoStepPurchase(
                purchaseId: purchase.purchaseId,
                onFailure: (error) => {
                    loadingIndicator.Hide();
                    OnRuStorePaymentException(error);
                },
                onSuccess: () => {
                    loadingIndicator.Hide();
                    LoadPurchases();
                });
        }

        public void ShowToast(string message) {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject utils = new AndroidJavaObject("com.plugins.payexample.RuStorePayAndroidUtils")) {
                utils.Call("showToast", currentActivity, message);
            }
        }

        private void OnError(RuStoreError error) {
            messageBox.Show(error.name, error.description);
            Debug.LogErrorFormat("{0} : {1}", error.name, error.description);
        }

        private void OnRuStorePaymentException(RuStoreError error) {
            var message = "";
            switch (error) {
                case RuStorePaymentException.RuStorePaymentNetworkException networkException:
                    message = string.Format(
                        "{0}\ncode: {1}\nid: {2}",
                        networkException.description,
                        networkException.code,
                        networkException.id
                    );

                    messageBox.Show(error.name, message);
                    Debug.LogErrorFormat("{0} : {1}", error.name, message);
                    break;

                case RuStorePaymentException.ProductPurchaseException productPurchaseException:
                    message = string.Format(
                        "Sandbox: {0}",
                        productPurchaseException.sandbox?.ToString() ?? "null"
                    );

                    messageBox.Show(error.name, message);
                    Debug.LogErrorFormat("{0} : {1}", error.name, message);
                    break;

                default:
                    OnError(error);
                    break;
            }
        }

        // Добавьте этот метод в класс ExampleController
        private void HandlePurchaseByProductId(ProductPurchaseResult result, Product product)
        {
            string productId = product.productId.value;

            // Здесь указываете ID ваших товаров из RuStore консоли
            switch (productId)
            {
                case "norek2": // ID товара 1
                    OnPurchaseNoRek();
                 
                    break;

                case "h3h": // ID товара 2
                    OnPurchaseh3h();
                 
                    break;

                case "h24h": // ID товара 3
                    OnPurchaseh24h();
                  
                    break;

                case "premium_monthly": // Подписка 1
                    OnPurchasePremiumMonthly();
                    ShowToast("Премиум на месяц активирован!");
                    break;

                case "premium_yearly": // Подписка 2
                    OnPurchasePremiumYearly();
                    ShowToast("Премиум на год активирован!");
                    break;

                default:
                    Debug.LogWarning($"Unknown product ID: {productId}");
                    ShowToast("Товар куплен!");
                    break;
            }
        }

        // Ваши методы для каждого товара (добавьте их в класс ExampleController)
        private void OnPurchaseNoRek()
        {
            saver.DisableAds();
        }

        private void OnPurchaseh3h()
        {
            saver.ActivateUnlimitedLives3Hours();


        }

        private void OnPurchaseh24h()
        {

            saver.ActivateUnlimitedLives24Hours();

        }

        private void OnPurchasePremiumMonthly()
        {
            // Ваш метод для подписки 1
            Debug.Log("Активирована месячная подписка");
            // Например: SubscriptionManager.Instance.ActivatePremium(30);
        }

        private void OnPurchasePremiumYearly()
        {
            // Ваш метод для подписки 2
            Debug.Log("Активирована годовая подписка");
            // Например: SubscriptionManager.Instance.ActivatePremium(365);
        }









        // Новый метод для проверки покупок при старте
        private void CheckPurchasesOnStart()
        {
            // Получаем ВСЕ покупки пользователя (только CONFIRMED/ACTIVE статусы)
            RuStorePayClient.Instance.GetPurchases(
                productType: null, // Все типы товаров
                purchaseStatus: null, // Все статусы (по умолчанию CONFIRMED и ACTIVE)
                onFailure: (error) => {
                    Debug.LogWarning("Не удалось проверить покупки при старте: " + error.description);
            // Можно загрузить локальное сохранение как fallback
            LoadLocalPurchaseState();
                },
                onSuccess: (purchases) => {
                    Debug.Log($"Найдено покупок при старте: {purchases.Count}");

            // Проходим по всем покупкам и активируем соответствующие функции
            foreach (var purchase in purchases)
                    {
                // Проверяем, что покупка подтверждена или активна
                if (IsPurchaseActive(purchase))
                        {
                            ActivatePurchasedProductOnStart(purchase);
                        }
                    }
                });
        }

        // Проверка, активна ли покупка
        private bool IsPurchaseActive(IPurchase purchase)
        {
            string status = purchase.status.ToString();

            // Для продуктов
            if (purchase is ProductPurchase)
            {
                return status == "CONFIRMED" || status == "PAID";
            }
            // Для подписок
            else if (purchase is SubscriptionPurchase)
            {
                return status == "ACTIVE";
            }

            return false;
        }

        // Активация купленного товара при старте
        private void ActivatePurchasedProductOnStart(IPurchase purchase)
        {
            string productId = "";

            if (purchase is ProductPurchase productPurchase)
            {
                productId = productPurchase.productId.value;
            }
            else if (purchase is SubscriptionPurchase subscriptionPurchase)
            {
                productId = subscriptionPurchase.productId.value;
            }

            if (!string.IsNullOrEmpty(productId))
            {
                Debug.Log($"Активируем купленный товар при старте: {productId}");

                switch (productId)
                {
                    case "norek2":
                        OnPurchaseNoRek();
                        Debug.Log("Товар 'norek1' активирован (отключение рекламы)");
                        break;

 

                    default:
                        Debug.Log($"Неизвестный продукт при старте: {productId}");
                        break;
                }
            }
        }

        // Fallback: загрузка локального сохранения (на случай если RuStore недоступен)
        private void LoadLocalPurchaseState()
        {
            // Можно сохранять в PlayerPrefs при успешной покупке
            if (PlayerPrefs.GetInt("norek1_purchased", 0) == 1)
            {
                OnPurchaseNoRek();
            }

        }






    }
}
