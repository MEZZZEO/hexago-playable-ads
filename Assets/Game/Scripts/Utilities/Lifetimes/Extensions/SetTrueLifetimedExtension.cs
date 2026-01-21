using JetBrains.Collections.Viewable;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class SetTrueLifetimedExtension
    {
        public static void SetTrueLifetimed(this IViewableProperty<bool> property, JetBrains.Lifetimes.Lifetime lifetime)
        {
            property.Value = true;
            lifetime.OnTermination(() => property.Value = false);
        }
    }
}
