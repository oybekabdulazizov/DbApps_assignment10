using Project01.DTOs;
using Project01.DTOs.Requests;
using Project01.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.Services
{
    public interface IDbService
    {
        public List<GetStudentRes> GetStudents();
        public GetStudentRes GetStudent(string index);
        public EnrollStudentRes EnrollStudent(EnrollStudentReq request); // same as AddStudent
        public List<PromoteStudentRes> PromoteStudent(PromoteStudentReq request);
        public void DeleteStudent(string index);
        public GetStudentRes UpdateStudentData(string index, UpdateStudentDataReq request);
    }
}
