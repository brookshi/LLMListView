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

using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LLM
{
    public class SwipeConfig
    {
        public EasingFunctionBase LeftEasingFunc { get; set; }

        public EasingFunctionBase RightEasingFunc { get; set; }

        public TranslateTransform MainTransform { get; set; }

        public ScaleTransform SwipeClipTransform { get; set; }

        public RectangleGeometry SwipeClipRectangle { get; set; }

        public int Duration { get; set; }

        public SwipeMode LeftSwipeMode { get; set; }

        public SwipeMode RightSwipeMode { get; set; }

        public SwipeDirection Direction { get; set; }

        public double LeftActionRateForSwipeLength { get; set; }

        public double RightActionRateForSwipeLength { get; set; }

        public double LeftSwipeLengthRate { get; set; }

        public double RightSwipeLengthRate { get; set; }

        public double ItemActualWidth { get; set; }

        public double CurrentSwipeWidth { get; set; }


        public double LeftRateForActualWidth { get { return LeftSwipeLengthRate * LeftActionRateForSwipeLength; } }

        public double RightRateForActualWidth { get { return RightSwipeLengthRate * RightActionRateForSwipeLength; } }

        public bool CanSwipeLeft { get { return Direction == SwipeDirection.Left && LeftSwipeMode != SwipeMode.None; } }

        public bool CanSwipeRight { get { return Direction == SwipeDirection.Right && RightSwipeMode != SwipeMode.None; } }

        public EasingFunctionBase EasingFunc { get { return Direction == SwipeDirection.Left ? LeftEasingFunc : RightEasingFunc; } }

        public double SwipeLengthRate { get { return Direction == SwipeDirection.Left ? LeftSwipeLengthRate : RightSwipeLengthRate; } }

        public double ActionRateForSwipeLength { get { return Direction == SwipeDirection.Left ? LeftActionRateForSwipeLength : RightActionRateForSwipeLength; } }

        public SwipeMode SwipeMode { get { return Direction == SwipeDirection.Left ? LeftSwipeMode : RightSwipeMode; } }

        public double CurrentSwipeRate { get { return CurrentSwipeWidth / ItemActualWidth / SwipeLengthRate; } }

        public double TriggerActionTargetWidth { get { return ItemActualWidth * SwipeLengthRate; } }
    }
}
