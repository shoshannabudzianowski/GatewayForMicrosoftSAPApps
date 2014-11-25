using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

namespace VCOTLKAPPWeb.Utlilites
{
    public static class DataGetter
    {
        public static string[][] GetDataMatrix(string requestUrl, string accessToken)
        {
            var jsonResult = GetJsonString(requestUrl, accessToken);
            var jsonValue = JObject.Parse(jsonResult)["d"]["results"];
            var dataCol = jsonValue.ToObject<List<DataModel>>();
            return dataCol.Select((item) =>
            {
                return new string[] { 
                item.StockNo.ToString(),
                item.Price,
                item.Year.ToString(),
                item.Brand,
                item.Model,
                item.Engine,
                item.MaxPower.ToString(),
                item.BodyStyle,
                item.Transmission
            };
            }).ToArray();
        }
        private static string GetJsonString(string requestUrl, string accessToken)
        {
            if (string.IsNullOrEmpty(requestUrl))
                return string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                return client.DownloadString(requestUrl);
            }
        }
    }

}