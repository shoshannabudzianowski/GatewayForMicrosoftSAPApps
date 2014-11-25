using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SAP2SharePointWeb
{
    // Provides token handling logic for applications that use Azure AD as the OAuth Secure Token Service
    internal static class AADAuthHelper
    {
        // Fields
        // All variable information is stored in web.config.
        private static readonly string _authority = ConfigurationManager.AppSettings["Authority"];
        private static readonly string _appRedirectUrl = ConfigurationManager.AppSettings["AppRedirectUrl"];
        private static readonly string _resourceUrl = ConfigurationManager.AppSettings["ResourceUrl"];
        private static readonly string _clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static readonly ClientCredential _clientCredential = new ClientCredential(
                                   ConfigurationManager.AppSettings["ida:ClientID"],
                                   ConfigurationManager.AppSettings["ida:ClientKey"]);

        private static readonly AuthenticationContext _authenticationContext =
                    new AuthenticationContext("https://login.windows.net/common/" +
                                              ConfigurationManager.AppSettings["Authority"]);
        // Properties

        // The Azure AD form where the user logs in. AAD issues an authorization code
        // to the application that is seeking access to the protected resource.
        // See "Authorization Code Grant Flow" [msdn.microsoft.com/en-us/library/dn645542.aspx]
        private static string AuthorizeUrl
        {
            get
            {
                return string.Format("https://login.windows.net/{0}/oauth2/authorize?response_type=code&redirect_uri={1}&client_id={2}&state={3}",
                    _authority,
                    _appRedirectUrl,
                    _clientId,
                    Guid.NewGuid().ToString());
            }
        }

        public static Tuple<string, DateTimeOffset> AccessToken
        {
            get
            {
                return HttpContext.Current.Session["AccessTokenWithExpireTime-" + _resourceUrl]
                       as Tuple<string, DateTimeOffset>;
            }

            set { HttpContext.Current.Session["AccessTokenWithExpireTime-" + _resourceUrl] = value; }
        }

        private static bool IsAccessTokenValid
        {
            get
            {
                return AccessToken != null &&
                    !string.IsNullOrEmpty(AccessToken.Item1) &&
                    AccessToken.Item2 > DateTimeOffset.UtcNow;
            }
        }

        private static string RefreshToken
        {
            get { return HttpContext.Current.Session["RefreshToken" + _resourceUrl] as string; }
            set { HttpContext.Current.Session["RefreshToken-" + _resourceUrl] = value; }
        }

        private static bool IsRefreshTokenValid
        {
            get { return !string.IsNullOrEmpty(RefreshToken); }
        }

        // Methods

        private static bool IsAuthorizationCodeNotNull(string authCode)
        {
            return !string.IsNullOrEmpty(authCode);
        }

        // Gets an access token and a refresh token using authorization code 
        // received from AAD.
        private static Tuple<Tuple<string, DateTimeOffset>, string> AcquireTokensUsingAuthCode(string authCode)
        {
                var authResult = _authenticationContext.AcquireTokenByAuthorizationCode(
                authCode,
                new Uri(_appRedirectUrl),
                _clientCredential,
                _resourceUrl);

                return new Tuple<Tuple<string, DateTimeOffset>, string>(
                          new Tuple<string, DateTimeOffset>(authResult.AccessToken, authResult.ExpiresOn),
                          authResult.RefreshToken);
       }

        // Replaces an expired access token by using a long-lived refresh token 
        // received from AAD.
        private static Tuple<string, DateTimeOffset> RenewAccessTokenUsingRefreshToken()
        {
            var authResult = _authenticationContext.AcquireTokenByRefreshToken(
                                 RefreshToken,
                                 _clientId,
                                 _clientCredential,
                                 _resourceUrl);

            return new Tuple<string, DateTimeOffset>(authResult.AccessToken, authResult.ExpiresOn);
        }

        // Gets a new access token if the cached one is expired. Call this before every
        // call to the GWM OData endpoint.
        // Parameter "page" is the page object from which the method is called, so the
        // method has access to the Request.QueryString and the Response objects.
        internal static void EnsureValidAccessToken(Page page)
        {
              if (IsAccessTokenValid) 
              {
                  return;
              }
              else if (IsRefreshTokenValid) 
              {
	              AccessToken = RenewAccessTokenUsingRefreshToken();
	              return;
              }
              else if (IsAuthorizationCodeNotNull(page.Request.QueryString["code"]))
              {
                  Tuple<Tuple<string, DateTimeOffset>, string> tokens = null;
                  try
                  {
                      tokens = AcquireTokensUsingAuthCode(page.Request.QueryString["code"]);
                  }
                  catch 
                  {
                      // Some errors, such as expired authcode, are fixed if user logs in to AAD again.
                      // For more extensive error handling advice, see ""Token Issuance Endpoint Errors"
                      // [http://msdn.microsoft.com/en-us/library/azure/dn645548.aspx]
                      page.Response.Redirect(AuthorizeUrl);
                  }
                  AccessToken = tokens.Item1;
	              RefreshToken = tokens.Item2;
	              return;
              }
              else
              {
	              // There's no valid token or authorization code, so user must login to AAD.
                  page.Response.Redirect(AuthorizeUrl);
              }
        }


    }
}