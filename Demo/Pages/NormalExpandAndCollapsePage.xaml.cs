using ListViewSample.Model;
using LLM;
using System;
using System.Collections.Generic;
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
        public NormalExpandAndCollapsePage()
        {
            this.InitializeComponent();
            var Contacts = Contact.GetContacts(140);
            if (Contacts.Count > 0)
            {
                MasterListView.ItemsSource = Contacts;
            }
        }

        private void ItemSwipeComplete(object sender, SwipeCompleteEventArgs args)
        {
            var txtCtrl = (sender as LLMListViewItem).GetSwipeControl<TextBlock>(args.SwipeDirection, "LeftSwipeText");
            if (txtCtrl == null)
                return;

            (txtCtrl.RenderTransform as TranslateTransform).X = 0;
        }
        
        private void ItemSwipeTriggerInTouch(object sender, SwipeTriggerEventArgs args)
        {
            //var txtCtrl = (sender as LLMListViewItem).GetSwipeControl<TextBlock>(args.SwipeDirection, "LeftSwipeText");
            //if (txtCtrl == null)
            //    return;

            //(txtCtrl.RenderTransform as TranslateTransform).X = 0;
        }
    }
}
