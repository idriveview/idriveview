using IDriveView.AddWindows;
using IDriveView.CreateClient;
using IDriveView.HelpCode;
using IDriveView.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace IDriveView.WorkClasses
{
    class OpenPersonalOfficeUser
    {
        private static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        // Открыть окно: Личный кабинет и заполнить данные из облака
        public static void OpenPersonalOffice(object sender, RoutedEventArgs e)
        {
            Button buttonClick = (Button)sender;
            // условие работает, если нажата кнопка "Личный кабинет" в вверху приложения
            if (buttonClick != null && buttonClick.Name == "personalOffice")
            {
                if (mainWindow.personalOfficeUser.Visibility == Visibility.Visible)
                {
                    mainWindow.personalOfficeUser.Visibility = Visibility.Collapsed;
                    return;
                }
                else mainWindow.personalOfficeUser.Visibility = Visibility.Visible;

                // Ставим галочку, если вход по паролю
                // записываем пользователя в статическую переменную
                var userIDriveJson = StaticVariables.IDriveOnDuty;
                // проверяем на RememberMe
                var Key = Properties.Settings.Default.KeyCrypt;
                var RememberMe = ConfigurationAES.Decrypt(userIDriveJson.RememberMe, Key);
                if (RememberMe == "True") mainWindow.enterPassword.IsChecked = false;
                else mainWindow.enterPassword.IsChecked = true;
            }
            
            // получаем пользователя из статической переменной
            var userIDrive = StaticVariables.currentUserInformation;
            // из Базы данных получаем пользователя
            var user = MainWindow.dataBase.Users.FirstOrDefault(w => w.Name == userIDrive.Name);
            if (user == null) return;

            string textInfo = userIDrive.Name + "\n" +
                              "Дата создания: " + user.StartRegistration + "\n" +
                              "Email: " + userIDrive.Email + "\n" +
                              "Endpoint: " + userIDrive.Endpoint + "\n" +
                              "AccessKeyID: " + userIDrive.AccessKeyID;
            mainWindow.userInformation.Text = textInfo;

            //mainWindow.pathFolderSavePictures.Text = user.PathSavePictures;

            // тариф
            mainWindow.tarifPlain.Text = user.Tariff.Split(' ')[0];
            if(user.Tariff.Split(' ')[1] == "Гб") mainWindow.tarifGbTb.SelectedIndex = 0;
            else mainWindow.tarifGbTb.SelectedIndex = 1;

            // получаем данные о трафике пользователя
            WorkDateBase.GetData(null, null);
        }

        // Закрыть окно: Личный кабинет
        public static void ClosePersonalOffice(object sender, RoutedEventArgs e)
        {
            mainWindow.personalOfficeUser.Visibility = Visibility.Collapsed;
        }

        // Изменить вход по паролю или без (изменить RemamberMe)
        public static async void ChengeEnterPassword(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            // меняем в конфигурациях настройки входа в приложение: RemamberMe
            await AddRegistrationWorkJson.ChengeRemaberMe(StaticVariables.IDriveOnDuty, mainWindow.enterPassword.IsChecked);
        }

        // Заполнить данные о трафике пользователя
        public static async Task FillTraficUser(DateTime date, long sizeDownload, long sizeUpload, long sizeWatch)
        {
            mainWindow.startDateTraffic.Content = date.ToString(); 
            mainWindow.uploadUserInformation.Content = Services.BiteToKbToMbToGb("", sizeUpload); 
            mainWindow.downloadUserInformation.Content = Services.BiteToKbToMbToGb("", sizeDownload);
            mainWindow.downloadWatchVideo.Content = Services.BiteToKbToMbToGb("", sizeWatch);
        }

        // Сбросить данные о трафике пользователя
        public static async Task ResetTraficUser(DateTime date, long sizeDownload, long sizeUpload, long sizeWatch)
        {
            mainWindow.startDateTraffic.Content = date.ToString();
            mainWindow.uploadUserInformation.Content = Services.BiteToKbToMbToGb("", sizeUpload);
            mainWindow.downloadUserInformation.Content = Services.BiteToKbToMbToGb("", sizeDownload);
            mainWindow.downloadWatchVideo.Content = Services.BiteToKbToMbToGb("", sizeWatch);
        }

        // Выйти из сессии
        public static async void ExitSessionUser(object sender, RoutedEventArgs e)
        {
            ClosePersonalOffice(null, null);// Закрываем окно личного кабинета
            //Services.DeleteFolderPreview(); // Удаляем папку для хранения файлов
            Services.ClearFolderPreview(); // Очищает папку для хранения файлов

            //Settings.pathFolderSavePictures = ""; // сбрасываем путь к папке для хранения файлов

            // Меняем кнопку входа на кнопку выхода
            ManagementContentsWindow.ChangeButtonLoginToLogout("", "", true);

            // переходим в режим отоборажения контента: Grid (по умолчанию)
            await ManagementContentsWindow.VeiwGridWindow();

            // сброс имени файла в шапке окна
            mainWindow.pathToElement.Content = "";

            // Удаляем все дочерние элементы из wrapPanel и из поле пути к выбранной папке ( pathToSelectFolder )
            mainWindow.pathToSelectFolder.Children.Clear();
            mainWindow.wrapPanelView.Children.Clear();
            mainWindow.stackPanelView.Children.Clear();

            // обнуляем прогресс занятого пространства
            mainWindow.usedSpace.Text = "0";
            mainWindow.availableSpace.Text = "0";
            mainWindow.progressSpace.Value = 25;

            // обнуляем футер
            mainWindow.countFolder.Text = "0";
            mainWindow.countFiles.Text = "0";
            mainWindow.countElement.Text = "0";
            mainWindow.textProgressUpload.Text = "";

            // очищаем список загрузок
            DownUpContent.DeleteProugressFinishedFiles(null, null);
            // закрываем окно загрузок
            mainWindow.progressFilesBorder.Visibility = Visibility.Collapsed;
            mainWindow.toggleButtonShowWindowUpload.IsChecked = false;

            // делаем окно не кликабельным
            mainWindow.DockPanelContentMain.IsHitTestVisible = false;

            // получаем данные о трафике пользователя (этот метод ещё упорядочивает данные в Базе данных)
            WorkDateBase.GetData(null, null);
        }
    }
}
