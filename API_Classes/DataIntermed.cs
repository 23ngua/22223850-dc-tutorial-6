using System;

namespace API_Classes
{
    // Data transferred between the Data API, Business API and GUI
    public class DataIntermed
    {
        public int bal;
        public uint acct;
        public uint pin;
        public string fname;
        public string lname;
        public byte[] profilePicture;
    }
}
