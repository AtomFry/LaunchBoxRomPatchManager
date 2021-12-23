using System.Drawing;
using Unbroken.LaunchBox.Plugins;
using LaunchBoxRomPatchManager.View;
using Unbroken.LaunchBox.Plugins.Data;
using LaunchBoxRomPatchManager.ViewModel;

namespace LaunchBoxRomPatchManager.LaunchBoxPlugins
{
    class ImportRomHackMenuItem : IGameMenuItemPlugin
    {
        public bool SupportsMultipleGames => false;

        public string Caption => "Import ROM Hack";

        public Image IconImage => Properties.Resources.RomHackingIcon;

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool GetIsValidForGame(IGame selectedGame)
        {
            return true;
        }

        public bool GetIsValidForGames(IGame[] selectedGames)
        {
            return false;
        }

        public void OnSelected(IGame selectedGame)
        {
            ImportRomHackViewModel importRomHackViewModel = new ImportRomHackViewModel(selectedGame);
            ImportRomHackView importRomHackView = new ImportRomHackView(importRomHackViewModel);
            importRomHackView.Show();
        }

        public void OnSelected(IGame[] selectedGames)
        {
            throw new System.NotImplementedException("Importing ROM hacks is not supported for multiple games");
        }
    }
}
