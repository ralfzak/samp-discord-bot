using System.Collections.Generic;

namespace app.Models
{
    public class WikiResponseModel
    {
        public string status { get; set; }
        public string description { get; set; }
        public string parameters { get; set; }
        public List<WikiResponseModelParameter> param { get; set; }
        public List<string> examples { get; set; }

        public WikiResponseModel()
        {
            param = new List<WikiResponseModelParameter>();
        }

        public class WikiResponseModelParameter
        {
            public string name { get; set; }
            public string desc { get; set; }
        }
    }
}
