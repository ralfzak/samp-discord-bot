using System.Collections.Generic;

namespace domain.Models
{
    public class WikiPageData
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Arguments { get; set; }
        public Dictionary<string, string> ArgumentsDescriptions { get; set; }
        public string CodeExample { get; set; }
    }
}
