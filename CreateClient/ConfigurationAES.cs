
using IDriveView.Debuging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IDriveView.CreateClient
{
    class ConfigurationAES
    {
        // --- Метод для шифрования текста ---------------------------------------------------------
        public static async Task<string> EncryptAcync(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                byte[] encrypted;

                // Создаем объект AES
                using (Aes aesAlg = Aes.Create())
                {
                    // Устанавливаем ключ
                    aesAlg.Key = GenerateKey(key, aesAlg.KeySize / 8);
                    // Генерируем случайный вектор инициализации
                    aesAlg.GenerateIV();

                    // Создаем шифратор
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Преобразуем текст в байты
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                    // Шифруем асинхронно
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Записываем IV в начало потока асинхронно
                        await msEncrypt.WriteAsync(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            // Асинхронная запись данных
                            await csEncrypt.WriteAsync(plainBytes, 0, plainBytes.Length);
                            csEncrypt.FlushFinalBlock(); // Завершаем шифрование (синхронный вызов, но можно оставить)
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                // Возвращаем зашифрованные данные в формате Base64
                return Convert.ToBase64String(encrypted);
            }
            catch (EncoderFallbackException ex)
            {
                throw new ArgumentException("Ошибка кодирования входного текста в UTF-8.", nameof(plainText), ex);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("Ошибка при шифровании данных.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Неизвестная ошибка при шифровании.", ex);
            }
        }
        //public static string Encrypt(string plainText, string key)
        //{
        //    if (string.IsNullOrEmpty(plainText))
        //        throw new ArgumentNullException(nameof(plainText));
        //    if (string.IsNullOrEmpty(key))
        //        throw new ArgumentNullException(nameof(key));

        //    byte[] encrypted;

        //    // Создаем объект AES
        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        // Устанавливаем ключ (должен быть 16, 24 или 32 байта для AES-128, AES-192 или AES-256 соответственно)
        //        aesAlg.Key = GenerateKey(key, aesAlg.KeySize / 8);
        //        // Генерируем случайный вектор инициализации
        //        aesAlg.GenerateIV();

        //        // Создаем шифратор
        //        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        //        // Преобразуем текст в байты
        //        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        //        // Шифруем
        //        using (var msEncrypt = new MemoryStream())
        //        {
        //            // Записываем IV в начало потока
        //            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

        //            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        //                csEncrypt.FlushFinalBlock();
        //            }
        //            encrypted = msEncrypt.ToArray();
        //        }
        //    }

        //    // Возвращаем зашифрованные данные в формате Base64
        //    return Convert.ToBase64String(encrypted);
        //}

        // Вспомогательный метод для генерации ключа фиксированной длины
        private static byte[] GenerateKey(string key, int size)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, size);
            return keyBytes;
        }

        // --- Метод для дешифрования текста ---------------------------------------------------------
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = GenerateKey(key, aesAlg.KeySize / 8);

                    byte[] iv = new byte[16];
                    Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                _ = Logger.Log($"Зашифрованная строка не является корректной Base64-строкой. Ошибка: {ex.Message}");
                return string.Empty;
            }
            catch (CryptographicException ex)
            {
                _ = Logger.Log($"Ошибка при расшифровке: данные повреждены или ключ неверный. Ошибка: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _ = Logger.Log($"Неизвестная ошибка при расшифровке. Ошибка: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
