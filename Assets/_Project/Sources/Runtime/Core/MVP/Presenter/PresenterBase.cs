using Sources.Runtime.Core.MVP.View;

namespace Sources.Runtime.Core.MVP.Presenter
{
    public abstract class PresenterBase<TModel, TView> : IPresenter<TModel, TView> where TModel : class where TView : class, IView
    {
        protected readonly TModel Model;
        protected readonly TView View;

        public PresenterBase(TModel model, TView view)
        {
            Model = model;
            View = view;
        }

        public abstract void Initialize();

        public abstract void Dispose();
    }
}