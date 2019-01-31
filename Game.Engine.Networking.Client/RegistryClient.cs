namespace Game.API.Client
{
    using System;

    public class RegistryClient : APIClientBase
    {
        public RegistryClient(Uri uri)
            : base(uri)
        { }

        public UserMethods User { get => new UserMethods(this); }
        public RegistryMethods Registry { get => new RegistryMethods(this); }

    }
}
