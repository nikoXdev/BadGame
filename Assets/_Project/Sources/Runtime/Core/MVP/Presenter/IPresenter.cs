using System;
using Sources.Runtime.Core.Contracts;
using Sources.Runtime.Core.MVP.Model;
using Sources.Runtime.Core.MVP.View;

namespace Sources.Runtime.Core.MVP.Presenter
{
    internal interface IPresenter<TModel, TView> : IInitializable, IDisposable, IModel where TModel : class where TView : class, IView
    {
    }
}