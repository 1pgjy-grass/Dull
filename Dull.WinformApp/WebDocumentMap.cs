using CsvHelper.Configuration;

namespace Dull.WinformApp
{
    class WebDocumentMap : ClassMap<WebDocument>
    {
        public WebDocumentMap()
        {
            Map(m => m.Id).Name("ID");
            Map(m => m.Catagory).Name("CATAGORY");
            Map(m => m.Title).Name("TITLE");
            Map(m => m.Url).Name("URL");
        }
    }
}
