using ListViewSample.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Demo.Pages
{
    public sealed partial class CustomizePullToRefreshPage : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public CustomizePullToRefreshPage()
        {
            this.InitializeComponent();
            Loaded += PullToRefreshPage_Loaded;
        }

        private void PullToRefreshPage_Loaded(object sender, RoutedEventArgs e)
        {
            Contacts = Contact.GetContacts(50);
            MasterListView.DataContext = this;
            MasterListView.Refresh = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                Contacts.Clear();
                Contact.GetContacts(50).ToList().ForEach(o => Contacts.Add(o));
                MasterListView.SetRefresh(false);
            };
        }
    }
}
