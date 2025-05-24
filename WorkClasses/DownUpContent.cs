using Amazon.Runtime.Internal.Transform;
using IDriveView.AddWindows;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace IDriveView.WorkClasses
{
    class DownUpContent
    {
        private static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        // --- Загрузка файлов в облако ---------------------------------------------------------------------------
        public async void UpLoadFilesAsync(object sender, RoutedEventArgs e)
        {
            // Диалог выбора файлов
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true, // Разрешаем выбирать несколько файлов
                Title = "Выберите файлы"
            };

            // путь к папке загрузки в облаке (открытая папка в программе)
            string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;
            openFolderCloud = openFolderCloud == "/" ? "" : openFolderCloud;

            if (openFileDialog.ShowDialog() == true)
            {
                // разбиваем файлы до 16мб и свыше 16мб в разные списки
                List<string> files = new List<string>();
                List<string> filesLarge = new List<string>();
                foreach (string file in openFileDialog.FileNames)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    long fileSize = fileInfo.Length;
                    if (fileSize <= 16 * 1024 * 1024) files.Add(fileInfo.FullName);
                    else filesLarge.Add(fileInfo.FullName);
                }
 
                // если список с файлами до 16мб не пуст
                if (files.Count > 0)
                {
                    // создаём словарь для загрузки файлов
                    var filesToUpload = new Dictionary<string, string>();
                    foreach (var path in files)
                    {
                        string fileName = Path.GetFileName(path);
                        filesToUpload[path] = Path.Combine(openFolderCloud, fileName);
                    }

                    var memberOfQueue = new QueueUploadFiles(openFolderCloud, filesToUpload, "");
                    // если не идёт загрузка файлов в данный момент, то запускаем загрузку немедленно
                    // если загрузка идёт в данный момент, то помещаем объект в очередь
                    if (StaticVariables.nowUploadFiles == false)
                    {
                        // показываем окно загрузок              
                        mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                        mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                        await UpLoadFilesAsyncContinue(memberOfQueue);
                    }
                    else
                    {
                        StaticVariables.queueUploadFiles.Enqueue(memberOfQueue);
                        StaticVariables.showQueueUploadFiles.Enqueue("Быстрая загрузка файлов до 16 мб");
                        MainWindow.queueUploadViewModel.Items.Add("Быстрая загрузка файлов до 16 мб");
                    }
                }
                // если список с файлами свыше 16мб не пуст
                if (filesLarge.Count > 0)
                {
                    foreach (var file in filesLarge)
                    {
                        var memberOfQueue = new QueueUploadFiles(openFolderCloud, null, file);
                        // если не идёт загрузка фалов в данный момент, то запускаем загрузку немедленно
                        // если загрузка идёт в данный момент, то помещаем объект в очередь
                        if (StaticVariables.nowUploadFiles == false)
                        {
                            // показываем окно загрузок              
                            mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                            mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                            await UpLoadFilesAsyncContinue(memberOfQueue);
                        }
                        else
                        {
                            StaticVariables.queueUploadFiles.Enqueue(memberOfQueue);
                            StaticVariables.showQueueUploadFiles.Enqueue(Path.GetFileName(file));
                            MainWindow.queueUploadViewModel.Items.Add(Path.GetFileName(file));
                        }
                    }
                }
            }
        }
        public async static Task UpLoadFilesAsyncContinue(QueueUploadFiles memberOfQueue)
        {
            if (memberOfQueue.FilesForUpload != null)
            {
                DialogWindows.CreateProgressBlock(mainWindow.progressFilesBorder, mainWindow.progressFilesStackPanel, "Быстрая загрузка файлов до 16 мб");
                mainWindow.progressFilesScrollVewer.PageDown();
                new ContentRequestAdvanced().UploadFilesAdvancedAsync(memberOfQueue.OpenFolderCloud, memberOfQueue.FilesForUpload);
            }
            else
            {
                DialogWindows.CreateProgressBlock(mainWindow.progressFilesBorder, mainWindow.progressFilesStackPanel, Path.GetFileName(memberOfQueue.FileLargeForUpload));
                mainWindow.progressFilesScrollVewer.PageDown();
                await new ContentRequestAdvanced().UploadLargeFileAdvancedAsync(memberOfQueue.OpenFolderCloud, memberOfQueue.FileLargeForUpload);
            }
        }
        // --- Загрузка папки в облако -----------------------------------------------------------------------------
        public async void UpLoadFolderAsync(object sender, RoutedEventArgs e)
        {
            // путь к папке загрузки в облаке (открытая папка в программе)
            string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;

            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await DialogWindows.CreateProgressWindow("Загрузка папки...");// окно прогресса - открыто

                // Получаем имя папки, которую загружаем в облако (корневая папка)
                string pathFolderPC = dialog.FileName;
                string nameFilderPC = Path.GetFileName(pathFolderPC);
                string pathParentFolderPC = Path.GetDirectoryName(pathFolderPC).Replace('\\', '/') + '/';
                var NameBucket = Settings.mainBucket;

                // Список папок, создаваемых в облаке: относительный путь (рекурсивно)
                List<string> folders = new List<string>();
                // Словарь файлов: абсолютный путь на ПК - абсоблютный путь в облаке 
                Dictionary<string, string> files = new Dictionary<string, string>();

                if (Directory.Exists(pathFolderPC))
                {
                    folders = Directory.GetDirectories(pathFolderPC, "*", SearchOption.AllDirectories)
                                       .Select(dir => Path.GetRelativePath(pathFolderPC, dir))
                                       .Select(dir => Path.Combine(openFolderCloud, nameFilderPC, dir))
                                       .Select(dir => dir.Replace('\\', '/'))
                                       .ToList();

                    files = Directory.GetFiles(pathFolderPC, "*", SearchOption.AllDirectories)
                                    .ToDictionary(
                                        file => new FileInfo(Path.GetFullPath(file)).FullName.Replace('\\', '/'),
                                        file => new FileInfo(Path.GetFullPath(file)).FullName.Replace('\\', '/').Replace(pathParentFolderPC, openFolderCloud)
                                    );
                                    //.ToDictionary(
                                    //    file => Path.GetFullPath(file),
                                    //    file => Path.Combine(openFolderCloud, nameFilderPC, Path.GetRelativePath(pathFolderPC, file)).Replace('\\', '/')
                                    //);
                }
                // Добавляем первым элементом путь к загружаемой папке (в облаке)
                folders.Insert(0, Path.Combine(openFolderCloud, nameFilderPC));

                await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

                await UpLoadFolderContinueAsync(folders, files);
            }
        }
        // --- Загрузка папки в облако перетаскиванием -----------------------------------------------------------------------------
        public async Task UpLoadDragAndDropAsync(string[] pathUploadElements)
        {
            await DialogWindows.CreateProgressWindow("Загрузка контента...");// окно прогресса - открыто

            // путь к папке загрузки в облаке (открытая папка в программе)
            string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;

            // путь к папке выгрузки файлов с ПК в облако (если путь длинный, то может не отображаться, по этому: new FileInfo()):
            string pathOpenFolderPC;
            if (File.Exists(pathUploadElements[0])) pathOpenFolderPC = Path.GetDirectoryName(new FileInfo(pathUploadElements[0]).FullName).Replace('\\', '/') + '/';
            else pathOpenFolderPC = Path.GetDirectoryName(pathUploadElements[0]).Replace('\\', '/') + '/';

            // Список папок, создаваемых в облаке: абсоблютный путь в облаке
            List<string> folders = new List<string>();
            // Словарь файлов: абсолютный путь на ПК - абсоблютный путь в облаке 
            Dictionary<string, string> files = new Dictionary<string, string>();

            // Перебираем папки и файлы первого уровня
            foreach (string pathElement in pathUploadElements)
            {
                // Перебираем вложенные папки файлы следующих уровней
                if (Directory.Exists(pathElement))
                {
                    List<string> folderItem = new List<string>();
                    Dictionary<string, string> filesItem = new Dictionary<string, string>();

                    string pathFolderPC = pathElement.Replace('\\', '/');

                    folders.Add(pathFolderPC.Replace(pathOpenFolderPC, openFolderCloud));

                    // достаём папки в этой папке
                    folderItem = Directory.GetDirectories(pathFolderPC, "*", SearchOption.AllDirectories)
                                       .Select(dir => dir.Replace(pathOpenFolderPC, openFolderCloud).Replace('\\', '/'))
                                       .ToList();
                    // достаём файлы в этой папке
                    filesItem = Directory.GetFiles(pathFolderPC, "*", SearchOption.AllDirectories)
                                    .ToDictionary(
                                        file => new FileInfo(Path.GetFullPath(file)).FullName.Replace('\\', '/'),
                                        file => new FileInfo(Path.GetFullPath(file)).FullName.Replace('\\', '/').Replace(pathOpenFolderPC, openFolderCloud)
                                    );
                                    //.ToDictionary(
                                    //    file => Path.GetFullPath(file).Replace('\\', '/'),
                                    //    file => file.Replace(pathOpenFolderPC, openFolderCloud).Replace('\\', '/')
                                    //);

                    // объединение списков папок
                    folders.AddRange(folderItem);
                    // объединение словарей с файлами
                    files = files.Concat(filesItem).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                else
                {
                    string pathFilePC = new FileInfo(pathElement).FullName.Replace('\\', '/');
                    // абсолютный путь на ПК - абсоблютный путь в облаке 
                    files[pathFilePC] = pathFilePC.Replace(pathOpenFolderPC, openFolderCloud);
                }
            }

            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

            await UpLoadFolderContinueAsync(folders, files);
        }
        public async Task UpLoadFolderContinueAsync(List<string> folders, Dictionary<string, string> files)
        {
            // путь к папке загрузки в облаке (открытая папка в программе)
            string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;

            await DialogWindows.CreateProgressWindow("Создание папок...");// окно прогресса - открыто
            // Создаём папки в облаке
            foreach (var folder in folders)
            {
                var result = await new ContentRequest().CreateFolderAsync(Settings.mainBucket, folder);
                if (!result.Success) await DialogWindows.InformationWindow(result.Message);
            }
            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

            // делим файлы на два словаря: до 16 мб и больше
            Dictionary<string, string> filesSmall = new Dictionary<string, string>();
            Dictionary<string, string> filesLarge = new Dictionary<string, string>();

            foreach (var file in files)
            {
                FileInfo fileInfo = new FileInfo(file.Key);
                long fileSize = fileInfo.Length;
                if (fileSize <= 16 * 1024 * 1024) filesSmall.Add(file);
                else filesLarge.Add(file);
            }

            // если словарь с файлами до 16мб не пуст
            if (filesSmall.Count > 0)
            {
                var memberOfQueue = new QueueUploadFiles(openFolderCloud, filesSmall, "");
                // если не идёт загрузка фалов в данный момент, то запускаем загрузку немедленно
                // если загрузка идёт в данный момент, то помещаем объект в очередь
                if (StaticVariables.nowUploadFiles == false)
                {
                    // показываем окно загрузок              
                    mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                    mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                    await UpLoadFilesAsyncContinue(memberOfQueue);
                }
                else
                {
                    StaticVariables.queueUploadFiles.Enqueue(memberOfQueue);
                    StaticVariables.showQueueUploadFiles.Enqueue("Быстрая загрузка файлов до 16 мб");
                    MainWindow.queueUploadViewModel.Items.Add("Быстрая загрузка файлов до 16 мб");
                }
            }
            // если словарь с файлами свыше 16мб не пуст
            if (filesLarge.Count > 0)
            {
                foreach (var file in filesLarge)
                {
                    string directory = Path.GetDirectoryName(file.Value).Replace('\\', '/') + '/';
                    var memberOfQueue = new QueueUploadFiles(directory, null, file.Key);
                    // если не идёт загрузка фалов в данный момент, то запускаем загрузку немедленно
                    // если загрузка идёт в данный момент, то помещаем объект в очередь
                    if (StaticVariables.nowUploadFiles == false)
                    {
                        // показываем окно загрузок              
                        mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                        mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                        await UpLoadFilesAsyncContinue(memberOfQueue);
                    }
                    else
                    {
                        StaticVariables.queueUploadFiles.Enqueue(memberOfQueue);
                        StaticVariables.showQueueUploadFiles.Enqueue(Path.GetFileName(file.Key));
                        MainWindow.queueUploadViewModel.Items.Add(Path.GetFileName(file.Key));
                    }
                }
            }
        }
        // --- Удаление блоков прогресса закачанных файлов (имя файла, проценты, прогресс и крестик для остановки прогресса) ---
        public static async void DeleteProugressFinishedFiles(object sender, RoutedEventArgs e)
        {
            var grids = mainWindow.progressFilesStackPanel.Children
                        .OfType<Grid>()
                        .Where(grid => grid.Tag is bool tagValue && tagValue == false)
                        .ToList();
            foreach (var grid in grids)
            {
                mainWindow.progressFilesStackPanel.Children.Remove(grid);
            }
        }

        // --- Скачивание файлов на ПК ---------------------------------------------------------------------------
        public async Task DownLoadFilesAsync(string ParentDirectory)
        {
            // Получаем все Border с фоном LightBlue
            List<Border> borders = mainWindow.wrapPanelView.Children
                .OfType<Border>()
                .Where(b => (b.Background as SolidColorBrush)?.Color == Colors.LightBlue)
                .ToList();

            List<string> folders = new List<string>();
            List<string> folders2 = new List<string>();

            List<string> filesSmall = new List<string>();
            List<string> filesLarge = new List<string>();

            foreach (var border in borders)
            {
                string pathElem = border.ToolTip.ToString();

                if (pathElem.EndsWith("/")) folders.Add(pathElem);
                else
                {
                    long fileSize = ((FolderOrFileModel)border.Tag).Size;
                    if (fileSize <= 16 * 1024 * 1024) filesSmall.Add(pathElem);
                    else filesLarge.Add(pathElem);
                }
            }


            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Получаем имя папки на ПК, в которую загружаем контент 
                string pathFolderPC = dialog.FileName.Replace('\\', '/');


                // создание папок на ПК и рекурсия всех папок с получением всех файлов
                if (folders.Count != 0)
                {
                    foreach (var folder in folders)
                    {
                        var resust = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, folder, "");

                        foreach (var item in resust.listObjects)
                        {
                            //Output.WriteLine(item.Key);
                            if (item.Key.EndsWith("/")) folders2.Add(item.Key);
                            else
                            {
                                if (item.Size <= 16 * 1024 * 1024) filesSmall.Add(item.Key);
                                else filesLarge.Add(item.Key);
                            }
                        }
                    }

                    List<string> pathCreateFoldersPC = new List<string>();

                    foreach (var folder in folders.Concat(folders2).ToList())
                    {
                        string pathCreateFolderPC = "";
                        if (ParentDirectory == "") pathCreateFolderPC = Path.Combine(pathFolderPC, folder).Replace('\\', '/');
                        else pathCreateFolderPC = folder.Replace(ParentDirectory, pathFolderPC);

                        pathCreateFoldersPC.Add(pathCreateFolderPC);
                    }

                    foreach (var folder in pathCreateFoldersPC)
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                    }
                }// создание папок закончено

                // Создание словаря для отправки малых файлов на скачивание
                Dictionary<string, string> filesSmallToDownload = new Dictionary<string, string>();
                if (filesSmall.Count != 0)
                {
                    foreach (var fiele in filesSmall)
                    {
                        string pathSaveFilePC = "";
                        if (ParentDirectory == "") pathSaveFilePC = Path.Combine(pathFolderPC, fiele).Replace('\\', '/');
                        else pathSaveFilePC = fiele.Replace(ParentDirectory, pathFolderPC);

                        filesSmallToDownload[fiele] = pathSaveFilePC;
                    }
                }

                // если словарь с файлами до 16мб не пуст
                if (filesSmall.Count > 0)
                {
                    var memberOfQueue = new QueueUploadFiles("", filesSmallToDownload, "");
                    // если не идёт загрузка фалов в данный момент, то запускаем загрузку немедленно
                    // если загрузка идёт в данный момент, то помещаем объект в очередь
                    if (StaticVariables.nowDownloadFiles == false)
                    {
                        // показываем окно загрузок              
                        //mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                        //mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                        DownLoadFilesAsyncContinue(memberOfQueue);
                    }
                    else
                    {
                        StaticVariables.queueDownloadFiles.Enqueue(memberOfQueue);
                        //StaticVariables.showQueueDownloadFiles.Enqueue("Быстрая загрузка файлов до 16 мб");
                        //MainWindow.queueDownloadViewModel.Items.Add("Быстрая загрузка файлов до 16 мб");
                    }
                }
                // если словарь с файлами свыше 16мб не пуст
                if (filesLarge.Count > 0)
                {
                    foreach(var fiele in filesLarge)
                    {
                        // Определение путей для скачивания файла
                        string pathSaveFilePC = "";
                        if (ParentDirectory == "") pathSaveFilePC = Path.Combine(pathFolderPC, fiele).Replace('\\', '/');
                        else pathSaveFilePC = fiele.Replace(ParentDirectory, pathFolderPC);

                        var memberOfQueue = new QueueUploadFiles("", null, fiele, pathSaveFilePC);
                        // если не идёт загрузка фалов в данный момент, то запускаем загрузку немедленно
                        // если загрузка идёт в данный момент, то помещаем объект в очередь
                        if (StaticVariables.nowDownloadFiles == false)
                        {
                            // показываем окно загрузок              
                            //mainWindow.toggleButtonShowWindowUpload.IsChecked = true;
                            //mainWindow.progressFilesBorder.Visibility = Visibility.Visible;
                            DownLoadFilesAsyncContinue(memberOfQueue);
                        }
                        else
                        {
                            StaticVariables.queueDownloadFiles.Enqueue(memberOfQueue);
                            //StaticVariables.showQueueDownloadFiles.Enqueue("Быстрая загрузка файлов до 16 мб");
                            //MainWindow.queueDownloadViewModel.Items.Add("Быстрая загрузка файлов до 16 мб");
                        }
                    }
                }
            }
        }
        public async static Task DownLoadFilesAsyncContinue(QueueUploadFiles memberOfQueue)
        {
            if (memberOfQueue.FilesForUpload != null)
            {
                //DialogWindows.CreateProgressBlock(mainWindow.progressFilesBorder, mainWindow.progressFilesStackPanel, "Быстрая загрузка файлов до 16 мб");
                //mainWindow.progressFilesScrollVewer.PageDown();
                await new ContentRequestAdvanced().DownloadFilesAdvancedAsync(memberOfQueue.FilesForUpload);
            }
            else
            {
                //DialogWindows.CreateProgressBlock(mainWindow.progressFilesBorder, mainWindow.progressFilesStackPanel, Path.GetFileName(memberOfQueue.FileLargeForUpload));
                //mainWindow.progressFilesScrollVewer.PageDown();
                await new ContentRequestAdvanced().DownloadLargeFileAdvancedAsync(memberOfQueue.FileLargeForUpload, memberOfQueue.PathSaveToPC);
            }
        }

        // --- Скачивание картинок на ПК для превью или просмотра ---------------------------------------------------------------------------
        public async Task DownLoadPicturesAsync(string pathOpenFolderInCloud)
        {
            Dictionary<string, string> filePictures = new Dictionary<string, string>();

            if (StaticVariables.listPicturesFilesOnly.Count != 0)
            {
                // путь к папке для сохранения картинок из облака на ПК
                string pathSaveFolderPicturesPC = Path.Combine(Settings.pathFolderSavePictures, StaticVariables.listPicturesFilesOnly[0].ParentDirectory).Replace('\\', '/');

                // Создание директории, если её нет
                if (!Directory.Exists(pathSaveFolderPicturesPC))
                {
                    try
                    {
                        Directory.CreateDirectory(pathSaveFolderPicturesPC);
                    }
                    catch (Exception ex)
                    {
                        await DialogWindows.InformationWindow("Ошибка создания папки: " + ex.Message);
                        return;
                    }
                }

                List<string> picturesDownloaded = new List<string>();
                
                foreach (var elem in StaticVariables.listPicturesFilesOnly)
                {
                    string pathSaveFilePC = Path.Combine(Settings.pathFolderSavePictures, elem.Path);
                    // если файл не скачан, то скачать
                    if (!File.Exists(pathSaveFilePC))
                    {
                        filePictures[elem.Path] = pathSaveFilePC;
                    }
                    else
                    {
                        picturesDownloaded.Add(elem.Path);
                    }
                }

                // зогрузка превью из скачанных файлов
                foreach (var file in picturesDownloaded)
                {
                    await Task.Delay(10);
                    await ChengeViewWindow.FillElemWindowPictures(file);
                }

            }

            //Output.WriteLine("Файлы:");
            //foreach (var file in filePictures)
            //{
            //    Output.WriteLine(file.Key);
            //    Output.WriteLine(file.Value);
            //    Output.WriteLine("---------");
            //}
            //Output.WriteLine("Файлы2:");
            //foreach (var file in StaticVariables.listFoldersAndFiles)
            //{
            //    if (Services.CheckOnPicture(Path.GetExtension(file.Path)))
            //    {
            //        Output.WriteLine(file.Path);
            //        Output.WriteLine("---------");
            //    }
            //}

            // скачивание картинок, с последующем представлением их в превью
            if (filePictures.Count > 0)
            {
                // быстрое отображение - не скачивать картинки и не отображть превью, если зажата кнопка: Z
                if (!Keyboard.IsKeyDown(Key.Z))
                {
                    await new ContentRequestAdvanced().DownloadFilesAdvancedAsync(filePictures, true);
                }
            }
            
        }

        
    }
}
