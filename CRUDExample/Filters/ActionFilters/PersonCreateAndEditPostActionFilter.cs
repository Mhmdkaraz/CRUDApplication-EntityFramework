using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters {
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter {
        private readonly ICountriesGetterService _countriesService;
        private readonly ILogger<PersonCreateAndEditPostActionFilter> _logger;

        public PersonCreateAndEditPostActionFilter(ICountriesGetterService countriesService, ILogger<PersonCreateAndEditPostActionFilter> logger) {
            _countriesService = countriesService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            if(context.Controller is PersonsController personsController) {
                if (!personsController.ModelState.IsValid) {
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();
                    personsController.ViewBag.Countries = countries.Select(temp => new SelectListItem() {
                        Text = temp.CountryName, Value = temp.CountryId.ToString()
                    });
                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v=>v.Errors).Select(v=>v.ErrorMessage);
                    context.Result = personsController.View(context.ActionArguments["personRequest"]);//short-circuits or skip the subsequent action filters & action method
                } else {
                    await next();
                }
            } else {
                await next();
            }
            _logger.LogInformation("In after logic of PersonsCreateAndEdit Action filter");
        }
    }
}
