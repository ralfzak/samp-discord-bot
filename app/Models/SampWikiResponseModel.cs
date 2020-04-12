using System.Collections.Generic;

namespace app.Models
{
    public class SampWikiResponseModel
    {
        public string status { get; set; }
        public string description { get; set; }
        public string parameters { get; set; }
        public List<SampWikiResponseModelParameter> param { get; set; }
        public List<string> examples { get; set; }

        public SampWikiResponseModel()
        {
            param = new List<SampWikiResponseModelParameter>();
        }

        public class SampWikiResponseModelParameter
        {
            public string name { get; set; }
            public string desc { get; set; }
        }
    }
}
