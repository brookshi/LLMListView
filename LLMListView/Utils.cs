using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace LLM
{
    public class Utils
    {
        public static DoubleAnimation CreateDoubleAnimation(DependencyObject target, string propertyPath, EasingFunctionBase easingFunc, double to, double duration)
        {
            var anim = new DoubleAnimation()
            {
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = easingFunc
            };
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, propertyPath);
            return anim;
        }
    }
}
