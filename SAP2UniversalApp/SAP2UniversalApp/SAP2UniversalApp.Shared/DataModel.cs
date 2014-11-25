using System;
using System.Collections.Generic;
using System.Text;

namespace SAP2UniversalApp
{
    // The fields defined below are only an example. Change the names and types
    // of the fields (and the number of fields) to match the data from the SAP 
    // OData endpoint. 
    public class DataModel
    {
        public string Title
        {
            get
            {
                return String.Format("{0}-{1}", Brand, Model);
            }
        }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string Engine { get; set; }
        public string ExtColor { get; set; }
        public string Price { get; set; }
        public string Status { get; set; }
        public string ContactPhone { get; set; }
    }
}
