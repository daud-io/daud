namespace Game.API.Client
{
    using System;

    public class APIClient : APIClientBase
    {
        public APIClient(Uri uri)
            : base(uri)
        { }

        public UserMethods User { get => new UserMethods(this); }
        public ServerMethods Server { get => new ServerMethods(this); }
        public WorldMethods World { get => new WorldMethods(this); }
        public PlayerMethods Player { get => new PlayerMethods(this); }
    }
}
