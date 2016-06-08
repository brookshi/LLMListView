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
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        private SwipeReleaseAnimationConstructor _swipeAnimationConstructor;
        private bool _isTriggerInTouch = false;

        public event SwipeBeginEventHandler SwipeBeginInTouch;
        public event SwipeProgressEventHandler SwipeProgressInTouch;
        public event SwipeCompleteEventHandler SwipeRestoreComplete;
        public event SwipeCompleteEventHandler SwipeTriggerComplete;
        public event SwipeReleaseEventHandler SwipeBeginTrigger;
        public event SwipeReleaseEventHandler SwipeBeginRestore;
        public event SwipeTriggerEventHandler SwipeTriggerInTouch;

        public SwipeConfig Config { get { return _swipeAnimationConstructor == null ? null : _swipeAnimationConstructor.Config; } }

        #region property

        public bool IsSwipeEnabled
        {
            get { return (bool)GetValue(IsSwipeEnabledProperty); }
            set { SetValue(IsSwipeEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsSwipeEnabledProperty =
            DependencyProperty.Register("IsSwipeEnabled", typeof(bool), typeof(LLMListViewItem), new PropertyMetadata(true, (s,e)=>
            {
                var listViewItem = s as LLMListViewItem;
                if (listViewItem == null)
                    return;

                listViewItem.ItemManipulationMode = (bool)e.NewValue ? ManipulationModes.TranslateX | ManipulationModes.System : ManipulationModes.System;
            }));

        public ManipulationModes ItemManipulationMode
        {
            get { return (ManipulationModes)GetValue(ItemManipulationModeProperty); }
            set { SetValue(ItemManipulationModeProperty, value); }
        }
        public static readonly DependencyProperty ItemManipulationModeProperty =
            DependencyProperty.Register("ItemManipulationMode", typeof(ManipulationModes), typeof(LLMListViewItem), new PropertyMetadata(ManipulationModes.TranslateX | ManipulationModes.System));

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


        public int BackAnimDuration
        {
            get { return (int)GetValue(BackAnimDurationProperty); }
            set { SetValue(BackAnimDurationProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty BackAnimDurationProperty =
            DependencyProperty.Register("BackAnimDuration", typeof(int), typeof(LLMListViewItem), new PropertyMetadata(200));

        public SwipeMode LeftSwipeMode
        {
            get { return (SwipeMode)GetValue(LeftSwipeModeProperty); }
            set { SetValue(LeftSwipeModeProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty LeftSwipeModeProperty =
            DependencyProperty.Register("LeftSwipeMode", typeof(SwipeMode), typeof(LLMListViewItem), new PropertyMetadata(SwipeMode.None));

        public EasingFunctionBase LeftBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(LeftBackAnimEasingFunctionProperty); }
            set { SetValue(LeftBackAnimEasingFunctionProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty LeftBackAnimEasingFunctionProperty =
            DependencyProperty.Register("LeftBackAnimEasingFunction", typeof(EasingFunctionBase), typeof(LLMListViewItem), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        public double LeftSwipeMaxLength
        {
            get { return (double )GetValue(LeftSwipeMaxLengthProperty); }
            set { SetValue(LeftSwipeMaxLengthProperty, value); }
        }
        public static readonly DependencyProperty LeftSwipeMaxLengthProperty =
            DependencyProperty.Register("LeftSwipeMaxLength", typeof(double ), typeof(LLMListViewItem), new PropertyMetadata(0.0));

        public double LeftSwipeLengthRate
        {
            get { return (double)GetValue(LeftSwipeLengthRateProperty); }
            set { SetValue(LeftSwipeLengthRateProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty LeftSwipeLengthRateProperty =
            DependencyProperty.Register("LeftSwipeLengthRate", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(1.0));

        public double LeftActionRateForSwipeLength
        {
            get { return (double)GetValue(LeftActionRateForSwipeLengthProperty); }
            set { SetValue(LeftActionRateForSwipeLengthProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty LeftActionRateForSwipeLengthProperty =
            DependencyProperty.Register("LeftActionRateForSwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.5));

        public double ActualLeftSwipeLengthRate { get { return LeftSwipeMaxLength == 0 ? LeftSwipeLengthRate : LeftSwipeMaxLength / ActualWidth; } }


        public SwipeMode RightSwipeMode
        {
            get { return (SwipeMode)GetValue(RightSwipeModeProperty); }
            set { SetValue(RightSwipeModeProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty RightSwipeModeProperty =
            DependencyProperty.Register("RightSwipeMode", typeof(SwipeMode), typeof(LLMListViewItem), new PropertyMetadata(SwipeMode.None));

        public double RightSwipeMaxLength
        {
            get { return (double)GetValue(RightSwipeMaxLengthProperty); }
            set { SetValue(RightSwipeMaxLengthProperty, value); }
        }
        public static readonly DependencyProperty RightSwipeMaxLengthProperty =
            DependencyProperty.Register("RightSwipeMaxLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.0));

        public double RightSwipeLengthRate
        {
            get { return (double)GetValue(RightSwipeLengthRateProperty); }
            set { SetValue(RightSwipeLengthRateProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty RightSwipeLengthRateProperty =
            DependencyProperty.Register("RightSwipeLengthRate", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(1.0));

        public double ActualRightSwipeLengthRate { get { return RightSwipeMaxLength == 0 ? RightSwipeLengthRate : RightSwipeMaxLength / ActualWidth; } }

        public double RightActionRateForSwipeLength
        {
            get { return (double)GetValue(RightActionRateForSwipeLengthProperty); }
            set { SetValue(RightActionRateForSwipeLengthProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty RightActionRateForSwipeLengthProperty =
            DependencyProperty.Register("RightActionRateForSwipeLength", typeof(double), typeof(LLMListViewItem), new PropertyMetadata(0.5));

        public EasingFunctionBase RightBackAnimEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(RightBackAnimEasingFunctionProperty); }
            set { SetValue(RightBackAnimEasingFunctionProperty, value); UpdateConfig(); }
        }
        public static readonly DependencyProperty RightBackAnimEasingFunctionProperty =
            DependencyProperty.Register("RightBackAnimEasingFunction", typeof(EasingFunctionBase), typeof(LLMListViewItem), new PropertyMetadata(new ExponentialEase() { EasingMode = EasingMode.EaseOut }));

        #endregion


        public LLMListViewItem()
        {
            this.DefaultStyleKey = typeof(LLMListViewItem);
            this.Loaded += LLMListViewItem_Loaded;
        }

        private void LLMListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            _swipeAnimationConstructor = SwipeReleaseAnimationConstructor.Create(new SwipeConfig());
            UpdateConfig();
            SizeChanged += LLMListViewItem_SizeChanged;
        }

        private void UpdateConfig()
        {
            if (Config == null)
                return;

            Config.Duration = BackAnimDuration;
            Config.LeftEasingFunc = LeftBackAnimEasingFunction;
            Config.RightEasingFunc = RightBackAnimEasingFunction;
            Config.LeftSwipeMode = LeftSwipeMode;
            Config.RightSwipeMode = RightSwipeMode;
            Config.MainTransform = _mainLayerTransform;
            Config.SwipeClipTransform = _swipeLayerClipTransform;
            Config.SwipeClipRectangle = _swipeLayerClip;
            Config.LeftActionRateForSwipeLength = LeftActionRateForSwipeLength;
            Config.RightActionRateForSwipeLength = RightActionRateForSwipeLength;
            Config.LeftSwipeLengthRate = ActualLeftSwipeLengthRate;
            Config.RightSwipeLengthRate = ActualRightSwipeLengthRate;
            Config.ItemActualWidth = ActualWidth;
        }

        private void LLMListViewItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateConfig();
            ResetSwipe();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mainLayerTransform = (TranslateTransform)GetTemplateChild("ContentPresenterTranslateTransform");
            _swipeLayerClipTransform = (ScaleTransform)GetTemplateChild("SwipeLayerClipTransform");
            _swipeLayerClip = (RectangleGeometry)GetTemplateChild("SwipeLayerClip");
            _rightSwipeContent = (ContentControl)GetTemplateChild("RightSwipeContent");
            _leftSwipeContent = (ContentControl)GetTemplateChild("LeftSwipeContent");
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            ResetSwipe();
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            SwipeBeginInTouch?.Invoke(this);
            var cumulativeX = e.Cumulative.Translation.X;
            var deltaX = e.Delta.Translation.X;

            if (Config.Direction == SwipeDirection.None)
            {
                ResetSwipe();
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
            else if (swipeLengthRate <= ActualLeftSwipeLengthRate)
            {
                _swipeLayerClip.Rect = new Rect(0, 0, Math.Max(0, cumulativeX), ActualHeight);
                _mainLayerTransform.X = cumulativeX;
                SwipeActionInTouch(cumulativeX, deltaX);
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
            else if (swipeLengthRate <= ActualRightSwipeLengthRate)
            {
                _swipeLayerClip.Rect = new Rect(ActualWidth + cumulativeX, 0, Math.Max(0, -cumulativeX), ActualHeight);
                _mainLayerTransform.X = cumulativeX;
                SwipeActionInTouch(cumulativeX, deltaX);
            }
        }

        private void SwipeActionInTouch(double cumulativeX, double deltaX)
        {
            double currRate = Math.Abs(cumulativeX) / ActualWidth;
            var isTriggerRate = currRate >= (Config.Direction == SwipeDirection.Left ? LeftActionRateForSwipeLength / ActualLeftSwipeLengthRate : RightActionRateForSwipeLength / ActualRightSwipeLengthRate);
            if (_isTriggerInTouch != isTriggerRate)
            {
                _isTriggerInTouch = isTriggerRate;
                SwipeTriggerInTouch?.Invoke(this, new SwipeTriggerEventArgs(Config.Direction, isTriggerRate));
            }
            SwipeProgressInTouch?.Invoke(this, new SwipeProgressEventArgs(Config.Direction, cumulativeX, deltaX, Math.Abs(cumulativeX) / ActualWidth));
        }

        private void ResetSwipe()
        {
            if (Config == null)
                return;

            Config.Direction = SwipeDirection.None;
            _swipeLayerClip.Rect = new Rect(0, 0, 0, 0);
            _mainLayerTransform.X = 0;
            _isTriggerInTouch = false;
        }

        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
        {
            var oldDirection = Config.Direction;
            bool isFixMode = Config.SwipeMode == SwipeMode.Fix;
            var swipeRate = e.Cumulative.Translation.X / ActualWidth * Config.SwipeLengthRate;
            _swipeAnimationConstructor.Config.CurrentSwipeWidth = Math.Abs(_mainLayerTransform.X);

            _swipeAnimationConstructor.DisplaySwipeAnimation(
                (easingFunc, itemToX, duration) => ReleaseAnimationBeginTrigger(oldDirection, easingFunc, itemToX, duration),
                () => ReleaseAnimationTriggerComplete(oldDirection),
                (easingFunc, itemToX, duration) => ReleaseAnimationBeginRestore(oldDirection, easingFunc, itemToX, duration),
                () => ReleaseAnimationRestoreComplete(oldDirection)
            );

            Config.Direction = SwipeDirection.None;
        }

        private void ReleaseAnimationBeginTrigger(SwipeDirection direction, EasingFunctionBase easingFunc, double itemToX, double duration)
        {
            SwipeBeginTrigger?.Invoke(this, new SwipeReleaseEventArgs(direction, easingFunc, itemToX, duration));
        }

        private void ReleaseAnimationTriggerComplete(SwipeDirection direction)
        {
            if (Config.SwipeMode == SwipeMode.Fix)
            {
                Config.Direction = direction;
            }
            SwipeTriggerComplete?.Invoke(this, new SwipeCompleteEventArgs(direction));
        }

        private void ReleaseAnimationBeginRestore(SwipeDirection direction, EasingFunctionBase easingFunc, double itemToX, double duration)
        {
            SwipeBeginRestore?.Invoke(this, new SwipeReleaseEventArgs(direction, easingFunc, itemToX, duration));
        }

        private void ReleaseAnimationRestoreComplete(SwipeDirection direction)
        {
            SwipeRestoreComplete?.Invoke(this, new SwipeCompleteEventArgs(direction));
        }

        public T GetSwipeControl<T>(SwipeDirection direction, string name) where T : FrameworkElement
        {
            if (direction == SwipeDirection.None)
                return default(T);

            var contentCtrl = (direction == SwipeDirection.Left ? _leftSwipeContent : _rightSwipeContent) as DependencyObject;
            return Utils.FindVisualChild<T>(contentCtrl, name);
        }
    }
}
