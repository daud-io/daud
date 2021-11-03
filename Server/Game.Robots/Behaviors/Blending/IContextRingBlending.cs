namespace Game.Robots.Behaviors.Blending
{
    using System.Collections.Generic;

    public interface IContextRingBlending
    {
        (ContextRing, float, bool) Blend(IEnumerable<ContextRing> contexts, bool doBoost);
    }
}