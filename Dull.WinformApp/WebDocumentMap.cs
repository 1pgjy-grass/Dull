using CsvHelper.Configuration;

namespace Dull.WinformApp
{
    class WebDocumentMap : ClassMap<WebDocument>
    {
        public WebDocumentMap()
        {
            Map(m => m.Id).Name("ID");
            Map(m => m.Catagory).Name("分类");
            Map(m => m.Url).Name("文档URL");
            Map(m => m.Title).Name("文档标题");
        }
    }
}
