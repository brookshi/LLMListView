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
    public sealed partial class LoadMorePage : Page
    {
        const int MAX_PAGE = 3;
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();
        int _currentPageNum = 1;

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        public LoadMorePage()
        {
            this.InitializeComponent();
            Loaded += LoadMorePage_Loaded;
        }

        private void LoadMorePage_Loaded(object sender, RoutedEventArgs e)
        {
            Contacts = Contact.GetContacts(30);
            MasterListView.DataContext = this;
            MasterListView.LoadMore = async () =>
            {
                if (_currentPageNum < MAX_PAGE)
                {
                    await Task.Delay(2000);
                    _currentPageNum++;
                    Contact.GetContacts(30).ToList().ForEach(o => Contacts.Add(o));
                }
                MasterListView.FinishLoadingMore();
            };
        }
    }
}
