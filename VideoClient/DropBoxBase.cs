using Dropbox.Api;
using System.Net.Http;
using System.Windows;

namespace VideoClient
{
    class DropBoxBase
    {
        #region Variables  
        private string oauth2State;
        private const string RedirectUri = "https://localhost/authorize"; // Same as we have configured Under [Application] -> settings -> redirect URIs.  
        #endregion

        #region Constructor  
        public DropBoxBase(string ApiKey, string ApiSecret, string ApplicationName = "TestApp")
        {
            try
            {
                AppKey = ApiKey;
                AppSecret = ApiSecret;
                AppName = ApplicationName;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Properties  
        public string AppName
        {
            get; private set;
        }
        public string AuthenticationURL
        {
            get; private set;
        }
        public string AppKey
        {
            get; private set;
        }

        public string AppSecret
        {
            get; private set;
        }

        public string AccessTocken
        {
            get; private set;
        }
        public string Uid
        {
            get; private set;
        }
        #endregion

        #region UserDefined Methods  

        /// <summary>  
        /// This method is to generate Authentication URL to redirect user for login process in Dropbox.  
        /// </summary>  
        /// <returns></returns>  
        public string GeneratedAuthenticationURL()
        {
            try
            {
                this.oauth2State = Guid.NewGuid().ToString("N");
                Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, AppKey, RedirectUri, state: oauth2State);
                AuthenticationURL = authorizeUri.AbsoluteUri.ToString();
                return authorizeUri.AbsoluteUri.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>  
        /// This method is to generate Access Token required to access dropbox outside of the environment (in ANy application).  
        /// </summary>  
        /// <returns></returns>  
        public string GenerateAccessToken()
        {
            try
            {
                string _strAccessToken = string.Empty;

                if (CanAuthenticate())
                {
                    if (string.IsNullOrEmpty(AuthenticationURL))
                    {
                        throw new Exception("AuthenticationURL is not generated !");

                    }
                    Login login = new Login(AppKey, AuthenticationURL, this.oauth2State); // WPF window with Webbrowser control to redirect user for Dropbox login process.  
                    login.Owner = Application.Current.MainWindow;
                    login.ShowDialog();
                    if (login.Result)
                    {
                        _strAccessToken = login.AccessToken;
                        AccessTocken = login.AccessToken;
                        Uid = login.Uid;
                        DropboxClientConfig CC = new DropboxClientConfig(AppName, 1);
                        HttpClient HTC = new HttpClient();
                        HTC.Timeout = TimeSpan.FromMinutes(10); // set timeout for each ghttp request to Dropbox API.  
                        CC.HttpClient = HTC;
                    }
                    else
                    {
                        AccessTocken = string.Empty;
                        Uid = string.Empty;
                    }
                }

                return _strAccessToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Validation Methods  
        /// <summary>  
        /// Validation method to verify that AppKey and AppSecret is not blank.  
        /// Mendatory to complete Authentication process successfully.  
        /// </summary>  
        /// <returns></returns>  
        public bool CanAuthenticate()
        {
            try
            {
                if (AppKey == null)
                {
                    throw new ArgumentNullException("AppKey");
                }
                if (AppSecret == null)
                {
                    throw new ArgumentNullException("AppSecret");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion
    }
}
