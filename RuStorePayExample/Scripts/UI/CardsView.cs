using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuStore.PayExample.UI {
    
    public class CardsView : MonoBehaviour {
        [SerializeField]
        private GameObject prefab;

        private GameObject[] items = { };

        public void SetData<T>(List<T> data) {
            Array.ForEach(items, i => Destroy(i));

            var index = 0;
            items = new GameObject[data.Count];

            data.ForEach(d => {
                var view = items[index++] = Instantiate(prefab, transform).gameObject;
                view.GetComponent<ICardView<T>>().SetData(d);
            });
        }
    }
}
