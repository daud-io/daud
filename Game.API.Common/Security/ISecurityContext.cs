namespace Game.API.Common.Security
{
    using Game.API.Common.Models;

    public interface ISecurityContext
    {
        bool IsAuthenticated { get; }
        UserIdentifier UserIdentifier { get; }
        string[] SecurityIdentifiers { get; }

        void AssumeUser(UserModel user);
        void AssumeToken(string token);
    }
}
