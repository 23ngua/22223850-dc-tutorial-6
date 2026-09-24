using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Classes;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.OpenApi.Models;

namespace BusinessWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        // REST client used to communicate with Data Web API
        private static readonly RestClient client = new RestClient("http://localhost:5004");

        // POST: api/search
        [HttpPost]
        public async Task<IActionResult?> Post(SearchData searchData)
        {
            try
            {
                // Get total number of records from Data Web API
                RestRequest countRequest = new RestRequest("api/values");

                RestResponse countResponse = await client.ExecuteGetAsync(countRequest);

                if (!countResponse.IsSuccessful || string.IsNullOrWhiteSpace(countResponse.Content))
                {
                    throw new Exception("Unable to retrieve the number of records from the Data Web API.");
                }

                int totalEntries = JsonConvert.DeserializeObject<int>(countResponse.Content);

                // Search through Data Web API one surname at a time
                for (int i = 0; i < totalEntries; i++)
                {
                    RestRequest lastNameRequest = new RestRequest("api/lastname/" + i.ToString());

                    RestResponse lastNameResponse = await client.ExecuteGetAsync(lastNameRequest);

                    if (!lastNameResponse.IsSuccessful || string.IsNullOrWhiteSpace(lastNameResponse.Content))
                    {
                        throw new Exception("Unable to retrieve a last name from the Data Web API.");
                    }

                    string? currentLastName = JsonConvert.DeserializeObject<string>(lastNameResponse.Content);

                    // Stop at first matching surname
                    if (currentLastName != null && currentLastName.Equals(searchData.searchStr, StringComparison.OrdinalIgnoreCase))
                    {
                        // Retrieve the complete matching account
                        RestRequest getAllRequest = new RestRequest("api/getall/" + i.ToString());

                        RestResponse getAllResponse = await client.ExecuteGetAsync(getAllRequest);

                        if (string.IsNullOrWhiteSpace(getAllResponse.Content))
                        {
                            throw new Exception("The Data Web API returned an empty account response.");
                        }

                        // If Data Web API returns an error, deserialize and forward its ErrorData
                        if (!getAllResponse.IsSuccessful)
                        {
                            ErrorData? apiError = JsonConvert.DeserializeObject<ErrorData>(getAllResponse.Content);

                            if (apiError != null)
                            {
                                return StatusCode((int)getAllResponse.StatusCode, apiError);
                            }

                            throw new Exception("The Data Web API returned an invalid error response");
                        }

                        DataIntermed? data = JsonConvert.DeserializeObject<DataIntermed>(getAllResponse.Content);

                        if (data == null)
                        {
                            throw new Exception("The accoutn response could not be deserialized.");
                        }

                        return Ok(data);
                    }
                }

                // No surname matched - not an exception
                return Ok((DataIntermed?)null);
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
