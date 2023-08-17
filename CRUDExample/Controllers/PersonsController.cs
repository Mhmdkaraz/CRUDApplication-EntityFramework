using CRUDExample.Filters.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers {
    [Route("[controller]")]
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {
            "MyKey-FromController","MyValue-FromController" },Order = 2)]
    public class PersonsController : Controller {
        //private fields
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger) {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }
        [Route("[action]")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter),Arguments = new object[] {
            "MyKey-FromAction","MyValue-FromAction" },Order = 1)]
        public async Task<IActionResult> Index(string searchBy, string searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC) {
            _logger.LogInformation("Index action method of PersonsController");
            _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");
            //searching
            //ViewBag.SearchFields = new Dictionary<string, string>() {
            //    {nameof(PersonResponse.PersonName),"Person Name" },
            //    {nameof(PersonResponse.Email),"Email" },
            //    {nameof(PersonResponse.DateOfBirth),"Date of Birth" },
            //    {nameof(PersonResponse.Gender),"Gender" },
            //    {nameof(PersonResponse.CountryId),"Country" },
            //    {nameof(PersonResponse.Address),"Address" }
            //};
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            //ViewBag.CurrentSearchBy = searchBy;
            //ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            //ViewBag.CurrentSortBy = sortBy;
            //ViewBag.CurrentSortOrder = sortOrder.ToString();
            return View(sortedPersons);
        }

        //Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
        [Route("[action]")]
        [HttpGet]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {
            "MyKey-FromAction","MyValue-FromAction" })]
        public async Task<IActionResult> Create() {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                Text = temp.CountryName, Value = temp.CountryId.ToString()
            });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest) {
            if (!ModelState.IsValid) {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                    Text = temp.CountryName, Value = temp.CountryId.ToString()
                });
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(personAddRequest);
            }
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid personId) {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personId);
            if (personResponse == null) {
                return RedirectToAction("Index", "Persons");
            }
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                Text = temp.CountryName, Value = temp.CountryId.ToString()
            });

            return View(personUpdateRequest);
        }

        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest) {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personUpdateRequest.PersonId);
            if (personResponse == null) {
                return RedirectToAction("Index", "Persons");
            }
            if (ModelState.IsValid) {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index", "Persons");
            } else {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                    Text = temp.CountryName, Value = temp.CountryId.ToString()
                });
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(personResponse.ToPersonUpdateRequest());
            }
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? personId) {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personId);
            if (personResponse == null) {
                return RedirectToAction("Index", "Persons");
            }
            return View(personResponse);
        }
        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest) {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personUpdateRequest.PersonId);
            if (personResponse == null)
                return RedirectToAction("Index", "Persons");

            await _personsService.DeletePerson(personUpdateRequest.PersonId);
            return RedirectToAction("Index", "Persons");
        }

        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF() {
            List<PersonResponse> persons = await _personsService.GetAllPersons();
            return new ViewAsPdf("PersonsPDF", persons, ViewData) {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() {
                    Top = 20, Bottom = 20, Left = 20, Right = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV() {
            MemoryStream memoryStream = await _personsService.GetPersonCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        //[Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel() {
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }

    }
}
