using Zenject;

namespace Game.Scripts.View.Core
{
    public class InjectParentAttribute : InjectAttributeBase
    {
        public InjectParentAttribute(bool optional = false)
        {
            Source = InjectSources.Parent;
            Optional = optional;
        }
    }
}
