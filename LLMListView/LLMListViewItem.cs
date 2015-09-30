using System;
using System.Collections.Generic;
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
    public sealed class LLMListViewItem : ListViewItem
    {
        private TranslateTransform _mainLayerTransform;
        private ScaleTransform _swipeLayerClipTransform;
        private RectangleGeometry _swipeLayerClip;
        private ContentControl _rightSwipeContent;
        private ContentControl _leftSwipeContent;
        private Border _mainLayer;
        private SwipeAnimationConstructor _swipeAnimationConstructor;

        public event SwipeProgressEventHandler SwipeProgress;
        public event SwipeCompleteEventHandler SwipeComplete;
        public event SwipeTriggerEventHandler SwipeTrigger;

        public SwipeConfig Config { get { return _swipeAnimationConstructor.Config; } }

        #region property

        public SwipeMode LeftSwipeMode
        {
            get { return (SwipeMode)GetValue(LeftSwipeModeProperty); }
            set { SetValue(LeftSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty LeftSwipeModeProperty =
            DependencyProperty.Register("LeftSwipeMode", typeof(SwipeMode), typeof(LLMListViewItem), new PropertyMetadata(SwipeMode.Fix));

        public SwipeMode RightSwipeMode
        {
            get { return (SwipeMode)GetValue(RightSwipeModeProperty); }
            set { SetValue(RightSwipeModeProperty, value); }
        }
        public static readonly DependencyProperty RightSwipeModeProperty =
            DependencyProperty.Register("RightSwipeMode", typeof(SwipeMode), typeof(LLMListViewItem), new PropertyMetadata(SwipeMode.Fix));

        public int BackAnimDuration
        {
            get { return (int)GetValue(BackAnimDurationProperty); }
            set { SetValue(BackAnimDurationProperty, value); }
        }
        public static readonly DependencyProperty BackAnimDurationProperty =
            DependencyProperty.Register("BackAnimDuration", typeof(int), typeof(LLMListViewItem), new PropertyMetadata(200));

        public EasingFunctionBase LeftBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(LeftBackAnimEasingFunctionProperty); }
            set { SetValue(LeftBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty LeftBackAnimEasingFunctionProperty =
            DependencyProperty.Register("BackEasingFunction", typeof(EasingFunctionBase), typeof(LLMListViewItem), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public EasingFunctionBase RightBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(RightBackAnimEasingFunctionProperty); }
            set { SetValue(RightBackAnimEasingFunctionProperty, value); }
        }
        public static readonly DependencyProperty RightBackAnimEasingFunctionProperty =
            DependencyProperty.Register("BackEasingFunction", typeof(EasingFunctionBase), typeof(LLMListViewItem), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public DataTemplate LeftSwipeContentTemplate
        {
            get { return (DataTemplate)GetValue(LeftSwipeContentTemplateProperty); }
            set { SetValue(LeftSwipeContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty LeftSwipeContentTemplateProperty =
            DependencyProperty.Register("LeftSwipeContentTemplate", typeof(DataTemplate), typeof(LLMListViewItem), new PropertyMetadata(null));

        public DataTemplate RightSwipeContentTemplate
        {
            get { return (DataTemplate)GetValue(RightSwipeContentTemplateProperty); }
            set { SetValue(RightSwipeContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty RightSwipeContentTemplateProperty =
            DependencyProperty.Register("RightSwipeContentTemplate", typeof(DataTemplate), typeof(LLMListViewItem), new PropertyMetadata(null));

        public double LeftSwipeLengthRate
        {
            get { return (double)GetValue(LeftSwipeLengthRateProperty); }
            set { SetValue(LeftSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty LeftSwipeLengthRateProperty =
            DependencyProperty.Register("LeftSwipeLengthRate", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.1));

        public double RightSwipeLengthRate
        {
            get { return (double)GetValue(RightSwipeLengthRateProperty); }
            set { SetValue(RightSwipeLengthRateProperty, value); }
        }
        public static readonly DependencyProperty RightSwipeLengthRateProperty =
            DependencyProperty.Register("RightSwipeLengthRate", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(1.0));

        public double LeftActionRateForSwipeLength
        {   
            get { return (double)GetValue(LeftActionRateForSwipeLengthProperty); }
            set { SetValue(LeftActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty LeftActionRateForSwipeLengthProperty =
            DependencyProperty.Register("LeftActionRateForSwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.5));

        public double RightActionRateForSwipeLength
        {
            get { return (double)GetValue(RightActionRateForSwipeLengthProperty); }
            set { SetValue(RightActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty RightActionRateForSwipeLengthProperty =
            DependencyProperty.Register("RightActionRateForSwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.5));

        #endregion


        public LLMListViewItem()
        {
            this.DefaultStyleKey = typeof(LLMListViewItem);
            this.Loaded += LLMListViewItem_Loaded;
        }

        private void LLMListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            _swipeAnimationConstructor = SwipeAnimationConstructor.Create(new SwipeConfig() {
                Duration = BackAnimDuration,
                LeftEasingFunc = LeftBackAnimEasingFunction,
                RightEasingFunc = RightBackAnimEasingFunction,
                LeftSwipeMode = LeftSwipeMode,
                RightSwipeMode = RightSwipeMode,
                MainTransform = _mainLayerTransform,
                SwipeClipTransform = _swipeLayerClipTransform,
                SwipeClipRectangle = _swipeLayerClip,
                LeftActionRateForSwipeLength = LeftActionRateForSwipeLength,
                RightActionRateForSwipeLength = RightActionRateForSwipeLength,
                LeftSwipeLengthRate = LeftSwipeLengthRate,
                RightSwipeLengthRate = RightSwipeLengthRate,
                ItemActualWidth = ActualWidth
            });
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mainLayer = (Border)GetTemplateChild("MainLayer");
            _mainLayerTransform = (TranslateTransform)GetTemplateChild("MainLayerTransform");
            _swipeLayerClipTransform = (ScaleTransform)GetTemplateChild("SwipeLayerClipTransform");
            _swipeLayerClip = (RectangleGeometry)GetTemplateChild("SwipeLayerClip");
            _rightSwipeContent = (ContentControl)GetTemplateChild("RightSwipeContent");
            _leftSwipeContent = (ContentControl)GetTemplateChild("LeftSwipeContent");
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            var cumulativeX = e.Cumulative.Translation.X;
            var deltaX = e.Delta.Translation.X;

            if (Config.Direction == SwipeDirection.None)
            {
                Config.Direction = deltaX > 0 ? SwipeDirection.Left : SwipeDirection.Right;
                _leftSwipeContent.Visibility = Config.CanSwipeLeft ? Visibility.Visible : Visibility.Collapsed;
                _rightSwipeContent.Visibility = Config.CanSwipeRight ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (Config.CanSwipeLeft)
            {
                SwipeToLeft(cumulativeX, deltaX);
            }
            else if(Config.CanSwipeRight)
            {
                SwipeToRight(cumulativeX, deltaX);
            }
        }

        void SwipeToLeft(double cumulativeX, double deltaX)
        {
            cumulativeX = deltaX + _mainLayerTransform.X;
            var swipeLengthRate = Math.Abs(cumulativeX) / ActualWidth;

            if (cumulativeX <= 0)
            {
                ResetSwipe();
            }
            else if (swipeLengthRate <= LeftSwipeLengthRate)
            {
                _swipeLayerClip.Rect = new Rect(0, 0, Math.Max(0, cumulativeX), ActualHeight);
                _mainLayerTransform.X = cumulativeX;
                if (SwipeProgress != null)
                {
                    SwipeProgress(this, new SwipeProgressEventArgs(Config.Direction, cumulativeX, deltaX, cumulativeX / ActualWidth));
                }
            }
        }

        private void SwipeToRight(double cumulativeX, double deltaX)
        {
            cumulativeX = deltaX + _mainLayerTransform.X;
            var swipeLengthRate = Math.Abs(cumulativeX) / ActualWidth;

            if (cumulativeX >= 0)
            {
                ResetSwipe();
            }
            else if (swipeLengthRate <= RightSwipeLengthRate)
            {
                _swipeLayerClip.Rect = new Rect(ActualWidth + cumulativeX, 0, Math.Max(0, -cumulativeX), ActualHeight);
                _mainLayerTransform.X = cumulativeX;

                if (SwipeProgress != null)
                {
                    SwipeProgress(this, new SwipeProgressEventArgs(Config.Direction, cumulativeX, deltaX, cumulativeX / ActualWidth));
                }
            }
        }

        private void ResetSwipe()
        {
            Config.Direction = SwipeDirection.None;
            _swipeLayerClip.Rect = new Rect(0, 0, 0, 0);
            _mainLayerTransform.X = 0;
        }

        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
        {
            var oldDirection = Config.Direction;
            bool isFixMode = Config.SwipeMode == SwipeMode.Fix;
            var swipeRate = e.Cumulative.Translation.X / ActualWidth * Config.SwipeLengthRate;
            _swipeAnimationConstructor.Config.CurrentSwipeWidth = Math.Abs(_mainLayerTransform.X);

            _swipeAnimationConstructor.DisplaySwipeAnimation( 
                ()=> 
                {
                    if (isFixMode)
                    {
                        Config.Direction = oldDirection;
                    }
                    if(SwipeTrigger!= null)
                    {
                        SwipeTrigger(this, new SwipeTriggerEventArgs(oldDirection));
                    }
                }, 
                () => 
                {
                    if (SwipeComplete != null)
                    {
                        SwipeComplete(this, new SwipeCompleteEventArgs(oldDirection));
                    }
                }
            );

            Config.Direction = SwipeDirection.None;
        }

        public T GetSwipeControl<T>(SwipeDirection direction, string name) where T : FrameworkElement
        {
            if (direction == SwipeDirection.None)
                return default(T);

            var contentCtrl = _leftSwipeContent.Content as DependencyObject;
            return Utils.FindVisualChild<T>(_leftSwipeContent, name);
        }
    }
}
