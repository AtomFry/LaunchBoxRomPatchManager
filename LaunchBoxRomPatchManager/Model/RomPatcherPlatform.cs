namespace LaunchBoxRomPatchManager.Model
{
    public class RomPatcherPlatform
    {
        // in launchbox platforms only have a name so the name will be used for the id
        public string Id { get; set; }
        public string Name { get; set; }

        public string RomPatcherId { get; set; }
        public RomPatcher RomPatcher { get; set; }
    }
}
