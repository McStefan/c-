using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace RankedLoyaltyPlugin.Licensing
{
    /// <summary>
    /// Validates plugin license.
    /// </summary>
    public class LicenseValidator
    {
        /// <summary>
        /// Loaded license.
        /// </summary>
        public LicenseModel License { get; private set; }

        /// <summary>
        /// Gets value indicating if license is valid.
        /// </summary>
        public bool IsValid { get; private set; }

        private readonly string _licensePath;

        /// <summary>
        /// Initializes validator.
        /// </summary>
        public LicenseValidator(string licensePath)
        {
            _licensePath = licensePath;
        }

        /// <summary>
        /// Validate license file.
        /// </summary>
        public bool Validate()
        {
            try
            {
                if (!File.Exists(_licensePath))
                    return false;
                var json = File.ReadAllText(_licensePath);
                License = JsonConvert.DeserializeObject<LicenseModel>(json);
                if (License.Expires <= DateTime.UtcNow)
                    return false;
                var data = $"{License.CompanyId}|{License.RegisterId}|{License.Expires:O}";
                var signature = Convert.FromBase64String(License.Signature);
                using (var rsa = RSA.Create())
                {
                    var publicPem = LoadPublicKey();
                    rsa.ImportFromPem(publicPem);
                    IsValid = rsa.VerifyData(Encoding.UTF8.GetBytes(data), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);
                IsValid = false;
            }
            return IsValid;
        }

        private string LoadPublicKey()
        {
            using (var stream = typeof(LicenseValidator).Assembly.GetManifestResourceStream("public.pem"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
