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
    public class SwipeAnimationConstructor
    {
        private SwipeAnimatorConfig _config = new SwipeAnimatorConfig();

        public SwipeAnimatorConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public static SwipeAnimationConstructor Create(SwipeAnimatorConfig config)
        {
            SwipeAnimationConstructor constructor = new SwipeAnimationConstructor();
            constructor.Config = config;
            return constructor;
        }

        public void DisplaySwipeAnimation(SwipeDirection direction, double currentSwipeRate, Action triggerCallback, Action restoreCallback)
        {
            var swipeMode = direction == SwipeDirection.Left ? _config.LeftSwipeMode : _config.RightSwipeMode;
            var swipeAnimator = GetSwipeAnimator(swipeMode);
            if (swipeAnimator == null)
                return;

            if(swipeAnimator.ShouldTriggerAction(direction, Config, currentSwipeRate))
            {
                swipeAnimator.ActionTrigger(direction, Config, triggerCallback);
            }
            else
            {
                swipeAnimator.Restore(direction, Config, restoreCallback);
            }
        }

        public ISwipeAnimator GetSwipeAnimator(SwipeMode mode)
        {
            switch (mode)
            {
                case SwipeMode.Collapse:
                    return CollapseSwipeAnimator.Instance;
                case SwipeMode.None:
                    return null;
                default:
                    throw new NotSupportedException("not supported swipe mode");
            }
        }
    }

    public interface ISwipeAnimator
    {
        void Restore(SwipeDirection direction, SwipeAnimatorConfig config, Action restoreCallback);
        void ActionTrigger(SwipeDirection direction, SwipeAnimatorConfig config, Action triggerCallback);
        bool ShouldTriggerAction(SwipeDirection direction, SwipeAnimatorConfig config, double currentSwipeRate);
    }

    public abstract class BaseSwipeAnimator : ISwipeAnimator
    {
        public abstract void ActionTrigger(SwipeDirection direction, SwipeAnimatorConfig config, Action triggerCallback);

        public virtual bool ShouldTriggerAction(SwipeDirection direction, SwipeAnimatorConfig config, double currentSwipeRate)
        {
            var swipeTrigActionRate = direction == SwipeDirection.Left ? config.LeftActionRateForSwipeLength : config.RightActionRateForSwipeLength;
           
            return swipeTrigActionRate <= currentSwipeRate;
        }

        public void Restore(SwipeDirection direction, SwipeAnimatorConfig config, Action restoreCallback)
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
                if (restoreCallback != null)
                    restoreCallback();
            };

            animStory.Begin();
        }
    }

    public class CollapseSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new CollapseSwipeAnimator();

        public override void ActionTrigger(SwipeDirection direction, SwipeAnimatorConfig config, Action triggerCallback)
        {
            Restore(direction, config, triggerCallback);
        }
    }

    public class SwipeAnimatorConfig
    {
        public EasingFunctionBase LeftEasingFunc { get; set; }

        public EasingFunctionBase RightEasingFunc { get; set; }

        public TranslateTransform MainTransform { get; set; }

        public TranslateTransform SwipeClipTransform { get; set; }

        public RectangleGeometry SwipeClipRectangle { get; set; }

        public int Duration { get; set; }

        public SwipeMode LeftSwipeMode { get; set; }

        public SwipeMode RightSwipeMode { get; set; }

        //public double LeftSwipeLengthRate { get; set; }

        //public double RightSwipeLengthRate { get; set; }

        public double LeftActionRateForSwipeLength { get; set; }

        public double RightActionRateForSwipeLength { get; set; }
    }
}
