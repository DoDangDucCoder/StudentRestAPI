using System;

namespace StudentRestAPI
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public int DepartmenId { get; set; }
        public string PhotoPath { get; set; }
        public DateTime DateOfBirth { get; internal set; }
    }
}
