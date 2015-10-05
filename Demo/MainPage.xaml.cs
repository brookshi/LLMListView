using Demo.Pages;
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

namespace Demo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            FrameContent.Navigate(typeof(HomePage));
        }

        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            FrameContent.Navigate(typeof(HomePage));
            SplitView.IsPaneOpen = false;
        }

        private void NavigateToNormalExpandAndCollapse(object sender, RoutedEventArgs e)
        {
            FrameContent.Navigate(typeof(NormalExpandAndCollapsePage));
            SplitView.IsPaneOpen = false;
        }

        private void NavigateToCustomExpandAndCollapse1(object sender, RoutedEventArgs e)
        {
            FrameContent.Navigate(typeof(CustomExpandAndCollapsePage1));
            SplitView.IsPaneOpen = false;
        }
    }
}
