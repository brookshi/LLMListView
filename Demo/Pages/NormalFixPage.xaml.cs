using ListViewSample.Model;
using LLM;
using LLMListView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Demo.Pages
{
    public sealed partial class NormalFixPage : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public NormalFixPage()
        {
            this.InitializeComponent();
            Contacts = Contact.GetContacts(140);
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            var item = Utils.FindVisualParent<LLMListViewItem>(sender as AppBarButton);
            var itemData = item.Content as Contact;
            var dlg = new MessageDialog("Edit " + itemData.Name);
            dlg.Commands.Add(new UICommand("OK"));
            dlg.Commands.Add(new UICommand("Cancel"));
            await dlg.ShowAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = Utils.FindVisualParent<LLMListViewItem>(sender as AppBarButton);
            var itemData = item.Content as Contact;
            var dlg = new MessageDialog("Comfire Delete "+ itemData.Name + "?");
            dlg.Commands.Add(new UICommand("OK", new UICommandInvokedHandler( param => 
            {
                _contacts.Remove(itemData);
            })));
            dlg.Commands.Add(new UICommand("Cancel"));
            await dlg.ShowAsync();
        }

        private void MasterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MasterListView_ItemSwipeBeginTrigger(object sender, SwipeReleaseEventArgs args)
        {

        }

        private void MasterListView_ItemSwipeTriggerInTouch(object sender, SwipeTriggerEventArgs args)
        {
        }

        private void MasterListView_ItemSwipeProgressInTouch(object sender, SwipeProgressEventArgs args)
        {
            var item = sender as LLMListViewItem;
            if (item == null)
                return;

            if (!item.IsSelected)
                item.IsSelected = true;
        }
    }
}
