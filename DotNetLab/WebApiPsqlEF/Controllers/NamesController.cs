using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApiPsqlEF.Models;

namespace WebApiPsqlEF.Controllers
{
    public class NamesController : ApiController
    {
        private TestDBEntities db = new TestDBEntities();

        // GET: api/Names
        public IQueryable<Name> GetName()
        {
            return db.Name;
        }

        // GET: api/Names/5
        [ResponseType(typeof(Name))]
        public async Task<IHttpActionResult> GetName(int id)
        {
            Name name = await db.Name.FindAsync(id);
            if (name == null)
            {
                return NotFound();
            }

            return Ok(name);
        }

        // PUT: api/Names/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutName(int id, Name name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != name.id)
            {
                return BadRequest();
            }

            db.Entry(name).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NameExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Names
        [ResponseType(typeof(Name))]
        public async Task<IHttpActionResult> PostName(Name name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Name.Add(name);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (NameExists(name.id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = name.id }, name);
        }

        // DELETE: api/Names/5
        [ResponseType(typeof(Name))]
        public async Task<IHttpActionResult> DeleteName(int id)
        {
            Name name = await db.Name.FindAsync(id);
            if (name == null)
            {
                return NotFound();
            }

            db.Name.Remove(name);
            await db.SaveChangesAsync();

            return Ok(name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NameExists(int id)
        {
            return db.Name.Count(e => e.id == id) > 0;
        }
    }
}