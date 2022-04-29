using QuickMusic3.Core;
using System.ComponentModel;

namespace QuickMusic3.MVVM.ViewModel;

public class HomeViewModel : BaseViewModel
{
    public override SharedState Shared { get; }
    public HomeViewModel(SharedState shared)
    {
        Shared = shared;
    }
}
