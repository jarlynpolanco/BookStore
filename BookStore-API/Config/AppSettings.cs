using System.Collections.Generic;

namespace BookStore_API.Config
{
    public class AppSettings
    {
        public List<ConnectionString> ConnectionStrings { get; set; }
        public Jwt Jwt { get; set; }
    }

    public class ConnectionString
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Jwt 
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public double ExpirationTime { get; set; }
    }
}
