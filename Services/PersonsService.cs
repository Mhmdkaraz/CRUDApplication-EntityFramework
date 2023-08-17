using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Services {
    public class PersonsService : IPersonsService {
        //private field
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;
        public PersonsService(IPersonsRepository personsRepository, ILogger<PersonsService> logger, IDiagnosticContext diagnosticContext) {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest) {
            //check if PersonAddRequest is not null
            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest));
            //Model Validation 
            ValidationHelper.ModelValidation(personAddRequest);
            //convert personAddRequest into Person type
            Person person = personAddRequest.ToPerson();
            //generate PersonId
            person.PersonId = Guid.NewGuid();
            //add person object to persons list
            //_db.Persons.Add(person);
            //_db.SaveChanges();
            await _personsRepository.AddPerson(person);
            //converts the Person object into PersonResponse type
            return person.ToPersonResponse();

        }

        public async Task<List<PersonResponse>> GetAllPersons() {
            _logger.LogInformation("GetAllPersons of PersonsService");
            var persons = await _personsRepository.GetAllPersons();
            return persons.Select(person => person.ToPersonResponse()).ToList();
            //_db.sp_GetAllPersons().Select(person => ConvertPersonToPersonResponse(person)).ToList(); 
            //_db.Persons.ToList().Select(person => ConvertPersonToPersonResponse(person)).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonId(Guid? personId) {
            if (personId == null)
                return null;
            Person? person = await _personsRepository.GetPersonByPersonId(personId.Value);
            if (person == null)
                return null;
            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString) {
            _logger.LogInformation("GetFilteredPersons of PersonsService");
            List<Person> persons = searchBy switch {
                nameof(PersonResponse.PersonName) =>
                 await _personsRepository.GetFilteredPersons(temp => (temp.PersonName.Contains(searchString))),
                nameof(PersonResponse.Email) =>
                 await _personsRepository.GetFilteredPersons(temp => temp.Email.Contains(searchString)),
                nameof(PersonResponse.DateOfBirth) =>
                await _personsRepository.GetFilteredPersons(temp => temp.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString)),
                nameof(PersonResponse.Gender) =>
                  await _personsRepository.GetFilteredPersons(temp => temp.Gender.Contains(searchString)),
                nameof(PersonResponse.CountryId) =>
                 await _personsRepository.GetFilteredPersons(temp => temp.Country.CountryName.Contains(searchString)),
                nameof(PersonResponse.Address) =>
                  await _personsRepository.GetFilteredPersons(temp => temp.Address.Contains(searchString)),
                _ => await _personsRepository.GetAllPersons()

            };
            _diagnosticContext.Set("Persons", persons);
            return persons.Select(temp => temp.ToPersonResponse()).ToList();
        }
        //public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString) {
        //    List<PersonResponse> allPersons = GetAllPersons();
        //    List<PersonResponse> matchingPersons = allPersons;

        //    if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
        //        return matchingPersons;

        //    PropertyInfo? property = typeof(PersonResponse).GetProperty(searchBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        //    if (property != null) {
        //        matchingPersons = allPersons
        //            .Where(person => {
        //                var value = property.GetValue(person)?.ToString();
        //                return !string.IsNullOrEmpty(value) && value.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        //            })
        //            .ToList();
        //    }

        //    return matchingPersons;
        //}
        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder) {
            _logger.LogInformation("GetSortedPersons of PersonsService");
            if (string.IsNullOrEmpty(sortBy))
                return allPersons;
            List<PersonResponse>? sortedPersons = (sortBy, sortOrder) switch {

                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC)
                => allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC)
                => allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Email), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC)
                => allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),
                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC)
                => allPersons.OrderBy(temp => temp.Age).ToList(),
                (nameof(PersonResponse.Age), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC)
               => allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Gender), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC)
                => allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Address), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC)
               => allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC)
                => allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                _ => allPersons

            };
            return sortedPersons;
        }
        //public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder) {
        //    if (string.IsNullOrEmpty(sortBy))
        //        return allPersons;

        //    PropertyInfo property = typeof(PersonResponse).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        //    if (property != null) {
        //        if (sortOrder == SortOrderOptions.ASC) {
        //            allPersons = allPersons.OrderBy(person => property.GetValue(person, null)).ToList();
        //        } else if (sortOrder == SortOrderOptions.DESC) {
        //            allPersons = allPersons.OrderByDescending(person => property.GetValue(person, null)).ToList();
        //        }
        //    }

        //    return allPersons;
        //}
        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest) {
            //check if "personUpdateRequest" in not null
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(Person));
            //Validate all properties of "personUpdateRequest"
            ValidationHelper.ModelValidation(personUpdateRequest);
            //get matching person object to update
            Person? matchingPerson = await _personsRepository.GetPersonByPersonId(personUpdateRequest.PersonId);
            if (matchingPerson == null)
                throw new ArgumentException("Given person id doesn't exist");
            ////update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryId = personUpdateRequest.CountryId;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;
            await _personsRepository.UpdatePerson(matchingPerson);
            //Person person = personUpdateRequest.ToPerson();
            //await _db.sp_UpdatePerson(person);
            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personId) {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));
            Person? person = await _personsRepository.GetPersonByPersonId(personId.Value);
            if (person == null)
                return false;
            await _personsRepository.DeletePersonByPersonId(person.PersonId);
            //await _db.sp_DeletePerson(personId);
            return true;
        }

        public async Task<MemoryStream> GetPersonCSV() {
            MemoryStream memoryStream = new MemoryStream();
            //writes the content into the memory stream
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            //All Properties
            //CsvWriter csvWriter = new CsvWriter(streamWriter,CultureInfo.InvariantCulture,leaveOpen:true);
            //csvWriter.WriteHeader<PersonResponse>();
            //csvWriter.NextRecord();
            //List<PersonResponse> persons = await _db.Persons.Include("Country").Select(temp=>temp.ToPersonResponse()).ToListAsync();
            //await csvWriter.WriteRecordsAsync(persons);

            //Custom Properties
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            //PersonName,Email,DateOfBirth,Age,Gender,Country,Address,ReceiveNewsLetters
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();//moves to the next line
            List<PersonResponse> persons = await GetAllPersons();
            foreach (PersonResponse person in persons) {
                csvWriter.WriteField($"{person.PersonName}");
                csvWriter.WriteField($"{person.Email}");
                if (person.DateOfBirth.HasValue)
                    csvWriter.WriteField($"{person.DateOfBirth.Value.ToString("yyyy-MM-dd")}");
                else
                    csvWriter.WriteField("");
                csvWriter.WriteField($"{person.Age}");
                csvWriter.WriteField($"{person.Gender}");
                csvWriter.WriteField($"{person.Country}");
                csvWriter.WriteField($"{person.Address}");
                csvWriter.WriteField($"{person.ReceiveNewsLetters}");
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsExcel() {
            //epplussoftware.com/docs
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream)) {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");
                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "Date of Birth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Country";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "Receive News Letters";
                using (ExcelRange headCells = worksheet.Cells["A1:H1"]) {
                    headCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.IndianRed);
                    headCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();
                foreach (PersonResponse person in persons) {
                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");
                    else
                        worksheet.Cells[row, 3].Value = "";
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;
                    row++;
                }
                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
