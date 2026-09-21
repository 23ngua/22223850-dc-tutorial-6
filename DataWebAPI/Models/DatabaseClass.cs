namespace DataWebAPI.Models
{
    // Stores the generated account records for Data Web API
    // Singleton ensures the 100,000-record database is created only once
    internal class DatabaseClass
    {
        // One shared database instance for lifetime of applicaiton
        public static DatabaseClass Instance { get; } = new DatabaseClass();

        private readonly List<DataStruct> dataStruct;

        // Private constructor prevents other DatabaseClass instances being created
        private DatabaseClass()
        {
            dataStruct = new List<DataStruct>();

            DatabaseGenerator generator = new DatabaseGenerator();

            for (int i = 0; i < 100000; i++)
            {
                DataStruct record = new DataStruct();

                generator.GetNextAccount(
                    out record.pin,
                    out record.acctNo,
                    out record.firstName,
                    out record.lastName,
                    out record.balance,
                    out record.profilePicture);

                dataStruct.Add(record);
            }
        }

        public int GetNumRecords()
        {
            return dataStruct.Count;
        }

        public uint GetAcctNoByIndex(int index)
        {
            return dataStruct[index].acctNo;
        }

        public uint GetPINByIndex(int index)
        {
            return dataStruct[index].pin;
        }

        public int GetBalanceByIndex(int index)
        {
            return dataStruct[index].balance;
        }

        public string GetFirstNameByIndex(int index)
        {
            return dataStruct[index].firstName;
        }

        public string GetLastNameByIndex(int index)
        {
            return dataStruct[index].lastName;
        }

        public byte[] GetProfilePictureByIndex(int index)
        {
            return dataStruct[index].profilePicture;
        }
    }
}
