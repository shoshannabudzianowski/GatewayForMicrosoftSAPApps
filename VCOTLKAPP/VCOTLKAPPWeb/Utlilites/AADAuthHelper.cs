using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace VCOTLKAPPWeb.Utlilites
{
    internal static class AADAuthHelper
    {
        private static readonly string _authority = ConfigurationManager.AppSettings["Authority"];
        private static readonly string _appHostName = ConfigurationManager.AppSettings["AppHostName"];
        private static readonly string _resourceUrl = ConfigurationManager.AppSettings["ResourceUrl"];
        private static readonly string _clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static readonly ClientCredential _clientCredential = new ClientCredential(
            ConfigurationManager.AppSettings["ida:ClientID"],
            ConfigurationManager.AppSettings["ida:ClientKey"]);
        private static readonly AuthenticationContext _authenticationContext = new AuthenticationContext("https://login.windows.net/common/" + _authority);

        internal static string CurrentHostType
        {
            get
            {
                var hostInfo = HttpContext.Current.Session["_host_Info"] as string;
                return string.IsNullOrEmpty(hostInfo) ? "client" : hostInfo;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    HttpContext.Current.Session["_host_Info"] = value;
                }
            }
        }
        private static string AppRedirectUrl
        {
            get
            {
                if (string.Compare(CurrentHostType, "client", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return _appHostName + "Pages/Authenticate.aspx";
                }
                return _appHostName + "Pages/Default.aspx";
            }
        }
        private static string CurrentStateGuid
        {
            get
            {
                return HttpContext.Current.Session["state_guid"] as string;
            }
            set
            {
                if (!string.IsNullOrEmpty(value as string))
                {
                    HttpContext.Current.Session["state_guid"] = value;
                }
            }
        }
        internal static bool StoreAuthorizationCodeFromRequest(HttpRequest request)
        {
            var stateGuid = request.QueryString["state"];
            if (!string.IsNullOrEmpty(stateGuid))
            {
                var authCode = request.QueryString["code"];
                if (!string.IsNullOrEmpty(authCode))
                {
                    HttpContext.Current.Cache[stateGuid] = authCode;
                }
            }
            return IsAuthorizationCodeNotEmpty;
        }
        internal static string AuthorizationCode
        {
            get
            {
                if (!string.IsNullOrEmpty(CurrentStateGuid))
                {
                    return HttpContext.Current.Cache[CurrentStateGuid] as string;
                }
                return string.Empty;
            }
        }
        internal static string AuthorizeUrl
        {
            get
            {
                CurrentStateGuid = Guid.NewGuid().ToString();
                return string.Format("https://login.windows.net/{0}/oauth2/authorize?response_type=code&redirect_uri={1}&client_id={2}&state={3}",
                    _authority,
                    AppRedirectUrl,
                    _clientId,
                    CurrentStateGuid);
            }
        }
        private static string RefreshToken
        {
            get
            {
                return HttpContext.Current.Session["RefreshToken" + _resourceUrl] as string;
            }
            set
            {
                HttpContext.Current.Session["RefreshToken-" + _resourceUrl] = value;
            }
        }
        private static bool IsRefreshTokenNotEmpty
        {
            get
            {
                return !string.IsNullOrEmpty(RefreshToken);
            }
        }
        internal static bool IsAuthorized
        {
            get
            {
                return IsAccessTokenValid || IsRefreshTokenNotEmpty || IsAuthorizationCodeNotEmpty;
            }
        }
        private static Tuple<Tuple<string, DateTimeOffset>, string> AcquireTokensUsingAuthCode(string authCode)
        {
            var authResult = _authenticationContext.AcquireTokenByAuthorizationCode(
                authCode,
                new Uri(AppRedirectUrl),
                _clientCredential,
                _resourceUrl);
            return new Tuple<Tuple<string, DateTimeOffset>, string>(
                new Tuple<string, DateTimeOffset>(authResult.AccessToken, authResult.ExpiresOn),
                    authResult.RefreshToken);
        }
        private static Tuple<string, DateTimeOffset> RenewAccessTokenUsingRefreshToken()
        {
            var authResult = _authenticationContext.AcquireTokenByRefreshToken(
                RefreshToken,
               _clientCredential,
               _resourceUrl);
            return new Tuple<string, DateTimeOffset>(authResult.AccessToken, authResult.ExpiresOn);
        }
        internal static string EnsureValidAccessToken(HttpContext currentContext)
        {
            if (IsAccessTokenValid)
            {
                return AccessToken.Item1;
            }
            else if (IsRefreshTokenNotEmpty)
            {
                AccessToken = RenewAccessTokenUsingRefreshToken();
                return AccessToken.Item1;
            }
            else if (StoreAuthorizationCodeFromRequest(currentContext.Request))
            {
                Tuple<Tuple<string, DateTimeOffset>, string> tokens = AcquireTokensUsingAuthCode(AuthorizationCode);
                AccessToken = tokens.Item1;
                RefreshToken = tokens.Item2;
                return AccessToken.Item1;
            }
            else
            {
                throw new InvalidOperationException("Please sign in first.");
            }
        }

        private static bool IsAuthorizationCodeNotEmpty
        {
            get
            {
                return !string.IsNullOrEmpty(AuthorizationCode);
            }
        }

        internal static Tuple<string, DateTimeOffset> AccessToken
        {
            get {
                return HttpContext.Current.Session["AccessTokenWithExpireTime-" + _resourceUrl] as Tuple<string, DateTimeOffset>;
            }
            set {
                HttpContext.Current.Session["AccessTokenWithExpireTime-" + _resourceUrl] = value;
            }
        }

        private static bool IsAccessTokenValid
        {
            get {
                return AccessToken != null &&
                    !string.IsNullOrEmpty(AccessToken.Item1) &&
                    AccessToken.Item2 > DateTimeOffset.UtcNow;
            }
        }
    }

}