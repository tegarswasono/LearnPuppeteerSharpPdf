using LearnPuppeteerSharpPDF.Models;
using PuppeteerSharp;
using System.Text;

namespace LearnPuppeteerSharpPDF.Helpers
{
    public static class PdfHelper
    {
        public static async Task Example()
        {
            string outputFile = "D:\\test.pdf";

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();

            await page.GoToAsync("http://www.google.com"); // In case of fonts being loaded from a CDN, use WaitUntilNavigation.Networkidle0 as a second param.
            await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.
            await page.PdfAsync(outputFile);
        }

        public static async Task GenerateProduct()
        {
            var content = await System.IO.File.ReadAllTextAsync(Path.Combine("Templates", "Product.html"));
            var pdfOption = new PdfOptions()
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4
                //Format = PuppeteerSharp.Media.PaperFormat(3.3m, 12.5m),
                //MarginOptions = new PuppeteerSharp.Media.MarginOptions()
                //{
                //    Top = "5"  
                //}
            };
            content = ReplaceProductWithData(content);
            string outputFile = "D:\\test.pdf";

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(content);
            await page.PdfAsync(outputFile);
        }

        public static async Task<byte[]> GetProduct()
        {
            var content = await System.IO.File.ReadAllTextAsync(Path.Combine("Templates", "Product.html"));
            var pdfOption = new PdfOptions()
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4
            };
            content = ReplaceProductWithData(content);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(content);
            var result = await page.PdfStreamAsync(pdfOption);
            var result1 = ToByteArray(result);
            result.Dispose();

            return result1;
        }

        private static string ReplaceProductWithData(string content)
        {
            var products = new List<ProductModel>()
            {
                new() { Name = "John Tshirt", Description = "desc 1", Price = 20, Stok = 100 },
                new() { Name = "Wick Tshirt", Description = "desc 2", Price = 10, Stok = 200 },
                new() { Name = "Elizabet Tshirt", Description = "desc 3", Price = 40, Stok = 300 },
            };
            var content1 = new StringBuilder();
            int no = 1;
            foreach (var product in products)
            {
                content1.Append("<tr>");
                content1.Append("<td>" + (no++) + "</td>");
                content1.Append("<td>" + product.Name + "</td>");
                content1.Append("<td>" + product.Description + "</td>");
                content1.Append("<td>" + product.Price + "</td>");
                content1.Append("<td>" + product.Stok + "</td>");
                content1.Append("</tr>");
            }
            content = content.Replace("{{content}}", content1.ToString());
            return content;
        }

        public static byte[] ToByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
