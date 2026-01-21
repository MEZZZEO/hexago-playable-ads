using Game.Scripts.View.Core;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Game.View.Gameplay.Tutorial
{
    public class HexaTutorialUIProtocol : IProtocol
    {
        public IViewableProperty<bool> IsActive { get; } = new ViewableProperty<bool>(false);
        public IViewableProperty<bool> ShowHand { get; } = new ViewableProperty<bool>(false);
        public IViewableProperty<Vector3> HandStartPosition { get; } = new ViewableProperty<Vector3>(Vector3.zero);
        public IViewableProperty<Vector3> HandEndPosition { get; } = new ViewableProperty<Vector3>(Vector3.zero);
    }
}