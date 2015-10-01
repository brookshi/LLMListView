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
        private SwipeConfig _config = new SwipeConfig();

        public SwipeConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public static SwipeAnimationConstructor Create(SwipeConfig config)
        {
            SwipeAnimationConstructor constructor = new SwipeAnimationConstructor();
            constructor.Config = config;
            return constructor;
        }

        public void DisplaySwipeAnimation(Action triggerCallback, Action restoreCallback)
        {
            var swipeAnimator = GetSwipeAnimator(_config.SwipeMode);

            if (swipeAnimator == null)
                return;

            if(swipeAnimator.ShouldTriggerAction(Config))
            {
                swipeAnimator.ActionTrigger(Config, triggerCallback);
            }
            else
            {
                swipeAnimator.Restore(Config, restoreCallback);
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
                case SwipeMode.Expand:
                    return ExpandSwipeAnimator.Instance;
                case SwipeMode.None:
                    return null;
                default:
                    throw new NotSupportedException("not supported swipe mode");
            }
        }
    }

    public interface ISwipeAnimator
    {
        void Restore(SwipeConfig config, Action restoreCallback);
        void ActionTrigger(SwipeConfig config, Action triggerCallback);
        bool ShouldTriggerAction(SwipeConfig config);
    }

    public abstract class BaseSwipeAnimator : ISwipeAnimator
    {
        public abstract void ActionTrigger(SwipeConfig config, Action triggerCallback);

        public virtual bool ShouldTriggerAction(SwipeConfig config)
        {
            return config.ActionRateForSwipeLength <= config.CurrentSwipeRate;
        }

        public void Restore(SwipeConfig config, Action restoreCallback)
        {
            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", config.EasingFunc, 0, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", config.EasingFunc, 0, config.Duration));

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

        public override void ActionTrigger(SwipeConfig config, Action triggerCallback)
        {
            Restore(config, triggerCallback);
        }
    }

    public class FixedSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new FixedSwipeAnimator();

        public override void ActionTrigger(SwipeConfig config, Action triggerCallback)
        {
            var targetWidth = config.TriggerActionTargetWidth;
            var clipScaleX = targetWidth / config.CurrentSwipeWidth;

            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", config.EasingFunc, targetWidth, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", config.EasingFunc, clipScaleX, config.Duration));

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

    public class ExpandSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new ExpandSwipeAnimator();

        public override void ActionTrigger(SwipeConfig config, Action triggerCallback)
        {
            var targetX = config.Direction == SwipeDirection.Left ? -config.ItemActualWidth : config.ItemActualWidth;
            var clipScaleX = config.ItemActualWidth / config.CurrentSwipeWidth;

            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", config.EasingFunc, targetX, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", config.EasingFunc, clipScaleX, config.Duration));

            animStory.Completed += (sender, e) =>
            {
                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                config.SwipeClipTransform.ScaleX = 1;

                if (triggerCallback != null)
                    triggerCallback();
            };

            animStory.Begin();
        }
    }
}
