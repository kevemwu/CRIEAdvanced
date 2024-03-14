using System.Security.Cryptography;
using System.Text;

namespace CRIEAdvanced.Helpers
{
    public class RsaHelper
    {
        public String RsaEncrypt(String publicKey, String content)
        {
            string encrypt;
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(publicKey);
                    encrypt = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(content), true));
                }
            }
            catch
            {
                encrypt = "WrongDataCrypt";
            }
            return encrypt;
        }

        public String RsaDecrypt(String privateKey, String encryptedContent)
        {
            string decrypt;
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKey);
                    decrypt = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedContent), true));
                }
            }
            catch
            {
                decrypt = "WrongDataCrypt";
            }
            return decrypt;
        }

        // .NET Core 使用 AES 加解密的程式碼
        //https://blog.johnwu.cc/article/net-core-aes-cryptography.html

        public string EncryptAES(string text, string key, string iv)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(text);
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            var transform = aes.CreateEncryptor();
            return Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
        }

        public string DecryptAES(string text, string key, string iv)
        {
            var encryptBytes = Convert.FromBase64String(text);
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            var transform = aes.CreateDecryptor();
            return Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));
        }

        //加密
        public String AesEncryptBase64(string text, string aesKey, string aesIV)
        {
            try
            {
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CFB;

                MD5 md5 = MD5.Create();
                SHA256 sha256 = SHA256.Create();
                byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(aesKey));
                byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(aesIV));
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;

                byte[] dataByteArray = Encoding.UTF8.GetBytes(text);
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch
            {
                return "WrongDataCrypt";
            }
        }

        public String AesDecryptBase64(string text, string aesKey, string aesIV)
        {
            try
            {
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CFB;

                MD5 md5 = MD5.Create();
                SHA256 sha256 = SHA256.Create();
                byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(aesKey));
                byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(aesIV));
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;

                byte[] dataByteArray = Convert.FromBase64String(text);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                return "WrongDataCrypt";
            }
        }
    }
}
