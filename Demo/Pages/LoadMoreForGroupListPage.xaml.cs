using ListViewSample.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class LoadMoreForGroupListPage : Page
    {
        const int MAX_PAGE = 3;
        const int COUNT_IN_PAGE = 30;
        ObservableCollection<GroupInfoList> _allContacts;
        ObservableCollection<GroupInfoList> _contacts = new ObservableCollection<GroupInfoList>();
        int _currentPageNum = 1;

        public ObservableCollection<GroupInfoList> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        public LoadMoreForGroupListPage()
        {
            this.InitializeComponent();
            DataContext = this;
            Loaded += LoadMorePage_Loaded;
        }

        private void LoadMorePage_Loaded(object sender, RoutedEventArgs e)
        {
            _allContacts = Contact.GetContactsGrouped(100);
            UpdateContactsForPage();
            MasterListView.LoadMore = () =>
            {
                if (_currentPageNum < MAX_PAGE)
                {
                    _currentPageNum++;
                    UpdateContactsForPage();
                    MasterListView.FinishLoadMore();
                }
            };
        }

        void UpdateContactsForPage()
        {
            var count = 0;
            var groupIndex = 0;
            foreach(var group in _allContacts)
            {
                for (int i=0; i< group.Count;i++)
                {
                    if(i + GetAllCountBeforeGroupIndex(groupIndex) >= (_currentPageNum - 1) * COUNT_IN_PAGE && count < COUNT_IN_PAGE)
                    {
                        var item = _contacts.FirstOrDefault(o => o.Key.Equals(group.Key));
                        if(item == null)
                        {
                            item = new GroupInfoList() { Key = group.Key };
                            _contacts.Add(item);
                        }
                        item.Add(group[i]);
                        count++;
                    }
                }
                groupIndex++;
            }
        }

        int GetAllCountBeforeGroupIndex(int groupIndex)
        {
            var count = 0;
            for(int i=0;i<groupIndex;i++)
            {
                count += _allContacts[i].Count;
            }
            return count;
        }
    }
}
