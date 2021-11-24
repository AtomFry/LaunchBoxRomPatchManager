using LaunchBoxRomPatchManager.DataProvider.Lookups;
using LaunchBoxRomPatchManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.ModelWrapper
{
    public class RomPatcherPlatformWrapper : ModelWrapperBase<RomPatcherPlatform>
    {
        private IPlatform[] _validPlatforms = PluginHelper.DataManager.GetAllPlatforms();

        public RomPatcherPlatformWrapper(RomPatcherPlatform model) : base(model)
        {            
        }

        public string PlatformId
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string PlatformIdOriginalValue => GetOriginalValue<string>(nameof(PlatformId));

        public bool PlatformIdIsChanged => GetIsChanged(nameof(PlatformId));

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(PlatformId))
            {
                yield return new ValidationResult("A platform is required", new[] { nameof(PlatformId) });
            }
            else
            {
                if (!_validPlatforms.Any(platform => platform.Name.Equals(PlatformId, StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return new ValidationResult("The platform must a platform name", new[] { nameof(PlatformId) });
                }
            }
        }
    }
}
