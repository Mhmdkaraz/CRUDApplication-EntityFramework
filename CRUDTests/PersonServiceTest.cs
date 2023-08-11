using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;

namespace CRUDTests {
    public class PersonServiceTest {
        //private fields
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        //constructor
        public PersonServiceTest(ITestOutputHelper testOutputHelper) {
            _fixture = new Fixture();
            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            //Craete mock for DbContext
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
              new DbContextOptionsBuilder<ApplicationDbContext>().Options
             );

            //Access Mock DbContext object
            ApplicationDbContext dbContext = dbContextMock.Object;

            //Create mocks for DbSets'
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            //Create services based on mocked DbContext object
            _countriesService = new CountriesService(dbContext);

            _personService = new PersonsService(dbContext, _countriesService);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson() {
            //Arrange
            PersonAddRequest? personAddRequest = null;
            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () => {
            //    //Act
            //    await _personService.AddPerson(personAddRequest);
            //});
            
            //act
            Func<Task> action = async () => {
                await _personService.AddPerson(personAddRequest);
            };
            //Fluent assertion
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        //When we supply personName as null value, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameNull() {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string).Create();
            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //Act
            //    await _personService.AddPerson(personAddRequest);
            //});
            //ACT
            Func<Task> action = async () => {
                await _personService.AddPerson(personAddRequest);
            };
            //Fluent assertion
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When we supply proper person details, it should insert person into the person list;
        //and it should return an object of PersonResponse, which include the newly generated person id
        [Fact]
        public async Task AddPerson_ProperPersonDetails() {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@example.com").Create();
            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);
            List<PersonResponse> person_list = await _personService.GetAllPersons();
            //Assert
            //Assert.True(person_response_from_add.PersonId != Guid.Empty);
            //Assert.Contains(person_response_from_add, person_list);
            person_response_from_add.PersonId.Should().NotBe(Guid.Empty);
            person_list.Should().Contain(person_response_from_add);
        }
        #endregion

        #region GetPersonByPersonId
        //if we supply null as PersonId, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonId_NullPersonId() {
            //Arrange
            Guid? personId = null;
            //Act
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonId(personId);
            //Assert
            //Assert.Null(person_response_from_get);
            person_response_from_get.Should().BeNull();
        }
        //if we supply a valid person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonId_ValidPersonId() {
            //Arrange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response = await _countriesService.AddCountry(country_request);
            //Act
            PersonAddRequest person_request = _fixture.Build<PersonAddRequest>().
                With(temp => temp.Email, "person@example.com").Create();
            PersonResponse? person_response_from_add = await _personService.AddPerson(person_request);
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonId(person_response_from_add.PersonId);
            //Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);
            person_response_from_get.Should().Be(person_response_from_add);
        }
        #endregion

