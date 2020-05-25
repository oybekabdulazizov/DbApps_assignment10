using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Project01.DTOs;
using Project01.DTOs.Requests;
using Project01.DTOs.Responses;
using Project01.Helpers;
using Project01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Project01.Services
{
    public class SqlServerDbService : IDbService
    {
        private readonly universityContext iContext;
        public SqlServerDbService(universityContext icontext)
        {
            iContext = icontext;
        }

        #region GetStudents
        public List<GetStudentRes> GetStudents()
        {
            var students = iContext.Student
                                   .Include(s => s.IdEnrollmentNavigation)
                                   .ThenInclude(s => s.IdStudyNavigation)
                                   .Select(student => new GetStudentRes
                                   {
                                       IndexNumber = student.IndexNumber,
                                       FirstName = student.FirstName,
                                       LastName = student.LastName,
                                       BirthDate = student.BirthDate,
                                       Semester = student.IdEnrollmentNavigation.Semester,
                                       Studies = student.IdEnrollmentNavigation.IdStudyNavigation.Name
                                   }).ToList();

            return students;
        }
        #endregion

        #region GetStudent(index)
        public GetStudentRes GetStudent(string index)
        {
            var student = iContext.Student
                                  .Include(s => s.IdEnrollmentNavigation)
                                  .ThenInclude(s => s.IdStudyNavigation)
                                  .Where(s => s.IndexNumber == index)
                                  .Select(student => new GetStudentRes
                                  {
                                      IndexNumber = student.IndexNumber,
                                      FirstName = student.FirstName,
                                      LastName = student.LastName,
                                      BirthDate = student.BirthDate,
                                      Semester = student.IdEnrollmentNavigation.Semester,
                                      Studies = student.IdEnrollmentNavigation.IdStudyNavigation.Name
                                  });
            return new GetStudentRes
            {
                IndexNumber = student.First().IndexNumber,
                FirstName = student.First().FirstName,
                LastName = student.First().LastName,
                BirthDate = student.First().BirthDate,
                Semester = student.First().Semester,
                Studies = student.First().Studies
            };
        }
        #endregion

        #region EnrollStudent(request)
        public EnrollStudentRes EnrollStudent(EnrollStudentReq request)
        {
            int _semester = 1;
            // checks if all the passed parameters are valid 
            if(string.IsNullOrEmpty(request.IndexNumber) || string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) 
                                                         || string.IsNullOrEmpty(request.BirthDate.ToString()) || string.IsNullOrEmpty(request.Studies))
            {
                throw new MyException(MyException.ExceptionType.EmptyParameter, "All the parameters are required!");
            }

            // checks if a student with given index number already exists or not. If exists, then throws MyException
            var student = iContext.Student.Where(s => s.IndexNumber == request.IndexNumber).ToList();
            if (student.Count() != 0)
            {
                throw new MyException(MyException.ExceptionType.BadRequest, "Student with this ID already exists!");
            }
            
            // checks if a passenger study field exits or not. If not, then throws MyException
            var _idStudy = iContext.Studies.Where(i => i.Name == request.Studies).Select(i => i.IdStudy);
            if(_idStudy.Count() == 0)
            {
                throw new MyException(MyException.ExceptionType.NotFound, "This faculty does not exist!");
            }

            // checks if an enrollment exists with given study field and semester 1. If not, then creates a new enrollment 
            int maxIdEnrollment = 0;
            var _idEnrollment = iContext.Enrollment.Where(e => e.IdStudy == _idStudy.First() && e.Semester == 1).Select(e => e.IdEnrollment);
            if (_idEnrollment.Count() == 1)
            {
                maxIdEnrollment = _idEnrollment.First(); 
            }
            else
            {
                // creating a new enrollment and getting its ID 
                maxIdEnrollment = iContext.Enrollment.Max(i => i.IdEnrollment);
                maxIdEnrollment++;
                var newEnrollment = new Enrollment
                {
                    IdEnrollment = maxIdEnrollment,
                    Semester = _semester,
                    IdStudy = _idStudy.First(),
                    StartDate = DateTime.Now
                };
                iContext.Enrollment.Add(newEnrollment);
            }

            // after everything is successful, here we add a new student to semester 1 with given study field 
            var newStudent = new Student
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdEnrollment = maxIdEnrollment
            };

            iContext.Student.Add(newStudent);
            // saviong all the changes we make above 
            iContext.SaveChanges();

            // returning response to the user about the successful operation 
            return new EnrollStudentRes
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Semester = _semester,
                Studies = request.Studies
            };
        }
        #endregion



        // I typed all the C# code here instead of just calling the procedure. 
        // Also, I added some news to this endpoint as well, like you can promote or depromote studens only if a semester you are depromoting to exists. 
        // I will add some news again :)
        // If my code is to much and there is exists some useful compact approach, plase let me know. 
        #region PromoteStudent(index)
        public List<PromoteStudentRes> PromoteStudent(PromoteStudentReq request)
        {
            // check if the passed paramters are not invalid or empty 
            if (string.IsNullOrEmpty(request.Study) || request.OldSemester < 1 || request.NewSemester < 1)
            {
                throw new MyException(MyException.ExceptionType.EmptyParameter, "Please enter valid parameters!");
            }

            // find the IdStudy if the passed study field exists 
            var _idStudy = iContext.Studies.Where(i => i.Name == request.Study)
                                           .Select(i => i.IdStudy);
            if (_idStudy.Count() < 1)
            {
                throw new MyException(MyException.ExceptionType.NotFound, "This faculty field does not exist!");
            }

            // find the IdEnrollment with given IdStudy and OldSemester parameters 
            var _oldSemesterIdEnrollment = iContext.Enrollment
                                                   .Where(e => e.IdStudy == _idStudy.First() && e.Semester == request.OldSemester)
                                                   .Select(i => i.IdEnrollment).First();
            if (_oldSemesterIdEnrollment < 1)
            {
                throw new MyException(MyException.ExceptionType.NotFound,
                                                $"Semester {request.OldSemester} of {request.Study} field does not exist!");
            }

            // find the idEnrollment with given IdStudy and NewSemester parameters 
            var _newSemesterIdEnrollment = iContext.Enrollment
                                                   .Where(e => e.IdStudy == _idStudy.First() && e.Semester == request.NewSemester)
                                                   .Select(e => e.IdEnrollment).First();
            if (_newSemesterIdEnrollment < 1)
            {
                // if the newSemesterIdEnrollment does not exist, we will create a new enrollment for the promotion
                _newSemesterIdEnrollment = iContext.Enrollment.Max(e => e.IdEnrollment);
                _newSemesterIdEnrollment++;
                var newEnrollment = new Enrollment
                {
                    IdEnrollment = _newSemesterIdEnrollment,
                    Semester = request.NewSemester,
                    IdStudy = _idStudy.First(),
                    StartDate = DateTime.Now
                };
                iContext.Enrollment.Add(newEnrollment);
            }

            // ===============================================================================
            // this is list of students who are being promoted. This is used a return response
            List<PromoteStudentRes> studentsPromoted = new List<PromoteStudentRes>();
            // =============================================================================== 

            // getting Ids of all students was in old semester of given study field 
            var studentsIDs = iContext.Student.Where(s => s.IdEnrollment == _oldSemesterIdEnrollment)
                                              .Select(s => s.IndexNumber).ToList();

            // let's find those students from our DbSet<Student> and promote them 
            foreach (var student in iContext.Student)
            {
                for (int i = 0; i < studentsIDs.Count(); i++)
                {
                    if (student.IndexNumber == studentsIDs.ElementAt(i))
                    {
                        studentsPromoted.Add(new PromoteStudentRes
                        {
                            FirstName = student.FirstName,
                            LastName = student.LastName,
                            Study = request.Study,
                            OldSemester = request.OldSemester,
                            NewSemester = request.NewSemester
                        });
                        var res = iContext.Database
                                            .ExecuteSqlRaw(@$"UPDATE Student SET IdEnrollment={_newSemesterIdEnrollment} 
                                                              WHERE IndexNumber = '{student.IndexNumber}';");
                    }
                }
            }

            iContext.SaveChanges();
            return studentsPromoted;

            // var userType = iContext.Database.ExecuteSqlRaw($"EXEC PromoteStudents @study = {request.Study}, @semester = {request.OldSemester}");

        }
        #endregion

        #region DeleteStudent(index)
        public void DeleteStudent(string index)
        {
            if (string.IsNullOrEmpty(index))
            {
                throw new MyException(MyException.ExceptionType.EmptyParameter, "Please enter a valid parameter!");
            }

            var studentsIdList = iContext.Student.Select(s => s.IndexNumber).ToList();
            if (!studentsIdList.Contains(index))
            {
                throw new MyException(MyException.ExceptionType.NotFound,
                                      "Student with this ID does not exist. Thus, removal was not successful!");
            }

            /*var student = iContext.Student.Find(index);
            iContext.Student.Remove(student);
            iContext.SaveChanges();*/

            var student = new Student
            {
                IndexNumber = index
            };
            iContext.Attach(student);
            // iContext.Student.Remove(student);
            iContext.Entry(student).State = EntityState.Deleted;
            iContext.SaveChanges();
        }
        #endregion

        #region UpdateStudentData(index, request)
        public GetStudentRes UpdateStudentData(string index, UpdateStudentDataReq request)
        {
            var studentIds = iContext.Student.Select(s => s.IndexNumber);
            
            if (!studentIds.Contains(index))
            {
                throw new MyException(MyException.ExceptionType.NotFound, 
                                            "Student with this ID does not exist. Thus, you cannot update any data");
            }


            var student = new Student
            {
                IndexNumber = index,
                FirstName = request.FirstName, 
                LastName = request.LastName
            };
            iContext.Attach(student);
            if(request.FirstName != null)
            {
                iContext.Entry(student).Property("FirstName").IsModified = true;
            }
            if(request.LastName != null)
            {
                iContext.Entry(student).Property("LastName").IsModified = true;
            }
            iContext.SaveChanges();

            return GetStudent(index);
        }
        #endregion
    }
}
