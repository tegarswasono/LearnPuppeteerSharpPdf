using LearnPuppeteerSharpPDF.Helpers;
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
            await PdfHelper.Example();
            return Ok();
        }

        [HttpGet]
        [Route("generate-from-html")]
        public async Task<IActionResult> GenerateFromHtml()
        {
            await PdfHelper.GenerateProduct();
            return Ok();
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> Download() 
        {
            var result1 = await PdfHelper.GetProduct();
            string filename = $"product-report-{DateTime.Now.ToString("yyMMddHHmmss")}.pdf";
            return File(result1, MediaTypeNames.Application.Pdf, filename);
        }
    }
}
