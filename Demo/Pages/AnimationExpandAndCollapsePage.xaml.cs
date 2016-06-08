using ListViewSample.Model;
using LLM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Demo.Pages
{
    public sealed partial class AnimationExpandAndCollapsePage : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public AnimationExpandAndCollapsePage()
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

        private void ItemSwipeProgressInTouch(object sender, SwipeProgressEventArgs args)
        {
            Debug.WriteLine("Direction: " + args.SwipeDirection + " , Length: " + args.Cumulative + " , rate: " + args.CurrentRate);

            if (args.SwipeDirection == SwipeDirection.None)
                return;

            var panel = Getpanel(sender, args.SwipeDirection);
            SwipeMovePanel(panel, args);
        }

        private void ItemSwipeBeginTrigger(object sender, SwipeReleaseEventArgs args)
        {
            if (args.SwipeDirection == SwipeDirection.None)
                return;

            var panel = Getpanel(sender, args.SwipeDirection);
            SwipeReleasePanel(panel, args);
        }

        private void ItemSwipeBeginRestore(object sender, SwipeReleaseEventArgs args)
        {
            if (args.SwipeDirection == SwipeDirection.None)
                return;

            var panel = Getpanel(sender, args.SwipeDirection);
            SwipeReleasePanel(panel, args);
        }

        StackPanel Getpanel(object sender, SwipeDirection direction)
        {
            var llmItem = sender as LLMListViewItem;
            StackPanel panel = null;

            if (direction == SwipeDirection.Right)
            {
                panel = llmItem.GetSwipeControl<StackPanel>(direction, "RightPanel");
            }
            else
            {
                panel = llmItem.GetSwipeControl<StackPanel>(direction, "LeftPanel");
            }

            return panel;
        }

        void SwipeMovePanel(StackPanel panel, SwipeProgressEventArgs args)
        {
            var cumlative = Math.Abs(args.Cumulative);

            if (panel == null && cumlative - panel.ActualWidth >= 0)
                return;

            if (args.CurrentRate < 0.3 && cumlative - panel.ActualWidth > 0)
            {
                (panel.RenderTransform as TranslateTransform).X += args.Delta / 2;
            }
            else if (args.CurrentRate >= 0.3 && args.CurrentRate < 0.4 && cumlative - panel.ActualWidth > 0)
            {
                (panel.RenderTransform as TranslateTransform).X += args.Delta * 2;
            }
            else
            {
                (panel.RenderTransform as TranslateTransform).X = args.SwipeDirection == SwipeDirection.Left ? cumlative - panel.ActualWidth : panel.ActualHeight - cumlative;
            }
        }

        void SwipeReleasePanel(StackPanel panel, SwipeReleaseEventArgs args)
        {
            var story = new Storyboard();
            var transform = panel.RenderTransform as TranslateTransform;
            story.Children.Add(Utils.CreateDoubleAnimation(transform, "X", args.EasingFunc, args.ItemToX, args.Duration - 10));
            story.Begin();
        }
    }
}
