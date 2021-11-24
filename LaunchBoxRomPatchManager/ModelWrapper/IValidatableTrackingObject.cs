using System.ComponentModel;

namespace LaunchBoxRomPatchManager.ModelWrapper
{
    public interface IValidatableTrackingObject : IRevertibleChangeTracking, INotifyPropertyChanged
    {
        bool IsValid { get; }
    }
}
