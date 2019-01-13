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

        public float ShotThrust(int fleetSize)
        {
            return fleetSize * ShotThrustM + ShotThrustB;
        }
        private float ShotThrustM { get => Get<float>("ShotThrustM"); }
        private float ShotThrustB { get => Get<float>("ShotThrustB"); }

        public bool TeamMode { get => Get<bool>("TeamMode", false); }

        public float Drag { get => Get<float>("Drag"); }

        // this is bad that we need this... it means we have
        // physics that aren't time scaled, rather step scaled.
        public int StepSize { get => Get<int>("StepSize", 40); }


        private T Get<T>(string key, T defaultValue = default(T))
        {
            if (Hook.ContainsKey(key))
                return (T)System.Convert.ChangeType(Hook[key], typeof(T));
            else
                return defaultValue;
        }
    }
}
