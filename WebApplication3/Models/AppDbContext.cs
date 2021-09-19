using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WebApplication3.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
            : base("DBConnection")
        { }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                    .HasIndex(u => new { u.tnumber ,u.staff})
                    .HasName("IX_tnumber_staff01")
                    //на первом єтапе разработки индекс сделал уникальным, но при таком индексе нельза записать несколько внештатных
                    //потому переделал на неуникальный, сам индекс оставил для оптимизации поиска дублей перед попыткой сохранения
                    //в реальном проэкте сделал-бы уникальный индекс по табельному номеру для записей котторые в штате
                    //но такую возможность в codefirst не нашел, походу ее нету в дефолтовом ентитифрамеворк - так как не все субд такое умеют
                    .IsUnique(false);
        }
    }
}