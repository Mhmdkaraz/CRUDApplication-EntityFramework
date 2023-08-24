using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts {
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesUploaderService {
        /// <summary>
        /// Uplaods countries from excel file into database
        /// </summary>
        /// <param name="formFile">Excel file with list of countries</param>
        /// <returns>Return number of countries added</returns>
        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
    }
}