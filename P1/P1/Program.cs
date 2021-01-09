using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
namespace P1
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Latittude { get; set; }
        public int Longitude { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public int CityId { get; set; }
    }

    public class Context : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasKey(x => x.Id);
            modelBuilder.Entity<Person>().Property(x => x.Name).HasMaxLength(128);
            modelBuilder.Entity<City>().HasKey(x => x.Id);
            modelBuilder.Entity<Person>().Property(x => x.Address.Street).IsRequired();
        }

    }
    public class AdressRepository
    {
        public List<string> AllStreetsInCityWhereLiveMore10People(int cityId)
        {
            using (var ctx = new Context())
            {
                return ctx.Persons.Where(p=> p.Address.CityId== cityId).GroupBy(p=> p.Address.Street)
                .Where(g => g.Count() > 10).Select(g => g.Key).ToList();
            }
        }
     }
        class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
