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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LLM
{
    public class BackAnimationConstructor
    {
        private BackAnimationConfig _config = new BackAnimationConfig();

        public BackAnimationConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public static BackAnimationConstructor Create(BackAnimationConfig config)
        {
            BackAnimationConstructor constructor = new BackAnimationConstructor();
            constructor.Config = config;
            return constructor;
        }

        public void DisplayBackAnimation(SwipeDirection direction)
        {
            var swipeMode = direction == SwipeDirection.Left ? _config.LeftSwipeMode : _config.RightSwipeMode;
            switch(swipeMode)
            {
                case SwipeMode.Collapse:
                    CollapseDisplayBackAnimation.Instance.Display(direction, Config);
                    break;
                default:
                    throw new NotSupportedException("not supported swipe mode");
            }
        }
    }

    public interface IDisplayBackAnimation
    {
        void Display(SwipeDirection direction, BackAnimationConfig config);
    }

    public class CollapseDisplayBackAnimation : IDisplayBackAnimation
    {
        public readonly static IDisplayBackAnimation Instance = new CollapseDisplayBackAnimation();

        public void Display(SwipeDirection direction, BackAnimationConfig config)
        {
            var clipTo = direction == SwipeDirection.Left ? -config.SwipeClipRectangle.Rect.Width : config.SwipeClipRectangle.Rect.Width;
            var easingFunc = direction == SwipeDirection.Left ? config.LeftEasingFunc : config.RightEasingFunc;

            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", easingFunc, 0, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "X", easingFunc, clipTo, config.Duration));

            animStory.Completed += (sender, e) =>
            {
                config.SwipeClipTransform.X = 0;
                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                if (config.Callback != null)
                    config.Callback();
            };

            animStory.Begin();
        }
    }

    public class BackAnimationConfig
    {
        public EasingFunctionBase LeftEasingFunc { get; set; }

        public EasingFunctionBase RightEasingFunc { get; set; }

        public TranslateTransform MainTransform { get; set; }

        public TranslateTransform SwipeClipTransform { get; set; }

        public RectangleGeometry SwipeClipRectangle { get; set; }

        public int Duration { get; set; }

        public Action Callback { get; set; }

        public SwipeMode LeftSwipeMode { get; set; }

        public SwipeMode RightSwipeMode { get; set; }
    }
}
