using System.Drawing;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CefSharp;
using CefSharp.OffScreen;

namespace OzonParser {
    
    internal static class Program {
        
        private static async Task Main() {
            await Cef.InitializeAsync(new CefSettings() {
                LogSeverity = LogSeverity.Disable
            });

            string address = "https://www.ozon.ru/product/1195594338";
            
            using var browser = new ChromiumWebBrowser(address);
            var initResult = await browser.WaitForInitialLoadAsync();
            
            if (!initResult.Success) {
                Console.WriteLine("Не удалось загрузить сайт!");
                return;
            }
            
            await Task.Delay(500);

            
            var doc = await GetHtmlSource(await browser.GetSourceAsync());
            var items = doc.QuerySelectorAll(".i8t.ti8");

            foreach (var item in items) {
                Console.WriteLine(item.Html());
            }
            
            Console.WriteLine("Done!");
            Console.WriteLine("Нажмите любую клавишу, чтобы закрыть консоль");
            Console.ReadKey();
            
            Cef.Shutdown();
            browser.Dispose();
        }

        private static async Task<IHtmlDocument> GetHtmlSource(string html) {
            var parser = new HtmlParser();
            return await parser.ParseDocumentAsync(html);
            // var config = Configuration.Default;
            // var context = BrowsingContext.New(config);
            // return await context.OpenAsync(req => req.Content(html));
        }
        
    }
    
}