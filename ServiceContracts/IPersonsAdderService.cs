using System;
using System.Collections.Generic;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts {
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsAdderService {
        /// <summary>
        /// Adds a new person into the list of persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>
        /// Returns the same person details, along with the newly generated PersonId
        /// </returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);
    }
}
