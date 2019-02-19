namespace HelloMeatbags
{
    using System.Threading.Tasks;

    class Program
    {
        public async static Task Main(string[] args)
        {
            // create a new instance of our robot
            var robot = new HelloMeatbagsRobot();

            // start it on the public server, team room
            await robot.StartAsync("https://us.daud.io", "team");
        }
    }
}
