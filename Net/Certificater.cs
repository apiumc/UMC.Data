using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace UMC.Net
{




    public class Certificater
    {

        public static Dictionary<string, Certificater> Certificates
        {
            get
            {
                return _certificates;
            }
        }
        static Dictionary<string, Certificater> _certificates = new Dictionary<string, Certificater>();

        public string Name
        {
            get; set;
        }
        public int Status
        {
            get; set;
        }
        public X509Certificate Certificate
        {
            get; set;
        }
        public int Time
        {
            get; set;
        }
    }
}