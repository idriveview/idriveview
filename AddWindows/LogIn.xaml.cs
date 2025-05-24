using IDriveView.CreateClient;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using IDriveView.WorkClasses;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace IDriveView.AddWindows
{
    /// <summary>
    /// Логика взаимодействия для LogIn.xaml
    /// </summary>
    public partial class LogIn : Window
    {
        MainWindow _mainWindow;
        public LogIn(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += LogIn_Loaded;

            loginLogIn.Click += LoginLogIn_Click;// открыть окно с авторизованными пользователями
            loginRegistation.Click += LoginRegistation_Click; // регистрация в приложении
            closeLogIn.MouseLeftButtonUp += closeLogIn_Click; // закрыть окно регистрации
        }

        private void LogIn_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private async void LoginRegistation_Click(object sender, RoutedEventArgs e)
        {
            // !!! - Проверяем на наличие открытого окна Information - !!!
            var checkWindowInfo = DialogWindows.FindChildBorder(GridMain, "Information");
            if (checkWindowInfo != null) return;

            // Получение данных с формы
            string Name = loginName.Text.Trim();
            string Email = loginEmail.Text.Trim();
            string Endpoint = loginEndpoint.Text.Trim();
            string AccessKeyID = accessKeyID.Password.Trim();
            string SecretAccessKey = secretAccessKey.Password.Trim();
            string Password = passwordBox.Password.Trim();
            string RememberMe = rememberMe.IsChecked.Value.ToString();
            var userIDrive = new UserIDrive(Name, Email, Endpoint, AccessKeyID, SecretAccessKey, Password, RememberMe);
            // Валидация введённых данных
            var result = AddRegistrationLogIn.ValidateRegistration(userIDrive, this);
            if (!result.Success)
            {
                Output.WriteLine("Ошибка валидации");
                await DialogWindows.InformationWindow(result.Message, "LogIn");
                return;
            }
            Output.Write("Валидация пройдена успешно!");

            // Кодирование данных
            StaticVariables.IDriveOnDuty = new UserIDrive(
                userIDrive.Name, // Оставляем Name без изменений
                await ConfigurationAES.EncryptAcync(userIDrive.Email, userIDrive.Password),
                await ConfigurationAES.EncryptAcync(userIDrive.Endpoint, userIDrive.Password),
                await ConfigurationAES.EncryptAcync(userIDrive.AccessKeyID, userIDrive.Password),
                await ConfigurationAES.EncryptAcync(userIDrive.SecretAccessKey, userIDrive.Password),
                await ConfigurationAES.EncryptAcync(userIDrive.Password, Properties.Settings.Default.KeyCrypt),
                await ConfigurationAES.EncryptAcync(userIDrive.RememberMe, Properties.Settings.Default.KeyCrypt)
            );

            await DialogWindows.CreateProgressWindow("Проверка данных...", "LogIn");// окно прогресса - открыто

            // Проверка доступа интернета и доступа к сайту облака
            var (successInternet, messageInternet) = await InternetAvailability.CheckInternet(true, userIDrive.Endpoint);
            if (!successInternet)
            {
                await Logger.Log($"Ошибка проверки интернета: {messageInternet}");
                await DialogWindows.InformationWindow(messageInternet);
                return;
            }
            else
            {
                if(messageInternet.Contains("Неопознанное облако")) await DialogWindows.InformationWindow("Облако не опознанно, но подключение продолжится");
            }
            // Проверка данных входа в облако
            var resultCheckData = await S3ClientFactory.CheckLoginS3ClientAsync();
            if (!resultCheckData.Success)
            {
                await DialogWindows.DeleteProgressWindow("LogIn");// окно прогресса - закрыто
                await DialogWindows.InformationWindow(resultCheckData.Message, "LogIn");
                Output.Write(resultCheckData.Message);
                return;
            }
            Output.Write(resultCheckData.Message);
            // Добавление нового пользователя в JSON файл 
            if (RememberMe == "False") StaticVariables.IDriveOnDuty.Password = "password";

            var (success, message) = await AddRegistrationWorkJson.AddUser(StaticVariables.IDriveOnDuty);
            if (success)
            {
                // Создаём список пользователей в окне пользователей
                var (success2, message2) = await AddRegistrationLogIn.FillListUsers();
                await Logger.Log(success2 ? $"Новый пользователь успешно добавлен" : $"Ошибка: {message2}");
                if (!success2)
                {
                    await DialogWindows.InformationWindow(message2);
                    return;
                }

                // Создаём пользователя в Базе данных 
                WorkDateBase.AddUser(Name);

                await DialogWindows.DeleteProgressWindow("LogIn");// окно прогресса - закрыто
                Close();
            }
            else if (message == "userAlreadyExists")
            {
                Output.Write("Пользователь с таким именем уже существует!");
                await DialogWindows.DeleteProgressWindow("LogIn");// окно прогресса - закрыто
                await DialogWindows.InformationWindow("Пользователь с таким именем уже существует!", "LogIn");
            }
            else
            {
                Output.WriteLine("Ошибка: " + message);
                await DialogWindows.DeleteProgressWindow("LogIn");// окно прогресса - закрыто
                await DialogWindows.InformationWindow("Ошибка: " + message, "LogIn");
            }
        }

        // --- Открыть окно с авторизованными пользователями IDriveView -------------
        private void LoginLogIn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.accountViewBorder.Visibility = Visibility.Visible;
            Close();
        }
        // --- Перейти на сайт регистрации -----------------------------------------
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Hyperlink_GetPassword(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

        }
        //// --- Закрытие окна ошибок -------------------------------------------------------
        //private async void closeErrorBorder(object sender, RoutedEventArgs e)
        //{
        //    await DialogWindows.DeleteProgressWindow("LogIn");// окно прогресса - закрыто
        //}
        // --- Закрытие она на крестик -------------------------------------------------------
        private void closeLogIn_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Закрывает текущее окно
        }
    }
}
