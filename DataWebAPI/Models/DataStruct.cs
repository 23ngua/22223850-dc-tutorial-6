namespace DataWebAPI.Models
{
    // Represents one account record stored in the Data Web API
    internal class DataStruct
    {
        public uint acctNo;
        public uint pin;
        public int balance;
        public string firstName;
        public string lastName;
        public byte[]? profilePicture;

        public DataStruct()
        {
            acctNo = 0;
            pin = 0;
            balance = 0;
            firstName = "";
            lastName = "";
            profilePicture = null;
        }
    }
}
