using AutoFixture;
using Moq;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ServiceContracts.DTO;
using CRUDExample.Controllers;
using ServiceContracts;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Mvc;
using Entities;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace CRUDTests {
    public class PersonsControllerTest {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly IFixture _fixture;
        public PersonsControllerTest()
        {
            _fixture = new Fixture();
            _personsServiceMock = new Mock<IPersonsService>();
            _countriesServiceMock = new Mock<ICountriesService>();
            _loggerMock = new Mock<ILogger<PersonsController>>();
            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
            _logger = _loggerMock.Object;
        }
        #region Index
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList() {
            //Arrange
            List<PersonResponse> persons_response_list = _fixture.Create<List<PersonResponse>>();
            PersonsController personsController = new PersonsController(_personsService,_countriesService, _logger);
            _personsServiceMock.Setup(temp=>temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(persons_response_list);
            _personsServiceMock.Setup(temp=>temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(),It.IsAny<string>(),It.IsAny<SortOrderOptions>())).ReturnsAsync(persons_response_list);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(persons_response_list);
        }
        #endregion
        #region Create
        //[Fact]
        //public async Task Create_IfModelErrors_ToReturnCreateView() {
        //    //Arrange
        //    PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();
        //    PersonResponse person_response = _fixture.Create<PersonResponse>();
        //    List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
        //    _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);
        //    _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);
        //    PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

        //    //Act
        //    personsController.ModelState.AddModelError("PersonName","Person name can't be blank");
        //    IActionResult result = await personsController.Create(person_add_request);
            
        //    //Assert
        //    ViewResult viewResult = Assert.IsType<ViewResult>(result);
        //    viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
        //    viewResult.ViewData.Model.Should().Be(person_add_request);
        //}

        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndex() {
            //Arrange
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);
            _countriesServiceMock.Setup(temp=>temp.GetAllCountries()).ReturnsAsync(countries);
            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            IActionResult result = await personsController.Create(person_add_request);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion
        #region Edit
        //[Fact]
        //public async Task Edit_IfNoModelErrors_ToReturnRedirectToIndex() {
        //    //Arrange
        //    PersonUpdateRequest person_update = _fixture.Create<PersonUpdateRequest>();
        //    PersonResponse person_response = _fixture.Create<PersonResponse>();
        //    _personsServiceMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);
        //    PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);
        //    //Act
        //    IActionResult result = await personsController.Edit(person_update);
        //    //Assert
        //    RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        //    redirectResult.ActionName.Should().Be("Index");

        //}
        //[Fact]
        //public async Task Edit_IfModelErrors_ToReturnRedirectToIndex() {
        //    //Arrange
        //    PersonResponse person_response = _fixture.Build<PersonResponse>().With(temp=>temp.Gender,"Male").Create();
        //    PersonUpdateRequest person_update = _fixture.Create<PersonUpdateRequest>();
        //    List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();
        //    _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);
        //    _personsServiceMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);
        //    PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);
        //    //Act
        //    personsController.ModelState.AddModelError("PersonName", "Person name can't be blank");
        //    IActionResult result = await personsController.Edit(person_update);
        //    //Assert
        //    RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        //    //viewResult.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
        //    //viewResult.ViewData.Model.Should().Be(person_response.ToPersonUpdateRequest());
        //}
        #endregion
    }
}
