using System.Reflection;

namespace DataWebAPI.Models
{
    internal class DatabaseGenerator
    {
        private readonly List<byte[]> profilePictures;
        private readonly Random rand;

        public DatabaseGenerator()
        {
            rand = new Random();
            profilePictures = new List<byte[]>();

            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (string resource in assembly.GetManifestResourceNames())
            {
                if (resource.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    resource.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resource);

                    if (stream != null)
                    {
                        using MemoryStream memoryStream = new MemoryStream();

                        stream.CopyTo(memoryStream);
                        profilePictures.Add(memoryStream.ToArray());
                    }
                }
            }
        }

        private string GetFirstName()
        {
            string[] firstNames =
            {
                "James", "Emily", "Michael", "Sarah", "Daniel",
                "Olivia", "William", "Sophia", "Ethan", "Emma"
            };

            return firstNames[rand.Next(firstNames.Length)];
        }

        private string GetLastname()
        {
            string[] lastNames =
            {
                "Smith", "Johnson", "Williams", "Brown", "Jones",
                "Miller", "Davis", "Wilson", "Taylor", "Anderson"
            };

            return lastNames[rand.Next(lastNames.Length)];
        }

        private uint GetPIN()
        {
            return (uint)rand.Next(1000, 10000);
        }

        private uint GetAcctNo()
        {
            return (uint)rand.Next(10000000, 99999999);
        }

        private int GetBalance()
        {
            return rand.Next(-5000, 100001);
        }

        private byte[] GetProfilePicture()
        {
            return profilePictures[rand.Next(profilePictures.Count)];
        }

        public void GetNextAccount(
            out uint pin,
            out uint acctNo,
            out string firstName,
            out string lastName,
            out int balance,
            out byte[] profilePicture)
        {
            pin = GetPIN();
            acctNo = GetAcctNo();
            firstName = GetFirstName();
            lastName = GetLastname();
            balance = GetBalance();
            profilePicture = GetProfilePicture();
        }
    }
}
