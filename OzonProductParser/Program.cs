using CefSharp;
using ParserLib;
using System.Text.RegularExpressions;

namespace OzonProductParser {
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

            browser.Load(refs[0]);
            bool success = await Parser.WaitLoad();

            if(!success) throw new Exception("Couldn't load url!");

            Console.WriteLine("Loading product...");
            await Task.Delay(5000);

            var doc = await Parser.GetHtmlSource(await browser.GetSourceAsync());
            ProdModel prod = new();

            prod.Title = doc.QuerySelector(".lq2")!.TextContent;

            Regex reg = new Regex(@"&(\w*);");
            string desc = doc.QuerySelector(".ra-a1")!
                .InnerHtml.Replace("<br>", "\n");
            desc = reg.Replace(desc, "");
            prod.Description = desc;
            //prod.ProdParams = doc.QuerySelector("")!.TextContent;


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

            ExcelParser.ConvertProdToExcel(new ProdModel[] {prod});

            await Console.Out.WriteLineAsync("Done!");
            await Console.Out.WriteLineAsync("PRESS ANY BUTTON TO CLOSE CONSOLE");

            Console.ReadKey();
        }
    }
}