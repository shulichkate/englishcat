using System;
using UnityEngine;
using UnityEngine.UI;
using RuStore.PayClient;

namespace RuStore.PayExample.UI {

    public class PurchaseCardView : MonoBehaviour, ICardView<IPurchase> {

        [SerializeField]
        private Text purchaseId;

        [SerializeField]
        private Text invoiceId;

        [SerializeField]
        private Text productId;

        [SerializeField]
        private Text orderId;

        [SerializeField]
        private Text amount;

        [SerializeField]
        private Text time;

        [SerializeField]
        private Text status;

        public static event EventHandler OnConfirmPurchase;
        public static event EventHandler OnCancelPurchase;
        public static event EventHandler OnGetPurchase;

        private IPurchase purchase = null;

        public void SetData(IPurchase purchase) {
            this.purchase = purchase;

            if (purchaseId != null) purchaseId.text = purchase.purchaseId.value;
            if (invoiceId != null) invoiceId.text = purchase.invoiceId.value;
            if (orderId != null) orderId.text = purchase.orderId?.value;
            if (amount != null) amount.text = purchase.amountLabel.value;
            if (time != null) time.text = BuildLocalDateTimeString(purchase.purchaseTime);
            if (status != null) status.text = purchase.status.ToString();

            if (productId != null) {
                if (purchase is ProductPurchase productPurchase) SetProductPurchaseData(productPurchase);
                if (purchase is SubscriptionPurchase subscriptionPurchase) SetSubscriptionPurchaseData(subscriptionPurchase);
            }
        }

        private void SetProductPurchaseData(ProductPurchase purchase) {
            productId.text = purchase.productId.value;
        }

        private void SetSubscriptionPurchaseData(SubscriptionPurchase purchase) {
            productId.text = purchase.productId.value;
        }

        private string BuildLocalDateTimeString(DateTime? utcDateTime) {
            if (utcDateTime == null) return null;

            var localDateTime = utcDateTime.Value.ToLocalTime();

            var offset = TimeZoneInfo.Local.GetUtcOffset(localDateTime);
            var signString = offset >= TimeSpan.Zero ? "+" : "-";
            var utcOffsetString = $"(UTC{signString}{offset:hh\\:mm})";

            var zoneId = new AndroidJavaObject("java.util.TimeZone")
                .CallStatic<AndroidJavaObject>("getDefault")
                .Call<string>("getID");
            var parts = zoneId.Split('/');
            var localZoneName = (parts.Length > 1 ? parts[^1] : zoneId).Replace("_", " ");

            return string.Format("{0} {1} {2}", localDateTime.ToString(), utcOffsetString, localZoneName);
        }

        public IPurchase GetData() => purchase;

        public void ConfirmPurchase() => OnConfirmPurchase?.Invoke(this, EventArgs.Empty);

        public void CancelPurchase() => OnCancelPurchase?.Invoke(this, EventArgs.Empty);

        public void GetPurchase() => OnGetPurchase?.Invoke(this, EventArgs.Empty);
    }
}
