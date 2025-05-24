
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using IDriveView.AddWindows;
using IDriveView.CreateClient;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.WorkClasses;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IDriveView.CreateReqest
{
    internal class ContentRequest
    {
        private IAmazonS3 _s3Client;
        public ContentRequest()
        {
            _s3Client = S3ClientFactory.GetS3Client().s3Client;
        }

        // --- Создать папку -----------------------------------------------------------------------
        public async Task<(bool Success, string Message)> CreateFolderAsync(string nameBucket, string folderName)
        {
            try
            {
                // Проверка входных параметров
                if (string.IsNullOrWhiteSpace(nameBucket))
                    return(false, "Имя бакета не может быть пустым");

                if (string.IsNullOrWhiteSpace(folderName))
                    return (false, "Имя папки не может быть пустым");

                // Убедимся, что folderName заканчивается на '/'
                folderName = folderName.TrimEnd('/') + '/';

                var request = new PutObjectRequest
                {
                    BucketName = nameBucket,
                    Key = folderName,
                    ContentBody = ""
                };

                var response = await _s3Client.PutObjectAsync(request);

                // Проверка успешности операции
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (false, $"Не удалось создать папку. Статус код: {response.HttpStatusCode}");
                }
                return (true, $"Папка: \"{folderName}\" успешно создана.");
            }
            catch (AmazonS3Exception ex)
            {
                // Обработка специфичных для S3 ошибок
                return (false, $"Ошибка S3 при создании папки: {ex.Message}");
            }
            catch (Exception ex)
            {
                // проверка соединения с интернетом
                var result = await InternetAvailability.CheckInternet();
                if (result.Success)
                {
                    await Logger.Log($"Ошибка при создании папки: {ex.Message}");
                    return (false, $"Ошибка при создании папки: {ex.Message}");
                }
                else
                {
                    await Logger.Log($"Создание папки: {result.Message}");
                    return (false, result.Message);
                }
            }
        }

        // --- Удаление папки/файла ------------------------------------------------
        public async Task<(bool Success, string Message)> DeleteFolderOrFileAsync(string nameBucket, string elememtName)
        {
            try
            {
                await _s3Client.DeleteObjectAsync(nameBucket, elememtName);
                return (true, $"Файл/папка {elememtName} успешно удален.");
            }
            catch (AmazonS3Exception s3Ex)
            {
                // Обработка специфичных ошибок S3
                string errorMessage = s3Ex.ErrorCode switch
                {
                    "AccessDenied" => "Ошибка: Нет прав доступа для удаления файла/папки.",
                    "BucketNotFound" => $"Ошибка: Бакет '{nameBucket}' не найден.",
                    "InvalidRequest" => "Ошибка: Некорректный запрос к серверу.",
                    _ => $"Ошибка S3 при удалении: {s3Ex.Message}"
                };
                await Logger.Log($"Обработка специфичных ошибок S3: {errorMessage}");
                return (false, $"Обработка специфичных ошибок S3: {errorMessage}");
            }
            catch (TaskCanceledException)
            {
                await Logger.Log($"Ошибка: Операция удаления была отменена или превышено время ожидания.");
                return (false, "Ошибка: Операция удаления была отменена или превышено время ожидания.");
            }
            catch (Exception ex)
            {
                // проверка соединения с интернетом
                var result = await InternetAvailability.CheckInternet();
                if (result.Success)
                {
                    await Logger.Log($"Неизвестная ошибка при удалении файла/папки: {ex.Message}");
                    return (false, $"Неизвестная ошибка при удалении файла/папки: {ex.Message}");
                }
                else
                {
                    await Logger.Log($"Oшибка при удалении файла/папки: {result.Message}");
                    return (false, result.Message);
                }
            }
        }

        // --- Загрузка файла в бакет / папку с возможностью отмены загрузки -------------------------------------------------
        public async Task UploadFileAsync(string nameBucket, string key, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = nameBucket,
                    Key = key,
                    FilePath = filePath
                };

                var response = await _s3Client.PutObjectAsync(request, cancellationToken);
                MessageBox.Show($"Файл {key} успешно загружен.");
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены операции
                MessageBox.Show("Загрузка файла была отменена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
        public async Task UploadFileAsync2(string key, string filePath)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = Settings.mainBucket,
                    Key = key,
                    FilePath = filePath
                };

                var response = await _s3Client.PutObjectAsync(request);
                MessageBox.Show($"Файл {key} успешно загружен.");
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены операции
                MessageBox.Show("Загрузка файла была отменена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
        // --- Скачивание файла из бакета -------------------------------------------------------
        public async Task DownloadFile(string nameBucket, string key, string destinationPath)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = nameBucket,
                    Key = key
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var fileStream = File.Create(destinationPath))
                {
                    await responseStream.CopyToAsync(fileStream);
                }

                MessageBox.Show($"Файл {key} успешно скачан в {destinationPath}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}");
            }
        }
        // --- Скачивание файла из бакета в память и отображение в TextBox ----------------------------------------------
        public async Task DownloadFileToMemoryAndDisplay(string nameBucket, string key, TextBox textBox)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = nameBucket,
                    Key = key
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var memoryStream = new MemoryStream())
                {
                    await responseStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin); // Сбрасываем позицию в начало потока

                    // Читаем содержимое как текст (предполагая, что файл текстовый)
                    using (var reader = new StreamReader(memoryStream))
                    {
                        string content = await reader.ReadToEndAsync();
                        textBox.Text = content; // Отображаем в TextBox
                    }
                }

                //MessageBox.Show($"Файл {key} успешно загружен и отображен.");
            }
            catch (Exception ex)
            {
                await Logger.Log($"Ошибка при згрузке текстового файла: {ex.Message}");
            }
        }
        // --- Загрузка файла в бакет из оперетивной памяти ------------------------------------------------
        public async Task<bool> UploadTextBoxContentToCloud(string nameBucket, string key, TextBox textBox)
        {
            try
            {
                // Получаем текст из TextBox
                string content = textBox.Text;

                // Преобразуем текст в поток
                var byteArray = Encoding.UTF8.GetBytes(content);
                using var memoryStream = new MemoryStream(byteArray);

                // Создаём запрос на загрузку (замену существующего файла)
                var request = new PutObjectRequest
                {
                    BucketName = nameBucket,
                    Key = key,
                    InputStream = memoryStream,
                    ContentType = "text/plain"
                };

                // Загружаем в облако
                var response = await _s3Client.PutObjectAsync(request);

                //MessageBox.Show($"Файл {key} успешно сохранён в облаке.");

                return true;
            }
            catch (Exception ex)
            {
                await Logger.Log($"Ошибка при сохранении файла в облако: {ex.Message}");
                return false;
            }
        }
        // --- Скачивание файла из бакета с возможностью отмены -------------------------------------------------------
        public async Task DownloadFileAsync(string nameBucket, string key, string destinationPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = nameBucket,
                    Key = key
                };

                using (var response = await _s3Client.GetObjectAsync(request, cancellationToken))
                using (var responseStream = response.ResponseStream)
                using (var fileStream = File.Create(destinationPath))
                {
                    // Проверяем возможность отмены перед копированием
                    cancellationToken.ThrowIfCancellationRequested();

                    // Копируем поток с поддержкой отмены
                    await responseStream.CopyToAsync(fileStream, cancellationToken);

                    // Убеждаемся, что данные записаны на диск
                    await fileStream.FlushAsync(cancellationToken);
                }

                MessageBox.Show($"Файл {key} успешно скачан в {destinationPath}.");
            }
            catch (OperationCanceledException)
            {
                // Удаляем частично загруженный файл при отмене
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        File.Delete(destinationPath);
                    }
                    catch (Exception deleteEx)
                    {
                        MessageBox.Show($"Операция отменена, но возникла ошибка при удалении файла: {deleteEx.Message}");
                    }
                }
                MessageBox.Show("Скачивание файла было отменено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при скачивании файла: {ex.Message}");
            }
        }

        // --- Параллельная загрузка файлов с прогрессом и отменой -----------------------------------------------------------------------------------
        public async Task UploadFilesAsync(string nameBucket, MainWindow mainWindow, Dictionary<string, string> files, IProgress<int> progress, CancellationToken token)
        {
            var semaphore = new SemaphoreSlim(5);// Максимальное количество одновременных загрузок
            var tasks = new List<Task>();
            int completed = 0;
            int totalFiles = files.Count;

            try
            {
                foreach (var file in files)
                {
                    var localPath = file.Key;
                    var s3Key = file.Value;

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync(token); // Учитываем отмену
                            token.ThrowIfCancellationRequested(); // Проверяем отмену перед операцией

                            var request = new PutObjectRequest
                            {
                                BucketName = nameBucket,
                                Key = s3Key,
                                FilePath = localPath
                            };
                            // Если это видео,то получаем и добавляем в метаданные его длительность
                            string metaDuration = Services.GetDutationVideo(localPath);
                            request.Metadata.Add("x-amz-meta-durationVideo", metaDuration);

                            await _s3Client.PutObjectAsync(request, token); // Передаем токен в клиент

                            Interlocked.Increment(ref completed);
                            int percent = (int)((completed / (double)totalFiles) * 100);
                            progress?.Report(percent);

                            _ = mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                mainWindow.textProgressUpload.Text = $"Прогресс: {percent}% ( {completed}  из  {totalFiles})";
                            }));

                            await mainWindow.Dispatcher.BeginInvoke(new Action(async () =>
                            {
                                // Добавляем згруженные bytes в Базу данных
                                await WorkDateBase.AddByteUpload(new FileInfo(localPath).Length);
                            }));

                        }
                        catch (TaskCanceledException)
                        {
                            throw new Exception("Ошибка: Операция удаления была отменена или превышено время ожидания.");
                        }
                        catch (OperationCanceledException)
                        {
                            throw new Exception("\nОперация была отменена.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"\nОшибка загрузки: {s3Key}: {ex.Message}");
                            throw new Exception($"\nОшибка загрузки: {s3Key}: {ex.Message}");
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, token));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // Ловим исключения от всех задач
                foreach (var task in tasks.Where(t => t.IsFaulted))
                {
                    if (task.Exception?.InnerException is OperationCanceledException)
                    {
                        throw new OperationCanceledException("Загрузка была отменена.", task.Exception);
                    }
                    else
                    {
                        throw task.Exception ?? new Exception("Неизвестная ошибка при загрузке.");
                    }
                }
            }
            finally
            {
                semaphore.Dispose();
            }
        }
        #region Параллельное скачивание файлов с возможностью отмены 1
        // --- Параллельное скачивание файлов с возможностью отмены -------- иногда выдаёт ошибки при скачивании, следующий метод доработан с повторными попытками -----------
        //public async Task DownloadFilesAsync(string nameBucket, Dictionary<string, string> files, MainWindow mainWindow, IProgress<int> progress, CancellationToken token)
        //{
        //    var semaphore = new SemaphoreSlim(5);// Максимальное количество одновременных скачиваний
        //    var tasks = new List<Task>();
        //    int completed = 0;
        //    int totalFiles = files.Count;

        //    try
        //    {
        //        foreach (var file in files)
        //        {
        //            var s3Key = file.Key;
        //            var localPath = file.Value;

        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await Task.Delay(100);
        //                await semaphore.WaitAsync(token); // Учитываем отмену
        //                try
        //                {
        //                    token.ThrowIfCancellationRequested(); // Проверяем отмену перед операцией

        //                    var request = new GetObjectRequest
        //                    {
        //                        BucketName = nameBucket,
        //                        Key = s3Key
        //                    };

        //                    using (var response = await _s3Client.GetObjectAsync(request, token))
        //                    using (var responseStream = response.ResponseStream)
        //                    using (var fileStream = File.Create(localPath))
        //                    {
        //                        await responseStream.CopyToAsync(fileStream, token);
        //                    }

        //                    // Обновляем прогресс
        //                    Interlocked.Increment(ref completed);
        //                    int percent = (int)((completed / (double)totalFiles) * 100);
        //                    progress?.Report(percent);

        //                    _ = mainWindow.Dispatcher.BeginInvoke(new Action(() =>
        //                    {
        //                        mainWindow.textProgressUpload.Text = $"Прогресс: {percent}% ( {completed}  из  {totalFiles} )";
        //                    }));
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    throw new Exception("\nОперация была отменена.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"\nОшибка скачивания {s3Key}: {ex.Message}");
        //                    throw new Exception($"\nОшибка скачивания {s3Key}: {ex.Message}");
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                }
        //            }, token));
        //        }

        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception)
        //    {
        //        // Ловим исключения от всех задач
        //        foreach (var task in tasks.Where(t => t.IsFaulted))
        //        {
        //            if (task.Exception?.InnerException is OperationCanceledException)
        //            {
        //                throw new OperationCanceledException("Скачивание было отменено.", task.Exception);
        //            }
        //            else
        //            {
        //                throw task.Exception ?? new Exception("Неизвестная ошибка при скачивании.");
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        semaphore.Dispose();
        //    }
        //}
        #endregion
        #region Параллельное скачивание файлов с возможностью отмены 2
        // --- Параллельное скачивание файлов с возможностью отмены (с повтором, при неудачном скачивании) ---------------------------------------------
        //public async Task DownloadFilesAsync(string nameBucket, Dictionary<string, string> files, MainWindow mainWindow, IProgress<int> progress, CancellationToken token)
        //{
        //    var semaphore = new SemaphoreSlim(5);// Максимальное количество одновременных скачиваний
        //    var tasks = new List<Task>();
        //    int completed = 0;
        //    int totalFiles = files.Count;

        //    try
        //    {
        //        foreach (var file in files)
        //        {
        //            var s3Key = file.Key;
        //            var localPath = file.Value;

        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await Task.Delay(50);
        //                await semaphore.WaitAsync(token); // Учитываем отмену
        //                try
        //                {
        //                    token.ThrowIfCancellationRequested(); // Проверяем отмену перед операцией

        //                    var request = new GetObjectRequest
        //                    {
        //                        BucketName = nameBucket,
        //                        Key = s3Key
        //                    };

        //                    // Добавляем логику повторных попыток
        //                    int maxRetries = 2; // Максимум 2 повторные попытки
        //                    int retryCount = 0;
        //                    bool success = false;
        //                    Exception lastException = null;

        //                    while (retryCount <= maxRetries && !success)
        //                    {
        //                        try
        //                        {
        //                            using (var response = await _s3Client.GetObjectAsync(request, token))
        //                            using (var responseStream = response.ResponseStream)
        //                            using (var fileStream = File.Create(localPath))
        //                            {
        //                                await responseStream.CopyToAsync(fileStream, token);
        //                            }
        //                            success = true; // Если скачивание прошло успешно, выходим из цикла
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            retryCount++;
        //                            lastException = ex;

        //                            if (retryCount > maxRetries)
        //                            {
        //                                // Если превысили количество попыток, бросаем исключение
        //                                throw new Exception($"Не удалось скачать {s3Key} после {maxRetries + 1} попыток: {ex.Message}", ex);
        //                            }

        //                            // Логируем попытку
        //                            await Logger.Log($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");
        //                            Output.WriteLine($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");

        //                            // Задержка перед следующей попыткой (увеличиваем с каждой попыткой)
        //                            await Task.Delay(1000 * retryCount, token);
        //                        }
        //                    }

        //                    // Обновляем прогресс
        //                    Interlocked.Increment(ref completed);
        //                    int percent = (int)((completed / (double)totalFiles) * 100);
        //                    progress?.Report(percent);

        //                    _ = mainWindow.Dispatcher.BeginInvoke(new Action(() =>
        //                    {
        //                        mainWindow.textProgressUpload.Text = $"Прогресс: {percent}% ( {completed}  из  {totalFiles} )";
        //                    }));
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    throw new Exception("\nОперация была отменена.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"\nОшибка скачивания {s3Key}: {ex.Message}");
        //                    throw new Exception($"\nОшибка скачивания {s3Key}: {ex.Message}");
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                }
        //            }, token));
        //        }

        //        await Task.WhenAll(tasks);
        //    }
        //    catch (Exception)
        //    {
        //        // Ловим исключения от всех задач
        //        foreach (var task in tasks.Where(t => t.IsFaulted))
        //        {
        //            if (task.Exception?.InnerException is OperationCanceledException)
        //            {
        //                throw new OperationCanceledException("Скачивание было отменено.", task.Exception);
        //            }
        //            else
        //            {
        //                throw task.Exception ?? new Exception("Неизвестная ошибка при скачивании.");
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        semaphore.Dispose();
        //    }
        //}
        #endregion
        #region Параллельное скачивание файлов с возможностью отмены 3
        //// --- Параллельное скачивание файлов с возможностью отмены 3 (с повтором, при неудачном скачивании и пропуске неудачных файлов) -----------------------
        /// не отлавливает исключение отмены скачивания
        //public async Task DownloadFilesAsync(string nameBucket, Dictionary<string, string> files, MainWindow mainWindow, IProgress<int> progress, CancellationToken token, bool preview = false)
        //{
        //    var semaphore = new SemaphoreSlim(5);
        //    var tasks = new List<Task<bool>>(); // Изменяем тип задач на Task<bool>, чтобы возвращать статус скачивания
        //    int completed = 0;
        //    int totalFiles = files.Count;
        //    var failedFiles = new List<string>(); // Список для хранения файлов, которые не удалось скачать
        //    var successFiles = new List<string>(); // Список для хранения файлов, которые удалось скачать

        //    try
        //    {
        //        foreach (var file in files)
        //        {
        //            var s3Key = file.Key;
        //            var localPath = file.Value;
        //            Output.WriteLine(localPath);
        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await semaphore.WaitAsync(token); // Учитываем отмену
        //                try
        //                {
        //                    token.ThrowIfCancellationRequested(); // Проверяем отмену перед операцией

        //                    var request = new GetObjectRequest
        //                    {
        //                        BucketName = nameBucket,
        //                        Key = s3Key
        //                    };

        //                    // Логика повторных попыток
        //                    int maxRetries = 4; // Максимум 4 повторные попытки
        //                    int retryCount = 0;
        //                    bool success = false;

        //                    while (retryCount <= maxRetries && !success)
        //                    {
        //                        try
        //                        {
        //                            using (var response = await _s3Client.GetObjectAsync(request, token))
        //                            using (var responseStream = response.ResponseStream)
        //                            using (var fileStream = File.Create(localPath))
        //                            {
        //                                await responseStream.CopyToAsync(fileStream, token);
        //                            }
        //                            success = true; // Скачивание прошло успешно
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            retryCount++;
        //                            if (retryCount > maxRetries)
        //                            {
        //                                // Если превысили количество попыток, логируем и возвращаем false
        //                                await Logger.Log($"Не удалось скачать {s3Key} после {maxRetries + 1} попыток: {ex.Message}");
        //                                Output.WriteLine($"Не удалось скачать {s3Key} после {maxRetries + 1} попыток: {ex.Message}");
        //                                return false; // Файл не скачан
        //                            }

        //                            await Logger.Log($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");
        //                            Output.WriteLine($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");
        //                            await Task.Delay(1000 * retryCount, token);
        //                        }
        //                    }

        //                    if (success)
        //                    {
        //                        // Обновляем прогресс только для успешно скачанных файлов
        //                        Interlocked.Increment(ref completed);
        //                        int percent = (int)((completed / (double)totalFiles) * 100);
        //                        progress?.Report(percent);

        //                        _ = mainWindow.Dispatcher.BeginInvoke(new Action(() =>
        //                        {
        //                            mainWindow.textProgressUpload.Text = $"Прогресс: {percent}% ( {completed}  из  {totalFiles} ), в очереди {StaticVariables.queueDownloadFiles.Count}";
        //                        }));

        //                        // заполнение превью картинками
        //                        if (preview)
        //                        {
        //                            _ = mainWindow.Dispatcher.BeginInvoke(new Action(async () =>
        //                            {
        //                                _ = new DownUpContent().FillElemWindowPictures(file.Key);
        //                            }));
        //                        }

        //                        successFiles.Add(localPath); // Список для хранения файлов, которые удалось скачать
        //                    }

        //                    return success; // Возвращаем true, если файл успешно скачан
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    throw new Exception("\nОперация была отменена.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception($"\nОшибка скачивания {s3Key}: {ex.Message}");
        //                }
        //                finally
        //                {
        //                    semaphore.Release();
        //                }
        //            }, token));
        //        }

        //        // Ждём завершения всех задач
        //        var results = await Task.WhenAll(tasks);

        //        // Собираем список файлов, которые не удалось скачать
        //        failedFiles = files.Keys
        //            .Where((key, index) => !results[index]) // Если результат false, файл не скачан
        //            .ToList();

        //        // Если есть неудачные скачивания, выводим информацию
        //        if (failedFiles.Any())
        //        {
        //            // Собираем список не скачанных файлов в строку
        //            string failedFilesString = string.Join(Environment.NewLine, failedFiles);

        //            await Logger.Log($"Не удалось скачать следующие файлы: \n{failedFilesString}");
        //            await DialogWindows.InformationWindow($"Не удалось скачать следующие файлы: \n{failedFilesString}");
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        throw new OperationCanceledException("Скачивание было отменено.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ловим другие неожиданные исключения
        //        throw new Exception("Произошла неизвестная ошибка при скачивании.", ex);
        //    }
        //    finally
        //    {
        //        semaphore.Dispose();
        //    }
        //}
        #endregion
        // --- Параллельное скачивание файлов с возможностью отмены 4 (с повтором, при неудачном скачивании и пропуске неудачных файлов) -----------------------
        public async Task DownloadFilesAsync(string nameBucket, Dictionary<string, string> files, MainWindow mainWindow, IProgress<int> progress, CancellationToken token, bool preview = false)
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = new List<Task<bool>>();
            int completed = 0;
            int totalFiles = files.Count;
            var failedFiles = new List<string>(); // файлы, которые не удалось скачать (что-то не так с файлами)
            var successFiles = new List<string>(); // файлы, которые успешно скачаны

            try
            {
                foreach (var file in files)
                {
                    var s3Key = file.Key;
                    var pathFilePC = file.Value;

                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(token); // Учитываем отмену
                        try
                        {
                            token.ThrowIfCancellationRequested(); // Проверяем отмену перед операцией

                            var request = new GetObjectRequest
                            {
                                BucketName = nameBucket,
                                Key = s3Key
                            };

                            int maxRetries = 4;
                            int retryCount = 0;
                            bool success = false;

                            while (retryCount <= maxRetries && !success)
                            {
                                try
                                {
                                    using (var response = await _s3Client.GetObjectAsync(request, token))
                                    using (var responseStream = response.ResponseStream)
                                    using (var fileStream = File.Create(pathFilePC))
                                    {
                                        await responseStream.CopyToAsync(fileStream, token);
                                    }
                                    success = true;
                                }
                                catch (Exception ex) when (ex is not OperationCanceledException) // Не перехватываем OperationCanceledException здесь
                                {
                                    retryCount++;
                                    if (retryCount > maxRetries)
                                    {
                                        await Logger.Log($"Не удалось скачать {s3Key} после {maxRetries + 1} попыток: {ex.Message}");
                                        Output.WriteLine($"Не удалось скачать {s3Key} после {maxRetries + 1} попыток: {ex.Message}");
                                        return false;
                                    }

                                    await Logger.Log($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");
                                    Output.WriteLine($"Попытка {retryCount} для {s3Key} не удалась: {ex.Message}. Повтор через 1 секунду...");
                                    await Task.Delay(1000 * retryCount, token);
                                }
                            }

                            if (success)
                            {
                                Interlocked.Increment(ref completed);
                                int percent = (int)((completed / (double)totalFiles) * 100);
                                progress?.Report(percent);

                                _ = mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    mainWindow.textProgressUpload.Text = $"Прогресс: {percent}% ( {completed}  из  {totalFiles} ), в очереди {StaticVariables.queueDownloadFiles.Count}";
                                }));

                                if (preview)
                                {
                                    _ = mainWindow.Dispatcher.BeginInvoke(new Action(async () =>
                                    {
                                        _ = ChengeViewWindow.FillElemWindowPictures(file.Key);
                                    }));
                                }
                                await mainWindow.Dispatcher.BeginInvoke(new Action(async () =>
                                {
                                    // Добавляем скачанные bytes в Базу данных
                                    await WorkDateBase.AddByteDownload(new FileInfo(pathFilePC).Length);
                                }));
                                

                                successFiles.Add(pathFilePC);
                            }

                            return success;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, token));
                }

                // Ждём завершения всех задач
                var results = await Task.WhenAll(tasks);

                failedFiles = files.Keys
                    .Where((key, index) => !results[index])
                    .ToList();

                if (failedFiles.Any())
                {
                    string failedFilesString = string.Join(Environment.NewLine, failedFiles);
                    await Logger.Log($"Не удалось скачать следующие файлы: \n{failedFilesString}");
                    await DialogWindows.InformationWindow($"Не удалось скачать следующие файлы: \n{failedFilesString}");
                }
            }
            catch (OperationCanceledException)
            {
                // удаляем частично скачанные файлы
                foreach (var file in files.Values)
                {
                    if (!successFiles.Contains(file) && File.Exists(file))
                    {
                        try
                        {
                            File.Delete(file);
                            Output.WriteLine($"Удалён: {file}");
                        }
                        catch (Exception ex)
                        {
                            Output.WriteLine($"Ошибка при удалении недокачанного файла {file}: {ex.Message}");
                            await Logger.Log($"Ошибка при удалении недокачанного файла {file}: {ex.Message}");
                        }
                    }
                }
                throw new OperationCanceledException("Скачивание было отменено.");
            }
            catch (AggregateException ex)
            {
                // Разворачиваем AggregateException для поиска OperationCanceledException
                if (ex.InnerExceptions.Any(e => e is OperationCanceledException))
                {
                    throw new OperationCanceledException("Скачивание было отменено.", ex);
                }
                throw new Exception("Произошла неизвестная ошибка при скачивании.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Произошла неизвестная ошибка при скачивании.", ex);
            }
            finally
            {
                semaphore.Dispose();
            }
        }

        // --- Загрузка на облако больших файлов с многопоточной загрузкой --------------------------------------
        public async Task UploadLargeFileAsync(string nameBucket, string objectKey, string filePath, string metaDuration, MainWindow mainWindow, CancellationToken cancellationToken = default)
        {
            try
            {
                var transferConfig = new TransferUtilityConfig
                {
                    ConcurrentServiceRequests = 5 // Количество одновременных запросов
                };
                // Настраиваем TransferUtility для многопоточной загрузки
                var transferUtility = new TransferUtility(_s3Client, transferConfig);

                // Параметры загрузки
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    FilePath = filePath,
                    Key = objectKey,
                    BucketName = nameBucket,
                    // Настройка размера части для multipart upload (по умолчанию 5MB)
                    PartSize = 10 * 1024 * 1024,
                    // Опционально: устанавливаем тип контента
                    ContentType = "application/octet-stream"
                };
                // Добавляем метаданные вручную (если это видео, то его длительность)
                uploadRequest.Metadata.Add("x-amz-meta-durationVideo", metaDuration);

                // Обработка прогресса
                uploadRequest.UploadProgressEvent += (sender, e) =>
                {
                    mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        mainWindow.textProgressUpload.Text = $"Прогресс: {e.PercentDone}% ({e.TransferredBytes / 1024 / 1024} MB из {e.TotalBytes / 1024 / 1024} MB)";
                        StaticVariables.activProgressBar.Value = e.PercentDone;
                        StaticVariables.activProgressPercentText.Text = e.PercentDone.ToString();                       
                    }));
                    
                };

                // Запоминаем время начала загрузки для возможной очистки
                DateTime uploadStartTime = DateTime.Now;

                // Регистрируем отмену через CancellationToken
                cancellationToken.Register(async () =>
                {
                    // Прерываем текущую загрузку через токен
                    Console.WriteLine("Загрузка прервана пользователем.");

                    // Очищаем все незавершенные multipart-загрузки, начатые до текущего момента
                    await transferUtility.AbortMultipartUploadsAsync(nameBucket, uploadStartTime, CancellationToken.None);
                });

                await transferUtility.UploadAsync(uploadRequest, cancellationToken);

                // Добавляем загруженные в облако bytes  в Базу данных
                await WorkDateBase.AddByteUpload(new FileInfo(filePath).Length);

                Console.WriteLine("Загрузка успешно завершена!");

            }
            catch (OperationCanceledException)
            {
                throw new Exception("Загрузка была отменена.");
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Ошибка S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Общая ошибка: {ex.Message}");
            }
        }

        // --- Скачивание на ПК больших файлов с многопоточной загрузкой --------------------------------------
        public async Task DownloadLargeFileAsync(string nameBucket, string filePath, string objectKey, MainWindow mainWindow, CancellationTokenSource cts)
        {
            CancellationToken cancellationToken = cts.Token;

            try
            {
                var transferConfig = new TransferUtilityConfig
                {
                    ConcurrentServiceRequests = 5 // Количество одновременных запросов
                };

                var transferUtility = new TransferUtility(_s3Client, transferConfig);

                var downloadRequest = new TransferUtilityDownloadRequest
                {
                    BucketName = nameBucket,
                    Key = objectKey,
                    FilePath = filePath
                };

                // Получаем размер файла для прогресса
                var metadata = await _s3Client.GetObjectMetadataAsync(nameBucket, objectKey, cancellationToken);
                long totalBytes = metadata.ContentLength;

                // Запуск задачи для отслеживания прогресса
                var progressTask = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested) // Условие остановки
                    {
                        UpdateProgress();
                        await Task.Delay(1000, cancellationToken); // Обновление каждую секунду
                    }
                }, cancellationToken);

                await transferUtility.DownloadAsync(downloadRequest, cancellationToken);

                UpdateProgress();
                void UpdateProgress()
                {
                    if (File.Exists(filePath))
                    {
                        long bytesTransferred = new FileInfo(filePath).Length;
                        double percent = (bytesTransferred > 0) ? (double)bytesTransferred / totalBytes * 100 : 0;
                        Console.WriteLine($"Прогресс: {percent:0.##}% ({bytesTransferred / 1024 / 1024} MB из {totalBytes / 1024 / 1024} MB)");
                        mainWindow?.Dispatcher.Invoke(() => mainWindow.textProgressUpload.Text = $"Прогресс: {percent:0.##}% ({bytesTransferred / 1024 / 1024} MB из {totalBytes / 1024 / 1024} MB), в очереди {StaticVariables.queueDownloadFiles.Count}");
                    }
                }

                // Добавляем скачанные bytes на ПК в Базу данных
                await WorkDateBase.AddByteDownload(new FileInfo(filePath).Length);

                //MessageBox.Show("Скачивание успешно завершено!");
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Ошибка S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                    throw new OperationCanceledException("Скачивание прервано пользователем2.", ex, cancellationToken);
                }
                else throw new Exception($"Общая ошибка: {ex.Message}");
            }
            finally
            {
                cts?.Cancel(); // Останавливаем задачу прогресса, если она ещё работает
                cts?.Dispose();
                cts = null;
            }
        }
        // --- Получение списка объектов в бакете -----------------------------------------------
        /// <summary>
        /// pathFolder: путь к папке ("" - вывод объектов всего бакета), 
        /// delimiter: "/" - ограничиваемся первым уровнем, "" - выводим все объекты
        /// </summary>
        public async Task<(bool Success, List<string> listFolders, List<S3Object> listObjects)> ListObjectsAsync(string nameBucket, string pathFolder, string delimiter)
        {
            List<string> listFolders = new List<string>();
            List<S3Object> listObjects = new List<S3Object>();

            // Проверка входного параметра
            if (string.IsNullOrWhiteSpace(nameBucket))
            {
                await Logger.Log("Ошибка: имя бакета не может быть пустым или null.");
                return (false, listFolders, listObjects);
            }

            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = nameBucket,
                    Prefix = pathFolder, // Указываем префикс (папку)
                    MaxKeys = 1000, // Ограничение на количество возвращаемых объектов за один цикл (1000 по умолчанию)
                    Delimiter = delimiter // "/" - Ограничиваемся первым уровнем, "" - выводим все объекты
                };

                ListObjectsV2Response response;
                do
                {
                    // Выполняем запрос к S3
                    response = await _s3Client.ListObjectsV2Async(request);

                    // Проверка на null в ответе (маловероятно, но для надёжности)
                    if (response == null)
                    {
                        await Logger.Log("Ошибка: получен пустой ответ от сервера S3.");
                        return (false, listFolders, listObjects);
                    }

                    // Проверка на ошибки в самом ответе (например, коды ошибок S3)
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        await Logger.Log($"Ошибка S3: сервер вернул статус {response.HttpStatusCode}.");
                        return (false, listFolders, listObjects);
                    }

                    // Добавляем папки
                    foreach (var prefix in response.CommonPrefixes)
                    {
                        // Проверка на null или пустой путь (защита от некорректных данных)
                        if (prefix == null || string.IsNullOrEmpty(prefix))
                        {
                            await Logger.Log($"Пропущен некорректная папка: {prefix}");
                            continue; // Пропускаем некорректные объекты
                        }
                        listFolders.Add(prefix);
                    }

                    // Добавляем файлы
                    foreach (var obj in response.S3Objects)
                    {
                        // Проверка на null или пустой ключ (защита от некорректных данных) или если ключ равен префиксу
                        if (obj == null || string.IsNullOrEmpty(obj.Key) || obj.Key == request.Prefix)
                        {
                            await Logger.Log($"Пропущен некорректный объект: {obj}");
                            continue; // Пропускаем некорректные объекты
                        }
                        //listObjects.Add($"Объект: {obj.Key}, Размер: {obj.Size} байт, Последее изменение: {obj.LastModified}");
                        listObjects.Add(obj);
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return (true, listFolders, listObjects);
            }
            catch (AmazonS3Exception ex)
            {
                // Специфическая ошибка S3 (например, доступ запрещён, бакет не найден)
                await Logger.Log($"Ошибка S3: {ex.Message} (Код ошибки: {ex.ErrorCode}, Статус: {ex.StatusCode})");
                return (false, listFolders, listObjects);
            }
            catch (TimeoutException ex)
            {
                // Ошибка таймаута
                await Logger.Log($"Ошибка: превышено время ожидания запроса к S3: {ex.Message}");
                return (false, listFolders, listObjects);
            }
            catch (Exception ex)
            {
                // проверка соединения с интернетом
                var result = await InternetAvailability.CheckInternet();
                if (result.Success)
                {
                    await Logger.Log($"Неизвестная ошибка при получении списка объектов: {ex.Message}");
                    return (false, listFolders, listObjects);
                }
                else
                {
                    await Logger.Log("Проверьте соединение с интернетом или доступность IDrive");
                    return (false, null, null);
                }
            }
        }

        // --- Получение списка объектов в папке с фильтрацией по расширениям .jpg, .png, .webp ----------------------------------------
        public async Task<List<string>> ListObjectsInFolderWithFilterAsync(string nameBucket, string folderPath, string delimiter)
        {
            // Список расширений для фильтрации
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png", ".webp" };

            List<string> listObjects = new List<string>();

            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = nameBucket,
                    Prefix = folderPath, // Указываем префикс (папку)
                    Delimiter = delimiter, // Ограничиваем вывод только объектами внутри папки (опционально)
                    MaxKeys = 100        // Максимальное количество объектов в одном запросе
                };

                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);

                    // Фильтруем объекты по расширениям
                    var filteredObjects = response.S3Objects
                        .Where(obj => allowedExtensions.Contains(Path.GetExtension(obj.Key)))
                        .ToList();

                    // Выводим отфильтрованные объекты
                    foreach (var obj in filteredObjects)
                    {
                        Console.WriteLine($"Объект: {obj.Key}, Размер: {obj.Size} байт, Дата изменения: {obj.LastModified}");
                        listObjects.Add($"Объект: {obj.Key}, Размер: {obj.Size} байт, Дата изменения: {obj.LastModified}");
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                if (response.S3Objects.Count == 0)
                {
                    MessageBox.Show($"Папка {folderPath} пуста или не содержит файлов с расширениями .jpg, .png, .webp.");
                }
                return listObjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении объектов из папки {folderPath}: {ex.Message}");
                return listObjects;
            }
        }

        // --- Переименовать папку/файл -------------------------------------------------------------------------------
        #region Базовая реализация переименования
        //public async Task RenameObjectAsync(string oldPathElement, string newPathElement)
        //{
        //    var BucketName = Settings.mainBucket;

        //    // Шаг 1: Копирование объекта с новым именем
        //    var copyRequest = new CopyObjectRequest
        //    {
        //        SourceBucket = BucketName,
        //        SourceKey = oldPathElement,
        //        DestinationBucket = BucketName,
        //        DestinationKey = newPathElement
        //    };

        //    await _s3Client.CopyObjectAsync(copyRequest);

        //    // Шаг 2: Удаление старого объекта
        //    var deleteRequest = new DeleteObjectRequest
        //    {
        //        BucketName = BucketName,
        //        Key = oldPathElement
        //    };

        //    await _s3Client.DeleteObjectAsync(deleteRequest);
        //}
        #endregion
        public async Task RenameObjectAsync(string oldPathElement, string newPathElement)
        {
            try
            {
                // Проверка входных параметров
                if (string.IsNullOrWhiteSpace(oldPathElement))
                    throw new ArgumentException("Source path cannot be null or empty", nameof(oldPathElement));

                if (string.IsNullOrWhiteSpace(newPathElement))
                    throw new ArgumentException("Destination path cannot be null or empty", nameof(newPathElement));

                if (oldPathElement == newPathElement)
                    throw new ArgumentException("Source and destination paths cannot be the same");

                var bucketName = Settings.mainBucket;

                if (string.IsNullOrWhiteSpace(bucketName))
                    throw new InvalidOperationException("Bucket name is not configured in settings");

                // Проверка существования исходного объекта
                var headRequest = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = oldPathElement
                };

                try
                {
                    await _s3Client.GetObjectMetadataAsync(headRequest);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException($"Source object '{oldPathElement}' not found in bucket '{bucketName}'");
                }

                // Проверка, не существует ли уже целевой объект
                var headDestRequest = new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = newPathElement
                };

                try
                {
                    var metadata = await _s3Client.GetObjectMetadataAsync(headDestRequest);
                    throw new InvalidOperationException($"Destination object '{newPathElement}' already exists");
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    // Это нормальная ситуация - целевой объект не существует
                }

                // Настройка запроса на копирование
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = oldPathElement,
                    DestinationBucket = bucketName,
                    DestinationKey = newPathElement
                };

                // Выполнение копирования
                var response = await _s3Client.CopyObjectAsync(copyRequest);

                // Проверка успешности операции
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Failed to copy object. Status code: {response.HttpStatusCode}");
                }

                // Удаление старого файла
                var result = await DeleteFolderOrFileAsync(bucketName, oldPathElement);
                if(!result.Success) throw new InvalidOperationException($"Ошибка при удалении: {result.Message}");
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"S3 error occurred: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error copying object from '{oldPathElement}' to '{newPathElement}': {ex.Message}", ex);
            }
        }

        // Получение подписанной ссылки на видео из S3
        public async Task<string> GetSignedUrlFromS3Async(string filePath)
        {
            var _s3Client = S3ClientFactory.GetS3Client().s3Client;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = Settings.mainBucket,
                Key = filePath,
                Expires = DateTime.UtcNow.AddDays(1), // В продакшен будет: Один день. Максимум 7 дней
                //Expires = DateTime.UtcNow.AddMinutes(10), // URL действителен 10 минут (для тестирования)
                Verb = HttpVerb.GET // Метод HTTP (GET для скачивания)
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
