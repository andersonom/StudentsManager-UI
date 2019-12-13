using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManager.Domain.Interfaces.Services;
using StudentsManager.Domain.Models;
using StudentsManager.Domain.Bases;
using Microsoft.Extensions.Caching.Distributed;
using System;
using Newtonsoft.Json;

namespace StudentsManager.Controllers
{
    public class CoursesController : Controller
    {
        private static object syncObject = Guid.NewGuid();
        DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions();

        private readonly IAPIClient<Course> apiClientCourse;
        private readonly IDistributedCache distributeCache;
        private string serializedCoursesCache;
        private readonly int pageSize = 1; 

        public CoursesController(IAPIClient<Course> api, IDistributedCache cache)
        {
            cacheOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
            apiClientCourse = api;
            distributeCache = cache;
        }

        public async Task<PaginatedList<Course>> GetCourses(int? page)
        {
            PaginatedList<Course> courses = null;
            serializedCoursesCache = distributeCache.GetString($"courses{page}");
            if (serializedCoursesCache == null)
            {
                courses = await apiClientCourse.GetPaginatedListFromAPI($"api/Course/Paged?pageSize={pageSize}&page={page}");

                // Pattern Double-checked locking
                // https://en.wikipedia.org/wiki/Double-checked_locking
                lock (syncObject)
                {
                    serializedCoursesCache = distributeCache.GetString("courses");
                    if (serializedCoursesCache == null)
                    {
                        serializedCoursesCache = JsonConvert.SerializeObject(courses);
                        distributeCache.SetString("courses", serializedCoursesCache, cacheOptions);
                    }
                }

            }
            else
            {
                courses = JsonConvert
                    .DeserializeObject<PaginatedList<Course>>(serializedCoursesCache);
            }

            return courses;
        }

        // GET: Courses
        public async Task<IActionResult> Index(int? page)
        {
            ViewBag.ShowSearch = true;

            PaginatedList<Course> courses = await GetCourses(page);

            if (courses != null)
            {
                ViewBag.List = courses;

                return View(courses);
            }

            return NotFound();
        }

        public async Task<IActionResult> Search(string name, int? page)
        {
            ViewBag.ShowSearch = true;

            PaginatedList<Course> courses = await apiClientCourse.GetPaginatedListFromAPI($"api/course/name/{name}?pageSize={pageSize}&page={page}");

            if (courses != null)
            {
                ViewBag.List = courses;

                return View("Index", courses);
            }

            return NotFound();
        }

        //GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Course course = await apiClientCourse.GetEntityFromAPI($"api/course/{id}");

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Course course)
        {
            if (ModelState.IsValid)
            {
                await apiClientCourse.PostEntityToAPI("api/course/", course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Course course = await apiClientCourse.GetEntityFromAPI($"api/course/{id}");

            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await apiClientCourse.PutEntityToAPI($"api/Course/{course.Id}", course);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id).Result)
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
            return View(course);
        }

        // GET: Courses/Delete/5
        // public async Task<IActionResult> Delete(int? id)
        //{
        //if (id == null)
        //{
        //    return NotFound();
        //}

        //var course = await _context.Course
        //    .SingleOrDefaultAsync(m => m.Id == id);
        //if (course == null)
        //{
        //    return NotFound();
        //}

        //return View(course);
        //}

        // POST: Courses/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    //var course = await _context.Course.SingleOrDefaultAsync(m => m.Id == id);
        //    //_context.Course.Remove(course);
        //    //await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private async Task<bool> CourseExists(int id)
        {
            return await apiClientCourse.GetEntityFromAPI($"api/Course/{id}") != null;
        }
    }
}
