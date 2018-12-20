namespace Game.Robots.Behaviors.Blending
{
    using System.Collections.Generic;

    public interface IContextRingBlending
    {
        float Blend(IEnumerable<ContextRing> contexts);
    }
}