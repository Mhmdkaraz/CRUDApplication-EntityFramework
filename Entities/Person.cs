using Azure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities {
    /// <summary>
    /// Person domain model class
    /// </summary>
    public class Person {
        [Key]
        public Guid PersonId { get; set; }
        [StringLength(40)]//nvarchar(40)
        public string? PersonName { get; set; }
        [StringLength(40)]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(10)]
        public string? Gender { get; set; }
        public Guid? CountryId { get; set; }
        [StringLength(200)]
        public string? Address { get; set; }
        //bit
        public bool ReceiveNewsLetters { get; set; }
        public string? TIN { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country? Country { get; set; }

        public override string ToString() {
            return $"Person Id: {PersonId}, Person Name: {PersonName}," +
                $"Email: {Email}, Date of Birth: {DateOfBirth?.ToString("dd MM yyyy")}, " +
                $"Gender: {Gender}, CountryId: {CountryId}, Country: {Country}, Address: {Address}, " +
                $"Receive News Letters: {ReceiveNewsLetters}";
        }
    }
}
