using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Classes;
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
        public async Task<ActionResult<int>> Get()
        {
            try
            {
                RestRequest request = new RestRequest("api/values");

                RestResponse response = await client.ExecuteGetAsync(request);

                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    throw new Exception("Unable to retrieve the number of records from the Data Web API.");
                }

                if (!response.IsSuccessful)
                {
                    ErrorData? error = JsonConvert.DeserializeObject<ErrorData>(response.Content);

                    if (error != null)
                    {
                        return StatusCode((int)response.StatusCode, error);
                    }

                    throw new Exception("The Data Web API returned an invalid error response.");
                }

                int totalEntries = JsonConvert.DeserializeObject<int>(response.Content);

                return Ok(totalEntries);
            }
            catch (Exception ex)
            {
                ErrorData error = new ErrorData();

                error.exceptionType = ex.GetType().Name;
                error.message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }
    }
}
