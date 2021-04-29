namespace Discounting.Logics.Account
{
    public class BearerTokensOptions
    {
        public string Key { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
        public int AccessTokenExpirationSeconds { set; get; }
        public int RefreshTokenExpirationSeconds { set; get; }
        public bool AllowMultipleLoginsFromTheSameUser { set; get; }
        public bool AllowSignOutAllUserActiveClients { set; get; }
        public string RoleClaimType { set; get; }
        public string NameClaimType { set; get; }
    
    }
}