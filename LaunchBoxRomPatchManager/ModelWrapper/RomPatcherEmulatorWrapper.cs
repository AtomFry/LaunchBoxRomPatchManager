using LaunchBoxRomPatchManager.Model;

namespace LaunchBoxRomPatchManager.ModelWrapper
{
    public class RomPatcherEmulatorWrapper : ModelWrapperBase<RomPatcherEmulator>
    {
        public RomPatcherEmulatorWrapper(RomPatcherEmulator model) : base(model)
        {
        }

        public string Id
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string IdOriginalValue => GetOriginalValue<string>(nameof(Id));

        public bool IdIsChanged => GetIsChanged(nameof(Id));

        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string NameOriginalValue => GetOriginalValue<string>(nameof(Name));

        public bool NameIsChanged => GetIsChanged(nameof(Name));

    }
}
