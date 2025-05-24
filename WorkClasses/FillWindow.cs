using IDriveView.AddWindows;
using IDriveView.CreateClient;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using System.IO;
using System.Windows;

namespace IDriveView.WorkClasses
{
    class FillWindow
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        private static bool checkEnterAccount = true;

        // --- Войти в аккаунт пользователя (Часть 1 Подготовка данных) -------------------------------------------
        public static async Task EnterUserAccount(string nameUser)
        {
            await DialogWindows.CreateProgressWindow("Получение данных...");// окно прогресса - открыто

            // Очищаем окно и подготавливаем программу, перед входом нового пользователя
            OpenPersonalOfficeUser.ExitSessionUser(null, null);          

            checkEnterAccount = true; // отвечает за получение тарифа и трафика пользователя (в конце загрузки окна)

            // получаем список пользователей
            var listUsers = await AddRegistrationWorkJson.GetListUsersAsync();
            if(!listUsers.Success)
            {
                await DialogWindows.InformationWindow(listUsers.Message);
                return;
            }
            // получаем пользователя по имени
            var userIDrive = listUsers.userIDrives.Where(i => i.Name == nameUser).FirstOrDefault();
            // записываем пользователя в статическую переменную
            StaticVariables.IDriveOnDuty = userIDrive;

            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

            // проверяем на RememberMe
            var Key = Properties.Settings.Default.KeyCrypt;
            var RememberMe = ConfigurationAES.Decrypt(userIDrive.RememberMe, Key);
            if (RememberMe == "True")
            {
                string Password = ConfigurationAES.Decrypt(userIDrive.Password, Key);
                await Enter(Password);
            }
            else
            {
                await DialogWindows.PasswordWindow(Enter);
            }
        }
        // --- Вход в аккаунт пользователя (Часть 2 Вход в облако) ---------------------------------------------
        public static async Task Enter(string Password)
        {
            if (Password == null || Password == string.Empty)
            {
                Password = "txiAPu6bd5ZdlcP0E4/CbwQHCuBMzxq5Cpg9ezEy/u9sAUlSzNXHu2h7ADWk6u3Q";// заведомо неверный пароль
            }

            // Меняем кнопку выхода из аккаунта на кнопку входа 
            await ManagementContentsWindow.ChangeButtonLogoutToLogoin();
            await DialogWindows.CreateProgressWindow("Вход в аккаунт...");// окно прогресса - открыто

            // извлекаем пользователя в статическую переменную
            UserIDrive userIDrive = StaticVariables.IDriveOnDuty;
            // заполняем значение пароля (важно для случая, когда пароль вводится при входе в аккаунт)
            var Key = Properties.Settings.Default.KeyCrypt;
            userIDrive.Password = await ConfigurationAES.EncryptAcync(Password, Key);// для метода: S3ClientFactoryю.s3Client

            // сохранить пользователя в статической переменной для использования в OpenPersonalOfficeUser
            StaticVariables.currentUserInformation = new UserInformation
            {
                Name = userIDrive.Name,
                Email = ConfigurationAES.Decrypt(userIDrive.Email, Password),
                Endpoint = ConfigurationAES.Decrypt(userIDrive.Endpoint, Password),
                AccessKeyID = ConfigurationAES.Decrypt(userIDrive.AccessKeyID, Password)
            };

            //Output.WriteLine(userIDrive.Name);
            //Output.WriteLine(ConfigurationAES.Decrypt(userIDrive.Email, Password));
            //Output.WriteLine(ConfigurationAES.Decrypt(userIDrive.Endpoint, Password));
            //Output.WriteLine(ConfigurationAES.Decrypt(userIDrive.AccessKeyID, Password));
            //Output.WriteLine(ConfigurationAES.Decrypt(userIDrive.SecretAccessKey, Password));
            //Output.WriteLine(Password);
            //Output.WriteLine(ConfigurationAES.Decrypt(Settings.IDriveOnDuty.Password, Key));

            // проверяем любое поле на значение (например, Email. Оно ещё нам пригодится)
            var checkEmailString = ConfigurationAES.Decrypt(userIDrive.Email, Password);

            if (checkEmailString != null && checkEmailString != string.Empty)
            {
                // ПРОВЕРКА ДАННЫХ ВХОДА В ОБЛАКО
                var result = await S3ClientFactory.CheckLoginS3ClientAsync();              
                if (result.Success)
                {
                    await Logger.Log($"Пользователь {userIDrive.Name} успешно вошёл в аккаунт");

                    // Меняем кнопку входа на кнопку выхода
                    ManagementContentsWindow.ChangeButtonLoginToLogout(userIDrive.Name, checkEmailString);

                    // Заполняем переменную текущего пользователя
                    StaticVariables.currentUser = userIDrive.Name;
                    Output.WriteLine("sssssssssssssssssssssssssssss");

                    // Получаем имя бакета пользователя из облака
                    var nameBucket = await new BucketRequestAdvanced().GetFirstBucketAsync();
                    if (nameBucket == null) return;
                    Settings.mainBucket = nameBucket;
                    await Task.Delay(500);

                    // Заполняем окно контентом
                    await FillContentWindow("", "/");
                }
                else
                {
                    await Logger.Log($"Пользователь {userIDrive.Name} не вошёл в аккаунт");
                    await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
                    mainWindow.loginButton.Content = "Войти";
                    await DialogWindows.InformationWindow(result.Message);
                }
            }
            else
            {
                await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
                mainWindow.loginButton.Content = "Войти";
                await DialogWindows.InformationWindow("Ошибка входа в аккаунт. Проверьте пароль.");
            }

        }

