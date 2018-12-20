using System.Collections.Generic;

namespace Game.Robots.Behaviors
{
    public interface IContextRingBlending
    {
        float Blend(IEnumerable<ContextRing> contexts);
    }
}