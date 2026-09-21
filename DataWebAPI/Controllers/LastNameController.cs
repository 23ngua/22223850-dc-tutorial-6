using DataWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LastNameController : ControllerBase
    {
        // Shared singleton database instance
        private readonly DatabaseClass database = DatabaseClass.Instance;

        // GET: api/lastname/2
        [HttpGet("{index}")]
        public string Get(int index)
        {
            return database.GetLastNameByIndex(index);
        }
    }
}
