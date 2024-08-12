using LearnPuppeteerSharpPDF.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using System.Net.Mime;
using System.Text;

namespace LearnPuppeteerSharpPDF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfHelperController : ControllerBase
    {
        [HttpGet]
        [Route("generate")]
        public async Task<IActionResult> Generate()
        {
            string outputFile = "D:\\test.pdf";

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            
            await page.GoToAsync("http://www.google.com"); // In case of fonts being loaded from a CDN, use WaitUntilNavigation.Networkidle0 as a second param.
            await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.
            await page.PdfAsync(outputFile);
            return Ok();
        }

        [HttpGet]
        [Route("generate-from-html")]
        public async Task<IActionResult> GenerateFromHtml()
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
            content = ReplaceWithData(content);
            string outputFile = "D:\\test.pdf";

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(content);
            await page.PdfAsync(outputFile);

            return Ok();
        }

        private static string ReplaceWithData(string content)
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

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> Download() 
        {
            var content = await System.IO.File.ReadAllTextAsync(Path.Combine("Templates", "Product.html"));
            var pdfOption = new PdfOptions()
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4
            };
            content = ReplaceWithData(content);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(content);
            var result = await page.PdfStreamAsync(pdfOption);
            var result1 = ToByteArray(result);
            result.Dispose();

            string filename = $"product-report-{DateTime.Now.ToString("yyMMddHHmmss")}.pdf";
            return File(result1, MediaTypeNames.Application.Pdf, filename);
        }

        public static byte[] ToByteArray(Stream input)
        {
            // Create a MemoryStream
            using (MemoryStream ms = new MemoryStream())
            {
                // Copy the input stream to the memory stream
                input.CopyTo(ms);

                // Return the array of bytes
                return ms.ToArray();
            }
        }
    }
}
