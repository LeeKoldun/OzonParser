using CefSharp;
using ParserLib;

namespace OzonProdRefsParser {
    
    internal static class Program {
        static string? address = "";
        static string? fileName = "";
        static int awaitTime;

        static int pageCount = 1;
        const int maxPage = 100;
        static bool done = false;

        static void Main() {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif

            do {
                Console.Write("Enter ozon search url: ");
                address = Console.ReadLine();
                if(!string.IsNullOrEmpty(address)) break;
            } while(true);

            do {
                Console.Write("Enter save file name: ");
                fileName = Console.ReadLine();
                if(!string.IsNullOrEmpty(fileName)) break;
            } while(true);


            do {
                Console.Write("Enter load await time in seconds: ");
                bool success = int.TryParse(Console.ReadLine(), out int n);
                if(!success) continue;

                awaitTime = n * 1000;
                Console.WriteLine($"Await time: {n} seconds");
                break;
            }
            while(true);

            if(File.Exists($"{fileName}.csv")) File.Delete($"{fileName}.csv");
            if(!Directory.Exists("Refs_Result")) Directory.CreateDirectory("Refs_Result");

            Task.Run(() => MonitorKeypress());
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

        public static void MonitorKeypress() {
            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                // Wait for an ESC
            } while(cki.Key != ConsoleKey.Escape);

            // Cancel the token
            done = true;
            Console.WriteLine("\nCanceling operation...\n");
        }

        static async Task MainAsync() {
            var browser = Parser.Browser;

            Console.WriteLine("YOU CAN PRESS 'Escape' TO STOP REFS LOAD");
            Console.WriteLine("Loading catalogue...");
            await Task.Delay(awaitTime);

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

            using(var sw = new StreamWriter($"Refs_Result/{fileName}.csv", true)) { sw.Write((pageCount == 1 ? "" : ",") + string.Join(",", prodList)); }
            await Console.Out.WriteLineAsync("Wrote refs to csv file...");

            pageCount++;
            var btn = Parser.GetWidgetByName(doc, "megaPaginator");
            if(btn == null) {
                await Console.Out.WriteLineAsync("\nDONE\n");
                done = true;
                return;
            }

            string nextRef = btn
                .LastElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .LastElementChild!
                .FirstElementChild!
                .GetAttribute("href")!;

            await browser.LoadUrlAsync(Parser.BaseUrl + nextRef);

            //await browser.LoadUrlAsync(address + $"&page={pageCount}");

            //if (pageCount > maxPage) {
            //    await Console.Out.WriteLineAsync("\nDONE\n");
            //    done = true;
            //    return;
            //}
        }
        
    }
    
}