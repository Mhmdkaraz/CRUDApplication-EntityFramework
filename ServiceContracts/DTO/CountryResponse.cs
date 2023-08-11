using System;
using System.Collections.Generic;
using Entities;
namespace ServiceContracts.DTO {
    /// <summary>
    /// DTO class that is used as return type for most of the CountryService methods
    /// </summary>
    public class CountryResponse {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }

        //It compares the current object to another object of CountryResponse type and returns true, if both values are same; otherwise returns false;
        public override bool Equals(object? obj) {
            if (obj == null) return false;
            if (obj.GetType() != typeof(CountryResponse)) return false;
            CountryResponse country_to_compare = (CountryResponse)obj;
            return CountryId == country_to_compare.CountryId && CountryName == country_to_compare.CountryName;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
    public static class CountryExtensions {
        //Converts from Country object to CountryResponse object
        public static CountryResponse ToCountryResponse(this Country country) {
            return new CountryResponse() { CountryId = country.CountryId, CountryName = country.CountryName };
        }
    }
}
