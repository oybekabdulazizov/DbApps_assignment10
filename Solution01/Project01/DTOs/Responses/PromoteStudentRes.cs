using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.DTOs.Responses
{
    public class PromoteStudentRes
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Study { get; set; }
        public int NewSemester { get; set; }
        public int OldSemester { get; set; }
    }
}
