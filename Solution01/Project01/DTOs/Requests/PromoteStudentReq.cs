using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.DTOs.Requests
{
    public class PromoteStudentReq
    {
        public string Study { get; set; }
        public int  OldSemester { get; set; }
        public int NewSemester { get; set; }
    }
}
