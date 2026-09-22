using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Classes;
using Newtonsoft.Json;
using RestSharp;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetAllController : ControllerBase
    {
        // REST client used to communicate with Data Web API
        private static readonly RestClient client = new RestClient("http://localhost:5004");

        // GET: api/getall/2
        [HttpGet("{index}")]
        public async Task<DataIntermed> Get(int index)
        {
            // Request the complete account record from Data Web API
            RestRequest request = new RestRequest("api/getall/" + index.ToString());

            RestResponse response = await client.ExecuteGetAsync(request);

            // Convert returned JSON back into shared DataIntermed class
            DataIntermed data = JsonConvert.DeserializeObject<DataIntermed>(response.Content!)!;

            return data;
        }
    }
}