        #region GetAllPersons
        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList() {
            //Act
            List<PersonResponse> person_from_get = await _personService.GetAllPersons();
            //Assert
            //Assert.Empty(person_from_get);
            person_from_get.Should().BeEmpty();
        }
        //First, we will add a few persons; and then when we call GetAllPersons(), it should return the same person that were added
        [Fact]
        public async Task GetAllPersons_AddFewPersons() {
            //Arrange
            CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);
            PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person1@example.com").Create();
            PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person2@example.com").Create();
            PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person3@example.com").Create();

            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() {
                person_request_1,person_request_2,person_request_3
            };
            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();
            foreach (PersonAddRequest person_request in person_requests) {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            //Act 
            List<PersonResponse> person_response_list_from_get = await _personService.GetAllPersons();
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_get) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            //Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
            //    Assert.Contains(person_response_from_add, person_response_list_from_get);
            //}
            person_response_list_from_add.Should().BeEquivalentTo(person_response_list_from_add);
        }
        #endregion

        #region GetFilteredPersons
        //if the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText() {
            //Arrange
            CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);
            PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person1@example.com").Create();
            PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person2@example.com").Create();
            PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person3@example.com").Create();
            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() {
                person_request_1,person_request_2,person_request_3
            };
            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();
            foreach (PersonAddRequest person_request in person_requests) {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            //Act 
            List<PersonResponse> person_response_list_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "");
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_search) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            //Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
            //    Assert.Contains(person_response_from_add, person_response_list_from_search);
            //}
            person_response_list_from_search.Should().BeEquivalentTo(person_response_list_from_add);
        }
        //First we will add few persons; then wee will search based on person name with some search string. it should return the matching person
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName() {
            //Arrange
            CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);
            PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.CountryId, country_response_1.CountryId)
                .Create();
            PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Mary")
                .With(temp => temp.CountryId, country_response_1.CountryId)
                .With(temp => temp.Email, "person2@example.com").Create();
            PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Scott")
                .With(temp => temp.CountryId, country_response_2.CountryId)
                .With(temp => temp.Email, "person3@example.com").Create();
            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() {
                person_request_1,person_request_2,person_request_3
            };
            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();
            foreach (PersonAddRequest person_request in person_requests) {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            //Act 
            List<PersonResponse> person_response_list_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "ma");
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_search) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            //Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
            //    if (person_response_from_add.PersonName != null) {
            //        if (person_response_from_add.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase)) {
            //            Assert.Contains(person_response_from_add, person_response_list_from_search);
            //        }
            //    }

            //}
            person_response_list_from_search.Should()
                .OnlyContain(temp => temp.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region GetSortedPersons
        //When we sort based on the PersonName in DESC, it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons() {
            //Arrange
            CountryAddRequest country_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_request_2 = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_1 = await _countriesService.AddCountry(country_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_request_2);
            PersonAddRequest person_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Smith")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.CountryId, country_response_1.CountryId)
                .Create();
            PersonAddRequest person_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Mary")
                .With(temp => temp.CountryId, country_response_1.CountryId)
                .With(temp => temp.Email, "person2@example.com").Create();
            PersonAddRequest person_request_3 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.CountryId, country_response_2.CountryId)
                .With(temp => temp.Email, "person3@example.com").Create();
            List<PersonAddRequest> person_requests = new List<PersonAddRequest>() {
                person_request_1,person_request_2,person_request_3
            };
            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();
            foreach (PersonAddRequest person_request in person_requests) {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            List<PersonResponse> allpersons = await _personService.GetAllPersons();

            //Act 
            List<PersonResponse> person_response_list_from_sort = await _personService.GetSortedPersons(allpersons, nameof(Person.PersonName), SortOrderOptions.DESC);
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_sort) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();
            //Assert
            //for (int i = 0; i < person_response_list_from_add.Count; i++) {
            //    Assert.Equal(person_response_list_from_add[i], person_response_list_from_sort[i]);
            //}
            person_response_list_from_sort.Should().BeInDescendingOrder(temp=>temp.PersonName);
        }
        #endregion

        #region UpdatedPerson
        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson() {
            //Arrange
            PersonUpdateRequest? person_update_request = null;
            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
              await  _personService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        //When we supply invalid PersonId, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId() {
            //Arrange
            PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>().Create();
            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
                await _personService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When the personName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull() {
            //Arrange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response = await _countriesService.AddCountry(country_request);
            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.CountryId, country_response.CountryId)
                .Create();
            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);
            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            person_update_request.PersonName = null;
            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
                await _personService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails() {
            //Arrange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response = await _countriesService.AddCountry(country_request);
            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.CountryId, country_response.CountryId)
                .Create();
            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);
            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            person_update_request.PersonName = "William";
            person_update_request.Email = "william@example.com";
            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(person_update_request);
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonId(person_response_from_update.PersonId);
            //Assert
            //Assert.Equal(person_response_from_get, person_response_from_update);
            person_response_from_update.Should().BeEquivalentTo(person_response_from_get);

        }
        #endregion

        #region DeletePerson
        //If you supply valid PersonId, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonId() {
            //Arrange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response = await _countriesService.AddCountry(country_request);
            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.CountryId, country_response.CountryId)
                .Create();
            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);
            //Act
            bool isDeleted = await _personService.DeletePerson(person_response_from_add.PersonId);
            //Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        //If you supply invalid PersonId, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonId() {
            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());
            //Assert
            //Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
