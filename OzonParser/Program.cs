using CefSharp;
using ParserLib;

namespace OzonProdRefsParser {
    
    internal static class Program {
        
        static void Main() {
            Console.Write("Enter ozon search url: ");

            string? address = "";
            while(string.IsNullOrEmpty(address)) {
                address = Console.ReadLine();
            }

            Parser.Init(address).Wait();
            MainAsync().Wait();
            Parser.Shutdown();
        }

        static async Task MainAsync() {
            var browser = Parser.Browser;

            Console.WriteLine("Loading site...");
            await Task.Delay(5000);

            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
            var item = doc.QuerySelector(".widget-search-result-container")!;
            // Класс, в котором лежат товары
            var itemsWrapper = item.FirstElementChild;

            string className = itemsWrapper.FirstElementChild!.ClassName!.Replace(" ", ".").Insert(0, ".");
            Console.WriteLine($"Got class name: {className}");

            var products = doc.QuerySelectorAll(className);

            // Товары могут быть 2-х структур: с ссылкой внутри класса или с ссылкой внутри дочернего класса (div)
            bool isFirstTypeStruct = !products.First().FirstElementChild!.HasAttribute("href");
            await Console.Out.WriteLineAsync($"Is first struct type: {isFirstTypeStruct}\n\n");

            List<string> prodList = new();
            foreach ( var product in products ) {
                string refHtml = (isFirstTypeStruct ? product.FirstElementChild!.FirstElementChild : product.FirstElementChild)!.GetAttribute("href")!;
                await Console.Out.WriteLineAsync(refHtml);
                await Console.Out.WriteLineAsync("\n");

                prodList.Add(Parser.BaseUrl + refHtml);
            }
            Console.WriteLine("\n\n");
            await Console.Out.WriteLineAsync($"Got {products.Length} items on current page");

            using(var sw = new StreamWriter("ProductRefs.csv")) { sw.WriteLine(string.Join(",", prodList)); }
            await Console.Out.WriteLineAsync("Wrote csv file of refs...");

            Console.WriteLine("Done!");
            Console.WriteLine("PRESS ANY BUTTON TO EXIT CONSOLE");

            Console.ReadKey();
        }
        
    }
    
}