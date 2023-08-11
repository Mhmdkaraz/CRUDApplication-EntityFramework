using Entities;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace ServiceContracts.DTO {
    /// <summary>
    /// Acts as a DTO for updating an existing person
    /// </summary>
    public class PersonUpdateRequest {
        [Required]
        public Guid PersonId { get; set; }
        [Required(ErrorMessage = "Person Name can't be blank")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "Email value can't be blank")]
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public GenderOptions? Gender { get; set; }
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        /// <summary>
        /// Converts an object of PersonUpdateRequest into a new object of Person type
        /// </summary>
        /// <returns>Returns Person object</returns>
        public Person ToPerson() {
            return new Person() {
                PersonId = PersonId, PersonName = PersonName,
                Email = Email, DateOfBirth = DateOfBirth, Gender = Gender.ToString(),
                Address = Address, CountryId = CountryId, ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }
}
