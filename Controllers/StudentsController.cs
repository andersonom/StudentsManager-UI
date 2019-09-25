using System; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; 
using Newtonsoft.Json;
using StudentManager.Domain.Interfaces.Services;
using StudentsManager.Domain.Models;
using StudentsManager.Domain.Bases;

namespace StudentsManager.UI.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IAPIClient<Student> _apiClientStudent;
        private readonly IAPIClient<Course> _apiClientCourse;
        private readonly IAPIClient<Address> _apiClientAddress;

        private readonly int pageSize = 1;

        private readonly SelectList selectCourses;

        public StudentsController(IAPIClient<Student> apiClient, IAPIClient<Course> apiClientCourse, IAPIClient<Address> apiAddressCourse)
        {
            _apiClientStudent = apiClient;
            _apiClientCourse = apiClientCourse;
            _apiClientAddress = apiAddressCourse;

            selectCourses = new SelectList(_apiClientCourse.GetListFromAPI("api/Course").GetAwaiter().GetResult(), "Id", "Name"); ;
        }

        // GET: Students
        public async Task<IActionResult> Index(int? page)
        { 
            HttpContext.Session.SetString("Student", string.Empty);
            HttpContext.Session.SetString("ListAddress", string.Empty);

            ViewBag.ShowSearch = true;

            PaginatedList<Student> Students = await _apiClientStudent.GetPaginatedListFromAPI($"api/student?pageSize={pageSize}&page={page}");

            if (Students != null)
            {
                ViewBag.List = Students;

                return View(Students);
            }

            return NotFound(); 
        }


        public async Task<IActionResult> Search(string name, int? page) 
        { 
            ViewBag.ShowSearch = true;

            PaginatedList<Student> Students = await _apiClientStudent.GetPaginatedListFromAPI($"api/student/name/{name}?pageSize={pageSize}&page={page}");

            if (Students != null)
            {
                ViewBag.List = Students;

                return View("Index", Students);
            }

            return NotFound();
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Student student = await _apiClientStudent.GetEntityFromAPI($"api/student/{id}");

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewBag.Courses = selectCourses;

            Student student = null;

            String studSession = HttpContext.Session.GetString("Student");

            if(!String.IsNullOrEmpty(studSession))
            student = JsonConvert.DeserializeObject<Student>(studSession);

            String lstAddrSession = HttpContext.Session.GetString("ListAddress");

            if (!String.IsNullOrEmpty(lstAddrSession))
                ViewBag.ListAddress = JsonConvert.DeserializeObject<List<Address>>(lstAddrSession);

            if (student != null)
            {
                return View(student);
            }

            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            String lstAddrSession = HttpContext.Session.GetString("ListAddress");

            if (!String.IsNullOrEmpty(lstAddrSession))
                student.Addresses = JsonConvert.DeserializeObject<List<Address>>(lstAddrSession);

            if (ModelState.IsValid)
            {
                await _apiClientStudent.PostEntityToAPI("api/student/", student);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Student student = null;

            if (id == null)
            {
                return NotFound();
            }
            
            student = await _apiClientStudent.GetEntityFromAPI($"api/student/{id}");

            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Courses = selectCourses;

            return View(student);
        }

         
        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,CourseId,Surname,Gender,DOB")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiClientStudent.PutEntityToAPI($"api/Student/{student.Id}", student);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id).Result)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        //    // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Student student = await _apiClientStudent.GetEntityFromAPI($"api/student/{id}");

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        //    // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClientStudent.DeleteEntityFromAPI($"api/Student/{id}");

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> StudentExists(int id)
        {
            return await _apiClientStudent.GetEntityFromAPI($"api/Student/{id}") != null;
        }
    }
}
