using System;
using System.Xml.Serialization;

namespace ASPNETMVCWithServerCalls.Codes
{

    [Serializable]
    public class SPDatabaseDetailsRequest
    {

        [XmlIgnore]
        public string DatabaseId { get; set; }

    }

}
