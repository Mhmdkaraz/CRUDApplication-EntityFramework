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

namespace CRUDTests {
    public class CountriesServiceTest {
        private readonly ICountriesService _countriesService;
        private readonly IFixture _fixture;
        public CountriesServiceTest() {
            _fixture = new Fixture();
            var countriesInitialData = new List<Country> { };
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
            ApplicationDbContext dbContext = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            _countriesService = new CountriesService(dbContext);
        }

        #region AddCountry
        //when CountryAddRequset is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry() {
            //arrange
            CountryAddRequest? request = null;
            //assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () => {
            //    //act
            //    await _countriesService.AddCountry(request);
            //});
            Func<Task> action = async () => {
                await _countriesService.AddCountry(request);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();

        }
        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull() {
            //arrange
            CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string).Create();
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
        public async Task AddCountry_DuplicateCountryName() {
            //arrange
            CountryAddRequest? request1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest? request2 = request1;
            //assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //act
            //    await _countriesService.AddCountry(request1);
            //    await _countriesService.AddCountry(request2);

            //});
            Func<Task> action = async () => {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            };
            await action.Should().ThrowAsync<ArgumentException>();

        }
        //When you supply proper country name, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails() {
            //arrange
            CountryAddRequest? request = _fixture.Create<CountryAddRequest>();

            //act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();
            //assert
            //Assert.True(response.CountryId != Guid.Empty);
            ////calls Equals method internally
            //Assert.Contains(response, countries_from_GetAllCountries);
            countries_from_GetAllCountries.Should().Contain(response);

        }
        //objA.Equals(objB) (compares referece in case of the object)
        #endregion

        #region GetAllCountries
        //The list should be empty by default (before adding any countries)
        [Fact]
        public async Task GetAllCountries_EmptyList() {
            //Act
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            //Assert
            //Assert.Empty(actual_country_response_list);
            actual_country_response_list.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllCountries_AddFewCountries() {
            List<CountryAddRequest> country_request_list =
                new List<CountryAddRequest> {
                   _fixture.Create<CountryAddRequest>(),
                   _fixture.Create<CountryAddRequest>()
        };
            //Act
            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();
            foreach (CountryAddRequest country_add_request in country_request_list) {
                countries_list_from_add_country.Add
                    (await _countriesService.AddCountry(country_add_request));
            }
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();
            //Assert
            //foreach (CountryResponse expected_country in countries_list_from_add_country) {
            //    Assert.Contains(expected_country, actual_country_response_list);
            //}
            actual_country_response_list.Should().BeEquivalentTo(countries_list_from_add_country);
        }
        #endregion

        #region GetCountryByCountryId
        //If we supply null as CountryId, it should return null as CountryResponse
        [Fact]
        public async Task GetCountryByCountryId_NullCountryId() {
            //Arrange
            Guid? countryId = null;
            //Act
            CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryId(countryId);
            //Assert
            //Assert.Null(country_response_from_get_method);
            country_response_from_get_method.Should().BeNull();
        }
        //If we supply a valid country id, it should return the matching country details as CountryResponse object 
        [Fact]
        public async Task GetCountryByCountryId_ValidCountryId() {
            //Arrange
            CountryAddRequest? country_add_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_from_add_request = await _countriesService.AddCountry(country_add_request);
            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryId(country_response_from_add_request.CountryId);
            //Assert
            //Assert.Equal(country_response_from_add_request, country_response_from_get);
            country_response_from_get.Should().BeEquivalentTo(country_response_from_add_request);
        }
        #endregion

    }
}
