using LaunchBoxRomPatchManager.Event;
using LaunchBoxRomPatchManager.Helpers;
using LaunchBoxRomPatchManager.ViewModel;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LaunchBoxRomPatchManager.View
{
    /// <summary>
    /// Interaction logic for PatcherEditView.xaml
    /// </summary>
    public partial class PatcherEditView : Window
    {
        EventAggregator eventAggregator;

        public PatcherEditView(PatcherEditViewModel patcherEditViewModel)
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<PatcherEditClose>().Subscribe(OnPatcherEditClose);

            DataContext = patcherEditViewModel;
        }

        private void OnPatcherEditClose()
        {
            Close();
        }
    }
}
