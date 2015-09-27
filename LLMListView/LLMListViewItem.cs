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

        private SwipeDirection _direction = SwipeDirection.None;
        private TranslateTransform _mainLayerTransform;
        private TranslateTransform _swipeLayerClipTransform;
        private RectangleGeometry _swipeLayerClip;
        private ContentControl _rightSwipeContent;
        private ContentControl _leftSwipeContent;
        private Border _mainLayer;
        private BackAnimationConstructor _backAnimationConstructor;

        public event SwipeProgressEventHandler SwipeProgress;

        public event SwipeCompleteEventHandler SwipeComplete;

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

        public double SwipeLengthRate
        {
            get { return (double)GetValue(SwipeLengthProperty); }
            set { SetValue(SwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty SwipeLengthProperty =
            DependencyProperty.Register("SwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(1));

        public double ActionRateForSwipeLength
        {   
            get { return (double)GetValue(ActionRateForSwipeLengthProperty); }
            set { SetValue(ActionRateForSwipeLengthProperty, value); }
        }
        public static readonly DependencyProperty ActionRateForSwipeLengthProperty =
            DependencyProperty.Register("ActionRateForSwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.5));

        private bool CanSwipeLeft
        {
            get { return _direction == SwipeDirection.Left && LeftSwipeMode != SwipeMode.None; }
        }

        private bool CanSwipeRight
        {
            get { return _direction == SwipeDirection.Right && RightSwipeMode != SwipeMode.None; }
        }

        #endregion


        public LLMListViewItem()
        {
            this.DefaultStyleKey = typeof(LLMListViewItem);
            this.Loaded += LLMListViewItem_Loaded;
        }

        private void LLMListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            _backAnimationConstructor = BackAnimationConstructor.Create(new BackAnimationConfig() {
                Duration = BackAnimDuration,
                LeftEasingFunc = LeftBackAnimEasingFunction,
                RightEasingFunc = RightBackAnimEasingFunction,
                LeftSwipeMode = LeftSwipeMode,
                RightSwipeMode = RightSwipeMode,
                MainTransform = _mainLayerTransform,
                SwipeClipTransform = _swipeLayerClipTransform,
                SwipeClipRectangle = _swipeLayerClip,
            });
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mainLayer = (Border)GetTemplateChild("MainLayer");
            _mainLayerTransform = (TranslateTransform)GetTemplateChild("MainLayerTransform");
            _swipeLayerClipTransform = (TranslateTransform)GetTemplateChild("SwipeLayerClipTransform");
            _swipeLayerClip = (RectangleGeometry)GetTemplateChild("SwipeLayerClip");
            _rightSwipeContent = (ContentControl)GetTemplateChild("RightSwipeContent");
            _leftSwipeContent = (ContentControl)GetTemplateChild("LeftSwipeContent");
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            var cumulativeX = e.Cumulative.Translation.X;
            var deltaX = e.Delta.Translation.X;

            if (_direction == SwipeDirection.None)
            {
                _direction = deltaX > 0 ? SwipeDirection.Left : SwipeDirection.Right;
                _leftSwipeContent.Visibility = CanSwipeLeft ? Visibility.Visible : Visibility.Collapsed;
                _rightSwipeContent.Visibility = CanSwipeRight ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (CanSwipeLeft)
            {
                if (cumulativeX <= 0)
                {
                    _direction = SwipeDirection.None;
                }
                else
                { 
                    _swipeLayerClip.Rect = new Rect(0, 0, cumulativeX, ActualHeight);
                    _mainLayerTransform.X = cumulativeX;
                }
            }
            else if(CanSwipeRight)
            {
                if (cumulativeX >= 0)
                {
                    _direction = SwipeDirection.None;
                }
                else
                {
                    _swipeLayerClip.Rect = new Rect(ActualWidth + cumulativeX, 0, -cumulativeX, ActualHeight);
                    _mainLayerTransform.X = cumulativeX;
                }
            }

            if(CanSwipeLeft || CanSwipeRight)
            {
                if (SwipeProgress != null)
                {
                    SwipeProgress(this, new SwipeProgressEventArgs(_direction, cumulativeX, deltaX, cumulativeX / ActualWidth));
                }
            }
        }

        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
        {
            var oldDirection = _direction;
            _backAnimationConstructor.DisplayBackAnimation(_direction, () => {
                if (SwipeComplete != null)
                    SwipeComplete(this, new SwipeCompleteEventArgs(oldDirection));
            });
            _direction = SwipeDirection.None;
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
