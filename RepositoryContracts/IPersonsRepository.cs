using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContracts {
    /// <summary>
    /// Represents data access logic for managing Person entity
    /// </summary>
    public interface IPersonsRepository {

        /// <summary>
        /// Adds a person object to the data stor
        /// </summary>
        /// <param name="person">Person object to add</param>
        /// <returns>Returns the person object aafteer adding it to the table</returns>
        Task<Person> AddPerson(Person person);

        /// <summary>
        /// Returns all the persons in the data store
        /// </summary>
        /// <returns>List of person objects from the table</returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// Returns a person object based on the fiven personId
        /// </summary>
        /// <param name="personId">PersonId (guid) to search</param>
        /// <returns>A person object or null</returns>
        Task<Person?> GetPersonByPersonId(Guid personId);

        /// <summary>
        /// Returns all person objects based on the given expression
        /// </summary>
        /// <param name="predicate">LINQ expression to check</param>
        /// <returns>All matching persons with the given condition</returns>
        Task<List<Person>> GetFilteredPersons(Expression<Func<Person,bool>> predicate);

        /// <summary>
        /// Deletes a person object based on the person id
        /// </summary>
        /// <param name="personId">Person Id (guid) to search</param>
        /// <returns>
        /// Returns true if the deletion is successfull;
        /// otherwise, false
        /// </returns>
        Task<bool> DeletePersonByPersonId(Guid personId);

        /// <summary>
        /// Updates a person object (person name and other details)
        /// based on the given person id
        /// </summary>
        /// <param name="person">Person object to update</param>
        /// <returns>Returns the updated person object</returns>
        Task<Person> UpdatePerson(Person person);
       
    }
}
