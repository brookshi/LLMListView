using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
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

        public static T FindVisualChild<T>(DependencyObject obj, string childName) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T && ((T)child).Name == childName)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child, childName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            if (parent is T)
                return (T)parent;
            else
            {
                T parentOfParent = FindVisualParent<T>(parent);
                if (parentOfParent != null)
                    return parentOfParent;
            }
            return null;
        }
    }
}
