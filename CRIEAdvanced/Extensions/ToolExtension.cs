using System.Text.RegularExpressions;

namespace CRIEAdvanced.Extensions
{
    public partial class ToolExtension
    {
        private readonly IConfiguration _configuration;

        public ToolExtension(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public String GetUserIP(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6)
                {
                    remoteIp = remoteIp.MapToIPv4();
                }

                return remoteIp.ToString();
            }

            return String.Empty;
        }

        public Boolean IsNum(String val)
        {
            if (val.All(char.IsDigit))
                return true;

            return false;
        }

        public Boolean IsNumEG(String val)
        {
            Regex result = new Regex("[^A-Za-z0-9]");
            return !result.IsMatch(val);
        }
    }
}
