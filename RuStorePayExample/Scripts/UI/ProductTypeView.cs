using RuStore.PayClient;
using UnityEngine;
using UnityEngine.UI;

namespace RuStore.PayExample.UI {

    public class ProductTypeView : MonoBehaviour {
        [SerializeField]
        private Toggle _allTypes;

        [SerializeField]
        private Toggle _consumable;

        [SerializeField]
        private Toggle _nonConsumable;

        [SerializeField]
        private Toggle _subscription;

        private ProductType? _state = null;

        public delegate void OnValueChangedEventHandler(object sender, ProductType? e);
        public event OnValueChangedEventHandler onValueChangedEvent;

        void Start() {

            _allTypes.onValueChanged.AddListener((isOn) => { if (isOn) SetState(null); });
            _consumable.onValueChanged.AddListener((isOn) => { if (isOn) SetState(ProductType.CONSUMABLE_PRODUCT); });
            _nonConsumable.onValueChanged.AddListener((isOn) => { if (isOn) SetState(ProductType.NON_CONSUMABLE_PRODUCT); });
            _subscription.onValueChanged.AddListener((isOn) => { if (isOn) SetState(ProductType.SUBSCRIPTION); });
        }

        private void SetState(ProductType? value) {
            _state = value;
            onValueChangedEvent?.Invoke(this, value);
        }

        public ProductType? GetState() => _state;
    }
}
