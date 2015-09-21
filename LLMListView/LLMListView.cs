using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace LLM
{
    public sealed class LLMListView : ListView
    {
        private const int Refresh_Notify_Interval = 300;
        private const int Refresh_Status_Interval = 100;
        private const string Refreshing_State = "Refreshing";
        private const string To_Refresh_State = "ToRefresh";
        private const string Normal_State = "Normal";

        private bool _isNotifyToRefreshTimerStarting = false;
        private bool _isRefreshing = false;
        private ScrollViewer _scrollViewer;
        private Grid _container;
        private Border _pullToRefreshIndicator;
        private DispatcherTimer _timer;
        private DispatcherTimer _notifyToRefreshTimer;
        private ProgressBar _pullProgressBar;
        private ProgressRing _refreshProgressRing;

        public double RefreshAreaHeight
        {
            get { return (double)GetValue(RefreshAreaHeightProperty); }
            set { SetValue(RefreshAreaHeightProperty, value); }
        }
        public static readonly DependencyProperty RefreshAreaHeightProperty =
            DependencyProperty.Register("RefreshAreaHeight", typeof(double), typeof(LLMListView), new PropertyMetadata(50.0));


        public Brush RefreshIconColor
        {
            get { return (Brush)GetValue(RefreshIconColorProperty); }
            set { SetValue(RefreshIconColorProperty, value); }
        }
        public static readonly DependencyProperty RefreshIconColorProperty =
            DependencyProperty.Register("RefreshIconColor", typeof(Brush), typeof(LLMListView), new PropertyMetadata(Application.Current.Resources["ProgressBarForegroundThemeBrush"]));

        public string PullText
        {
            get { return (string)GetValue(PullTextProperty); }
            set { SetValue(PullTextProperty, value); }
        }
        public static readonly DependencyProperty PullTextProperty =
            DependencyProperty.Register("PullText", typeof(string), typeof(LLMListView), new PropertyMetadata("Pull to refresh"));


        public string RefreshText
        {
            get { return (string)GetValue(RefreshTextProperty); }
            set { SetValue(RefreshTextProperty, value); }
        }
        public static readonly DependencyProperty RefreshTextProperty =
            DependencyProperty.Register("RefreshText", typeof(string), typeof(LLMListView), new PropertyMetadata("Release to refresh"));


        public LLMListView()
        {
            DefaultStyleKey = typeof(LLMListView);
            Loaded += LLMListView_Loaded;
        }

        private void LLMListView_Loaded(object sender, RoutedEventArgs e)
        {
            InitTimer();
            VisualStateManager.GoToState(this, Normal_State, true);
        }

        private void InitTimer()
        {
            _notifyToRefreshTimer = new DispatcherTimer();
            _notifyToRefreshTimer.Interval = TimeSpan.FromMilliseconds(Refresh_Notify_Interval);
            _notifyToRefreshTimer.Tick += NotifyToRefreshTimer_Tick;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Refresh_Status_Interval);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");
            _container = (Grid)GetTemplateChild("Container");
            _pullToRefreshIndicator = (Border)GetTemplateChild("PullToRefreshIndicator");
            _pullProgressBar = (ProgressBar)GetTemplateChild("PullProgressBar");
            _refreshProgressRing = (ProgressRing)GetTemplateChild("RefreshProgressRing");

            _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            _scrollViewer.Margin = new Thickness(0, 0, 0, -RefreshAreaHeight);
            _scrollViewer.RenderTransform = new CompositeTransform() { TranslateY = -RefreshAreaHeight };

            SizeChanged += LLMListView_SizeChanged;
        }

        private void Timer_Tick(object sender, object e)
        {
            if (_isRefreshing)
                return;

            var pullOffsetRect = _pullToRefreshIndicator.TransformToVisual(_container).TransformBounds(new Rect(0, 0, ActualWidth, RefreshAreaHeight));
            var pullOffsetBottom = pullOffsetRect.Bottom;
            _pullProgressBar.Value = pullOffsetRect.Bottom * 100 / RefreshAreaHeight;

            if (pullOffsetBottom > RefreshAreaHeight)
            {
                VisualStateManager.GoToState(this, To_Refresh_State, true);
                if (!_isNotifyToRefreshTimerStarting)
                {
                    _isNotifyToRefreshTimerStarting = true;
                    _notifyToRefreshTimer.Start();
                }
            }
            else
            {
                _isNotifyToRefreshTimerStarting = false;
                _notifyToRefreshTimer.Stop();
            }
        }

        private void NotifyToRefreshTimer_Tick(object sender, object e)
        {
            SetRefresh(true);
        }

        public void SetRefresh(bool isRefresh)
        {
            _isRefreshing = isRefresh;
            VisualStateManager.GoToState(this, Refreshing_State, true);
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if(e.NextView.VerticalOffset == 0)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _notifyToRefreshTimer.Stop();
                _isNotifyToRefreshTimerStarting = false;
                VisualStateManager.GoToState(this, Normal_State, true);
            }
        }

        private void LLMListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }
    }
}
