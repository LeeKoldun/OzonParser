using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CefSharp;
using CefSharp.OffScreen;
using ParserLib;
using ParserLib.Models;
using System.Text.RegularExpressions;

namespace OzonProductParser
{
    internal static class Program {
        static bool done = false;

        static void Main() {
#if ANYCPU
            //Only required for PlatformTarget of AnyCPU
            CefRuntime.SubscribeAnyCpuAssemblyResolver();
#endif
            string[] refs;
            using (var sr = new StreamReader("ProductRefs.csv")) { refs = sr.ReadToEnd().Split(','); }

            ExcelParser.InitSheet();

            Task.Run(() => MonitorKeypress());
            Parser.Init("www.google.com").Wait();
            try {
                for (int i = 0; i < refs.Length; i++) {
                    if (done) break;
                    try {
                        MainAsync(refs[i], i, refs.Length).Wait();
                    }
                    catch (Exception e) {
                        Console.WriteLine("\n\nParse error\n\n");
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                    }
                }
            }
            catch(Exception e) {
                Console.WriteLine("\n\nERROR!\n");
                Console.WriteLine(e.Message);
                Console.WriteLine();
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
            await Task.Delay(5000);

            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());

            ProdModel prod = new();
            SellerModel seller = prod.Seller;


            // Product //

            // Url
            prod.Url = refProd.Split("?").First();
            await Console.Out.WriteLineAsync(prod.Url);

            // Title
            prod.Title = Parser.GetWidgetByName(doc, "webProductHeading")!.TextContent;

            // Description
            prod.Description = GetDescription(doc);

            // Params
            prod.ProdParams = GetParams(doc);

            // Price
            prod.Price = GetPrice(doc);

            // Rating
            prod.Rating = GetRating(doc);

            // RatingCount
            prod.RatingCount = GetRatingCount(doc);

            // ImgUrl
            prod.ImgUrl = GetImgUrl(doc);


            // Seller //

            // Name
            var sellerNode = GetSellerNode(doc);
            seller.Name = sellerNode.TextContent;

            // Url
            if(seller.Name != "OZON Россия") await GetSellerInfo(browser, seller, sellerNode);

            await Console.Out.WriteLineAsync($"\nLoaded {i + 1}/{length} products\n");

            ExcelParser.ConvertProdToExcel(prod);

            await Console.Out.WriteLineAsync("Wrote product to excel...\n");
        }

        private static async Task GetSellerInfo(ChromiumWebBrowser browser, SellerModel seller, IElement sellerNode) {
            string shopUrl = sellerNode.GetAttribute("href")!;
            shopUrl = shopUrl[0] == '/' ? Parser.BaseUrl + shopUrl : shopUrl;
            seller.Url = shopUrl;

            // Подбираемся к огрн :) //
            await browser.LoadUrlAsync(seller.Url);
            await CheckLoad();

            await Task.Delay(2000);
            
            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
            var sellerBarName = Parser.GetWidgetByName(doc, "sellerTransparency")!.Children.Last().ClassName;

            browser.ExecuteScriptAsync($"document.querySelector('.{sellerBarName}').lastChild.firstChild.click()");
            await Task.Delay(1000);

            doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());

            var textBlock = Parser.GetWidgetByName(doc, "textBlock")!;
            var info = textBlock
                .FirstElementChild!
                .FirstElementChild!
                .Children[1]!
                .FirstElementChild!;

            string[] shopInfo = info.InnerHtml.Split("<br>");

            // Ogrn
            seller.Ogrn = ulong.TryParse(shopInfo.Last(), out _) ? shopInfo.Last() : string.Empty;

            // FullName?
            seller.FullName = shopInfo.First();
        }

        private static IElement GetSellerNode(IHtmlDocument doc) {
            var webSeller = Parser.GetWidgetByName(doc, "webCurrentSeller");
            var seller = webSeller
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .LastElementChild!
                .LastElementChild!
                .QuerySelector("[href]")!;

            return seller;
        }

