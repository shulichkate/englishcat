using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using RuStore.PayClient;
using UnityEngine.Networking;

namespace RuStore.PayExample.UI {

    public class ProductCardView : MonoBehaviour, ICardView<Product> {

        [SerializeField]
        private RawImage productImage;

        [SerializeField]
        private Text productId;

        [SerializeField]
        private Text productTitle;

        [SerializeField]
        private Text productType;

        [SerializeField]
        private Text productAmount;

        [SerializeField]
        private Text productPrice;

        public static event EventHandler OnBuyProduct;

        private Product product = null;

        public void SetData(Product product) {
            this.product = product;

            StartCoroutine(LoadImage(product.imageUrl.value));

            if (productId != null) productId.text = product.productId.value;
            if (productTitle != null) productTitle.text = product.title.value;
            if (productType != null) productType.text = product.type.ToString();
            if (productAmount != null) productAmount.text = product.amountLabel.value;
            if (productPrice != null) productPrice.text = product.price?.value.ToString();
        }

        public Product GetData() {
            return product;
        }

        public void BuyProduct() {
            OnBuyProduct?.Invoke(this, EventArgs.Empty);
        }

        IEnumerator LoadImage(string url) {
            if (string.IsNullOrEmpty(url)) yield break;

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url)) {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success){
                    productImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                }
            }
        }
    }
}
