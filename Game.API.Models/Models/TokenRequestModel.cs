namespace Game.API.Common.Models
{
    public class TokenRequestModel
    {
        public TokenRequestModel() { }

        public TokenRequestModel(string userKey, string password)
        {
            this.Identifier = new UserIdentifier
            {
                UserKey = userKey
            };
            this.Password = password;
        }

        public UserIdentifier Identifier { get; set; }
        public string Password { get; set; }
        public string ClientClaims { get; set; }
    }
}
