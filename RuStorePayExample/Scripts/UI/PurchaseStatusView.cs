#nullable enable

using RuStore.PayClient;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RuStore.PayExample.UI {

    public class PurchaseStatusView : MonoBehaviour {
        [SerializeField]
        private Toggle? _allStatuses;

        [SerializeField]
        private Toggle? _paid;

        [SerializeField]
        private Toggle? _confirmed;

        [SerializeField]
        private Toggle? _active;

        [SerializeField]
        private Toggle? _paused;

        private Enum? _state = null;

        public delegate void OnValueChangedEventHandler(object sender, Enum? e);
        public event OnValueChangedEventHandler? onValueChangedEvent;

        void Start() {
            _allStatuses?.onValueChanged.AddListener((isOn) => { if (isOn) SetState(null); });

            _paid?.onValueChanged.AddListener((isOn) => { if (isOn) SetState(ProductPurchaseStatus.PAID); });
            _confirmed?.onValueChanged.AddListener((isOn) => { if (isOn) SetState(ProductPurchaseStatus.CONFIRMED); });

            _active?.onValueChanged.AddListener((isOn) => { if (isOn) SetState(SubscriptionPurchaseStatus.ACTIVE); });
            _paused?.onValueChanged.AddListener((isOn) => { if (isOn) SetState(SubscriptionPurchaseStatus.PAUSED); });
        }

        private void SetState(Enum? value) {
            _state = value;
            onValueChangedEvent?.Invoke(this, value);
        }

        public Enum? GetState() => _state;
    }
}
