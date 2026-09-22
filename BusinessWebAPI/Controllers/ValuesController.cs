using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        // REST client used to communicate with Data Web API
        private static readonly RestClient client = new RestClient("http://localhost:5004");

        // GET: api/values
        [HttpGet]
        public async Task<int> Get()
        {
            // Request total number of database records from Data Web API
            RestRequest request = new RestRequest("api/values");

            RestResponse response = await client.ExecuteGetAsync(request);

            // Data Web API returns the number as JSON/text
            int totalEntries = JsonConvert.DeserializeObject<int>(response.Content!);

            return totalEntries;
        }
    }
}
