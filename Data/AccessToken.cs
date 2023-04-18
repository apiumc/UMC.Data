using System;
using UMC.Data.Entities;

namespace UMC.Data
{
    public class AccessToken : UMC.Security.AccessToken
    {
        public AccessToken() : base(Guid.Empty)
        {

        }
        public AccessToken(Guid d) : base(d)
        {

        }
        public override void Commit(string deviceType, bool unqiue, string clientIP, string server)
        {

            this.ActiveTime = UMC.Data.Utility.TimeSpan();


            if (this.UserId.HasValue)
            {
                var point = clientIP;
                if (String.IsNullOrEmpty(server) == false)
                {
                    point = $"{clientIP}/{server}";
                }

                var sesion = new Session<UMC.Data.AccessToken>(this, this.Device.ToString());

                sesion.Commit(this.UserId.Value, deviceType, unqiue, point);
            }
        }
    }
}

