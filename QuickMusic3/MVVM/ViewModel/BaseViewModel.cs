using QuickMusic3.Core;

namespace QuickMusic3.MVVM.ViewModel;

public abstract class BaseViewModel : ObservableObject
{
    public abstract SharedState Shared { get; }
}
