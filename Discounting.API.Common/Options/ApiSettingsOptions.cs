namespace Discounting.API.Common.Options
{
    public class ApiSettingsOptions
    {
        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }
        public string RefreshTokenPath { get; set; }
        public string AccessTokenObjectKey { get; set; }
        public string RefreshTokenObjectKey { get; set; }
        public string AdminRoleName { get; set; }
        public string SignatureVerifierUrl { get; set; }

        /// <summary>
        /// Defines how often to retry to connect to db before giving up
        /// during the booting of our API
        /// </summary>
        public int DbConnectionRetryCount { get; set; } = 60;
        
        /// <summary>
        /// Interval in milliseconds to wait between connection checks
        /// </summary>
        public int DbConnectionRetryInterval { get; set; } = 5000;

        /// <summary>
        /// Expiration time in seconds - account activation / password reset token
        /// </summary>
        public int ActivationTokenExpirationDuration { get; set; }

        public int PasswordRetryLimit { get; set; }
    }
}