using QuickMusic3.Core;

namespace QuickMusic3.MVVM.ViewModel;

public class PlaylistViewModel : BaseViewModel
{
    public override SharedState Shared { get; }
    public PlaylistViewModel(SharedState shared)
    {
        Shared = shared;
    }
}
