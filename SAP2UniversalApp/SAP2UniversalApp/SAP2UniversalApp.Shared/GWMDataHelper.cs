using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SAP2UniversalApp
{
    internal class GWMDataHelper
    {
        internal static GWMDataHelper Instance = new GWMDataHelper();

        private GWMDataHelper() { }

        // Replace the value with your SAP OData endpoint stem. End it with a single forward slash.
        const string SAPEndPoint = "http://stem_of_SAP_OData_endpoint";
        // Replace parameter with an OData object and optional query parameters.
        const string SAPODataQuery = "/some_data_collection?$top=10";

        internal async Task<DataModel[]> GetGWMData()
        {
            var accessToken = AADAuthHelper.Instance.GetAccessToken();
            string requestUrl = SAPEndPoint + SAPODataQuery;
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await accessToken);
                    request.Headers.Add("Accept", "application/json");
                    HttpResponseMessage response = await client.SendAsync(request);
                    string dataString = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(dataString)["d"]["results"].ToObject<DataModel[]>();
                }
            }

        }
    }

}
