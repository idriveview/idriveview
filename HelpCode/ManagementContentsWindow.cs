using System.Threading.Tasks;
using System.Windows;

namespace IDriveView.HelpCode
{
    class ManagementContentsWindow
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;
        // --- Меняем кнопку входа в аккаунт на кнопку выхода ------------------------------------------------
        public static async Task ChangeButtonLoginToLogout(string name, string email, bool exit=false)
        {
            if (!exit)
            {
                mainWindow.loginButton.Visibility = Visibility.Collapsed;
                mainWindow.personalOffice.Visibility = Visibility.Visible;
                mainWindow.personalOffice.Content = name;
                if (email.Length > 2)
                    mainWindow.loginLabel.Content = email.Substring(0, 2).ToUpper();
                else
                    mainWindow.loginLabel.Content = "";
            }
            else
            {
                await ChangeButtonLogoutToLogoin("Войти");
            }
        }
        // --- Меняем кнопку выхода из аккаунта на кнопку входа ------------------------------------------------
        public static async Task ChangeButtonLogoutToLogoin(string nameloginButton="")
        {
            mainWindow.loginButton.Visibility = Visibility.Visible;
            mainWindow.personalOffice.Visibility = Visibility.Collapsed;
            mainWindow.personalOffice.Content = "";
            mainWindow.loginLabel.Content = "";
            // так получилось, что когда меняется пользователь, то название кнопки меняеся на "Войти", а потом на нового пользователя
            // эта процедура спасает от этого, и кнопка не мигает, при смене пользователя
            mainWindow.loginButton.Content = nameloginButton;
        }
        // --- Переходим в режим отоборажения контента: Grid (по умолчанию) -------------------------------
        public static async Task VeiwGridWindow()
        {
            mainWindow.viewGridOutline.Visibility = Visibility.Visible;
            mainWindow.viewAgendaOutline.Visibility = Visibility.Collapsed;
            mainWindow.stackPanelView.Visibility = Visibility.Collapsed;
        }
        // --- Переходим в режим отоборажения контента: Line (по умолчанию) -------------------------------
        public static async Task VeiwLineWindow()
        {
            mainWindow.viewGridOutline.Visibility = Visibility.Collapsed;
            mainWindow.viewAgendaOutline.Visibility = Visibility.Visible;
            mainWindow.stackPanelView.Visibility = Visibility.Visible;
        }
    }
}
