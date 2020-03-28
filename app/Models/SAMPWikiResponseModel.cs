using System.Collections.Generic;

namespace app.Models
{
    class SAMPWikiResponseModel
    {
        public string status { get; set; }
        public string description { get; set; }
        public string parameters { get; set; }
        public List<SAMPWikiResponseModelParameter> param { get; set; }
        public List<string> examples { get; set; }

        public SAMPWikiResponseModel()
        {
            param = new List<SAMPWikiResponseModelParameter>();
        }

        public class SAMPWikiResponseModelParameter
        {
            public string name { get; set; }
            public string desc { get; set; }
        }
    }
}
