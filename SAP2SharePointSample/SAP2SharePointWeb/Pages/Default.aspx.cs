using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;


namespace SAP2SharePointWeb
{
    public partial class Default : System.Web.UI.Page
    {
        // Replace the value with your SAP OData endpoint stem. End it with a single forward slash.
        private const string SAP_ODATA_URL = @"https://stem_of_SAP_OData_endpoint/";
               
        protected void Page_Load(object sender, EventArgs e)
        {
            // The next line avoids certificate errors while developing. 
            // Remove it for production application.
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;
            
            if (!IsPostBack)
            {
                // Replace parameter with an OData object and optional query parameters.
                GetSAPData("some_data_collection?$top=3");
            }

        }

        private void GetSAPData(string oDataQuery)
        {
            // Always ensure there is a valid cached access token before querying GWM.
            // Pass this page object as the parameter.
            AADAuthHelper.EnsureValidAccessToken(this);

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + AADAuthHelper.AccessToken.Item1;
                var jsonString = client.DownloadString(SAP_ODATA_URL + oDataQuery);
                var jsonValue = JObject.Parse(jsonString)["d"]["results"];
                var dataCol = jsonValue.ToObject<List<DataModel>>();

                var dataList = dataCol.Select((item) =>
                {
                    // replace the property names with names that match your data model class.
                    return item.Name + " " + item.Date + " " + item.Location;
                }).ToArray();

                DataListView.DataSource = dataList;
                DataListView.DataBind();
            }
        }
    }
}