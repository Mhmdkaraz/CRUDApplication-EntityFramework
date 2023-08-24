using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers {
    [Route("[controller]")]
    public class CountriesController : Controller {
        private readonly ICountriesUploaderService _countriesUplaoderService;

        public CountriesController(ICountriesUploaderService countriesUplaoderService) {
            _countriesUplaoderService = countriesUplaoderService;
        }
        [Route("[action]")]
        [HttpGet]
        public IActionResult UploadFromExcel() {
            return View();
        }
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile) {
            if (excelFile == null || excelFile.Length == 0) {
                ViewBag.ErrorMessage = "please select an xlsx file";
                return View();
            }
            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) {
                ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
                return View();
            }
            int countriesCountInserted = await _countriesUplaoderService.UploadCountriesFromExcelFile(excelFile);
            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";
            return View();

        }
    }
}
