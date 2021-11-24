namespace LaunchBoxRomPatchManager.Model
{
    public class RomPatcherEmulator
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RomPatcherId { get; set; }
        public RomPatcher RomPatcher { get; set; }
    }
}
