namespace Game.Registry
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Game.Registry.API.Authentication;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class TokenSecurityContext : ISecurityContext
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly JWT JWT;
        private UserModel UserModel = null;
        private IEnumerable<Claim> Claims = null;

        public TokenSecurityContext(IHttpContextAccessor httpContextAccessor, JWT jwt)
        {
            HttpContextAccessor = httpContextAccessor;
            JWT = jwt;
            Claims = httpContextAccessor?.HttpContext?.User.Claims;
        }

        private string this[string claimKey]
        {
            get
            {
                return Claims.FirstOrDefault(c => c.Type == claimKey)?.Value;
            }
        }

        string[] ISecurityContext.SecurityIdentifiers
        {
            get
            {
                if (this.UserModel != null)
                {
                    return UserModel.UserAccessIdentifiers.ToArray();
                }
                else
                {
                    var securityIdentifiersString = this[SecurityTokenClaimKeys.SECURITY_IDENTIFIERS];
                    if (String.IsNullOrWhiteSpace(securityIdentifiersString))
                        return new string[0];
                    else
                        return securityIdentifiersString?.Split(' ');
                }
            }
        }

        UserIdentifier ISecurityContext.UserIdentifier => UserModel?.Identifier ??
            new UserIdentifier
            {
                UserKey = this[SecurityTokenClaimKeys.USER_KEY]
                        ?? throw new Exception("No user in context")
            };

        bool ISecurityContext.IsAuthenticated =>
            this.UserModel != null
            || (this[SecurityTokenClaimKeys.USER_KEY] != null);

        public void AssumeToken(string token)
        {
            var jwt = JWT.Read(token);
            Claims = jwt.Claims;
        }

        void ISecurityContext.AssumeUser(UserModel user)
        {
            UserModel = user;
        }
    }
}
