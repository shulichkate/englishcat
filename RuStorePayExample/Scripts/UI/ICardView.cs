namespace RuStore.PayExample.UI {

    public interface ICardView<T> {

        T GetData();
        void SetData(T value);
    }
}