        // --- Вход в аккаунт пользователя (Часть 3) или Подготовка данных для отображения контента ---------------------------------------------
        /// <summary>
        /// pathOpenFolder: путь к папке ("/" - вывод объектов всего бакета), 
        /// delimiter: "/" - ограничиваемся первым уровнем, "" - выводим все объекты
        /// </summary>
        public static async Task FillContentWindow(string pathOpenFolder, string delimiter, bool createButton = true)
        {
            // Удаляем все дочерние элементы из wrapPanel и stackPanelView
            mainWindow.wrapPanelView.Children.Clear();
            mainWindow.stackPanelView.Children.Clear();
            mainWindow.scrollViewer.ScrollToTop(); // скролл возвращаем в начало
            // Очистка списка контена перед открыванием папки
            StaticVariables.listFoldersAndFiles.Clear();
            StaticVariables.listPicturesFilesOnly.Clear();
            // обнуляем отображение количества файлов и папок в папке
            mainWindow.countFolder.Text = "0";
            mainWindow.countFiles.Text = "0";
            mainWindow.countElement.Text = "0";

            // Прежде всего отключаем действующий token (очень важно при переходе между папками)
            new ContentRequestAdvanced().CancelDownloadFilesAdvancedAsync(null, null);


            // Список папкок для контекстного меню
            var listContextMenuFolders = new List<string>();

            // Создание списка объектов
            var list = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, pathOpenFolder, delimiter);
            if (list.Success)
            {
                int count1 = 0;
                // Список папок
                foreach (var pathFolder in list.listFolders)
                {
                    string folderName = Path.GetFileName(pathFolder.TrimEnd('/', '\\'));
                    string directoryName = Path.GetDirectoryName(pathFolder.TrimEnd('/', '\\'));

                    StaticVariables.listFoldersAndFiles.Add(new FolderOrFileModel("folder", folderName, pathFolder, directoryName, 0));
                    listContextMenuFolders.Add(folderName);
                    count1++;
                }
                int count2 = 0;
                // Список файлов
                foreach (var pathFile in list.listObjects)
                {
                    string fileName = Path.GetFileName(pathFile.Key);
                    string directoryName = Path.GetDirectoryName(pathFile.Key);

                    StaticVariables.listFoldersAndFiles.Add(new FolderOrFileModel("file", fileName, pathFile.Key, directoryName, pathFile.Size));

                    // сохраняем отдельно список файлов - картинок
                    if (Services.CheckOnPicture(Path.GetExtension(fileName)))
                    {
                        StaticVariables.listPicturesFilesOnly.Add(new FolderOrFileModel("file", fileName, pathFile.Key, directoryName, pathFile.Size));
                    }
                    count2++;
                }

                // Сортировка с использованием естественного порядка
                StaticVariables.listFoldersAndFiles.Sort((x, y) => Services.NaturalSortComparer(x.Path, y.Path));
                //StaticVariables.listPicturesFilesOnly.Sort((x, y) => Services.NaturalSortComparer(x.Path, y.Path));
                Output.WriteLine("StaticVariables.listPicturesFilesOnly.Count: " + StaticVariables.listPicturesFilesOnly.Count);
                // Сортировка списка: сначала идут "folder", затем "files", сохраняя порядок внутри групп
                StaticVariables.listFoldersAndFiles = StaticVariables.listFoldersAndFiles
                    .OrderBy(item => item.Type == "folder" ? 0 : 1) // Сначала "folder"
                    .ThenBy(item => StaticVariables.listFoldersAndFiles.IndexOf(item)) // Сохраняем исходный порядок
                    .ToList();

                // отображение количества файлов и папок в папке
                mainWindow.countFolder.Text = count1.ToString();
                mainWindow.countFiles.Text = count2.ToString();
                mainWindow.countElement.Text = (count1 + count2).ToString();

                //Output.WriteLine("Список объектов выбранной директории: " + pathOpenFolder);
                //foreach (var item in StaticVariables.listFoldersAndFiles)
                //{
                //    Output.WriteLine(item.Type + " -|- " + item.Name + " -|- " + item.Path + " -|- " + item.ParentDirectory + " -|- " + item.Size);
                //}
                // Отображение коли
                // Отображение объектов в окне
                if (mainWindow.viewGridOutline.Visibility == Visibility.Visible)
                {
                    await ChengeViewWindow.ViewContenGridOrLinetWindow(Settings.SetGridView());
                }
                else
                {
                    await ChengeViewWindow.ViewContenGridOrLinetWindow(Settings.SetLineView());
                }

                // создание кнопки в панели: pathToSelectFolder - навигация по пройденным папкам
                CreateElement.PathToSelectedFolder(mainWindow, mainWindow.pathToSelectFolder, pathOpenFolder, listContextMenuFolders, createButton);

                // сделать окно приложения активым
                mainWindow.DockPanelContentMain.IsHitTestVisible = true;
            }
            else
            {
                await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
                if (list.listObjects == null)
                {
                    await DialogWindows.InformationWindow("Проверьте соединение с интернетом");
                    return;
                }
                await DialogWindows.InformationWindow("Ошибка получения списка объектов. Посмотрите логи");
            }

