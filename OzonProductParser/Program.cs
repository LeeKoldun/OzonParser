using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CefSharp;
using ParserLib;
using ParserLib.Models;
using System.Text.RegularExpressions;

namespace OzonProductParser
{
    internal static class Program {

        static void Main() {
            string[] refs;
            using (var sr = new StreamReader("ProductRefs.csv")) { refs = sr.ReadToEnd().Split(','); }
            refs.ToList().RemoveRange(0, 13);

            Parser.Init("www.google.com").Wait();
            MainAsync(refs).Wait();
            Parser.Shutdown();
        }

        static async Task MainAsync(string[] refs) {
            var browser = Parser.Browser;
            List<ProdModel> prods = new();

            for(int i = 0; i < refs.Length; i++) { 
                await browser.LoadUrlAsync(refs[i]);
                await CheckLoad();

                Console.WriteLine("Loading product...");
                await Task.Delay(5000);

                var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());

                ProdModel prod = new();
                SellerModel seller = prod.Seller;

                // Product //

                // Url
                prod.Url = refs[i];
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
                    await Task.Delay(2000);

                    doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
                    string[] shopInfo = doc.QuerySelector(".tsBody600Medium")!.InnerHtml.Split("<br>");

                    // Ogrn
                    seller.Ogrn = int.TryParse( shopInfo.Last(), out int _ ) ? shopInfo.Last() : string.Empty;

                    // FullName?
                    seller.FullName = shopInfo.First();
                }

                await Console.Out.WriteLineAsync($"\nLoaded {i + 1}/{refs.Length} products\n");

                prods.Add(prod);
            }


            //for(int i = 0; i < refs.Length; i++) {
            //    browser.Load(refs[i]);

            //    await Console.Out.WriteLineAsync("Loading product...");
            //    await Task.Delay(2000);

            //    var bytes = await browser.CaptureScreenshotAsync();
            //    if(!Directory.Exists($"{Environment.CurrentDirectory}/images")) {
            //        Directory.CreateDirectory($"{Environment.CurrentDirectory}/images");
            //    }

            //    File.WriteAllBytes($"{Environment.CurrentDirectory}/images/prod{i}.png", bytes);

            //    await Console.Out.WriteLineAsync($"Saved prod {i + 1} / {refs.Length}");
            //}

            ExcelParser.ConvertProdToExcel(prods);

            await Console.Out.WriteLineAsync("Done!");
            await Console.Out.WriteLineAsync("PRESS ANY BUTTON TO CLOSE CONSOLE");

            Console.ReadKey();
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