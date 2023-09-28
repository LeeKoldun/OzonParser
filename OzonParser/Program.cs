using CefSharp;
using ParserLib;

namespace OzonProdRefsParser {
    
    internal static class Program {
        static string? address = "";
        static int pageCount = 1;
        const int maxPage = 100;
        static bool done = false;

        static void Main() {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif
            Console.Write("Enter ozon search url: ");

            while(string.IsNullOrEmpty(address)) {
                address = Console.ReadLine();
            }

            if(File.Exists("ProductRefs.csv")) File.Delete("ProductRefs.csv");

            Parser.Init(address).Wait();
            try {
                while(!done) {
                    MainAsync().Wait();
                }
            }
            catch (Exception e) {
                Console.WriteLine("\n\nERROR\n\n");
                Console.WriteLine(e.Message);
                Console.WriteLine();
            }

            Console.WriteLine("PRESS ANY BUTTON TO CLOSE CONSOLE");
            Console.ReadKey();

            Parser.Shutdown();
        }

        static async Task MainAsync() {
            var browser = Parser.Browser;

            Console.WriteLine("Loading catalogue...");
            await Task.Delay(5000);

            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
            var item = doc.QuerySelector(".widget-search-result-container")!;
            // Класс, в котором лежат товары
            var itemsWrapper = item.FirstElementChild!;

            string className = itemsWrapper.FirstElementChild!.ClassName!.Replace(" ", ".").Insert(0, ".");
            Console.WriteLine($"Got class name: {className}");

            var products = doc.QuerySelectorAll(className);

            // Товары могут быть 2-х структур: с ссылкой внутри класса или с ссылкой внутри дочернего класса (div)
            bool isFirstTypeStruct = !products.First().FirstElementChild!.HasAttribute("href");
            await Console.Out.WriteLineAsync($"Is first struct type: {isFirstTypeStruct}\n\n");

            List<string> prodList = new();
            foreach ( var product in products ) {
                string refHtml = (isFirstTypeStruct ? product.FirstElementChild!.FirstElementChild : product.FirstElementChild)!.GetAttribute("href")!.Split("?").First();
                await Console.Out.WriteLineAsync(refHtml);
                await Console.Out.WriteLineAsync("\n");

                prodList.Add(Parser.BaseUrl + refHtml);
            }
            Console.WriteLine("\n\n");
            await Console.Out.WriteLineAsync($"Got {products.Length} items on page {pageCount} / {maxPage} max");

            using(var sw = new StreamWriter("ProductRefs.csv", true)) { sw.Write((pageCount == 1 ? "" : ",") + string.Join(",", prodList)); }
            await Console.Out.WriteLineAsync("Wrote refs to csv file...");

            pageCount++;
            await browser.LoadUrlAsync(address + $"&page={pageCount}");

            if (pageCount > maxPage) {
                await Console.Out.WriteLineAsync("\nDONE\n");
                done = true;
                return;
            }
        }
        
    }
    
}