        private static string GetImgUrl(IHtmlDocument doc) {
            //var gallery = Parser.GetWidgetByName(doc, "webGallery")!
            //    .FirstElementChild!
            //    .LastElementChild!
            //    .FirstElementChild!
            //    .FirstElementChild!
            //    .Children[1];
            var imgContainer = Parser.GetWidgetByName(doc, "webGallery")!
                .FirstElementChild!
                .LastElementChild!
                .FirstElementChild!
                .FirstElementChild!;

            if(imgContainer.Children.Length > 1) {
                var gallery = imgContainer.Children[1];
                var imgs = gallery.QuerySelectorAll("[data-index] [src]");
                List<string> imgRefs = new();

                foreach(var img in imgs) imgRefs.Add(img.GetAttribute("src")!);
                foreach(var srcRef in imgRefs) {
                    if(srcRef.Contains("video")) continue;

                    return srcRef;
                }
            }
            else {
                return imgContainer.FirstElementChild!.GetAttribute("src")!;
            }

            return string.Empty;
        }

        private static string GetRatingCount(IHtmlDocument doc) {
            var reviewTab = Parser.GetWidgetByName(doc, "webReviewTabs")!;
            var ratingCount = reviewTab
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .LastElementChild!
                .TextContent
                .Trim();

            return ratingCount == "0" ? string.Empty : ratingCount;
        }

        private static string GetRating(IHtmlDocument doc) {
            var reviewTab = Parser.GetWidgetByName(doc, "webReviewTabs")!;
            var row = reviewTab.Children[1].FirstElementChild!;
            var column = row.Children.Last();
            var rating = column
                .Children[1]
                .FirstElementChild!
                .FirstElementChild!
                .LastElementChild!
                .TextContent;
            rating = rating.Split(" ").First();

            return rating == "Нет" ? string.Empty : rating;
        }

        private static string GetPrice(IHtmlDocument doc) {
            return Parser.GetWidgetByName(doc, "webPrice")!
                .FirstElementChild!
                .LastElementChild!
                .FirstElementChild!
                .FirstElementChild!
                .LastElementChild!
                .TextContent;
        }

        private static async Task CheckLoad() { if(!await Parser.WaitLoad()) throw new Exception("Couldn't load url!"); }

        private static string GetDescription(IHtmlDocument doc) {
            //bool isFirstTypeStruct = doc.QuerySelector(".ra-a1") != null;

            //if (isFirstTypeStruct) desc = doc.QuerySelector(".ra-a1")!.TextContent;
            //else
            //{
            //    var spans = doc.QuerySelectorAll(".ra-a1");
            //    foreach (var sp in spans)
            //    {
            //        desc += sp.TextContent;
            //    }
            //}

            Regex reg = new Regex(@"&(\w*);");
            var descSection = Parser.GetWidgetByIdName(doc, "section-description");
            if(descSection == null) return string.Empty;
            var desc = descSection
                .LastElementChild!
                .TextContent;

            desc = reg.Replace(desc, "").Trim().Replace("Показать полностью", "");
            return desc;
        }

        static string GetParams(IHtmlDocument doc) {
            string prodParams = "";
            string? paramClassName;

            var sections = Parser.GetWidgetByIdName(doc, "section-characteristics")!.Children[1].Children.ToList();
            bool multiChar = sections.Count > 1;
            foreach (var el in sections) {
                var columns = el.Children.ToList();
                if(multiChar) { 
                    columns.RemoveAt(0);
                }

                int max = columns.Count;
                for(int i = 0; i < max; i++) {
                    var rows = columns[i];
                    string name, value;

                    if(i == max - 1 && !multiChar && rows.GetAttribute("style") == null) break;

                    foreach(var row in rows.Children) {
                        name = row.Children[0].TextContent;
                        value = row.Children[1].TextContent;

                        prodParams += $"{name}&&{value};";
                    }
                }
            }

            return prodParams;
        }
    }
}