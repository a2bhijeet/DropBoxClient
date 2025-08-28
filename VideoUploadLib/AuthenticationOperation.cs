using Dropbox.Api;
using VideoBackend.Interface;

namespace VideoBackend
{
    public class AuthenticationOperation : IAuthenticationOperation
    {
        
        public AuthenticationOperation()
        {
        }

        public async Task Authorize()
        {
            // Your app key and secret from the Dropbox App Console
            string appKey = "VideoUpload262";
            string appSecret = "eehgx19oildrdpq";

            // Define the scopes your app needs
            var scopes = new List<string>
                {
                    "files.metadata.read",  // Example: Read file metadata
                    "files.content.read",   // Example: Read file content
                    "account_info.read"     // Example: Access account info
                };

            // OAuth2 authorization URL
            var redirectUri = "https://localhost/authorize"; // Replace with your redirect URI
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, appKey, redirectUri, state: null, tokenAccessType: TokenAccessType.Offline, scopeList: scopes.ToArray());

            Console.WriteLine("1. Go to: " + authorizeUri);
            Console.WriteLine("2. Authorize the app and paste the authorization code here:");
            string authCode = Console.ReadLine();

            // Exchange the authorization code for an access token
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(authCode, appKey, appSecret, redirectUri);

            Console.WriteLine("Access Token: " + response.AccessToken);
        }
    }
}
