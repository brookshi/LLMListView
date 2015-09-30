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

        public void DisplaySwipeAnimation(SwipeDirection direction, Action triggerCallback, Action restoreCallback)
        {
            var currentDirection = direction;
            var swipeMode = direction == SwipeDirection.Left ? _config.LeftSwipeMode : _config.RightSwipeMode;
            var swipeAnimator = GetSwipeAnimator(swipeMode);

            if (swipeAnimator == null)
                return;

            if(swipeAnimator.ShouldTriggerAction(direction, Config))
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
                case SwipeMode.Fix:
                    return FixedSwipeAnimator.Instance;
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
        bool ShouldTriggerAction(SwipeDirection direction, SwipeAnimatorConfig config);
    }

    public abstract class BaseSwipeAnimator : ISwipeAnimator
    {
        public abstract void ActionTrigger(SwipeDirection direction, SwipeAnimatorConfig config, Action triggerCallback);

        public virtual bool ShouldTriggerAction(SwipeDirection direction, SwipeAnimatorConfig config)
        {
            return config.GetActionRateForSwipeLength(direction) <= config.GetCurrentSwipeRate(direction);
        }

        public void Restore(SwipeDirection direction, SwipeAnimatorConfig config, Action restoreCallback)
        {
            var easingFunc = config.GetEasingFunc(direction);

            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", easingFunc, 0, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", easingFunc, 0, config.Duration));

            animStory.Completed += (sender, e) =>
            {
                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                config.SwipeClipTransform.ScaleX = 1;

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

    public class FixedSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new FixedSwipeAnimator();

        public override void ActionTrigger(SwipeDirection direction, SwipeAnimatorConfig config, Action triggerCallback)
        {
            var targetWidth = config.GetTriggerActionTargetWidth(direction);
            var clipScaleX = targetWidth / config.CurrentSwipeWidth;
            var easingFunc = config.GetEasingFunc(direction);

            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", easingFunc, targetWidth, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", easingFunc, clipScaleX, config.Duration));

            animStory.Completed += (sender, e) =>
            {
                config.SwipeClipTransform.ScaleX = 1;
                config.SwipeClipRectangle.Rect = new Rect(0, 0, targetWidth, config.SwipeClipRectangle.Rect.Height);

                if (triggerCallback != null)
                    triggerCallback();
            };

            animStory.Begin();
        }
    }

    public class SwipeAnimatorConfig
    {
        public EasingFunctionBase LeftEasingFunc { get; set; }

        public EasingFunctionBase RightEasingFunc { get; set; }

        public TranslateTransform MainTransform { get; set; }

        public ScaleTransform SwipeClipTransform { get; set; }

        public RectangleGeometry SwipeClipRectangle { get; set; }

        public int Duration { get; set; }

        public SwipeMode LeftSwipeMode { get; set; }

        public SwipeMode RightSwipeMode { get; set; }

        public double LeftActionRateForSwipeLength { get; set; }

        public double RightActionRateForSwipeLength { get; set; }

        public double LeftSwipeLengthRate { get; set; }

        public double RightSwipeLengthRate { get; set; }

        public double ItemActualWidth { get; set; }

        public double CurrentSwipeWidth { get; set; }

        public double LeftRateForActualWidth
        {
            get
            {
                return LeftSwipeLengthRate * LeftActionRateForSwipeLength;
            }
        }

        public double RightRateForActualWidth
        {
            get
            {
                return RightSwipeLengthRate * RightActionRateForSwipeLength;
            }
        }

        public EasingFunctionBase GetEasingFunc(SwipeDirection direction)
        {
            return direction == SwipeDirection.Left ? LeftEasingFunc : RightEasingFunc;
        }

        public double GetSwipeLengthRate(SwipeDirection direction)
        {
            return direction == SwipeDirection.Left ? LeftSwipeLengthRate : RightSwipeLengthRate;
        }

        public double GetActionRateForSwipeLength(SwipeDirection direction)
        {
            return direction == SwipeDirection.Left ? LeftActionRateForSwipeLength : RightActionRateForSwipeLength;
        }

        public SwipeMode GetSwipeMode(SwipeDirection direction)
        {
            return direction == SwipeDirection.Left ? LeftSwipeMode : RightSwipeMode;
        }

        public double GetCurrentSwipeRate(SwipeDirection direction)
        {
            return CurrentSwipeWidth / ItemActualWidth / GetSwipeLengthRate(direction);
        }

        public double GetTriggerActionTargetWidth(SwipeDirection direction)
        {
            return ItemActualWidth * GetSwipeLengthRate(direction);
        }
    }
}
