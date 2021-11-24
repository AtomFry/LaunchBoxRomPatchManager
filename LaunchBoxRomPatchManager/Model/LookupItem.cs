namespace LaunchBoxRomPatchManager.Model
{
    public class LookupItem
    {
        public string Id { get; set; }

        public string DisplayValue { get; set; }
    }

    public class NullLookupItem : LookupItem
    {
        public new string Id { get { return null; } }
    }
}