            // Установливаем путь сохранения превью-картинок, проверяем тариф
            if (checkEnterAccount)
            {
                await WorkDateBase.SetAccount(StaticVariables.currentUser);
                checkEnterAccount = false;
            }

            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
            //await Task.Delay(500);
            // после заполнение всего окна значками файлов
            // скачать все картинки в определённую папку для превью или просмотра
            _ = new DownUpContent().DownLoadPicturesAsync(pathOpenFolder);
        }
        // --- Удаление пользователя -------------------------------------------
        public static async Task DeleteUser(string nameUser)
        {
            var result = await AddRegistrationWorkJson.RemoveUser(nameUser);
            if(result.Success)
            {
                // Удаляем пользователя из Базы данных
                WorkDateBase.DeleteUser(nameUser);

                // Если происходит удаление текущего пользователя, то сбрасываем данные в окне
                if (mainWindow.personalOffice.Content.ToString() == nameUser)
                {
                    OpenPersonalOfficeUser.ExitSessionUser(null, null);
                }

                // Создаём список пользователей в окне пользователей
                var (success2, message2) = await AddRegistrationLogIn.FillListUsers();
                await Logger.Log(success2 ? $"Пользователь успешно удалён" : $"Ошибка: {message2}");
                if (!success2) await DialogWindows.InformationWindow(message2); 
            }
            else
            {
                await Logger.Log($"Пользователь не был удалён. Ошибка: {result.Message}");
                await DialogWindows.InformationWindow(result.Message);
            }
        }
    }
}
