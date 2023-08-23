using System;
using System.Collections.Generic;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts {
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsGetterService {
        
        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns>
        /// Returns a list of objects of PersonResponse type
        /// </returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns the person object based on the given person Id
        /// </summary>
        /// <param name="personId">Person id to search</param>
        /// <returns>
        /// Returns matching person object
        /// </returns>
        Task<PersonResponse?> GetPersonByPersonId(Guid? personId);

        /// <summary>
        /// Returns all person objects that matches with the given search field and search string
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">Search string to search</param>
        /// <returns>
        /// Returns all matching persons based on the given search field and search string
        /// </returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        /// <summary>
        /// Returns persons as CSV
        /// </summary>
        /// <returns>Returns the memory stream with CSV data</returns>
        Task<MemoryStream> GetPersonCSV();

        /// <summary>
        /// Returns persons as Excel
        /// </summary>
        /// <returns>Returns the memory stream with Excel data of persons</returns>
        Task<MemoryStream> GetPersonsExcel();
    }
}
