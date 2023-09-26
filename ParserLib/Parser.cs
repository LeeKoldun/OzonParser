using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserLib {
    static public class Parser {
        public static string BaseUrl { get; } = "https://www.ozon.ru";
        static public ChromiumWebBrowser Browser { get; set; }

        static public async Task<ChromiumWebBrowser> Init(string address) {
            await Console.Out.WriteLineAsync("Initializing Cef...");
            await Cef.InitializeAsync(new CefSettings() {
                LogSeverity = LogSeverity.Disable
            });
            await Console.Out.WriteLineAsync("Cef initialized");

            await Console.Out.WriteLineAsync("Initializing browser...");
            Browser = new ChromiumWebBrowser(address);
            await Console.Out.WriteLineAsync("browser initialized");


            var initResult = await Browser.WaitForInitialLoadAsync();

            if(!initResult.Success) {
                Console.WriteLine("Couldn't load site!");
                throw new Exception("Couldn't load site!");
            }

            return Browser;
        }

        public static async Task<bool> WaitLoad() {
            await Console.Out.WriteLineAsync("Browser is loading...");
            var result = await Browser.WaitForInitialLoadAsync();
            return result.Success;
        }

        static public void Shutdown() {
            Browser.Dispose();
            Cef.Shutdown();
        }

        public static async Task<IHtmlDocument> GetHtmlSource(string html) {
            var parser = new HtmlParser();
            return await parser.ParseDocumentAsync(html);
        }
    }
}
