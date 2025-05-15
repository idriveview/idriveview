using IDriveView.AddWindows;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IDriveView.WorkClasses
{
    // Класс работы с локальной базой данных
    public class WorkDateBase
    {
        private static MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        // --- Создать пользователя в базе данных --------------------------------
        public static void AddUser(string name)
        {
            DateTime now = DateTime.Now;
            DateTime withoutMilliseconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            MainWindow.dataBase.Users.Add(new Models.User
            {
                Name = name,
                StartRegistration = withoutMilliseconds,
                PathSavePictures = Settings.pathFolderSavePictures,
                Tariff = Settings.tariff,
                DateTimeStart = withoutMilliseconds
            });
            MainWindow.dataBase.SaveChanges();
        }
        // --- Удалить пользователя из базы данных ----------------------------------
        public static void DeleteUser(string name)
        {
            var user = MainWindow.dataBase.Users
                        .FirstOrDefault(w => w.Name == name);
            // если нет такого имени, выходим
            if (user is null) return;
            MainWindow.dataBase.Users.Remove(user);
            MainWindow.dataBase.SaveChanges();
        }
        // --- Трафик bytes выгруженные из облака --------------------------------
        public static async Task AddByteDownload(long bytes)
        {
            MainWindow.dataBase?.UsersDownloads?
                .FirstOrDefault(w => w.User.Name == StaticVariables.currentUser)?
                .DownloadList.Add(bytes);
            MainWindow.dataBase?.SaveChanges();
        }
        // --- Трафик bytes загруженные в облако --------------------------------------
        public static async Task AddByteUpload(long bytes)
        {
            MainWindow.dataBase?.UsersUploads?
                .FirstOrDefault(w => w.User.Name == StaticVariables.currentUser)?
                .UploadList.Add(bytes);
            MainWindow.dataBase?.SaveChanges();
        }
        // --- Трафик bytes просмотренных видео --------------------------------------
        public static async Task AddByteVideo(long bytes)
        {
            MainWindow.dataBase?.WatchDownloads?
                .FirstOrDefault(w => w.User.Name == StaticVariables.currentUser)?
                .WatchList.Add(bytes);
            MainWindow.dataBase?.SaveChanges();
        }

        // --- Обновить трафик и получить данные пользователя ----------------------
        public static async void GetData(object sender, RoutedEventArgs e)
        {
            if (StaticVariables.currentUser == null || StaticVariables.currentUser == "") return;

            // Получаем пользователя из базы данных
            var itemUser = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);

            if (itemUser is null)
            {
                Output.WriteLine("Пользователь не найден в базе данных.");
                return;
            }

            // Получение трафика
            var listBytesDown = itemUser.UserDownloadList.DownloadList;
            var listBytesWatch = itemUser.WatchDownloadList.WatchList;
            var listBytesUp = itemUser.UserUploadList.UploadList;

            itemUser.SizeDownload += listBytesDown.Sum();
            itemUser.SizeWatch += listBytesWatch.Sum();
            itemUser.SizeUpload += listBytesUp.Sum();

            listBytesDown.Clear();
            listBytesWatch.Clear();
            listBytesUp.Clear();

            MainWindow.dataBase.SaveChanges();

            await OpenPersonalOfficeUser.FillTraficUser(itemUser.DateTimeStart, itemUser.SizeDownload, itemUser.SizeUpload, itemUser.SizeWatch);

            //MessageBox.Show($"Пользователь: {StaticVariables.currentUser}\n" +
            //    $"Скачано: {Services.BiteToKbToMbToGb("", itemUser.SizeDownload)} байт\n" +
            //    $"Загружено: {Services.BiteToKbToMbToGb("", itemUser.SizeUpload)} байт");
        }
        // --- Сброс данных трафика пользователя ------------------------------------------------------------
        public static async void ResetData(object sender, RoutedEventArgs e)
        {
            string textMessage = "В результате этого действия данные трафика скачивания и загрузки обнуляться, а дата начала будет установлена на текущее время.\n" +
                "Вы уверены, что хотите продолжить?";

            // Создаём диалоговое окно с сообщением и двумя кнопками
            var dialog = new SmallDialog(textMessage);
            dialog.Owner = mainWindow;

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);

            DateTime now = DateTime.Now;
            user.DateTimeStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            user.SizeDownload = 0;
            user.SizeUpload = 0;
            user.SizeWatch = 0;
            user.UserDownloadList.DownloadList.Clear();
            user.UserUploadList.UploadList.Clear();
            user.WatchDownloadList.WatchList.Clear();

            MainWindow.dataBase.SaveChanges();

            await OpenPersonalOfficeUser.ResetTraficUser(user.DateTimeStart, user.SizeDownload, user.SizeUpload, user.SizeWatch);
        }

        // --- Настройка аккаунта при входе --------------------------------------------
        public static async Task SetAccount(string name)
        {
            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == name);
            if (user == null) return;

            // Путь к папке для сохранения картинок для превью
            //if (user.PathSavePictures == null || user.PathSavePictures == "")
            //{
            //    Settings.pathFolderSavePictures = Settings.pathFolderSavePicturesDefault;
            //    user.PathSavePictures = Settings.pathFolderSavePictures;
            //}
            //else
            //{
            //    Settings.pathFolderSavePictures = user.PathSavePictures;
            //}
            // Проверяем и устанавливаем тариф
            if (user.Tariff == null || user.Tariff == "")
            {
                user.Tariff = Settings.tariff;
            }
            // Проверяем и устанавливаем использованое пространство
            if (user.EmployedSpace == null || user.EmployedSpace == "")
            {
                user.EmployedSpace = Settings.EmployedSpace;
            }
            MainWindow.dataBase.SaveChanges();

            await SetUsedSpace(StaticVariables.currentUser);

        }
        // --- Загрузка использованного пространства в окно программы (из базы данных без подсчёта) ---
        public static async Task SetUsedSpace(string nameUser)
        {
            // из Базы данных получаем пользователя
            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == nameUser);
            if (user == null) return;
            // получаем данные о использованом пространстве из базы данных
            mainWindow.usedSpace.Text = user.EmployedSpace;
            int used = Services.KbMbGbToBite(user.EmployedSpace);
            // получает данные о тарифе
            mainWindow.availableSpace.Text = user.Tariff;
            long coefficient = user.Tariff.Split(' ')[1] == "Гб" ? 1024 * 1024 * 1024 : 1024L * 1024 * 1024 * 1024;
            long tariff = int.Parse(user.Tariff.Split(' ')[0]) * coefficient;

            // устанавливаем прогрессбар
            long procent =(long)used * 100 / tariff;
            //Output.Write($"Процент: {procent}");
            mainWindow.progressSpace.Value = (int)procent;

        }

        // --- Получение занятого пространства в облаке (перезагрузка значений прогрессбара) ---------
        public static async void ProgressSpaceBorder_Click(object sender, RoutedEventArgs e)
        {
            // прогресс включён
            mainWindow.calculationSpaceCloudBlur.Visibility = Visibility.Visible;

            var result = await new ContentRequest().ListObjectsAsync(Settings.mainBucket, "", "");
            
            var list = result.listObjects.Select(i => i.Size).ToList();

            string str = Services.BiteToKbToMbToGb("", list.Sum()).ToString();

            // из Базы данных получаем пользователя
            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);
            if (user == null) return;
            user.EmployedSpace = str;

            // обновить текст в окне
            await SetUsedSpace(StaticVariables.currentUser);

            // прогресс выключен
            mainWindow.calculationSpaceCloudBlur.Visibility = Visibility.Collapsed;
        }

        // --- Сохранить изменения тарифа --------------------------------------
        public static void SaveChengeTariff_Click(object sender, RoutedEventArgs e)
        {
            string bytes = mainWindow.tarifPlain.Text;
            string gbTb = mainWindow.tarifGbTb.SelectedIndex == 0 ? "Гб" : "Тб";
            string tariff = bytes + " " + gbTb;
            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);
            if (user == null) return;
            user.Tariff = tariff;
            MainWindow.dataBase.SaveChanges();

            mainWindow.availableSpace.Text = tariff;
        }

        // --- Выбрать путь к папке для сохранения картинок-превью (или по умолчанию) -------------
        //public static void ChoosePathFolderSavePictures_Click(object sender, RoutedEventArgs e)
        //{
        //    // Создаём новую папку
        //    string pathFolder = string.Empty;

        //    Button button = (Button)sender;

        //    // Если нажата кнопка "Выбрать папку" в окне "Личный кабинет"
        //    if (button != null && button.Name == "choosePathFolderSavePictures")
        //    {
        //        var dialog = new CommonOpenFileDialog
        //        {
        //            IsFolderPicker = true
        //        };
        //        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        //        {
        //            pathFolder = dialog.FileName;
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        pathFolder = Settings.pathFolderSavePicturesDefault;// путь по умолчанию
        //    }

        //    // Удаляем, если существует папка по старому пути
        //    if (Directory.Exists(Settings.pathFolderSavePictures))
        //    {
        //        Directory.Delete(Settings.pathFolderSavePictures, recursive: true); // recursive=true удаляет все вложенные файлы/папки
        //    }

        //    // сохранить в базе данных
        //    var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);
        //    if (user == null) return;
        //    user.PathSavePictures = pathFolder;
        //    // сохранить в настройках
        //    Settings.pathFolderSavePictures = pathFolder;
        //    // обновить текст в окне
        //    mainWindow.pathFolderSavePictures.Text = pathFolder;
        //}

        //// --- Записать использованное пространство в облако ---
        //public static async Task SaveUsedSpace()
        //{
        //    // из Базы данных получаем пользователя
        //    var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);
        //    if (user == null) return;
        //    // получаем данные о трафике пользователя
        //    user.EmployedSpace = mainWindow.usedSpace.Text;
        //}

        //// --- Изменить использованное пространство в облако ---
        //public static async Task ChengeUsedSpace()
        //{
        //    // из Базы данных получаем пользователя
        //    var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == StaticVariables.currentUser);
        //    if (user == null) return;
        //    // получаем данные о трафике пользователя
        //    user.EmployedSpace = mainWindow.usedSpace.Text;
        //}
    }
}
