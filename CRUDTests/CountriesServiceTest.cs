using System;
using System.Collections.Generic;
using System.Reflection;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using EntityFrameworkCoreMock;
using Moq;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using System.Diagnostics.Metrics;

namespace CRUDTests {
    public class CountriesServiceTest {
        private readonly ICountriesService _countriesService;
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly IFixture _fixture;
        public CountriesServiceTest() {
            _fixture = new Fixture();
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
            _countriesService = new CountriesService(_countriesRepository);
        }

        #region AddCountry
        //when CountryAddRequset is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException() {
            //arrange
            CountryAddRequest? request = null;
            Country country = _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);
            //Act + Assert
            Func<Task> action = async () => {
                await _countriesService.AddCountry(request);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();

        }
        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException() {
            //arrange
            CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string).Create();
            Country country = _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);
            //assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //act
            //    await _countriesService.AddCountry(request);
            //});
            Func<Task> action = async () => {
                await _countriesService.AddCountry(request);
            };
            await action.Should().ThrowAsync<ArgumentException>();

        }
        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException() {
            //arrange
            CountryAddRequest first_country_request = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName, "Test name").Create();
            CountryAddRequest second_country_request = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName, "Test name").Create();
            Country first_country = first_country_request.ToCountry();
            Country second_country = second_country_request.ToCountry();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(first_country);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(null as Country);
            CountryResponse first_country_from_add = await _countriesService.AddCountry(first_country_request);
            ////assert
            ////await Assert.ThrowsAsync<ArgumentException>(async () => {
            ////    //act
            ////    await _countriesService.AddCountry(request1);
            ////    await _countriesService.AddCountry(request2);
            ////});
            Func<Task> action = async () => {
                _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(first_country);
                _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(first_country);
                await _countriesService.AddCountry(second_country_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();

        }
        //When you supply proper country name, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_FullCountry_ToBeSuccessful() {
            //arrange
            CountryAddRequest? request = _fixture.Create<CountryAddRequest>();
            Country country = request.ToCountry();
            CountryResponse country_response = country.ToCountryResponse();
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(null as Country);

            //act
            CountryResponse country_from_add_country = await _countriesService.AddCountry(request);
            country.CountryId = country_from_add_country.CountryId;
            country_response.CountryId = country_from_add_country.CountryId;
            //assert
            //Assert.True(response.CountryId != Guid.Empty);
            ////calls Equals method internally
            //Assert.Contains(response, countries_from_GetAllCountries);
            country_from_add_country.CountryId.Should().NotBe(Guid.Empty);
            country_from_add_country.Should().BeEquivalentTo(country_response);

        }
        //objA.Equals(objB) (compares referece in case of the object)
        #endregion

        #region GetAllCountries
        //The list should be empty by default (before adding any countries)
        [Fact]
        public async Task GetAllCountries_ToBeEmptyList() {
            //Act
            List<Country> country_empty_list = new List<Country>();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(country_empty_list);
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();
            //Assert
            //Assert.Empty(actual_country_response_list);
            actual_country_response_list.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllCountries_ShouldHaveFewCountries() {
            List<Country> country_list =
                new List<Country> {
                   _fixture.Build<Country>().With(temp=>temp.Persons,null as List<Person>).Create(),
                   _fixture.Build<Country>().With(temp=>temp.Persons,null as List<Person>).Create()
                };
            List<CountryResponse> countries_list_expected = country_list.Select(temp=>temp.ToCountryResponse()).ToList();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(country_list);
            //Act
            List<CountryResponse> countries_list_actual = await _countriesService.GetAllCountries();
            //Assert
            //foreach (CountryResponse expected_country in countries_list_from_add_country) {
            //    Assert.Contains(expected_country, actual_country_response_list);
            //}
            countries_list_actual.Should().BeEquivalentTo(countries_list_expected);
        }
        #endregion

        #region GetCountryByCountryId
        //If we supply null as CountryId, it should return null as CountryResponse
        [Fact]
        public async Task GetCountryByCountryId_NullCountryId_ToBeNull() {
            //Arrange
            Guid? countryId = null;
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryId(It.IsAny<Guid>())).ReturnsAsync(null as Country);
            //Act
            CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryId(countryId);
            //Assert
            //Assert.Null(country_response_from_get_method);
            country_response_from_get_method.Should().BeNull();
        }
        //If we supply a valid country id, it should return the matching country details as CountryResponse object 
        [Fact]
        public async Task GetCountryByCountryId_ValidCountryId_ToBeSuccessful() {
            //Arrange
            Country country = _fixture.Build<Country>().With(temp=>temp.Persons,null as List<Person>).Create();
            CountryResponse country_response_expected = country.ToCountryResponse();
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryId(It.IsAny<Guid>())).ReturnsAsync(country);
            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryId(country.CountryId);
            //Assert
            //Assert.Equal(country_response_from_add_request, country_response_from_get);
            country_response_from_get.Should().Be(country_response_expected);
        }
        #endregion

    }
}
