using Game.Scripts.View.Core;
using JetBrains.Collections.Viewable;

namespace Game.View.Gameplay.Packshot
{
    public class HexaPackshotProtocol : IProtocol
    {
        public IViewableProperty<bool> IsVisible { get; } = new ViewableProperty<bool>(false);
        public Command PlayButtonCommand { get; } = new();
        public Command AnyClickCommand { get; } = new();
    }
}