using Domain.Entities;

namespace Infrastructure.Data.SeedData;

public static class PersonSeedData
{
    public static List<Person> Create()
    {
        var now = DateTimeOffset.UtcNow;
        var persons = new List<Person>
        {
            Person.CreateIndividual("Ivan", "Petrov", "8001011234", "BG8001011234"),
            Person.CreateIndividual("Maria", "Georgieva", "8502022345", "BG8502022345"),
            Person.CreateCompany("SiteWatch Services", null, "123456789", "BG123456789"),
        };

        foreach (var person in persons)
        {
            person.Created = now;
            person.CreatedBy = "System";
            person.LastModified = now;
            person.LastModifiedBy = "System";
        }

        return persons;
    }
}
