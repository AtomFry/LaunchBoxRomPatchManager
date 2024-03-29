﻿using System.Drawing;
using Unbroken.LaunchBox.Plugins;
using LaunchBoxRomPatchManager.View;

namespace LaunchBoxRomPatchManager.LaunchBoxPlugins
{
    class ManageRomPatchersMenuItem : ISystemMenuItemPlugin
    {
        public string Caption => "Manage ROM patchers";

        public Image IconImage => Properties.Resources.RomHackingIcon;

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            PatcherListView patcherListView = new PatcherListView();
            patcherListView.Show();
        }
    }
}
