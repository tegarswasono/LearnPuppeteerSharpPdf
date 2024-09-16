using LearnPuppeteerSharpPDF.Models;
using PuppeteerSharp;
using System.Text;

namespace LearnPuppeteerSharpPDF.Helpers
{
	public static class PdfHelperV2
	{
		private static Browser? browser;
		private static readonly BrowserFetcher fetcher = new();
		private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

		public static async Task<Browser> GetBrowserAsync()
		{
			if (browser == null)
			{
				await semaphoreSlim.WaitAsync();
				try
				{
					if (browser == null)
					{
						await fetcher.DownloadAsync();
						browser = (Browser)await Puppeteer.LaunchAsync(new LaunchOptions
						{
							Headless = true,
							Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
						});
					}
				}
				finally
				{
					semaphoreSlim.Release();
				}
			}
			return browser;
		}

		private static async Task<byte[]> GeneratePdfAsync(string content, PdfOptions pdfOptions)
		{
			var browser1 = await GetBrowserAsync();
			await using var page = await browser1.NewPageAsync();
			await page.SetContentAsync(content);

			var pdfStream = await page.PdfStreamAsync(pdfOptions);
			var result = ToByteArray(pdfStream);
			pdfStream.Dispose();

			return result;
		}

		public static async Task<byte[]> GetProduct()
		{
			var content = await System.IO.File.ReadAllTextAsync(Path.Combine("Templates", "Product.html"));
			var pdfOption = new PdfOptions()
			{
				Format = PuppeteerSharp.Media.PaperFormat.A4
			};
			content = ReplaceProductWithData(content);
			return await GeneratePdfAsync(content, pdfOption);
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
