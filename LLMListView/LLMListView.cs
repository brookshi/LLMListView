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
        private const string Unvalid_State = "Unvalid";

        private bool _isNotifyToRefreshTimerStarting = false;
        private bool _isRefreshing = false;
        private bool _isLoadingMore = false;

        private ScrollViewer _scrollViewer;
        private Grid _container;
        private Border _pullToRefreshIndicator;
        private DispatcherTimer _timer;
        private DispatcherTimer _notifyToRefreshTimer;
        private ProgressBar _pullProgressBar;
        private ProgressRing _refreshProgressRing;


        public Action RefreshData { get; set; }

        public Action LoadMore { get; set; }

        public bool SupportPullToRefresh
        {
            get { return (bool)GetValue(SupportPullToRefreshProperty); }
            set { SetValue(SupportPullToRefreshProperty, value); }
        }
        public static readonly DependencyProperty SupportPullToRefreshProperty =
            DependencyProperty.Register("SupportPullToRefresh", typeof(bool), typeof(LLMListView), new PropertyMetadata(false));

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

        #region list view item property

        public event SwipeProgressEventHandler ItemSwipeProgressInTouch;
        public event SwipeCompleteEventHandler ItemSwipeRestoreComplete;
        public event SwipeCompleteEventHandler ItemSwipeTriggerComplete;
        public event SwipeReleaseEventHandler ItemSwipeBeginTrigger;
        public event SwipeReleaseEventHandler ItemSwipeBeginRestore;
        public event SwipeTriggerEventHandler ItemSwipeTriggerInTouch;

        public DataTemplate ItemLeftSwipeContentTemplate
        {
            get { return (DataTemplate)GetValue(ItemLeftSwipeContentTemplateProperty); }
            set { SetValue(ItemLeftSwipeContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeContentTemplateProperty =
            DependencyProperty.Register("ItemLeftSwipeContentTemplate", typeof(DataTemplate), typeof(LLMListView), new PropertyMetadata(null));

        public DataTemplate ItemRightSwipeContentTemplate
        {
            get { return (DataTemplate)GetValue(ItemRightSwipeContentTemplateProperty); }
            set { SetValue(ItemRightSwipeContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeContentTemplateProperty =
            DependencyProperty.Register("ItemRightSwipeContentTemplate", typeof(DataTemplate), typeof(LLMListView), new PropertyMetadata(null));

        public int ItemBackAnimDuration
        {
            get { return (int)GetValue(ItemBackAnimDurationProperty); }
            set { SetValue(ItemBackAnimDurationProperty, value); }
        }
        public static readonly DependencyProperty ItemBackAnimDurationProperty =
            DependencyProperty.Register("ItemBackAnimDuration", typeof(int), typeof(LLMListView), new PropertyMetadata(200));


        public SwipeMode ItemLeftSwipeMode
        {
            get { return (SwipeMode)GetValue(ItemLeftSwipeModeProperty); }
            set { SetValue(ItemLeftSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeModeProperty =
            DependencyProperty.Register("ItemLeftSwipeMode", typeof(SwipeMode), typeof(LLMListView), new PropertyMetadata(SwipeMode.None));

        public EasingFunctionBase ItemLeftBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(ItemLeftBackAnimEasingFunctionProperty); }
            set { SetValue(ItemLeftBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftBackAnimEasingFunctionProperty =
            DependencyProperty.Register("ItemLeftBackAnimEasingFunction", typeof(EasingFunctionBase), typeof(LLMListView), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public double ItemLeftSwipeLengthRate
        {
            get { return (double)GetValue(ItemLeftSwipeLengthRateProperty); }
            set { SetValue(ItemLeftSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeLengthRateProperty =
            DependencyProperty.Register("ItemLeftSwipeLengthRate", typeof(double), typeof(LLMListView), new PropertyMetadata(1.0));

        public double ItemLeftSwipeMaxLength
        {
            get { return (double)GetValue(ItemLeftSwipeMaxLengthProperty); }
            set { SetValue(ItemLeftSwipeMaxLengthProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftSwipeMaxLengthProperty =
            DependencyProperty.Register("ItemLeftSwipeMaxLength", typeof(double), typeof(LLMListView), new PropertyMetadata(0.0));

        public double ItemLeftActionRateForSwipeLength
        {
            get { return (double)GetValue(ItemLeftActionRateForSwipeLengthProperty); }
            set { SetValue(ItemLeftActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty ItemLeftActionRateForSwipeLengthProperty =
            DependencyProperty.Register("ItemLeftActionRateForSwipeLength", typeof(double), typeof(LLMListView), new PropertyMetadata(0.5));


        public SwipeMode ItemRightSwipeMode
        {
            get { return (SwipeMode)GetValue(ItemRightSwipeModeProperty); }
            set { SetValue(ItemRightSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeModeProperty =
            DependencyProperty.Register("ItemRightSwipeMode", typeof(SwipeMode), typeof(LLMListView), new PropertyMetadata(SwipeMode.None));

        public EasingFunctionBase ItemRightBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(ItemRightBackAnimEasingFunctionProperty); }
            set { SetValue(ItemRightBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty ItemRightBackAnimEasingFunctionProperty =
            DependencyProperty.Register("ItemBackEasingFunction", typeof(EasingFunctionBase), typeof(LLMListView), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public double ItemRightSwipeLengthRate
        {
            get { return (double)GetValue(ItemRightSwipeLengthRateProperty); }
            set { SetValue(ItemRightSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeLengthRateProperty =
            DependencyProperty.Register("ItemRightSwipeLengthRate", typeof(double), typeof(LLMListView), new PropertyMetadata(1.0));

        public double ItemRightSwipeMaxLength
        {
            get { return (double)GetValue(ItemRightSwipeMaxLengthProperty); }
            set { SetValue(ItemRightSwipeMaxLengthProperty, value); }
        }
        public static readonly DependencyProperty ItemRightSwipeMaxLengthProperty =
            DependencyProperty.Register("ItemRightSwipeMaxLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.0));

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
            _pullProgressBar.Width = ActualWidth;
            InitTimer();
            if(SupportPullToRefresh)
                VisualStateManager.GoToState(this, Normal_State, false);
            else
                VisualStateManager.GoToState(this, Unvalid_State, false);
        }

        private void InitTimer()
        {
            if (!SupportPullToRefresh)
                return;

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
            SetItemBinding(item, LLMListViewItem.RightSwipeContentTemplateProperty, "ItemRightSwipeContentTemplate");
            SetItemBinding(item, LLMListViewItem.LeftSwipeLengthRateProperty, "ItemLeftSwipeLengthRate");
            SetItemBinding(item, LLMListViewItem.LeftActionRateForSwipeLengthProperty, "ItemLeftActionRateForSwipeLength");
            SetItemBinding(item, LLMListViewItem.RightSwipeLengthRateProperty, "ItemRightSwipeLengthRate");
            SetItemBinding(item, LLMListViewItem.RightActionRateForSwipeLengthProperty, "ItemRightActionRateForSwipeLength");
            SetItemBinding(item, LLMListViewItem.LeftSwipeMaxLengthProperty, "ItemLeftSwipeMaxLength");
            SetItemBinding(item, LLMListViewItem.RightSwipeMaxLengthProperty, "ItemRightSwipeMaxLength");

            item.SwipeProgressInTouch += Item_SwipeProgressInTouch;
            item.SwipeRestoreComplete += Item_SwipeStoreComplete;
            item.SwipeTriggerComplete += Item_SwipeTriggerComplete;
            item.SwipeBeginTrigger += Item_SwipeBeginTrigger;
            item.SwipeBeginRestore += Item_SwipeBeginRestore;
            item.SwipeTriggerInTouch += Item_SwipeTriggerInTouch;
            return item;
        }

        private void Item_SwipeTriggerComplete(object sender, SwipeCompleteEventArgs args)
        {
            if(ItemSwipeTriggerComplete != null)
            {
                ItemSwipeTriggerComplete(sender, args);
            }
        }

        private void Item_SwipeTriggerInTouch(object sender, SwipeTriggerEventArgs args)
        {
            if(ItemSwipeTriggerInTouch != null)
            {
                ItemSwipeTriggerInTouch(sender, args);
            }
        }

        private void Item_SwipeBeginRestore(object sender, SwipeReleaseEventArgs args)
        {
            if(ItemSwipeBeginRestore != null)
            {
                ItemSwipeBeginRestore(sender, args);
            }
        }

        private void Item_SwipeBeginTrigger(object sender, SwipeReleaseEventArgs args)
        {
            if(ItemSwipeBeginTrigger!=null)
            {
                ItemSwipeBeginTrigger(sender, args);
            }
        }

        private void Item_SwipeStoreComplete(object sender, SwipeCompleteEventArgs args)
        {
            if (ItemSwipeRestoreComplete != null)
            {
                ItemSwipeRestoreComplete(sender, args);
            }
        }

        private void Item_SwipeProgressInTouch(object sender, SwipeProgressEventArgs args)
        {
            if(ItemSwipeProgressInTouch != null)
            {
                ItemSwipeProgressInTouch(sender, args);
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

            if(SupportPullToRefresh)
            {
                _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                _scrollViewer.Margin = new Thickness(0, 0, 0, -RefreshAreaHeight);
                _scrollViewer.RenderTransform = new CompositeTransform() { TranslateY = -RefreshAreaHeight };
            }
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
            SizeChanged += LLMListView_SizeChanged;
        }

        private void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var bottomOffset = _scrollViewer.ExtentHeight - _scrollViewer.VerticalOffset - _scrollViewer.ViewportHeight;
            if (!_isLoadingMore && LoadMore != null && bottomOffset < 300)
            {
                _isLoadingMore = true;
                LoadMore();
            }
        }

        public void FinishLoadMore()
        {
            _isLoadingMore = false;
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
            if (_isRefreshing)
                return;

            SetRefresh(true);
            if (RefreshData != null)
            {
                RefreshData();
            }
        }

        public void SetRefresh(bool isRefresh)
        {
            _isRefreshing = isRefresh;
            if(_isRefreshing)
                VisualStateManager.GoToState(this, Refreshing_State, true);
            else
                VisualStateManager.GoToState(this, Normal_State, true);
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (!SupportPullToRefresh)
                return;

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
