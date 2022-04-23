using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;

namespace QuickMusic3.MVVM.ViewModel;

public class NowPlayingViewModel : BaseViewModel
{
    public override SharedState Shared { get; }
    public NowPlayingViewModel(SharedState shared)
    {
        Shared = shared;
    }
}
