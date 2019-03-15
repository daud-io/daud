namespace Game.Robots
{
    using Game.API.Common.Models;

    public class HookComputer
    {
        public Hook Hook { get; set; }

        public float ShipThrust(int fleetSize)
        {
            return fleetSize * Hook.BaseThrustM + Hook.BaseThrustB;
        }

        public float ShotThrust(int fleetSize)
        {
            return Hook.ShotThrustA / (fleetSize + Hook.ShotThrustB) + Hook.ShotThrustC;
        }
    }
}
