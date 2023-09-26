using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CefSharp;
using ParserLib;
using ParserLib.Models;
using System.Text.RegularExpressions;

namespace OzonProductParser
{
    internal static class Program {
        static bool done = false;

        static void Main() {
            string[] refs;
            using (var sr = new StreamReader("ProductRefs.csv")) { refs = sr.ReadToEnd().Split(','); }

            ExcelParser.InitSheet();

            Task.Run(() => MonitorKeypress());
            Parser.Init("www.google.com").Wait();
            try {
                for (int i = 0; i < refs.Length; i++) {
                    if (done) break;
                    MainAsync(refs[i], i, refs.Length).Wait();
                }
            }
            finally { 
                ExcelParser.SaveSheet();
                Console.WriteLine("Saved Excel");
            }
            Parser.Shutdown();

            Console.WriteLine("Done!");
            Console.WriteLine("PRESS ANY BUTTON TO CLOSE CONSOLE");

            Console.ReadKey();
        }

        public static void MonitorKeypress() {
            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                // Wait for an ESC
            } while (cki.Key != ConsoleKey.Escape);

            // Cancel the token
            done = true;
            Console.WriteLine("\nCanceling operation...\n");
        }

        static async Task MainAsync(string refProd, int i, int length) {
            var browser = Parser.Browser;

            await browser.LoadUrlAsync(refProd);
            await CheckLoad();

            Console.WriteLine("Loading product...");
            await Task.Delay(3000);

            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());

            ProdModel prod = new();
            SellerModel seller = prod.Seller;

            // Product //

            // Url
            prod.Url = refProd.Split("?").First();
            await Console.Out.WriteLineAsync(prod.Url);

            // Title
            prod.Title = doc.QuerySelector(".lq2")!.TextContent;

            // Description
            prod.Description = GetDescription(doc);

            // Params
            prod.ProdParams = GetParams(doc);

            // Price
            prod.Price = (doc.QuerySelector(".pl8.lp9.l9p.p7l") == null ? doc.QuerySelector(".p8l")! : doc.QuerySelector(".pl8.lp9.l9p.p7l"))!.TextContent; ;

            // Rating
            string rating = doc.QuerySelector(".x3r") != null ? doc.QuerySelector(".x3r")!.TextContent.Split(" ").First() : string.Empty;
            rating = rating == "Нет" ? string.Empty : rating;
            prod.Rating = rating;

            // RatingCount
            string ratingCount = doc.QuerySelector(".e8144-a9.e8144-b0") != null ? doc.QuerySelector(".e8144-a9.e8144-b0")!.TextContent.Replace("\n", "").Trim() : string.Empty;
            ratingCount = ratingCount == "0" ? string.Empty : ratingCount;
            prod.RatingCount = ratingCount;

            // ImgUrl
            prod.ImgUrl = doc.QuerySelector(".jr8") != null ? doc.QuerySelector(".jr8")!.FirstElementChild!.GetAttribute("src")! : doc.QuerySelector(".j1r")!.GetAttribute("src")!;

            // Seller //

            // Name
            seller.Name = doc.QuerySelector(".u5j")!.TextContent;

            // Url
            var shopUrlBody = (doc.QuerySelectorAll(".u5j") != null ? doc.QuerySelectorAll(".u5j").Last() : doc.QuerySelector(".j5u"))!;
            if(shopUrlBody.TextContent != "OZON Россия") {
                string shopUrl = shopUrlBody.GetAttribute("href")!;
                shopUrl = shopUrl[0] == '/' ? Parser.BaseUrl + shopUrl : shopUrl;
                seller.Url = shopUrl;
                
                // Подбираемся к огрн :) //
                await browser.LoadUrlAsync(seller.Url);
                await CheckLoad();

                await Task.Delay(1000);
                browser.ExecuteScriptAsync("document.querySelector('.uh7').lastChild.querySelector('.v4h').click()");
                await Task.Delay(1000);

                doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
                string[] shopInfo = doc.QuerySelector(".tsBody600Medium")!.InnerHtml.Split("<br>");

                // Ogrn
                seller.Ogrn = ulong.TryParse( shopInfo.Last(), out _ ) ? shopInfo.Last() : string.Empty;

                // FullName?
                seller.FullName = shopInfo.First();
            }

            await Console.Out.WriteLineAsync($"\nLoaded {i + 1}/{length} products\n");

            ExcelParser.ConvertProdToExcel(prod);

            await Console.Out.WriteLineAsync("Wrote product to excel...\n");
        }

        private static async Task CheckLoad() { if(!await Parser.WaitLoad()) throw new Exception("Couldn't load url!"); }

        private static string GetDescription(IHtmlDocument doc) {
            Regex reg = new Regex(@"&(\w*);");
            bool isFirstTypeStruct = doc.QuerySelector(".ra-a1") != null;

            string desc = "";
            if (isFirstTypeStruct) desc = doc.QuerySelector(".ra-a1")!.TextContent;
            else
            {
                var spans = doc.QuerySelectorAll(".ra-a1");
                foreach (var sp in spans)
                {
                    desc += sp.TextContent;
                }
            }

            desc = reg.Replace(desc, "");
            return desc;
        }

        static string GetParams(IHtmlDocument doc) {
            string prodParams = "";
            var parametrs = doc.QuerySelectorAll(".x9j");

            foreach(var par in parametrs) {
                string name = par.Children[0].TextContent;
                string value = par.Children[1].TextContent;
                prodParams += $"{name}&&{value};";
            }

            return prodParams;
        }
    }
}