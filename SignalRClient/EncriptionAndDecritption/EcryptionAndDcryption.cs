using System.Security.Cryptography;
using System.Text;

namespace SignalRClient.EncriptionAndDecritption
{
    public static class EcryptionAndDcryption
    {
        public static string EncryptStringWithSHA_OneSidedEncruption(string input) // use the encruption function with key if key is known 
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha.ComputeHash(inputBytes);
                string hashedString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                return hashedString?.ToString().Trim() ?? "";
            }
        }


        public static string EncryptString(string plainText, string encruptionKey)//works only server side
        {
            /*******************for client side *****************************/
            // Add this JavaScript function in your client-side script can 
            //return await JSRuntime.InvokeAsync<string>("encryptString", plainText, encryptionKey); // we can call like this

            //function encryptString(plainText, encryptionKey)
            //{
            //    var encrypted = CryptoJS.AES.encrypt(plainText, encryptionKey);
            //    return encrypted.toString();
            //}
            /*******************for client side *****************************/

            try
            {
                byte[] encruptionKeyBytes = Encoding.UTF8.GetBytes(encruptionKey);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = encruptionKeyBytes;
                    aesAlg.GenerateIV();

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    byte[] encryptedData;

                    using (var msEncrypt = new System.IO.MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        encryptedData = msEncrypt.ToArray();
                    }

                    byte[] ivAndEncryptedData = new byte[aesAlg.IV.Length + encryptedData.Length];
                    Array.Copy(aesAlg.IV, 0, ivAndEncryptedData, 0, aesAlg.IV.Length);
                    Array.Copy(encryptedData, 0, ivAndEncryptedData, aesAlg.IV.Length, encryptedData.Length);
                    return Convert.ToBase64String(ivAndEncryptedData);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static string DecryptString(string cipherText, string encruptionKey)//works only server side
        {
            byte[] encruptionKeyBytes = Encoding.UTF8.GetBytes(encruptionKey);

            byte[] ivAndEncryptedData = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = encruptionKeyBytes;
                byte[] iv = new byte[aesAlg.IV.Length];
                byte[] encryptedData = new byte[ivAndEncryptedData.Length - aesAlg.IV.Length];
                Array.Copy(ivAndEncryptedData, 0, iv, 0, iv.Length);
                Array.Copy(ivAndEncryptedData, iv.Length, encryptedData, 0, encryptedData.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                string decryptedText;

                using (var msDecrypt = new System.IO.MemoryStream(encryptedData))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                {
                    decryptedText = srDecrypt.ReadToEnd();
                }

                return decryptedText;
            }
        }
        public static string EncryptStringWithAES(string input, string encryptionKey)// works only server side
        {
            try
            {
                byte[] encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = encryptionKeyBytes;
                    aesAlg.GenerateIV();

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    byte[] encryptedData;

                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(input);
                            }
                        }

                        encryptedData = msEncrypt.ToArray();
                    }

                    byte[] ivAndEncryptedData = new byte[aesAlg.IV.Length + encryptedData.Length];
                    Array.Copy(aesAlg.IV, 0, ivAndEncryptedData, 0, aesAlg.IV.Length);
                    Array.Copy(encryptedData, 0, ivAndEncryptedData, aesAlg.IV.Length, encryptedData.Length);
                    return Convert.ToBase64String(ivAndEncryptedData);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}

