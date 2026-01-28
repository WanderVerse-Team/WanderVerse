using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WanderVerse.Framework.Utilities
{
    public static class EncryptionUtils
    {
        //Encrypts a given plain Text
        public static byte[] Encrypt(string plainText, string key, string iv)
        {
            using (Aes aes = Aes.Create())
            {
                //set the keys provided by the server
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        public static string Decrypt(byte[] cipherText, string key, string iv)
        {
            using (Aes aes = Aes.Create())
            {
                //set the keys provided by the server
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}