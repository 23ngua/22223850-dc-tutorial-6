using Microsoft.AspNetCore.Mvc;
using API_Classes;
using Newtonsoft.Json;
using RestSharp;

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
        public async Task<DataIntermed?> Post(SearchData searchData)
        {
            // Get total number of records from Data Web API
            RestRequest countRequest = new RestRequest("api/values");

            RestResponse countResponse = await client.ExecuteGetAsync(countRequest);

            int totalEntries = JsonConvert.DeserializeObject<int>(countResponse.Content!);

            // Search through Data Web API one surname at a time
            for (int i = 0; i < totalEntries; i++)
            {
                RestRequest lastNameRequest = new RestRequest("api/lastname/" + i.ToString());

                RestResponse lastNameResponse = await client.ExecuteGetAsync(lastNameRequest);

                string? currentLastName = JsonConvert.DeserializeObject<string>(lastNameResponse.Content!);

                // Stop at first matching surname
                if (currentLastName != null && currentLastName.Equals(searchData.searchStr, StringComparison.OrdinalIgnoreCase))
                {
                    // Retrieve the complete matching account
                    RestRequest getAllRequest = new RestRequest("api/getall/" + i.ToString());

                    RestResponse getAllResponse = await client.ExecuteGetAsync(getAllRequest);

                    DataIntermed? data = JsonConvert.DeserializeObject<DataIntermed>(getAllResponse.Content!);

                    return data;
                }
            }

            // No surname matched
            return null;
        }
    }
}
