using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Unbroken.LaunchBox.Plugins;
using LaunchBoxRomPatchManager.View;
using LaunchBoxRomPatchManager.ViewModel;

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
            RomPatcherMainWindow romPatcherMainWindow = new RomPatcherMainWindow();
            romPatcherMainWindow.Show();
        }
    }
}
