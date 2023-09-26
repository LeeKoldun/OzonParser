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

            Parser.Init("www.google.com").Wait();
            MainAsync(refs).Wait();
            Parser.Shutdown();
        }

        static async Task MainAsync(string[] refs) {
            var browser = Parser.Browser;
            int i = 0;

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

            // Title
            prod.Title = doc.QuerySelector(".lq2")!.TextContent;

            // Description
            prod.Description = GetDescription(doc);

            // Params
            prod.ProdParams = GetParams(doc);

            // Price
            prod.Price = (doc.QuerySelector(".pl8.lp9.l9p.p7l") == null ? doc.QuerySelector(".p8l")! : doc.QuerySelector(".pl8.lp9.l9p.p7l"))!.TextContent; ;

            // Rating
            prod.Rating = doc.QuerySelector(".x3r")!.TextContent.Split(" ").First();

            // RatingCount
            prod.RatingCount = doc.QuerySelector(".e8144-a9.e8144-b0")!.TextContent.Replace("\n", "").Trim();

            // ImgUrl
            prod.ImgUrl = doc.QuerySelector(".jr8")!.FirstElementChild!.GetAttribute("src")!;

            // Seller //

            // Name
            seller.Name = doc.QuerySelector(".u5j")!.TextContent;

            // Url
            seller.Url = doc.QuerySelector(".u5j")!.GetAttribute("href")!;

            // Подбираемся к огрн :) //
            await browser.LoadUrlAsync(seller.Url);
            await CheckLoad();

            await Task.Delay(2000);
            browser.ExecuteScriptAsync("document.querySelector('.uh7').lastChild.querySelector('.v4h').click()");
            await Task.Delay(1000);

            doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
            string[] shopInfo = doc.QuerySelector(".tsBody600Medium")!.InnerHtml.Split("<br>");

            // Ogrn
            seller.Ogrn = shopInfo.Length > 2 ? shopInfo[2] : string.Empty;

            // FullName?
            seller.FullName = shopInfo.First();


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

            ExcelParser.ConvertProdToExcel(new ProdModel[] { prod });

            await Console.Out.WriteLineAsync("Done!");
            await Console.Out.WriteLineAsync("PRESS ANY BUTTON TO CLOSE CONSOLE");

            Console.ReadKey();
        }

        private static async Task CheckLoad() { if(!await Parser.WaitLoad()) throw new Exception("Couldn't load url!"); }

        private static string GetDescription(IHtmlDocument doc) {
            Regex reg = new Regex(@"&(\w*);");
            string desc = doc.QuerySelector(".ra-a1")!
                .InnerHtml.Replace("<br>", "\n");
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