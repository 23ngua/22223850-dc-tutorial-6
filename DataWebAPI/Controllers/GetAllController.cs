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
        public DataIntermed Get(int index)
        {
            DataIntermed data = new DataIntermed();

            data.acct = database.GetAcctNoByIndex(index);
            data.pin = database.GetPINByIndex(index);
            data.bal = database.GetBalanceByIndex(index);
            data.fname = database.GetFirstNameByIndex(index);
            data.lname = database.GetLastNameByIndex(index);
            data.profilePicture = database.GetProfilePictureByIndex(index);

            return data;
        }
    }
}
