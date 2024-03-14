using CRIEAdvanced.Helpers;
using CRIEAdvanced.ViewModels;

namespace CRIEAdvanced.Extensions
{
    public class VerifyLogicExtension
    {
        private readonly IConfiguration _configuration;
        private readonly RsaHelper _rsaHelper;
        private readonly String _aesKey;
        private readonly String _aesIV;

        public VerifyLogicExtension(IConfiguration configuration, RsaHelper rsaHelper)
        {
            this._configuration = configuration;
            this._rsaHelper = rsaHelper;

            this._aesKey = _configuration.GetValue<string>("AesSettings:AesKey");
            this._aesIV = _configuration.GetValue<string>("AesSettings:AesIV");
        }

        public String? SetTokenStr(String val)
        {
            try
            {
                Random random = new();
                var regDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var tempVal = regDate + "_" + val + "_" + random.Next(100);

                var result = _rsaHelper.AesEncryptBase64(tempVal, this._aesKey, this._aesIV);
                if (result != "WrongDataCrypt")
                    return result;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public TempString? CheckTokenStr(String val)
        {
            try
            {
                var result = _rsaHelper.AesDecryptBase64(val, this._aesKey, this._aesIV);
                if (result != "WrongDataCrypt")
                {
                    string[] str = result.Split('_');
                    if (str.Length == 3)
                        return new TempString() { Value = str[1].ToString() };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


        public (String, String) GetTokenInfo(Boolean isAuthenticated, String val1, String val2)
        {
            if (isAuthenticated)
            {
                var userIP = CheckTokenStr(val1);
                var userID = CheckTokenStr(val2);
                if (userIP != null && userID != null)
                {
                    if (userIP.Value.Trim().Length != 0 && userID.Value.Trim().Length != 0)
                        return (userIP.Value, userID.Value);
                }
            }

            return (String.Empty, String.Empty);
        }
    }
}