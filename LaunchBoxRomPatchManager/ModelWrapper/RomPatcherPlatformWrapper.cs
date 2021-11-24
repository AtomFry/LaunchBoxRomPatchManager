using LaunchBoxRomPatchManager.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LaunchBoxRomPatchManager.ModelWrapper
{
    public class RomPatcherPlatformWrapper : ModelWrapperBase<RomPatcherPlatform>
    {
        public RomPatcherPlatformWrapper(RomPatcherPlatform model) : base(model)
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
