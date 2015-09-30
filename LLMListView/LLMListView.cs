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
using Windows.UI.Xaml.Media.Animation;

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

        #region list view item property

        public event SwipeProgressEventHandler ItemSwipeProgress;

        public event SwipeCompleteEventHandler ItemSwipeComplete;

        public event SwipeTriggerEventHandler ItemSwipeTrigger;

        public SwipeMode ItemLeftSwipeMode
        {
            get { return (SwipeMode)GetValue(ItemLeftSwipeModeProperty); }
            set { SetValue(ItemLeftSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeModeProperty =
            DependencyProperty.Register("ItemLeftSwipeMode", typeof(SwipeMode), typeof(LLMListView), new PropertyMetadata(SwipeMode.Fix));

        public SwipeMode ItemRightSwipeMode
        {
            get { return (SwipeMode)GetValue(ItemRightSwipeModeProperty); }
            set { SetValue(ItemRightSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeModeProperty =
            DependencyProperty.Register("ItemRightSwipeMode", typeof(SwipeMode), typeof(LLMListView), new PropertyMetadata(SwipeMode.Fix));

        public int ItemBackAnimDuration
        {
            get { return (int)GetValue(ItemBackAnimDurationProperty); }
            set { SetValue(ItemBackAnimDurationProperty, value); }
        }
        public static readonly DependencyProperty ItemBackAnimDurationProperty =
            DependencyProperty.Register("ItemBackAnimDuration", typeof(int), typeof(LLMListView), new PropertyMetadata(200));

        public EasingFunctionBase ItemLeftBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(ItemLeftBackAnimEasingFunctionProperty); }
            set { SetValue(ItemLeftBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftBackAnimEasingFunctionProperty =
            DependencyProperty.Register("ItemBackEasingFunction", typeof(EasingFunctionBase), typeof(LLMListView), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public EasingFunctionBase ItemRightBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(ItemRightBackAnimEasingFunctionProperty); }
            set { SetValue(ItemRightBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty ItemRightBackAnimEasingFunctionProperty =
            DependencyProperty.Register("ItemBackEasingFunction", typeof(EasingFunctionBase), typeof(LLMListView), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public DataTemplate ItemLeftSwipeContentTemplate
        {
            get { return (DataTemplate)GetValue(ItemLeftSwipeContentTemplateProperty); }
            set { SetValue(ItemLeftSwipeContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeContentTemplateProperty =
            DependencyProperty.Register("ItemLeftSwipeContentTemplate", typeof(DataTemplate), typeof(LLMListView), new PropertyMetadata(null));

        public double ItemLeftSwipeLengthRate
        {
            get { return (double)GetValue(ItemLeftSwipeLengthRateProperty); }
            set { SetValue(ItemLeftSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeLengthRateProperty =
            DependencyProperty.Register("ItemLeftSwipeLengthRate", typeof(double), typeof(LLMListView), new PropertyMetadata(0.2));

        public double ItemRightSwipeLengthRate
        {
            get { return (double)GetValue(ItemRightSwipeLengthRateProperty); }
            set { SetValue(ItemRightSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeLengthRateProperty =
            DependencyProperty.Register("ItemRightSwipeLengthRate", typeof(double), typeof(LLMListView), new PropertyMetadata(1.0));

        public double ItemLeftActionRateForSwipeLength
        {
            get { return (double)GetValue(ItemLeftActionRateForSwipeLengthProperty); }
            set { SetValue(ItemLeftActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftActionRateForSwipeLengthProperty =
            DependencyProperty.Register("ItemLeftActionRateForSwipeLength", typeof(double), typeof(LLMListView), new PropertyMetadata(0.5));

        public double ItemRightActionRateForSwipeLength
        {
            get { return (double)GetValue(ItemRightActionRateForSwipeLengthProperty); }
            set { SetValue(ItemRightActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty ItemRightActionRateForSwipeLengthProperty =
            DependencyProperty.Register("ItemRightActionRateForSwipeLength", typeof(double), typeof(LLMListView), new PropertyMetadata(0.5));

        #endregion


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

        protected override DependencyObject GetContainerForItemOverride()
        {
            LLMListViewItem item = new LLMListViewItem();
            SetItemBinding(item, LLMListViewItem.LeftSwipeModeProperty, "ItemLeftSwipeMode");
            SetItemBinding(item, LLMListViewItem.RightSwipeModeProperty, "ItemRightSwipeMode");
            SetItemBinding(item, LLMListViewItem.BackAnimDurationProperty, "ItemBackAnimDuration");
            SetItemBinding(item, LLMListViewItem.LeftBackAnimEasingFunctionProperty, "ItemLeftBackAnimEasingFunction");
            SetItemBinding(item, LLMListViewItem.RightBackAnimEasingFunctionProperty, "ItemRightBackAnimEasingFunction");
            SetItemBinding(item, LLMListViewItem.LeftSwipeContentTemplateProperty, "ItemLeftSwipeContentTemplate");
            SetItemBinding(item, LLMListViewItem.LeftSwipeLengthRateProperty, "ItemLeftSwipeLengthRate");
            SetItemBinding(item, LLMListViewItem.LeftActionRateForSwipeLengthProperty, "ItemLeftActionRateForSwipeLength");
            SetItemBinding(item, LLMListViewItem.RightSwipeLengthRateProperty, "ItemRightSwipeLengthRate");
            SetItemBinding(item, LLMListViewItem.RightActionRateForSwipeLengthProperty, "ItemRightActionRateForSwipeLength");

            item.SwipeProgress += Item_SwipeProgress;
            item.SwipeComplete += Item_SwipeComplete;
            item.SwipeTrigger += Item_SwipeTrigger;
            return item;
        }

        private void Item_SwipeTrigger(object sender, SwipeTriggerEventArgs args)
        {
            if(ItemSwipeTrigger!=null)
            {
                ItemSwipeTrigger(sender, args);
            }
        }

        private void Item_SwipeComplete(object sender, SwipeCompleteEventArgs args)
        {
            if (ItemSwipeComplete != null)
            {
                ItemSwipeComplete(sender, args);
            }
        }

        private void Item_SwipeProgress(object sender, SwipeProgressEventArgs args)
        {
            if(ItemSwipeProgress != null)
            {
                ItemSwipeProgress(sender, args);
            }
        }

        private void SetItemBinding(LLMListViewItem item, DependencyProperty originProperty, string targetProperty)
        {
            var binding = new Binding() { Source = this, Path = new PropertyPath(targetProperty) };
            BindingOperations.SetBinding(item, originProperty, binding);
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
