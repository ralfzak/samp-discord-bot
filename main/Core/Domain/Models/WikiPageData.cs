using System.Collections.Generic;

namespace main.Core.Domain.Models
{
    /// <summary>
    /// Responsible for carrying a parsed wiki article data.
    /// </summary>
    public class WikiPageData
    {
        /// <summary>
        /// The actual wiki article name.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The article wiki url.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// The article page description. A "Unknown Description" default value if article parsing failed.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Comma delimited set of arguments. A "()" default value if article parsing failed.
        /// </summary>
        public string Arguments { get; set; }
        
        /// <summary>
        /// All the wiki arguments with respective descriptions. An empty dictionary if article parsing failed.
        /// </summary>
        public Dictionary<string, string> ArgumentsDescriptions { get; set; }
        
        /// <summary>
        /// Wiki article code section. An empty value if article parsing failed.
        /// </summary>
        public string CodeExample { get; set; }
    }
}
