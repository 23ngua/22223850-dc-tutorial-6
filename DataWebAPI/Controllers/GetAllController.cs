using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Classes;
using DataWebAPI.Models;

namespace DataWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetAllController : ControllerBase
    {
        // Shared singleton database instance
        private readonly DatabaseClass database = DatabaseClass.Instance;

        // GET: api/getall/2
        [HttpGet("{index}")]
        public IActionResult Get(int index)
        {
            try
            {
                // Ensure requested database index is valid
                if (index < 0 || index >= database.GetNumRecords())
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "The requested account index is outside the valid range.");
                }

                DataIntermed data = new DataIntermed();

                data.acct = database.GetAcctNoByIndex(index);
                data.pin = database.GetPINByIndex(index);
                data.bal = database.GetBalanceByIndex(index);
                data.fname = database.GetFirstNameByIndex(index);
                data.lname = database.GetLastNameByIndex(index);
                data.profilePicture = database.GetProfilePictureByIndex(index);

                return Ok(data);
            }
            catch (Exception ex)
            {
                // Convert exception information into JSON-safe data
                ErrorData error = new ErrorData();

                error.exceptionType = ex.GetType().Name;
                error.message = ex.Message;

                return BadRequest(error);
            }
        }
    }
}
