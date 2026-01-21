using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class ViewAliveExtension
    {
        public static void ViewAlive(this IReadonlyProperty<Lifetime> me, Lifetime lifetime, Action<Lifetime> handler)
        {
            if (!lifetime.IsAlive) return;

            // nested lifetime is needed due to exception that could be thrown
            // while viewing a property change right at the moment of <param>lifetime</param>'s termination
            // but before <param>handler</param> gets removed
            var lf = lifetime == Lifetime.Eternal ? lifetime : Lifetime.Define(lifetime).Lifetime;
            var seq = new SequentialLifetimes(lf);
            
            me.Advise(lf, v =>
            {
                var next = seq.Next();
                if (v.IsAlive)
                {
                    handler(next.Intersect(v));
                }
            });
        }
    }
}
