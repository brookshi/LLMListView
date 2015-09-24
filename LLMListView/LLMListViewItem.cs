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

namespace LLM
{
    public sealed class LLMListViewItem : ListViewItem
    {
        private SwipeDirection _direction = SwipeDirection.None;
        private TranslateTransform _mainLayerTransform;
        private RectangleGeometry _swipeLayerClip;
        private ContentControl _rightSwipeContent;
        private ContentControl _leftSwipeContent;


        public SwipeMode SwipeMode
        {
            get { return (SwipeMode)GetValue(SwipeModeProperty); }
            set { SetValue(SwipeModeProperty, value); }
        }

        public static readonly DependencyProperty SwipeModeProperty =
            DependencyProperty.Register("SwipeMode", typeof(SwipeMode), typeof(LLMListViewItem), new PropertyMetadata(SwipeMode.Fix));


        public LLMListViewItem()
        {
            this.DefaultStyleKey = typeof(LLMListViewItem);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _mainLayerTransform = (TranslateTransform)GetTemplateChild("MainLayerTransform");
            _swipeLayerClip = (RectangleGeometry)GetTemplateChild("SwipeLayerClip");
            _rightSwipeContent = (ContentControl)GetTemplateChild("RightSwipeContent");
            _leftSwipeContent = (ContentControl)GetTemplateChild("LeftSwipeContent");
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            if(SwipeMode == SwipeMode.None)
            {
                e.Complete();
                return;
            }

            if(_direction == SwipeDirection.None)
            {
                _direction = e.Delta.Translation.X > 0 ? SwipeDirection.Left : SwipeDirection.Right;
                _leftSwipeContent.Visibility = _direction == SwipeDirection.Left ? Visibility.Visible : Visibility.Collapsed;
                _rightSwipeContent.Visibility = _direction == SwipeDirection.Right ? Visibility.Visible : Visibility.Collapsed;
            }
            else if(_direction == SwipeDirection.Left)
            {
                var translateX = Math.Max(0, e.Cumulative.Translation.X);
                _swipeLayerClip.Rect = new Rect(0, 0, translateX, ActualHeight);
                _mainLayerTransform.X = translateX;
            }
        }
    }
}
