using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities {
    public class ApplicationDbContext:DbContext {
        public ApplicationDbContext(DbContextOptions options):base(options)
        {
            
        }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            //Seed to Countries
            string countriesJson = System.IO.File.ReadAllText("countries.json");
            //deserialize : json to object
            List<Country> countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);
            
            foreach(Country country in countries)
                modelBuilder.Entity<Country>().HasData(country);
            //modelBuilder.Entity<Country>().HasData(new Country() {
            //    CountryId = Guid.NewGuid(), CountryName = "Sample"
            //});

            //Seed to Persons
            string personsJson = System.IO.File.ReadAllText("persons.json");
            List<Person> persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson);
            foreach(Person person in persons)
                modelBuilder.Entity<Person>().HasData(person);

            //Fluent API
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            //modelBuilder.Entity<Person>().HasIndex(temp => temp.TIN).IsUnique();

            modelBuilder.Entity<Person>()
                .HasCheckConstraint("CHK_TIN1", "len([TaxIdentificationNumber])=8");

            //Table Relations
            //modelBuilder.Entity<Person>(entity => {
            //    entity.HasOne<Country>(c => c.Country)
            //    .WithMany(p => p.Persons)
            //    .HasForeignKey(p=>p.CountryId);
            //});
        }

        public List<Person> sp_GetAllPersons() {
            return Persons.FromSqlRaw("EXEC [dbo].[GetAllPersons]").ToList();
        }
        public Task<int> sp_InsertPerson(Person person) {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PersonId",person.PersonId),
                new SqlParameter("@PersonName",person.PersonName),
                new SqlParameter("@Email",person.Email),
                new SqlParameter("@DateOfBirth",person.DateOfBirth),
                new SqlParameter("@Gender",person.Gender),
                new SqlParameter("@CountryId",person.CountryId),
                new SqlParameter("@Address",person.Address),
                new SqlParameter("@ReceiveNewsLetters",person.ReceiveNewsLetters),
            };
            return Database.ExecuteSqlRawAsync("EXEC [dbo].[InsertPerson] @PersonId, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters", parameters);
        }
        public Task<int> sp_UpdatePerson(Person person) {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PersonId",person.PersonId),
                new SqlParameter("@PersonName",person.PersonName),
                new SqlParameter("@Email",person.Email),
                new SqlParameter("@DateOfBirth",person.DateOfBirth),
                new SqlParameter("@Gender",person.Gender),
                new SqlParameter("@CountryId",person.CountryId),
                new SqlParameter("@Address",person.Address),
                new SqlParameter("@ReceiveNewsLetters",person.ReceiveNewsLetters),
            };
            return Database.ExecuteSqlRawAsync("EXEC [dbo].[UpdatePerson] @PersonId, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters", parameters);
        }
        public Task<int> sp_DeletePerson(Guid? personId) {
            SqlParameter parameter = new SqlParameter("@PersonId", personId);
            return Database.ExecuteSqlRawAsync("EXEC [dbo].[DeletePerson] @PersonId", parameter);
        }
    }
}
