using Amazon.S3;
using Amazon.S3.Model;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using System.Windows;

namespace IDriveView.CreateClient
{
    internal class S3ClientFactory
    {
        private static IAmazonS3 _s3Client;

        // --- Вход в облако по данным входа и Получение клиента для работы с облаком IDrive ----------------------------
        // клиен Singelton
        public static (bool Success, string Message, IAmazonS3 s3Client) GetS3Client()
        {
            try
            {
                if (_s3Client == null)
                {
                    var userIDrive = StaticVariables.IDriveOnDuty;

                    var Key = Properties.Settings.Default.KeyCrypt;
                    var Password = ConfigurationAES.Decrypt(userIDrive.Password, Key);

                    var config = new AmazonS3Config
                    {
                        ServiceURL = "https://" + ConfigurationAES.Decrypt(userIDrive.Endpoint, Password),
                        ForcePathStyle = true,
                        //AuthenticationRegion = "London-2",
                        SignatureVersion = "2",
                        Timeout = TimeSpan.FromSeconds(60), // Увеличьте таймаут (по умолчанию может быть 100)
                        ReadWriteTimeout = TimeSpan.FromSeconds(60),
                        MaxErrorRetry = 5 // Ограничение попыток повтора (было 2)
                    };

                    _s3Client = new AmazonS3Client(
                        ConfigurationAES.Decrypt(userIDrive.AccessKeyID, Password),
                        ConfigurationAES.Decrypt(userIDrive.SecretAccessKey, Password),
                        config
                    );
                }

                return (true, "Success", _s3Client);
            }
            catch (AmazonS3Exception ex)
            {
                _s3Client = null;
                return (false, $"Ошибка S33: {ex.Message} (Код: {ex.ErrorCode})", null);
            }
            catch (TimeoutException)
            {
                _s3Client = null;
                return (false, "Превышено время ожидания ответа от сервера", null);
            }
            catch (Exception ex)
            {
                _s3Client = null;
                return (false, $"Неизвестная ошибка при подключении: {ex.Message}", null);
            }
        }

        // --- Проверка правильности данных входа в облако через получение Buckets (применяется только при входе в аккаунт) ------------------------
        public static async Task<(bool Success, string Message)> CheckLoginS3ClientAsync()
        {
            _s3Client = null;

            // проверяем подключение
            var result = GetS3Client();
            // получаем клиента
            if (result.Success)
            {
                _s3Client = result.s3Client;
            }
            else
            {
                return (false, result.Message);
            }

            // Получаем список бакетов из облака за 20 секунд

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(20));
            var bucketTask = new BucketRequest().GetListBucketsAsync();

            // Ожидаем либо завершения bucketTask, либо таймаута
            var completedTask = await Task.WhenAny(bucketTask, timeoutTask);

            // Проверяем, сработал ли таймаут
            if (completedTask == timeoutTask)
            {
                return (false, "Нет доступа к облаку");
            }

            // Если таймаут не сработал, получаем результат bucketTask
            var response = await bucketTask;
            if (response.listBuckets != null)
            {
                return (true, "Подключение к облаку успешно установлено");
            }
            else
            {
                return (false, response.Message);
            }
        }
    }
}
