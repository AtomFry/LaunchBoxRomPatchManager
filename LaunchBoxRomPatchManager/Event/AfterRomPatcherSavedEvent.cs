using Prism.Events;

namespace LaunchBoxRomPatchManager.Event
{
    public class AfterRomPatcherSavedEvent : PubSubEvent<AfterRomPatcherSavedEventArgs>
    {
    }

    public class AfterRomPatcherSavedEventArgs
    {
        public string Id { get; set; }
        public string DisplayValue { get; set; }
    }
}
