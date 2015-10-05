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
    public delegate void AnimationCallback(EasingFunctionBase easingFunc, double itemToX, double duration);

    public class SwipeReleaseAnimationConstructor
    {
        private SwipeConfig _config = new SwipeConfig();

        public SwipeConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public static SwipeReleaseAnimationConstructor Create(SwipeConfig config)
        {
            SwipeReleaseAnimationConstructor constructor = new SwipeReleaseAnimationConstructor();
            constructor.Config = config;
            return constructor;
        }

        public void DisplaySwipeAnimation(AnimationCallback beginTriggerCallback, Action triggerCompleteCallback, AnimationCallback beginRestoreCallback, Action restoreCompleteCallback)
        {
            var swipeAnimator = GetSwipeAnimator(_config.SwipeMode);

            if (swipeAnimator == null)
                return;

            Config.ResetSwipeClipCenterX();

            if (swipeAnimator.ShouldTriggerAction(Config))
            {
                swipeAnimator.ActionTrigger(Config, beginTriggerCallback, triggerCompleteCallback);
            }
            else
            {
                swipeAnimator.Restore(Config, beginRestoreCallback, restoreCompleteCallback);
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
        void Restore(SwipeConfig config, AnimationCallback beginRestoreCallback, Action restoreCompleteCallback);
        void ActionTrigger(SwipeConfig config, AnimationCallback beginTriggerCallback, Action triggerCompleteCallback);
        bool ShouldTriggerAction(SwipeConfig config);
    }

    public abstract class BaseSwipeAnimator : ISwipeAnimator
    {
        public abstract void ActionTrigger(SwipeConfig config, AnimationCallback beginTriggerCallback, Action triggerCompleteCallback);

        public virtual bool ShouldTriggerAction(SwipeConfig config)
        {
            return config.ActionRateForSwipeLength <= config.CurrentSwipeRate;
        }

        public void Restore(SwipeConfig config, AnimationCallback beginRestoreCallback, Action restoreCompleteCallback)
        {
            if (beginRestoreCallback != null)
                beginRestoreCallback(config.EasingFunc, 0, config.Duration);

            DisplayAnimation(config, 0, 0, ()=>
            {
                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                config.SwipeClipTransform.ScaleX = 1;

                if (restoreCompleteCallback != null)
                    restoreCompleteCallback();
            });
        }

        protected void DisplayAnimation(SwipeConfig config, double itemTo, double clipTo, Action complete)
        {
            Storyboard animStory = new Storyboard();
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.MainTransform, "X", config.EasingFunc, itemTo, config.Duration));
            animStory.Children.Add(Utils.CreateDoubleAnimation(config.SwipeClipTransform, "ScaleX", config.EasingFunc, clipTo, config.Duration));

            animStory.Completed += (sender, e) =>
            {
                if (complete != null)
                    complete();
            };

            animStory.Begin();
        }
    }

    public class CollapseSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new CollapseSwipeAnimator();

        public override void ActionTrigger(SwipeConfig config, AnimationCallback beginTriggerCallback, Action triggerCompleteCallback)
        {
            if (beginTriggerCallback != null)
                beginTriggerCallback(config.EasingFunc, 0, config.Duration);

            DisplayAnimation(config, 0, 0, () =>
            {
                if (triggerCompleteCallback != null)
                    triggerCompleteCallback();

                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                config.SwipeClipTransform.ScaleX = 1;
            });
        }
    }

    public class FixedSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new FixedSwipeAnimator();

        public override void ActionTrigger(SwipeConfig config, AnimationCallback beginTriggerCallback, Action triggerCompleteCallback)
        {
            var targetWidth = config.TriggerActionTargetWidth;
            var clipScaleX = targetWidth / config.CurrentSwipeWidth;

            if (beginTriggerCallback != null)
                beginTriggerCallback(config.EasingFunc, targetWidth, config.Duration);

            DisplayAnimation(config, targetWidth, clipScaleX, ()=>
            {
                if (triggerCompleteCallback != null)
                    triggerCompleteCallback();

                config.SwipeClipTransform.ScaleX = 1;
                config.SwipeClipRectangle.Rect = new Rect(0, 0, targetWidth, config.SwipeClipRectangle.Rect.Height);
            });
        }
    }

    public class ExpandSwipeAnimator : BaseSwipeAnimator
    {
        public readonly static ISwipeAnimator Instance = new ExpandSwipeAnimator();

        public override void ActionTrigger(SwipeConfig config, AnimationCallback beginTriggerCallback, Action triggerCompleteCallback)
        {
            var targetX = config.Direction == SwipeDirection.Left ? config.ItemActualWidth : -config.ItemActualWidth;
            var clipScaleX = config.ItemActualWidth / config.CurrentSwipeWidth;

            if (beginTriggerCallback != null)
                beginTriggerCallback(config.EasingFunc, targetX, config.Duration);

            DisplayAnimation(config, targetX, clipScaleX, ()=>
            {
                if (triggerCompleteCallback != null)
                    triggerCompleteCallback();

                config.SwipeClipRectangle.Rect = new Rect(0, 0, 0, 0);
                config.SwipeClipTransform.ScaleX = 1;
            });
        }
    }
}
