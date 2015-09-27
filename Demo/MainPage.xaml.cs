using ListViewSample.Model;
using LLM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Demo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
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
                (txtCtrl.RenderTransform as TranslateTransform).X += args.Delta/3;
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
