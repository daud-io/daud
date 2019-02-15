namespace Game.Robots.Behaviors.Blending
{
    using System.Collections.Generic;

    public interface IContextRingBlending
    {
        (ContextRing, float) Blend(IEnumerable<ContextRing> contexts);
    }
}