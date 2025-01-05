using Cysharp.Threading.Tasks;
using System;
using System.Security.Cryptography;
using System.Text;

namespace starinc.io
{
    public class EncryptionManager : BaseManager
    {
        private const string AES_KEY = "aes_key";
        private byte[] _key = null;

        protected override async void OnAwake()
        {
            base.OnAwake();
            await InitializeData();
        }

        private async UniTask InitializeData()
        {
            if (_key != null) return;
            var stringKey = await CallAPI.GetAPIKey(AES_KEY);
            using (SHA256 sha256 = SHA256.Create())
            {
                _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringKey));
            }
        }

        /// <summary>
        /// AES 암호화 함수
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public async UniTask<string> Encrypt(string plainText)
        {
            if(_key == null) await InitializeData();
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                string ivString = Convert.ToBase64String(aes.IV);

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    string encryptedData = Convert.ToBase64String(encryptedBytes);
                    return $"{encryptedData}:{ivString}";
                }
            }
        }

        /// <summary>
        /// AES 복호화 함수
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public async UniTask<string> Decrypt(string encryptedText)
        {
            if (_key == null) await InitializeData();
            var parts = encryptedText.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Invalid encrypted text format");

            string encryptedData = parts[0];
            string ivString = parts[1];

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] iv = Convert.FromBase64String(ivString);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}