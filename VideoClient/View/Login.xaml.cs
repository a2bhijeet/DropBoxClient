using Dropbox.Api;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;

namespace VideoClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        #region Variables  
        private const string RedirectUri = "https://localhost/authorize";
        private string DropBoxAppKey = string.Empty;
        private string DropBoxAuthenticationURL = string.Empty;
        private string DropBoxoauth2State = string.Empty;
        #endregion

        #region Properties  
        public string AccessToken { get; private set; }

        public string UserId { get; private set; }

        public bool Result { get; private set; }
        #endregion

        public Login(string AppKey, string AuthenticationURL, string oauth2State)
        {
            InitializeComponent();
            DropBoxAppKey = AppKey;
            DropBoxAuthenticationURL = AuthenticationURL;
            DropBoxoauth2State = oauth2State;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var webView2Environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EdgeWebView2"));
            await Browser.EnsureCoreWebView2Async(webView2Environment);
            await Dispatcher.BeginInvoke(new Action(Navigate));
        }

        public void Navigate()
        {
            try
            {
                if (!string.IsNullOrEmpty(DropBoxAppKey))
                {
                    Uri authorizeUri = new Uri(DropBoxAuthenticationURL);
                    //Browser.Navigate(authorizeUri);

                    Browser.CoreWebView2.Navigate(authorizeUri.AbsoluteUri);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while generating Authentication URL. Please check the App Key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Browser_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(RedirectUri.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.  
                return;
            }

            try
            {

                OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(e.Uri));
                if (result.State != DropBoxoauth2State)
                {
                    return;
                }

                this.AccessToken = result.AccessToken;
                this.Uid = result.Uid;
                this.Result = true;
            }

            catch (ArgumentException ex)
            {
            }

            finally
            {
                e.Cancel = true;
                this.Close();
            }
        }
    }
}