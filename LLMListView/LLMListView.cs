#region License
//   Copyright 2015 Brook Shi
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using XP;

namespace LLM
{
    public sealed class LLMListView : ListView
    {
        private const int Refresh_Notify_Interval = 300;
        private const int Refresh_Status_Interval = 100;
        private const string Refreshing_State = "Refreshing";
        private const string Normal_State = "Normal";
        private const string Unvalid_State = "Unvalid";
        private const string RefreshBtn_Refreshing_State = "RefreshBtnRefreshing";
        private const string RefreshBtn_Normal_State = "RefreshBtnNormal";
        private const string RefreshBtn_Visible_State = "RefreshBtnVisible";
        private const string RefreshBtn_Collapse_State = "RefreshBtnCollapse";

        private bool _isNotifyToRefreshTimerStarting = false;
        private bool _isRefreshing = false;
        private bool _isLoadingMore = false;
        private double _lastVerticalOffset = 0;

        private ScrollViewer _scrollViewer;
        private Grid _container;
        private Border _pullToRefreshIndicator;
        private DispatcherTimer _timer;
        private DispatcherTimer _notifyToRefreshTimer;
        private ProgressBar _pullProgressBar;
        private ProgressRing _refreshProgressRing;
        private ProgressBar _loadMoreProgressBar;
        private XPButton _refreshButton;


        public Action Refresh { get; set; }

        public Action LoadMore { get; set; }

        public bool CanPullToRefresh
        {
            get { return (bool)GetValue(CanPullToRefreshProperty); }
            set { SetValue(CanPullToRefreshProperty, value); }
        }
        public static readonly DependencyProperty CanPullToRefreshProperty =
            DependencyProperty.Register("CanPullToRefresh", typeof(bool), typeof(LLMListView), new PropertyMetadata(false));

        public double RefreshAreaHeight
        {
            get { return (double)GetValue(RefreshAreaHeightProperty); }
            set { SetValue(RefreshAreaHeightProperty, value); }
        }
        public static readonly DependencyProperty RefreshAreaHeightProperty =
            DependencyProperty.Register("RefreshAreaHeight", typeof(double), typeof(LLMListView), new PropertyMetadata(50.0));

        public Brush RefreshProgressRingBrush
        {
            get { return (Brush)GetValue(RefreshProgressRingBrushProperty); }
            set { SetValue(RefreshProgressRingBrushProperty, value); }
        }
        public static readonly DependencyProperty RefreshProgressRingBrushProperty =
            DependencyProperty.Register("RefreshProgressRingBrush", typeof(Brush), typeof(LLMListView), new PropertyMetadata(Application.Current.Resources["ProgressBarForegroundThemeBrush"]));

        public Brush LoadMoreProgressBarBrush
        {
            get { return (Brush)GetValue(LoadMoreProgressBarBrushProperty); }
            set { SetValue(LoadMoreProgressBarBrushProperty, value); }
        }
        public static readonly DependencyProperty LoadMoreProgressBarBrushProperty =
            DependencyProperty.Register("LoadMoreProgressBarBrush", typeof(Brush), typeof(LLMListView), new PropertyMetadata(Application.Current.Resources["ProgressBarForegroundThemeBrush"]));

