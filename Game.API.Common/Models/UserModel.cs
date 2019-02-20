namespace Game.API.Common.Models
{
    using System.Collections.Generic;

    public class UserModel
    {
        public UserModel() { }
        public UserModel(UserIdentifier identifier)
        {
            this.Identifier = identifier;
        }

        public UserIdentifier Identifier { get; set; }

        public string NickName { get; set; }

        public IEnumerable<string> UserAccessIdentifiers { get; set; }
    }
}
