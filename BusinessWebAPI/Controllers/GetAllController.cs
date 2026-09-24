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
        public async Task<IActionResult> Get(int index)
        {
            try
            {
                // Request account record from Data Web API
                RestRequest request = new RestRequest("api/getall/" + index.ToString());

                RestResponse response = await client.ExecuteGetAsync(request);

                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    throw new Exception("The Data Web API returned an empty response.");
                }

                if (response.IsSuccessful)
                {
                    DataIntermed? data = JsonConvert.DeserializeObject<DataIntermed>(response.Content);

                    if (data == null)
                    {
                        throw new Exception("The account response could not be deserialized.");
                    }

                    return Ok(data);
                }

                ErrorData? error = JsonConvert.DeserializeObject<ErrorData>(response.Content);

                if (error == null)
                {
                    throw new Exception("The error response from the Data Web API could not be deserialized.");
                }

                return StatusCode((int)response.StatusCode, error);
            }
            catch (Exception ex)
            {
                // Handle failures occurring inside Business Web API
                ErrorData error = new ErrorData();

                error.exceptionType = ex.GetType().Name;
                error.message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }
    }
}
