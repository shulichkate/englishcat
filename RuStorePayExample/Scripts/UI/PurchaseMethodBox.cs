using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RuStore.PayExample.UI {

    public class PurchaseMethodBox : MonoBehaviour {

        [SerializeField]
        private Text _title;

        private Action _onPreferredOneStep;
        private Action _onPreferredTwoStep;
        private Action _onTwoStep;
        private Action _onCancel;

        public void Show(string title, Action onPreferredOneStep = null, Action onPreferredTwoStep = null, Action onTwoStep = null, Action onCancel = null) {
            _title.text = title;
            _onPreferredOneStep = onPreferredOneStep;
            _onPreferredTwoStep = onPreferredTwoStep;
            _onTwoStep = onTwoStep;
            _onCancel = onCancel != null ? onCancel : () => { gameObject.SetActive(false); };
            gameObject.SetActive(true);
        }

        public void PreferredOneStep() {
            gameObject.SetActive(false);
            _onPreferredOneStep?.Invoke();
        }

        public void PreferredTwoStep() {
            gameObject.SetActive(false);
            _onPreferredTwoStep?.Invoke();
        }

        public void TwoStep() {
            gameObject.SetActive(false);
            _onTwoStep?.Invoke();
        }

        public void Cancel() {
            gameObject.SetActive(false);
            _onCancel?.Invoke();
        }
    }
}
