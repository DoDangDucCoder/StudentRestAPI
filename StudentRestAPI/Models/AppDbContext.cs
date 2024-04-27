using Microsoft.EntityFrameworkCore;
using System;

namespace StudentRestAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Send Students Table
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 1,
                    FirstName = "Dang01",
                    LastName = "Duc01",
                    Email = "dangduc01@gmail.com",
                    DateOfBirth = new DateTime(2001, 1, 17),
                    Gender = Gender.Male,
                    DepartmenId = 1,
                    PhotoPath = "Images/01.png"
                });

            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 2,
                    FirstName = "Dang02",
                    LastName = "Duc02",
                    Email = "dangduc02@gmail.com",
                    DateOfBirth = new DateTime(2001, 2, 17),
                    Gender = Gender.Male,
                    DepartmenId = 4,
                    PhotoPath = "Images/02.png"
                });

            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 3,
                    FirstName = "Dang03",
                    LastName = "Duc03",
                    Email = "dangduc03@gmail.com",
                    DateOfBirth = new DateTime(2001, 3, 17),
                    Gender = Gender.Female,
                    DepartmenId = 2,
                    PhotoPath = "Images/03.png"
                });

            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 4,
                    FirstName = "Dang04",
                    LastName = "Duc04",
                    Email = "dangduc02@gmail.com",
                    DateOfBirth = new DateTime(2001, 4, 17),
                    Gender = Gender.Female,
                    DepartmenId = 3,
                    PhotoPath = "Images/04.png"
                });
        }
    }
}
