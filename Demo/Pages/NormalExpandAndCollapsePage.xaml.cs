using ListViewSample.Model;
using LLM;
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
    public sealed partial class NormalExpandAndCollapsePage : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public NormalExpandAndCollapsePage()
        {
            this.InitializeComponent();
            Contacts = Contact.GetContacts(140);
        }

        private void ItemSwipeTriggerComplete(object sender, SwipeCompleteEventArgs args)
        {
            var itemData = (sender as LLMListViewItem).Content as Contact;
            if (args.SwipeDirection == SwipeDirection.Left)
            {
                _contacts.Remove(itemData);
            }
            else
            {
                ContackStarDict[itemData] = itemData.IsStar;
            }
        }

        private void ItemSwipeTriggerInTouch(object sender, SwipeTriggerEventArgs args)
        {
            var itemData = (sender as LLMListViewItem).Content as Contact;
            if (args.SwipeDirection == SwipeDirection.Left)
            {
                itemData.IsDelete = args.IsTrigger;
            }
            else
            {
                itemData.IsStar = ContackStarDict.ContainsKey(itemData) && ContackStarDict[itemData] ? !args.IsTrigger : args.IsTrigger;
            }
        }
    }
}
