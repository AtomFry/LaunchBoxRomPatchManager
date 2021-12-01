using Unbroken.LaunchBox.Plugins.Data;

namespace LaunchBoxRomPatchManager.Model
{
    public class AdditionalAppToCopy
    {
        public IAdditionalApplication AdditionalApplication { get; set; }
        public bool Copy { get; set; }
    }
}
