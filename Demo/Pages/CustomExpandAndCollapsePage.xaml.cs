using ListViewSample.Model;
using LLM;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Demo.Pages
{
    public sealed partial class CustomExpandAndCollapsePage : Page
    {
        public CustomExpandAndCollapsePage()
        {
            this.InitializeComponent();
            var Contacts = Contact.GetContacts(140);
            if (Contacts.Count > 0)
            {
                MasterListView.ItemsSource = Contacts;
            }
        }

        public void ItemSwipeProgress(object sender, SwipeProgressEventArgs args)
        {
            Debug.WriteLine("Direction: " + args.SwipeDirection + " , Length: " + args.Cumulative + " , rate: " + args.CurrentRate);

            var txtCtrl = (sender as LLMListViewItem).GetSwipeControl<TextBlock>(args.SwipeDirection, "LeftSwipeText");
            if (txtCtrl == null)
                return;

            if (args.CurrentRate < 0.4 && args.Cumulative - txtCtrl.ActualWidth > 0)
            {
                (txtCtrl.RenderTransform as TranslateTransform).X += args.Delta / 3;
            }
            else if (args.CurrentRate >= 0.4 && args.CurrentRate < 0.6 && args.Cumulative - txtCtrl.ActualWidth > 0)
            {
                (txtCtrl.RenderTransform as TranslateTransform).X += args.Delta * 2;
            }
            else
            {
                (txtCtrl.RenderTransform as TranslateTransform).X = args.Cumulative - txtCtrl.ActualWidth;
            }
        }

        public void ItemSwipeComplete(object sender, SwipeCompleteEventArgs args)
        {
            var txtCtrl = (sender as LLMListViewItem).GetSwipeControl<TextBlock>(args.SwipeDirection, "LeftSwipeText");
            if (txtCtrl == null)
                return;

            (txtCtrl.RenderTransform as TranslateTransform).X = 0;
        }

        private T FindVisualChild<T>(DependencyObject obj, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && ((T)child).Name == childName)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child, childName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
