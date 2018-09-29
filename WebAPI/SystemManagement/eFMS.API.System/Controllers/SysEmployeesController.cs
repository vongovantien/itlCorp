using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysEmployeesController : ControllerBase
    {
        private readonly DNTDataContext _context;

        public SysEmployeesController(DNTDataContext context)
        {
            _context = context;
        }

        // GET: api/SysEmployees
        [HttpGet]
        public IEnumerable<SysEmployee> GetSysEmployee()
        {
            return _context.SysEmployee;
        }

        // GET: api/SysEmployees/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSysEmployee([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sysEmployee = await _context.SysEmployee.FindAsync(id);

            if (sysEmployee == null)
            {
                return NotFound();
            }

            return Ok(sysEmployee);
        }

        // PUT: api/SysEmployees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSysEmployee([FromRoute] string id, [FromBody] SysEmployee sysEmployee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sysEmployee.Id)
            {
                return BadRequest();
            }

            _context.Entry(sysEmployee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysEmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SysEmployees
        [HttpPost]
        public async Task<IActionResult> PostSysEmployee([FromBody] SysEmployee sysEmployee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SysEmployee.Add(sysEmployee);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SysEmployeeExists(sysEmployee.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSysEmployee", new { id = sysEmployee.Id }, sysEmployee);
        }

        // DELETE: api/SysEmployees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSysEmployee([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sysEmployee = await _context.SysEmployee.FindAsync(id);
            if (sysEmployee == null)
            {
                return NotFound();
            }

            _context.SysEmployee.Remove(sysEmployee);
            await _context.SaveChangesAsync();

            return Ok(sysEmployee);
        }

        private bool SysEmployeeExists(string id)
        {
            return _context.SysEmployee.Any(e => e.Id == id);
        }
    }
}