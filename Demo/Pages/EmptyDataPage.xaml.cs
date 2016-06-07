using ListViewSample.Model;
using LLMListView;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Demo.Pages
{
    public sealed partial class EmptyDataPage : Page
    {
        public Visibility RefreshBtnVisibility => Utils.IsOnMobile ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<Contact> Contacts { get; set; } = new ObservableCollection<Contact>();

        public EmptyDataPage()
        {
            InitializeComponent();
            Loaded += EmptyDataPage_Loaded;
        }

        private void EmptyDataPage_Loaded(object sender, RoutedEventArgs e)
        {
            Contacts = Contact.GetContacts(0);
            MasterListView.DataContext = this;
            MasterListView.Refresh = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                Contact.GetContacts(50).ToList().ForEach(o => Contacts.Add(o));
                MasterListView.SetRefresh(false);
            };
        }
    }
}
