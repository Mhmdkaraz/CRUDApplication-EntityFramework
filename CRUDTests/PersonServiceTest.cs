using Entities;
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
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;
using Serilog.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;

namespace CRUDTests {
    public class PersonServiceTest {
        //private fields
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        //constructor
        public PersonServiceTest(ITestOutputHelper testOutputHelper) {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var personsGetterloggerMock = new Mock<ILogger<PersonsGetterService>>();
            var personsAdderloggerMock = new Mock<ILogger<PersonsAdderService>>();
            var personsSorterloggerMock = new Mock<ILogger<PersonsSorterService>>();
            var personsUpdaterloggerMock = new Mock<ILogger<PersonsUpdaterService>>();
            var personsDeleterloggerMock = new Mock<ILogger<PersonsDeleterService>>();

            //Create services based on mocked DbContext object
            _personsGetterService = new PersonsGetterService(_personsRepository, personsGetterloggerMock.Object,diagnosticContextMock.Object);
            _personsAdderService = new PersonsAdderService(_personsRepository, personsAdderloggerMock.Object, diagnosticContextMock.Object);
            _personsSorterService = new PersonsSorterService(_personsRepository, personsSorterloggerMock.Object, diagnosticContextMock.Object);
            _personsUpdaterService = new PersonsUpdaterService(_personsRepository, personsUpdaterloggerMock.Object, diagnosticContextMock.Object);
            _personsDeleterService = new PersonsDeleterService(_personsRepository, personsDeleterloggerMock.Object, diagnosticContextMock.Object);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException() {
            //Arrange
            PersonAddRequest? personAddRequest = null;
            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () => {
            //    //Act
            //    await _personService.AddPerson(personAddRequest);
            //});

            //act
            Func<Task> action = async () => {
                await _personsAdderService.AddPerson(personAddRequest);
            };
            //Fluent assertion
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        //When we supply personName as null value, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameNull_ToBeArgumentException() {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string).Create();
            Person person = personAddRequest.ToPerson();
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            //ACT
            Func<Task> action = async () => {
                //it doesn't invoke the real repository
                await _personsAdderService.AddPerson(personAddRequest);
            };
            //Fluent assertion
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When we supply proper person details, it should insert person into the person list;
        //and it should return an object of PersonResponse, which include the newly generated person id
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful() {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@example.com").Create();
            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();
            //if we supply any argument value to the AddPerson method, it should return the same retrun value
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))//argument type
                .ReturnsAsync(person);//return type
            //Act
            PersonResponse person_response_from_add = await _personsAdderService.AddPerson(personAddRequest);
            person_response_expected.PersonId = person_response_from_add.PersonId;
            //Assert
            person_response_from_add.PersonId.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().BeEquivalentTo(person_response_expected);
        }
        #endregion

        #region GetPersonByPersonId
        //if we supply null as PersonId, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonId_NullPersonId_ToBeNull() {
            //Arrange
            Guid? personId = null;
            //Act
            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonId(personId);
            //Assert
            //Assert.Null(person_response_from_get);
            person_response_from_get.Should().BeNull();
        }
        //if we supply a valid person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonId_ValidPersonId_ToBeSuccessful() {
            //Arrange
            //CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();
            //CountryResponse country_response = await _countriesService.AddCountry(country_request);
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "person@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();
            PersonResponse person_response_expected = person.ToPersonResponse();
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);
            //Act
            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonId(person.PersonId);
            //Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);
            person_response_from_get.Should().Be(person_response_expected);
        }
        #endregion

        #region GetAllPersons
        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList() {
            //Arrange
            var persons = new List<Person>();
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);
            //Act
            List<PersonResponse> person_from_get = await _personsGetterService.GetAllPersons();
            //Assert
            //Assert.Empty(person_from_get);
            person_from_get.Should().BeEmpty();
        }
        //First, we will add a few persons; and then when we call GetAllPersons(), it should return the same person that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful() {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person1@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person2@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person3@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create()
            };
            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_expected in person_response_list_expected) {
                _testOutputHelper.WriteLine(person_response_expected.ToString());
            }
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);
            //Act 
            List<PersonResponse> person_response_list_from_get = await _personsGetterService.GetAllPersons();
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_get) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            //Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
            //    Assert.Contains(person_response_from_add, person_response_list_from_get);
            //}
            person_response_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
        }
        #endregion

        #region GetFilteredPersons
        //if the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful() {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person1@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person2@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person3@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create()

            };
            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_expected in person_response_list_expected) {
                _testOutputHelper.WriteLine(person_response_expected.ToString());
            }
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);
            //Act 
            List<PersonResponse> person_response_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_search) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            //Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
            //    Assert.Contains(person_response_from_add, person_response_list_from_search);
            //}
            person_response_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }
        //search based on person name with some search string. it should return the matching person
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful() {
            //Arrange

            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                 _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Mary")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Email, "person2@example.com")
                .Create(),
                 _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Scott")
                .With(temp => temp.Country,null as Country)
                .With(temp => temp.Email, "person3@example.com")
                .Create()
                };
            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_expected in person_response_list_expected) {
                _testOutputHelper.WriteLine(person_response_expected.ToString());
            }
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);
            //Act 
            List<PersonResponse> person_response_list_actual = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "ma");
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_actual) {
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
            person_response_list_actual.Should().BeEquivalentTo(person_response_list_expected);
        }
        #endregion

        #region GetSortedPersons
        //When we sort based on the PersonName in DESC, it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful() {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person1@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person2@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create(),
                _fixture.Build<Person>()
                .With(temp => temp.Email, "person3@example.com")
                .With(temp=>temp.Country,null as Country)
                .Create()

            };
            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);
            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add) {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }
            List<PersonResponse> allpersons = await _personsGetterService.GetAllPersons();

            //Act 
            List<PersonResponse> person_response_list_from_sort = await _personsSorterService.GetSortedPersons(allpersons, nameof(Person.PersonName), SortOrderOptions.DESC);
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in person_response_list_from_sort) {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }
            person_response_list_from_add = person_response_list_from_add.OrderByDescending(temp => temp.PersonName).ToList();
            //Assert
            //for (int i = 0; i < person_response_list_from_add.Count; i++) {
            //    Assert.Equal(person_response_list_from_add[i], person_response_list_from_sort[i]);
            //}
            person_response_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
        }
        #endregion

        #region UpdatedPerson
        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException() {
            //Arrange
            PersonUpdateRequest? person_update_request = null;
            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }
        //When we supply invalid PersonId, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ToBeArgumentException() {
            //Arrange
            PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>().Create();
            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //When the personName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException() {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();
            PersonResponse person_response_from_add = person.ToPersonResponse();
            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();
            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () => {
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () => {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }
        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful() {
            //Arrange

            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Rahman")
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();
            PersonResponse person_response_expected = person.ToPersonResponse();
            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);
            //Assert
            //Assert.Equal(person_response_from_get, person_response_from_update);
            person_response_from_update.Should().BeEquivalentTo(person_response_expected);

        }
        #endregion

        #region DeletePerson
        //If you supply valid PersonId, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonId_ToBeSuccessful() {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "person1@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Female")
                .Create();
            
            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(true);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);
            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonId);
            //Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        //If you supply invalid PersonId, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonId() {
            //Act
            bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());
            //Assert
            //Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
