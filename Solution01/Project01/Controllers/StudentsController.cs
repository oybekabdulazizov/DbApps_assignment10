using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project01.DTOs;
using Project01.DTOs.Requests;
using Project01.Helpers;
using Project01.Models;
using Project01.Services;

namespace Project01.Controllers
{
    [Route("api")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService iService;
        public StudentsController(IDbService iservice)
        {
            iService = iservice;
        }

        [HttpGet("students")]
        public IActionResult GetStudents()
        {
            var students = iService.GetStudents();
            if (students.Count == 0)
            {
                return Ok("No record!");
            }
            return Ok(students);
        }


        [HttpGet("student/{index}")]
        public IActionResult GetStudent(string index)
        {
            var student = iService.GetStudent(index);
            if (student == null)
            {
                return NotFound($"Student with ID {index} does not exist!");
            }
            return Ok(student);
        }


        // this enrollStudent endpoint can also be used as AddStudent 
        // because in both ways, we are adding a new student to our tabl and also IDEnrollment is NOT NULL attribute 
        // That is why, I did not write a code for AddStudent
        [HttpPost("enrollStudent")]
        public IActionResult EnrollStudent(EnrollStudentReq request)
        {
            try
            {
                var enrollment = iService.EnrollStudent(request);
                return Ok(enrollment);
            }
            catch (MyException ex)
            {
                if (ex.Type == MyException.ExceptionType.NotFound)
                {
                    return NotFound("This study does not exist!");
                }
                else if (ex.Type == MyException.ExceptionType.BadRequest)
                {
                    return BadRequest("Student with this ID already exists!");
                }
                else if (ex.Type == MyException.ExceptionType.EmptyParameter)
                {
                    return BadRequest("Please provice all the parameters!");
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }


        [HttpPut("updateStudent/{index}")]
        public IActionResult UpdateStudetData(string index, [FromBody]UpdateStudentDataReq request)
        {
            try
            {
                var modifiedStudent = iService.UpdateStudentData(index, request);
                return Ok(modifiedStudent);
            }
            catch (MyException ex)
            {
                if (ex.Type == MyException.ExceptionType.BadRequest)
                {
                    return BadRequest(ex.Message);
                }
                else if (ex.Type == MyException.ExceptionType.NotFound)
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }


        [HttpDelete("removeStudent/{index}")]
        public IActionResult DeleteStudent(string index)
        {
            try
            {
                iService.DeleteStudent(index);
                return Ok($"Student with ID {index} has been deleted!");
            }
            catch (MyException ex)
            {
                if (ex.Type == MyException.ExceptionType.EmptyParameter)
                {
                    return BadRequest(ex.Message);
                }
                else if (ex.Type == MyException.ExceptionType.NotFound)
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }


        [HttpPost("promoteStudents")]
        public IActionResult PromoteStudents(PromoteStudentReq request)
        {
            try
            {
                return Ok(iService.PromoteStudent(request));
            }
            catch (MyException ex)
            {
                if(ex.Type == MyException.ExceptionType.BadRequest)
                {
                    return BadRequest(ex.Message);
                }
                else if(ex.Type == MyException.ExceptionType.EmptyParameter)
                {
                    return BadRequest(ex.Message);
                }
                else if(ex.Type == MyException.ExceptionType.NotFound)
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }
    }
}