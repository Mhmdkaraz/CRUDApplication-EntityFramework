using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilter;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers {
    [Route("[controller]")]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {
    //        "MyKey-FromController","MyValue-FromController" ,3}, Order = 3)]
    [ResponseHeaderFilterFactory("MyKey-FromController", "MyValue-FromController", 3)]
    [TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonsAlwaysRunResultFilter))]

    public class PersonsController : Controller {
        //private fields
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ICountriesGetterService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsGetterService personsGetterService,
                IPersonsAdderService personsAdderService,
                IPersonsUpdaterService personsUpdaterService,
                IPersonsSorterService personsSorterService,
                IPersonsDeleterService personsDeleterService,
                ICountriesGetterService countriesService,
                ILogger<PersonsController> logger) {
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsUpdaterService = personsUpdaterService;
            _personsSorterService = personsSorterService;
            _personsDeleterService = personsDeleterService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        [ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {"MyKey-FromAction", "MyValue-FromAction", 1}, Order = 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
        [ResponseHeaderFilterFactory("MyKey-FromAction", "MyValue-FromAction", 1)]
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
            List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);
            //ViewBag.CurrentSearchBy = searchBy;
            //ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);
            //ViewBag.CurrentSortBy = sortBy;
            //ViewBag.CurrentSortOrder = sortOrder.ToString();
            return View(sortedPersons);
        }

        //Executes when the user clicks on "Create Person" hyperlink (while opening the create view)
        [Route("[action]")]
        [HttpGet]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] {
        //    "MyKey-FromAction","MyValue-FromAction",4 })]
        [ResponseHeaderFilterFactory("MyKey-FromAction", "MyValue-FromAction", 4)]

        public async Task<IActionResult> Create() {
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                Text = temp.CountryName, Value = temp.CountryId.ToString()
            });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeautureDisabledResourceFilter),Arguments = new object[] {false})]
        public async Task<IActionResult> Create(PersonAddRequest personRequest) {
            //if (!ModelState.IsValid) {
            //    List<CountryResponse> countries = await _countriesService.GetAllCountries();
            //    ViewBag.Countries = countries.Select(temp => new SelectListItem() {
            //        Text = temp.CountryName, Value = temp.CountryId.ToString()
            //    });
            //    ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            //    return View(personRequest);
            //}
            PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);
            return RedirectToAction("Index", "Persons");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        //[TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid personId) {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonId(personId);
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
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]

        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest) {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonId(personRequest.PersonId);
            if (personResponse == null) {
                return RedirectToAction("Index", "Persons");
            }
            //if (ModelState.IsValid) {
                PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
                return RedirectToAction("Index", "Persons");
            //} 
            //else {
            //    List<CountryResponse> countries = await _countriesService.GetAllCountries();
            //    ViewBag.Countries = countries.Select(temp => new SelectListItem() {
            //        Text = temp.CountryName, Value = temp.CountryId.ToString()
            //    });
            //    ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            //    return View(personResponse.ToPersonUpdateRequest());
            //}
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? personId) {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonId(personId);
            if (personResponse == null) {
                return RedirectToAction("Index", "Persons");
            }
            return View(personResponse);
        }
        [Route("[action]/{personId}")]
        [HttpPost]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest) {
            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonId(personUpdateRequest.PersonId);
            if (personResponse == null)
                return RedirectToAction("Index", "Persons");

            await _personsDeleterService.DeletePerson(personUpdateRequest.PersonId);
            return RedirectToAction("Index", "Persons");
        }

        [Route("PersonsPDF")]
        public async Task<IActionResult> PersonsPDF() {
            List<PersonResponse> persons = await _personsGetterService.GetAllPersons();
            return new ViewAsPdf("PersonsPDF", persons, ViewData) {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() {
                    Top = 20, Bottom = 20, Left = 20, Right = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("PersonsCSV")]
        public async Task<IActionResult> PersonsCSV() {
            MemoryStream memoryStream = await _personsGetterService.GetPersonCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        //[Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel() {
            MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }

    }
}
