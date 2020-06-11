using main.Core;
using main.Exceptions;
using main.Services;
using Moq;
using Xunit;

namespace test.Services
{
    public class WikiServiceTest
    {
        [Fact]
        public void Test_GetPageData_WithPageHavingBadContent_ThrowsInvalidWikiPageException()
        {
            var subject = Subject(MockHttpClient("any"));

            var exception = Assert.Throws<InvalidWikiPageException>(() => subject.GetPageData("any"));
            Assert.Equal("Failed to parse headers", exception.Message);
        }
        
        [Fact]
        public void Test_GetPageData_WithPageHavingBadWrongHeader_ThrowsInvalidWikiPageException()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div></div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var exception = Assert.Throws<InvalidWikiPageException>(() => 
                subject.GetPageData("anyPageTitle")
                );
            Assert.Equal("Failed to parse headers", exception.Message);
        }
        
        [Fact]
        public void Test_GetPageData_WithPageHavingNoScriptingSection_ThrowsInvalidWikiPageException()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"anything\"></div></div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var exception = Assert.Throws<InvalidWikiPageException>(() => 
                subject.GetPageData("PageTitle")
                );
            Assert.Equal("Failed to parse headers", exception.Message);
        }

        [Fact] 
        public void Test_GetPageData_WithPageHavingNoDescription_ReturnsDefaultValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("Unknown Description", result.Description);
        }

        [Fact] 
        public void Test_GetPageData_WithPageHavingDescription_ReturnsCorrectValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +
                "<div class=\"description\">AnyDescription</div>" +            
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("AnyDescription", result.Description);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingNoCodeExample_ReturnsEmptyValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +        
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Empty(result.CodeExample);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingCodeExample_ReturnsCorrectDecodedValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +  
                "<pre class=\"pawn\"><span>new</span> PlayerDeaths<span>&#91;</span><span>MAX_PLAYERS</span><span>&#93;</span>;</pre>" +
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("new PlayerDeaths[MAX_PLAYERS];", result.CodeExample);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingNoArguments_ReturnsCorrectDefaultValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +  
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("()", result.Arguments);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingNoArgumentDescriptions_ReturnsEmptyValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +  
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Empty(result.ArgumentsDescriptions);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingArgumentDescriptions_ReturnsCorrectValues()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +  
                "<div class=\"param\"><table><tr><td>argument1</td><td>Arg 1 description</td></tr></table></div>" +
                "<div class=\"param\"><table><tr><td>argument2</td><td>Arg 2 <a>description</a></td></tr></table></div>" +
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("Arg 1 description", result.ArgumentsDescriptions["argument1"]);
            Assert.Equal("Arg 2 description", result.ArgumentsDescriptions["argument2"]);
        }
        
        [Fact] 
        public void Test_GetPageData_WithPageHavingArguments_ReturnsCorrectValue()
        {
            var html = 
                "<html><head></head>" +
                "<body class=\"ns-0 ltr\"><div id=\"globalWrapper\"><div id=\"column-content\"><div id=\"content\">" +
                "<a name=\"top\" id=\"top\"></a>" +
                "<h1 class=\"firstHeading\">PageTitle</h1>" +
                "<div class=\"scripting\"></div>" +  
                "<div class=\"parameters\">AnyParameters</div>" +
                "</div></div></div>" +
                "</body>";
            var subject = Subject(MockHttpClient(html));

            var result = subject.GetPageData("PageTitle");
            
            Assert.Equal("AnyParameters", result.Arguments);
        }
        
        private WikiService Subject(Mock<IHttpClient> httpMock) =>
            new WikiService(httpMock.Object);

        private Mock<IHttpClient> MockHttpClient(string content)
        {
            var subject = new Mock<IHttpClient>();
            subject.Setup(s => s.GetContent(It.IsAny<string>())).Returns(content);
            return subject;
        }
    }
}
