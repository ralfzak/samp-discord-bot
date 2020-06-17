using System.Collections.Generic;

namespace main.Core.Domain.Models
{
    /// <summary>
    /// Responsible for carrying a parsed wiki article data.
    /// </summary>
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
