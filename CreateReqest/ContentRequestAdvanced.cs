using IDriveView.AddWindows;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using IDriveView.WorkClasses;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IDriveView.CreateReqest
{
    class ContentRequestAdvanced
    {
        private MainWindow mainWindow;

        private string NameBucket = Settings.mainBucket;
        public ContentRequestAdvanced()
        {
            mainWindow = Application.Current.MainWindow as MainWindow;
        }
        // --- Создание папки ---
        public async void CreateFolderAdvancedAsync(object sender, RoutedEventArgs e)
        {
            // получаем открытую папку
            PathToSelectFolder openFolder = StaticVariables.pathToSelectFolders.Peek();
            //Output.WriteLine(openFolder.Path);
            string openFolderPath = openFolder.Path == "/" ? "" : openFolder.Path;
            // получаем список папок в этой папке
            //var dataFolder = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, openFolderPath, "/");
            var dataFolder = StaticVariables.listFoldersAndFiles
                            .Where(i => i.Type == "folder")
                            .Select(i => i.Name)
                            .ToList();
            // вводим название новой папки, вызвав окно "Ввести данные"
            // если такой папки не существует, то создаём такую папку, если существует, просим пересмотреть название
            var nameCreateForlder = await new DialogWindows().EnterDataWindowWait(dataFolder, "Имя папки");
            Output.WriteLine(nameCreateForlder);
            
            var result = await new ContentRequest().CreateFolderAsync(Settings.mainBucket, Path.Combine(openFolderPath, nameCreateForlder.Trim()));
            if (!result.Success) await DialogWindows.InformationWindow(result.Message);

            // перезагрузить отображение текущей папки
            await FillWindow.FillContentWindow(openFolder.Path, "/", false);
        }
        // --- Удаление папки / файла ----------------------------------------------------
        //public async void DeleteFolderOrFileAdvancedAsync(string pathDeleteElement, string type)
        public async Task DeleteFolderOrFileAdvancedAsync()
        {
            await DialogWindows.CreateProgressWindow("Удаление...");// окно прогресса - открыто

            // Получаем все Border с фоном LightBlue
            List<Border> borders = mainWindow.wrapPanelView.Children
                .OfType<Border>()
                .Where(b => (b.Background as SolidColorBrush)?.Color == Colors.LightBlue)
                .ToList();

            // список папок и файлов выделенных в окне программы
            List<FolderOrFileModel> listSelectedElements = new List<FolderOrFileModel>();

            foreach (var border in borders)
            {
                var elem = StaticVariables.listFoldersAndFiles.Where(i => i.OuterBorder.Name == border.Name).FirstOrDefault();
                if (elem != null) listSelectedElements.Add(elem);
            }

            // если ничего не выделено, то выходим
            if (listSelectedElements.Count == 0)
            {
                await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
                return;
            }

            // Удаляем элементы из списка (если папки, то рекурсисно, если файл, то просто)
            await DeleteFolderOrFileAdvancedContinueAsync(listSelectedElements);

            // получаем открытую папку
            PathToSelectFolder openFolder = StaticVariables.pathToSelectFolders.Peek();
            // перезагрузить отображение текущей папки
            await FillWindow.FillContentWindow(openFolder.Path, "/", false);

            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
        }
        public async Task DeleteFolderOrFileAdvancedContinueAsync(List<FolderOrFileModel> listSelectedElements)
        {
            foreach (var selectedElement in listSelectedElements)
            {
                if (selectedElement.Type == "folder")
                {
                    StringBuilder resultError = new StringBuilder();
                    // получаем список всех элементов в этой папке
                    var data = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, selectedElement.Path, "");
                    // если папка не пуста, то удаляем все элементы последовательно
                    if (data.listObjects.Count != 0 || data.listFolders.Count != 0)
                    {
                        //await DialogWindows.CreateProgressWindow("Удаление папки...");// окно прогресса - открыто

                        foreach (var file in data.listObjects)
                        {
                            var answer = await new ContentRequest().DeleteFolderOrFileAsync(NameBucket, file.Key);
                            if (!answer.Success)
                            {
                                await Logger.Log($"Ошибка удаления файла: {file.Key}");
                                resultError.Append(file.Key);
                            }
                        }
                        foreach (var folder in data.listFolders)
                        {
                            var answer = await new ContentRequest().DeleteFolderOrFileAsync(NameBucket, folder);
                            if (!answer.Success)
                            {
                                await Logger.Log($"Ошибка удаления файла: {folder}");
                                resultError.Append(folder);
                            }
                        }

                        //await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

                        if (resultError.ToString() != "")
                            await DialogWindows.InformationWindow($"Проблемы с удалением (в логах подробнее): {resultError.ToString()}");
                    }
                }

                var result = await new ContentRequest().DeleteFolderOrFileAsync(NameBucket, selectedElement.Path);
                if (!result.Success) await DialogWindows.InformationWindow(result.Message);

            }
        }
        // --- // ---// --- не используется, но код должен быть рабочим
        // --- Загрузка файла в бакет -------------------------------------------------------
        //event Action StopUploadFileAdvancedAsync;
        //public async void UploadFileAdvancedAsync(object sender, RoutedEventArgs e)
        //{
        //    // Создаем токен отмены
        //    var cts = new CancellationTokenSource();
        //    StopUploadFileAdvancedAsync = null; // Сбрасываем старые подписчики
        //    StopUploadFileAdvancedAsync += () => cts.Cancel();
        //    //mainWindow.indicatorUploadFile.Visibility = Visibility.Visible;
        //    await new ContentRequest().UploadFileAsync(NameBucket, "pathUploadFileCloud", "pathUploadFilePC", cts.Token);
        //    //mainWindow.indicatorUploadFile.Visibility = Visibility.Collapsed;
        //}
        //// --- Отмена загрузки ---------------------------------------------------
        //public async void CancelUploadFileAdvancedAsync(object sender, RoutedEventArgs e)
        //{
        //    StopUploadFileAdvancedAsync?.Invoke();
        //}
        // --- // ---// ---

        // --- Скачивание файла из бакета с возможностью прерывания -------------------------------------------------------
        event Action StopDownloadFileAdvancedAsync;
        public async void DownloadFileAdvancedAsync(object sender, RoutedEventArgs e)
        {
            // Создаем токен отмены
            var cts = new CancellationTokenSource();
            StopDownloadFileAdvancedAsync = null; // Сбрасываем старые подписчики
            StopDownloadFileAdvancedAsync += () => cts.Cancel();
            //mainWindow.indicatorDownloadFile.Visibility = Visibility.Visible;
            await new ContentRequest().DownloadFileAsync(NameBucket, "pathUploadFileCloud", "pathUploadFilePC", cts.Token);
            //mainWindow.indicatorDownloadFile.Visibility = Visibility.Collapsed;
        }
        // --- Отмена скачивания ---------------------------------------------------
        public async void CancelDownloadFileAdvancedAsync(object sender, RoutedEventArgs e)
        {
            StopDownloadFileAdvancedAsync?.Invoke();
        }
        // --- // ---// --- не используется, но код должен быть рабочим
        // --- Получить все объекты ----------------------------------------------------------------
        //public async void ListObjectsAdvancedAsync(object sender, RoutedEventArgs e)
        //{
        //    var result = await new ContentRequest().ListObjectsAsync(NameBucket, "/", "/");
        //    foreach (var path in result.listFolders)
        //    {
        //        Output.WriteLine(path);
        //    }
        //    foreach (var obj in result.listObjects)
        //    {
        //        Output.WriteLine(obj.Key);
        //    }
        //}
        // --- // ---// --- 

        // --- // ---// --- не используется, но код должен быть рабочим
        // --- Получить объекты конкретной папки -----------------------------------------------------
        //public async void ListObjectsInFolderAdvancedAsync(object sender, RoutedEventArgs e)
        //{
        //    string delimiter = "/";
        //    delimiter = "";

        //    var result = await new ContentRequest().ListObjectsInFolderAsync(NameBucket, "pathFolderForGetObjects", delimiter);
        //    foreach (var obj in result)
        //    {
        //        Output.WriteLine(obj);
        //    }
        //}
        // --- // ---// --- 

        // --- Получить объекты конкретной папки (только картинки) -----------------------------------------------------
        public async void ListObjectsInFolderWithFilterAdvancedAsync(object sender, RoutedEventArgs e)
        {
            string delimiter = "/";
            delimiter = "";

            var result = await new ContentRequest().ListObjectsInFolderWithFilterAsync(NameBucket, "pathFolderForGetImages", delimiter);
            foreach (var obj in result)
            {
                Output.WriteLine(obj);
            }
        }
        // --- Параллельная загрузка файлов с прогрессом и отменой -----------------------------------------------------------------------------------
        public static event Action StopUploadFiles;
        //public async void UploadFilesAdvancedAsync(string pathFolderCloud, List<string> listPathFilesPC)
        public async void UploadFilesAdvancedAsync(string pathFolderCloud, Dictionary<string, string> filesToUpload)
        {
            //// Пример списков
            //var filesToUpload = new Dictionary<string, string>
            //{
            //    { "C:\\files\\aaa.mp4", "Therd/aaa.mp4" },
            //    { "C:\\files\\bbb.mp4", "Therd/bbb.mp4" },
            //    { "C:\\files\\ccc.mp4", "Therd/ccc.mp4" },
            //    { "C:\\files\\ddd.mp3", "Therd/ddd.mp3" },
            //    { "C:\\files\\eee.mp4", "Therd/eee.mp4" }
            //};

            // Настройка CancellationToken
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            StopUploadFiles = null; // Сбрасываем старые подписчики
            StopUploadFiles += () => cts.Cancel();

            // Настройка прогресс-бара
            var progress = new Progress<int>(percent =>
            {
                StaticVariables.activProgressBar.Value = percent;
                StaticVariables.activProgressPercentText.Text = percent.ToString();
                Console.Write($"\rОбщий прогресс: {percent}%");
            });
            try
            {
                Output.WriteLine("Начинаем параллельную загрузку...");
                StaticVariables.nowUploadFiles = true;
                await new ContentRequest().UploadFilesAsync(NameBucket, mainWindow, filesToUpload, progress, token);
                Output.WriteLine("Загрузка завершена.");            
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Ошибка: {ex.Message}");
            }

            // операции после скачивания
            await StaticVariables.ActionPostPartUploadFiles();

        }
        // --- Отмена загрузки ---------------------------------------------------
        public async void CancelUploadFilesAdvancedAsync(object sender, RoutedEventArgs e)
        {
            StopUploadFiles?.Invoke();
        }

        // --- Параллельное скачивание файлов с прогрессом и отменой -----------------------------------------------------------------------------------
        static event Action StopDownloadFiles;
        public async Task DownloadFilesAdvancedAsync(Dictionary<string, string> filesToUpload, bool preview = false)
        {
            // Пример списков
            //var filesToUpload = new Dictionary<string, string>
            //{
            //    {"Therd/aaa.mp4", "C:\\files2\\aaa.mp4"},
            //    {"Therd/bbb.mp4", "C:\\files2\\bbb.mp4"},
            //    {"Therd/ccc.mp4","C:\\files2\\ccc.mp4"},
            //    {"Therd/ddd.mp3", "C:\\files2\\ddd.mp3"},
            //    {"Therd/eee.mp4", "C:\\files2\\eee.mp4"}
            //};

            // Настройка CancellationToken
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            StopDownloadFiles = null; // Сбрасываем старые подписчики
            //StopDownloadFiles += () => cts.Cancel();
            // Добавляем обработчик с проверкой
            StopDownloadFiles += () =>
            {
                try
                {
                    if (!cts.IsCancellationRequested) // Проверяем, не отменён ли уже
                    {
                        cts.Cancel();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Игнорируем, если cts уже освобождён
                }
            };

            // Настройка прогресс-бара
            var progress = new Progress<int>(percent =>
            {
                Console.Write($"\rОбщий прогресс: {percent}%");
            });
            try
            {
                Output.WriteLine("Начинаем параллельное скачивание...");
                StaticVariables.nowDownloadFiles = true;
                await new ContentRequest().DownloadFilesAsync(NameBucket, filesToUpload, mainWindow, progress, token, preview);
                Output.WriteLine("Скачивание файлов завершено.");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"\nОшибка: {ex.Message}");
            }

            // операции после скачивания
            await StaticVariables.ActionPostPartDownloadFiles();
        }
        // --- Отмена загрузки ---------------------------------------------------
        public async void CancelDownloadFilesAdvancedAsync(object sender, RoutedEventArgs e)
        {
            StopDownloadFiles?.Invoke();
        }

        // --- Загрузка в облако большого файла с многопоточной загрузкой и отменой --------------------------------------
        event Action StopUploadLargeFile;
        public async Task UploadLargeFileAdvancedAsync(string pathFolderCloud, string pathFilesPC)
        {
            //string filePath = @"C:\LargeFiles2/AAA.zip"; // Путь к вашему файлу
            //string objectKey = "Bbb/AAA.zip";       // Ключ объекта в облаке

            string nameFile = Path.GetFileName(pathFilesPC);
            string pathFileCloud = Path.Combine(pathFolderCloud, nameFile);
            pathFileCloud = pathFileCloud.TrimStart('/');

            // Если это видео,то получаем и передаём его длительность
            string metaDuration = Services.GetDutationVideo(pathFilesPC);

            // Создаем CancellationTokenSource для управления отменой
            using var cts = new CancellationTokenSource();

            StopUploadLargeFile = null; // Сбрасываем старые подписчики
            StopUploadLargeFile += () => cts.Cancel();

            try
            {
                Output.WriteLine($"Начало загрузки большого файла");
                await new ContentRequest().UploadLargeFileAsync(NameBucket, pathFileCloud, pathFilesPC, metaDuration, mainWindow, cts.Token);
                Output.WriteLine($"Загрузка файла большого завершена.");
                // операции после закрузки файла               
                await StaticVariables.ActionPostPartUploadFiles();
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Не удалось загрузить большой файл: {ex.Message}");
            }
        }
        // --- Отмена загрузки в облако большого файла ---------------------------------------------------
        public async void CancelUploadLargeFileAdvancedAsync(object sender, RoutedEventArgs e)
        {
            StopUploadLargeFile?.Invoke();
        }

        // --- Скачивание на ПК большого файла с многопоточной загрузкой --------------------------------------
        event Action StopDownloadLargeFile;
        public async Task DownloadLargeFileAdvancedAsync(string objectKey, string filePath)
        {
            //string filePath = @"C:\LargeFiles2/AAA.zip"; // Путь к вашему файлу
            //string objectKey = "Bbb/AAA.zip";       // Ключ объекта в облаке

            // Создаем CancellationTokenSource для управления отменой
            using var cts = new CancellationTokenSource();

            StopDownloadLargeFile = null; // Сбрасываем старые подписчики
            StopDownloadLargeFile += () => cts.Cancel();

            try
            {
                Output.WriteLine($"Начало скачивания большого файла");
                StaticVariables.nowDownloadFiles = true;
                await new ContentRequest().DownloadLargeFileAsync(NameBucket, filePath, objectKey, mainWindow, cts);
                Output.WriteLine($"Скачивание большого файла завершено.");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Не удалось загрузить большой файл: {ex.Message}");
            }

            // операции после скачивания
            await StaticVariables.ActionPostPartDownloadFiles();
        }
        // --- Отмена скачивания на ПК большого файла ---------------------------------------------------
        public async void CancelDownloadLargeFileAdvancedAsync(object sender, RoutedEventArgs e)
        {
            StopDownloadLargeFile?.Invoke();
        }

        // --- Переименовать элемент ------------------------------------------------------------------
        public async Task RenameObjectAdvancedAsync(string type, string oldElementPath, string pathDirectory)
        {
            var newName = await new DialogWindows().EnterDataWindowWait(new List<string>(), "Введите новое имя");
            newName = newName.Trim();
            string slesh = "";
            if (type == "folder") slesh = "/";
            else
            {
                string oldExtension = Path.GetExtension(oldElementPath);
                string newExtension = Path.GetExtension(newName);
                // Если расширение не oldExtension или отсутствует, меняем его
                if (string.IsNullOrEmpty(newExtension) || !newExtension.Equals(oldExtension, StringComparison.OrdinalIgnoreCase))
                {
                    newName = Path.ChangeExtension(newName, oldExtension);
                }
            }
            string newPathElement = Path.Combine(pathDirectory, newName).Replace('\\', '/') + slesh;

            if(oldElementPath == newPathElement) return;

            try
            {
                await RenameObjectAdvancedContinueAsync(type, oldElementPath, newPathElement);

                // путь к папке загрузки в облаке (открытая папка в программе)
                string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;
                // перезагрузить отображение текущей папки
                await FillWindow.FillContentWindow(openFolderCloud, "/", false);
            }
            catch (Exception ex)
            {
                await DialogWindows.InformationWindow(ex.Message);
                await Logger.Log(ex.ToString());
            }
        }
        public async Task RenameObjectAdvancedContinueAsync(string type, string oldElementPath, string newElementPath)
        {
            if (type == "folder")
            {
                StringBuilder resultError = new StringBuilder();

                // получаем список всех элементов в этой папке
                var data = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, oldElementPath, "");

                // если папка не пуста, то переименовываем все элементы последовательно
                if (data.listObjects.Count != 0)
                {
                    await DialogWindows.CreateProgressWindow("Переименование ...");// окно прогресса - открыто

                    // создаём папку с новым именем
                    await new ContentRequest().CreateFolderAsync(Settings.mainBucket, newElementPath);

                    foreach (var file in data.listObjects)
                    {
                        try
                        {
                            await new ContentRequest().RenameObjectAsync(file.Key, file.Key.Replace(oldElementPath, newElementPath));
                        }
                        catch(Exception ex)
                        {
                            await Logger.Log($"Ошибка удаления файла: {file.Key}. {ex.Message}");
                            resultError.Append(file.Key);
                        }
                    }

                    // удаляем папку со старым именем
                    await new ContentRequest().DeleteFolderOrFileAsync(Settings.mainBucket, oldElementPath);

                    await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

                    if (resultError.ToString() != "")
                        await DialogWindows.InformationWindow($"Проблемы с удалением (в логах подробнее): {resultError.ToString()}");
                }
                else
                {
                    await new ContentRequest().RenameObjectAsync(oldElementPath, newElementPath);
                }
            }
            else
            {
                await new ContentRequest().RenameObjectAsync(oldElementPath, newElementPath);
            }
        }    
    }
}
