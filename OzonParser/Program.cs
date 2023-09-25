using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CefSharp;
using CefSharp.OffScreen;

namespace OzonParser {
    
    internal static class Program {
        
        private static async Task Main() {
            await Cef.InitializeAsync(new CefSettings()
            {
                LogSeverity = LogSeverity.Disable
            });

            string address = "https://www.ozon.ru/category/avtomobilnye-kompressory-8577/?category_was_predicted=true&deny_category_prediction=true&from_global=true&text=%D0%BD%D0%B0%D1%81%D0%BE%D1%81+%D0%B0%D0%B2%D1%82%D0%BE%D0%BC%D0%BE%D0%B1%D0%B8%D0%BB%D1%8C%D0%BD%D1%8B%D0%B9";

            using (var browser = new ChromiumWebBrowser(address)) {
                var initResult = await browser.WaitForInitialLoadAsync();
            
                if (!initResult.Success) {
                    Console.WriteLine("Не удалось загрузить сайт!");
                    return;
                }
            
                await Task.Delay(500);

            
                var doc = await GetHtmlSource(await browser.GetSourceAsync());
                var item = doc.QuerySelector(".widget-search-result-container")!;
                var a = item.FirstElementChild;

                //foreach ( var child in a )
                //{
                //    Console.WriteLine(child.InnerHtml + "\n");
                //}

                string className = a.FirstElementChild!.ClassName!;
                className = className.Replace(" ", ".");

                var products = doc.QuerySelectorAll($".{className}");

                foreach ( var product in products )
                {
                    var refHtml = product.FirstElementChild!.FirstElementChild!;
                    await Console.Out.WriteLineAsync(refHtml.GetAttribute("href"));
                    await Console.Out.WriteLineAsync("\n");
                }
                Console.WriteLine("\n\n\n");

                browser.Dispose();
            }
            
            Console.WriteLine("Done!");
            Console.WriteLine("Нажмите любую клавишу, чтобы закрыть консоль");

            Console.ReadKey();
        }

        private static async Task<IHtmlDocument> GetHtmlSource(string html) {
            var parser = new HtmlParser();
            return await parser.ParseDocumentAsync(html);
        }
        
    }
    
}