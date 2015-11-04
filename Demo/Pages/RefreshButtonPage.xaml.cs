using ListViewSample.Model;
using LLM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Demo.Pages
{
    public sealed partial class RefreshButtonPage : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();
        public Visibility RefreshBtnVisibility { get { return Utils.IsOnMobile ? Visibility.Collapsed : Visibility.Visible; } }

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public RefreshButtonPage()
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
