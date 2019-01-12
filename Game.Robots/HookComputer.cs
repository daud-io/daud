namespace Game.Robots
{
    using System.Collections.Generic;

    public class HookComputer
    {
        public Dictionary<string, object> Hook { get; set; }

        public float ShipThrust(int fleetSize)
        {
            return fleetSize * BaseThrustM + BaseThrustB;
        }

        private float BaseThrustB { get => Get<float>("BaseThrustB"); }
        private float BaseThrustM { get => Get<float>("BaseThrustM"); }
        public bool TeamMode { get => Get<bool>("TeamMode", false); }

        // this is bad that we need this... it means we have
        // physics that aren't time scaled, rather step scaled.
        public int StepSize { get => Get<int>("StepSize", 40); }

        public float Drag { get => Get<float>("Drag"); }

        private T Get<T>(string key, T defaultValue = default(T))
        {
            if (Hook.ContainsKey(key))
                return (T)System.Convert.ChangeType(Hook[key], typeof(T));
            else
                return defaultValue;
        }
    }
}