        public Brush RefreshButtonForeground
        {
            get { return (Brush)GetValue(RefreshButtonForegroundProperty); }
            set { SetValue(RefreshButtonForegroundProperty, value); }
        }
        public static readonly DependencyProperty RefreshButtonForegroundProperty =
            DependencyProperty.Register("RefreshButtonForeground", typeof(Brush), typeof(LLMListView), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush RefreshButtonBackground
        {
            get { return (Brush)GetValue(RefreshButtonBackgroundProperty); }
            set { SetValue(RefreshButtonBackgroundProperty, value); }
        }
        public static readonly DependencyProperty RefreshButtonBackgroundProperty =
            DependencyProperty.Register("RefreshButtonBackground", typeof(Brush), typeof(LLMListView), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 33, 150, 243))));

        public Visibility RefreshButtonVisibility
        {
            get { return (Visibility)GetValue(RefreshButtonVisibilityProperty); }
            set { SetValue(RefreshButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RefreshButtonVisibilityProperty =
            DependencyProperty.Register("RefreshButtonVisibility", typeof(Visibility), typeof(LLMListView), new PropertyMetadata(Visibility.Collapsed));

       
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

        private void LLMListView_Loaded(object sender, RoutedEventArgs e)
        {
            InitLayout();

            InitTimer();

            InitVisualState();
        }

        private void InitLayout()
        {
            _pullProgressBar.Width = ActualWidth;
            _loadMoreProgressBar.Width = ActualWidth;
        }

        private void InitTimer()
        {
            if (!CanPullToRefresh || !Utils.IsOnMobile)
                return;

            _notifyToRefreshTimer = new DispatcherTimer();
            _notifyToRefreshTimer.Interval = TimeSpan.FromMilliseconds(Refresh_Notify_Interval);
            _notifyToRefreshTimer.Tick += NotifyToRefreshTimer_Tick;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Refresh_Status_Interval);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitVisualState()
        {
            if (CanPullToRefresh && Utils.IsOnMobile)
            {
                VisualStateManager.GoToState(this, Normal_State, false);
            }
            else
            {
                VisualStateManager.GoToState(this, Unvalid_State, false);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InitControls();

            InitScrollViewEventsForPullToRefresh();

            InitRefreshButtonClickEvent();

            InitOtherEvents();
        }

        private void InitControls()
        {
            _scrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");
            _container = (Grid)GetTemplateChild("Container");
            _pullToRefreshIndicator = (Border)GetTemplateChild("PullToRefreshIndicator");
            _pullProgressBar = (ProgressBar)GetTemplateChild("PullProgressBar");
            _refreshProgressRing = (ProgressRing)GetTemplateChild("RefreshProgressRing");
            _loadMoreProgressBar = (ProgressBar)GetTemplateChild("LoadMoreProgressBar");
            _refreshButton = (XPButton)GetTemplateChild("RefreshButton");
        }

        private void InitScrollViewEventsForPullToRefresh()
        {
            if (CanPullToRefresh && Utils.IsOnMobile)
            {
                _scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                _scrollViewer.Margin = new Thickness(0, 0, 0, -RefreshAreaHeight);
                _scrollViewer.RenderTransform = new CompositeTransform() { TranslateY = -RefreshAreaHeight };
            }
        }

        private void InitRefreshButtonClickEvent()
        {
            if (RefreshButtonVisibility == Visibility.Visible)
            {
                _refreshButton.Click += (s, e) =>
                {
                    if (_isRefreshing)
                        return;

                    SetRefresh(true);
                };
            }
        }

        private void InitOtherEvents()
        {
            _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
            SizeChanged += LLMListView_SizeChanged;
        }

        private void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            UpdateRefreshButtonState();

            UpdateLoadingMore();
        }

        private void UpdateRefreshButtonState()
        {
            if (_lastVerticalOffset < _scrollViewer.VerticalOffset)
            {
                VisualStateManager.GoToState(this, RefreshBtn_Collapse_State, true);
            }
            else
            {
                VisualStateManager.GoToState(this, RefreshBtn_Visible_State, true);
            }
            _lastVerticalOffset = _scrollViewer.VerticalOffset;
        }

        private void UpdateLoadingMore()
        {
            var bottomOffset = _scrollViewer.ExtentHeight - _scrollViewer.VerticalOffset - _scrollViewer.ViewportHeight;
            if (!_isLoadingMore && LoadMore != null && bottomOffset < 300)
            {
                ToggleLoadingMoreStatus(true);
                LoadMore();
            }
        }

        private void ToggleLoadingMoreStatus(bool isLoadingMore)
        {
            _isLoadingMore = isLoadingMore;
            _loadMoreProgressBar.Visibility = isLoadingMore ? Visibility.Visible : Visibility.Collapsed;
        }

        public void FinishLoadingMore()
        {
            ToggleLoadingMoreStatus(false);
        }

        public void SetRefresh(bool isRefresh)
        {
            _isRefreshing = isRefresh;
            if (_isRefreshing)
            {
                VisualStateManager.GoToState(this, RefreshState, true);
                if (Refresh != null)
                {
                    Refresh();
                }
            }
            else
            {
                VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        private string NormalState { get { return CanPullToRefresh && Utils.IsOnMobile ? Normal_State : RefreshBtn_Normal_State; } }

        private string RefreshState { get { return CanPullToRefresh && Utils.IsOnMobile ? Refreshing_State : RefreshBtn_Refreshing_State; } }

        #region events

        private void Timer_Tick(object sender, object e)
        {
            if (_isRefreshing)
                return;

            var pullOffsetRect = _pullToRefreshIndicator.TransformToVisual(_container).TransformBounds(new Rect(0, 0, ActualWidth, RefreshAreaHeight));
            var pullOffsetBottom = pullOffsetRect.Bottom;
            _pullProgressBar.Value = pullOffsetRect.Bottom * 100 / RefreshAreaHeight;

            if (pullOffsetBottom > RefreshAreaHeight)
            {
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
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (!CanPullToRefresh || !Utils.IsOnMobile)
                return;

            if (e.NextView.VerticalOffset == 0)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _notifyToRefreshTimer.Stop();
                _pullProgressBar.Value = 0;
                _isNotifyToRefreshTimerStarting = false;
            }
        }

        private void LLMListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        #endregion
    }
}
