using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using IDriveView.CreateClient;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using System.Windows;

namespace IDriveView.CreateReqest
{
    internal class BucketRequest
    {
        private IAmazonS3 _s3Client;
        public BucketRequest()
        {
            _s3Client = S3ClientFactory.GetS3Client().s3Client;
        }

        // --- Получить список Buckets ---
        public async Task<(List<S3Bucket> listBuckets, string Message)> GetListBucketsAsync()
        {
            // Создаем CancellationTokenSource с таймаутом, например, 10 секунд
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                if (_s3Client == null)
                {
                    await Logger.Log("S3 клиент не инициализирован");
                    return (null, "S3 клиент не инициализирован");
                }

                var result = await _s3Client.ListBucketsAsync(cts.Token).ConfigureAwait(false);

                if (result == null)
                {
                    await Logger.Log("Получен пустой ответ от сервиса IDrive S3");
                    return (null, "Получен пустой ответ от сервиса IDrive S3");
                }

                if (result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    await Logger.Log($"Ошибка при получении списка бакетов: {result.HttpStatusCode}");
                    return (null, "Ошибка при получении списка бакетов. См логи");
                }

                if (result.Buckets.Count == 0)
                {
                    await Logger.Log("Облако не содержит бакетов. Создайте бакет.");
                    return (null, "Облако не содержит бакетов. Создайте бакет.");
                }

                return (result.Buckets, "Список бакетов получен");
            }
            catch (AmazonS3Exception ex)
            {
                await Logger.Log($"Ошибка S3 сервиса: {ex.Message}, {ex.ErrorCode}");
                return (null, "Ошибка S3 сервиса. См логи");
            }
            catch (AmazonServiceException ex)
            {
                // Общие ошибки сервиса AWS
                return (null, $"Ошибка сервиса AWS: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                // Обработка случая, когда запрос был отменен из-за таймаута
                await Logger.Log("Запрос был отменен из-за таймаута");
                return (null, "Запрос был отменен из-за таймаута");
            }
            catch (Exception ex)
            {
                // проверка соединения с интернетом
                var result = await InternetAvailability.CheckInternet();
                if (result.Success)
                {
                    await Logger.Log($"Не удалось получить список бакетов: {ex.Message}");
                    return (null, $"Не удалось получить список бакетов: {ex.Message}");
                }
                else
                {
                    await Logger.Log($"Получение списка Buckets: {result.Message}");
                    return (null, result.Message);
                }
            }
        }
        // --- Создания Bucket ---
        public async Task<(bool Success, string Message)> CreateBucketAsync(string nameBucket)
        {
            try
            {
                // Проверка входных параметров
                if (string.IsNullOrWhiteSpace(nameBucket))
                {
                    return (false, "Ошибка: Имя bucket не может быть пустым или состоять только из пробелов");
                }

                // Проверка допустимой длины имени (для S3: 3-63 символа)
                if (nameBucket.Length < 3 || nameBucket.Length > 63)
                {
                    return (false, "Ошибка: Имя bucket должно содержать от 3 до 63 символов");
                }

                // Проверка на допустимые символы (S3 принимает только определенные символы)
                if (!System.Text.RegularExpressions.Regex.IsMatch(nameBucket, @"^[a-z0-9][a-z0-9.-]*[a-z0-9]$"))
                {
                    return (false, "Ошибка: Имя bucket содержит недопустимые символы. Используйте только строчные буквы, цифры, точки и дефисы");
                }

                // Отправка запроса
                await _s3Client.PutBucketAsync(nameBucket);

                return (true, "Bucket успешно создан");

            }
            catch (AmazonS3Exception ex)
            {
                // Обработка специфичных ошибок S3
                switch (ex.ErrorCode)
                {
                    case "BucketAlreadyExists":
                        return (false, "Этот Bucket уже существует");
                    case "BucketAlreadyOwnedByYou":
                        return (false, "Этот Bucket уже принадлежит вам");
                    case "InvalidBucketName":
                        return (false, "Ошибка: Неверный формат имени bucket");
                    case "TooManyBuckets":
                        return (false, "Ошибка: Превышен лимит количества buckets");
                    default:
                        return (false, $"Ошибка создания bucket: {ex.Message} (Код ошибки: {ex.ErrorCode})");
                }
            }
            catch (AmazonServiceException ex)
            {
                // Общие ошибки сервиса AWS
                return (false, $"Ошибка сервиса AWS: {ex.Message}");
            }
            catch (Exception ex)
            {
                // проверка соединения с интернетом
                var result = await InternetAvailability.CheckInternet();
                if (result.Success)
                {
                    await Logger.Log($"Не удалось создать Bucket: {ex.Message}");
                    return (false, $"Не удалось создать Bucket: {ex.Message}");
                }
                else
                {
                    await Logger.Log($"Создание Bucket: {result.Message}");
                    return (false, result.Message);
                }
            }
        }

        // --- Удалить Bucket ---
        public async Task DeleteBucketAsync(string nameBacket)
        {
            await _s3Client.DeleteBucketAsync(nameBacket);
        }

        // --- Сделать bucket публичным ---
        public async Task PrincipalBucketAsync(string nameBucket)
        {
            try
            {
                // Создаем политику для публичного доступа
                string publicPolicy = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Principal"": ""*"",
                        ""Action"": ""s3:GetObject"",
                        ""Resource"": ""arn:aws:s3:::{nameBucket}/*""
                    }}
                ]
            }}";

                // Применяем политику к bucket
                var request = new PutBucketPolicyRequest
                {
                    BucketName = nameBucket,
                    Policy = publicPolicy
                };

                await _s3Client.PutBucketPolicyAsync(request);
                MessageBox.Show($"Bucket {nameBucket} успешно сделан публичным.");
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show($"Ошибка при установке публичной политики: {ex.Message}");
                throw;
            }
        }

        // --- Сделать bucket приватным ---
        public async Task PrincipalDeleteBucket(string nameBucket)
        {
            try
            {
                // Удаляем политику bucket, что делает его приватным по умолчанию
                var request = new DeleteBucketPolicyRequest
                {
                    BucketName = nameBucket
                };

                await _s3Client.DeleteBucketPolicyAsync(request);
                MessageBox.Show($"Bucket {nameBucket} успешно сделан приватным (политика удалена).");
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении политики: {ex.Message}");
                throw;
            }
        }

        // --- Проверить текущую политику bucket ---
        public async Task PrincipalGetBucket(string nameBucket)
        {
            try
            {
                // Получаем текущую политику bucket
                var request = new GetBucketPolicyRequest
                {
                    BucketName = nameBucket
                };

                var response = await _s3Client.GetBucketPolicyAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show($"Текущая политика для bucket {nameBucket}:");
                    MessageBox.Show(response.Policy);
                }
                else
                {
                    MessageBox.Show($"Политика для bucket {nameBucket} не найдена (вероятно, bucket приватный).");
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageBox.Show($"Политика для bucket {nameBucket} отсутствует (bucket приватный).");
                }
                else
                {
                    MessageBox.Show($"Ошибка при получении политики: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
