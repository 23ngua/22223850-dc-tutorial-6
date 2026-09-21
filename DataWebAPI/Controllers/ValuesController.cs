using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataWebAPI.Models;

namespace DataWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        // Shared singleton database instance
        private readonly DatabaseClass database = DatabaseClass.Instance;

        // GET: api/values
        [HttpGet]
        public int Get()
        {
            return database.GetNumRecords();
        }
    }
}
