using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using VCOTLKAPPWeb.Utlilites;
using Newtonsoft.Json.Linq;

namespace VCOTLKAPPWeb.Pages
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AADAuthHelper.StoreAuthorizationCodeFromRequest(this.Request);
            
            }
        }

        protected void GetData_Click(object sender, EventArgs e)
        {
            var accessToken = AADAuthHelper.EnsureValidAccessToken(HttpContext.Current);

            using (WebClient client = new WebClient())
            {
                // To ignore private SSL certificate from our GWM
                ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;

                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                // Replace the value with your SAP OData endpoint stem and OData query parameters.
                var jsonString = client.DownloadString(@"https://gwmdemo.cloudapp.net:8081/perf/sap/opu/odata/sap/ZCAR_POC_SRV/ContosMotorCollection?$top=5");
                var jsonValue = JObject.Parse(jsonString)["d"]["results"];
                var dataCol = jsonValue.ToObject<List<DataModel>>();
                var dataList = dataCol.Select((item) =>
                {
                    var itemString = string.Empty;
                    var typeObj = item.GetType();
                    var fields = typeObj.GetFields();
                    foreach (var field in fields)
                    {
                        var value = field.GetValue(item);
                        itemString += (value != null) ? field.Name + ":" + value + "   " : string.Empty;
                    }

                    return itemString;
                }).ToArray();

                DataListView.DataSource = dataList;
                DataListView.DataBind();
            }
        }

        [WebMethod]
        public static string GetAuthorizeUrl(string hostName)
        {
            if (!AADAuthHelper.IsAuthorized)
            {
                AADAuthHelper.CurrentHostType = hostName;
                return AADAuthHelper.AuthorizeUrl;
            }

            return string.Empty;
        }
    }
}