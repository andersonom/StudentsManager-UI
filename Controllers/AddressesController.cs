using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManager.Domain.Interfaces.Services;
using StudentsManager.Domain.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; 

namespace StudentsManager.UI.Controllers
{
    public class AddressesController : Controller
    {
        private readonly IAPIClient<Address> _apiClientAddress;

        public AddressesController(IAPIClient<Address> apiClientAddress)
        {
            _apiClientAddress = apiClientAddress;
        }
        

        // GET: Addresses/Create //[Bind("Id,FirstName,Surname,CourseId,Gender,DOB")]
        [HttpPost]
        public IActionResult Create(Student student)
        {
            if (student != null)
                HttpContext.Session.SetString("Student", JsonConvert.SerializeObject(student));

            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateConfirm([Bind("Id,Street,PostalCode,City,State,Country,StudentId")] Address address)
        {
            Student student = null;
            List<Address> listAddr = null;

            String studSession = HttpContext.Session.GetString("Student");

            if (!String.IsNullOrEmpty(studSession))
                student = JsonConvert.DeserializeObject<Student>(studSession);

            if (student != null)
            {
                String lstAddrSession = HttpContext.Session.GetString("ListAddress");

                if (!String.IsNullOrEmpty(lstAddrSession))
                    listAddr = JsonConvert.DeserializeObject<List<Address>>(lstAddrSession);
                else
                    listAddr = new List<Address>(); 

                listAddr.Add(address);

                HttpContext.Session.SetString("ListAddress", JsonConvert.SerializeObject(listAddr));

                HttpContext.Session.SetString("Student", JsonConvert.SerializeObject(student));

                return RedirectToAction("Create", "Students");
            }

            return RedirectToAction("Index", "Students");
        }

        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _apiClientAddress.GetEntityFromAPI($"api/Address/{id}");

            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Street,PostalCode,City,State,Country,StudentId")] Address address)
        {
            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiClientAddress.PutEntityToAPI($"api/Address/{id}", address);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.Id).Result)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Edit), "Students", new { id = address.StudentId });
            }
            return View(address);
        }

        //// GET: Addresses/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var address = await _context.Address
        //        .SingleOrDefaultAsync(m => m.Id == id);
        //    if (address == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(address);
        //}

        //// POST: Addresses/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var address = await _context.Address.SingleOrDefaultAsync(m => m.Id == id);
        //    _context.Address.Remove(address);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private async Task<bool> AddressExists(int id)
        {
            return await _apiClientAddress.GetEntityFromAPI($"api/Address/{id}") != null;
        }
    }
}
