using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SAP2UniversalApp
{
    internal class AADAuthHelper
    {
        internal static AADAuthHelper Instance = new AADAuthHelper();

        private AADAuthHelper() { }

        // Set its value to the Office 365 domain (some_domain.onmicrosoft.com) of your organizational account
        private const string Authority = "";
        // Insert the client ID of your native app that you saved from your Azure AD directory
        private const string ClientID = "";
        // Set its value to the APP ID URI of SAP Gateway for Microsoft from your Azure AD directory
        private const string ResourceId = "";
        // Insert the Redirect URI of your native app that you saved from your Azure AD directory
        internal const string AppRedirectUrl = "";

        private string refreshToken;
        private Tuple<string, DateTimeOffset> accessToken;

        internal bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(this.refreshToken); }
        }

        internal async Task<string> GetAccessToken()
        {
            if (this.accessToken == null ||
                string.IsNullOrEmpty(this.accessToken.Item1) ||
                this.accessToken.Item2 <= DateTimeOffset.UtcNow)
            {
                await RenewAccessToken();
            }

            return this.accessToken.Item1;
        }

        internal string AuthorizeUrl
        {
            get
            {
                return string.Format("https://login.windows.net/{0}/oauth2/authorize?response_type=code&redirect_uri={1}&client_id={2}&resource={3}&state={4}",
                    Authority,
                    AppRedirectUrl,
                    ClientID,
                    ResourceId,
                    Guid.NewGuid().ToString());
            }
        }

        internal async Task AcquireTokenWithAuthorizeCode(string authCode)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("https://login.windows.net/{0}/oauth2/token", Authority));
            string tokenreq = string.Format(
                    "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}",
                    authCode,
                    ClientID,
                    WebUtility.UrlEncode(AppRedirectUrl));

            request.Content = new StringContent(tokenreq, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            JObject tokens = JObject.Parse(responseString);
            this.refreshToken = (string)tokens["refresh_token"];
            var accessTokenString = (string)tokens["access_token"];
            var accessTokenExpireOnSecond = (double)tokens["expires_on"];
            var accessTokenExpireOn = DateTimeOffset.Parse("1970/01/01 00:00:00Z");
            accessTokenExpireOn = accessTokenExpireOn.AddSeconds(accessTokenExpireOnSecond);
            this.accessToken = new Tuple<string, DateTimeOffset>(accessTokenString, accessTokenExpireOn);
        }

        private async Task RenewAccessToken()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("https://login.windows.net/{0}/oauth2/token", Authority));
            string tokenreq = string.Format(
                    "grant_type=refresh_token&refresh_token={0}&resource={1}&client_id={2}",
                    this.refreshToken,
                    WebUtility.UrlEncode(ResourceId),
                    ClientID);
            request.Content = new StringContent(tokenreq, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            JObject tokens = JObject.Parse(responseString);
            var accessTokenString = (string)tokens["access_token"];
            var accessTokenExpireOnSecond = (double)tokens["expires_on"];
            var accessTokenExpireOn = DateTimeOffset.Parse("1970/01/01 00:00:00Z");
            accessTokenExpireOn = accessTokenExpireOn.AddSeconds(accessTokenExpireOnSecond);
            this.accessToken = new Tuple<string, DateTimeOffset>(accessTokenString, accessTokenExpireOn);
        }
    }
}
