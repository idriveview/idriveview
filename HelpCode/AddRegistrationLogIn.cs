using IDriveView.AddWindows;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.Models;
using IDriveView.WorkClasses;
using MaterialDesignThemes.Wpf;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDriveView.HelpCode
{
    internal class AddRegistrationLogIn
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        // --- Заполнить список пользователей ---------------------------------------------------
        public static async Task<(bool Success, string Message)> FillListUsers()
        {
            try
            {
                // Очистка всех дочерних элементов StackPanel
                mainWindow.accountViewStackPanel.Children.Clear();

                var result = await AddRegistrationWorkJson.GetListUsersAsync();

                List<UserIDrive> userIDrives = result.userIDrives;

                for (int a = 0; a < userIDrives.Count; a++)
                {
                    DockPanel dockPanel = new DockPanel { LastChildFill = true };

                    // Создаём TextBlock
                    var myTextBlock = new TextBlock
                    {
                        Padding = new Thickness(5),
                        Margin = new Thickness(0, 5, 0, 5),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = (Brush)mainWindow.TryFindResource("PrimaryHueMidBrush"),
                        Style = (Style)mainWindow.FindResource("UnderlineOnHoverTextBlock"),
                        Cursor = Cursors.Hand,
                        Text = userIDrives[a].Name,
                    };
                    myTextBlock.MouseLeftButtonUp += MyTextBlock_MouseLeftButtonUp;
                    async void MyTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
                    {
                        TextBlock textBlock = (TextBlock)sender;
                        await FillWindow.EnterUserAccount(textBlock.Text);
                    }

                    Border border = new Border
                    {
                        Padding = new Thickness(4, 4, 5, 5),
                        Style = (Style)mainWindow.FindResource("ColorChangeHovering"),
                        Width = 24,
                        Height = 24,
                        Tag = userIDrives[a].Name
                    };
                    border.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                    async void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
                    {
                        Border borderClick = (Border)sender;
                        await FillWindow.DeleteUser(borderClick.Tag.ToString());
                    }

                    // Создание иконки Material Design
                    PackIcon packIcon = new PackIcon { Kind = PackIconKind.CloseCircleOutline };

                    DockPanel.SetDock(border, Dock.Right);
                    border.Child = packIcon;

                    // Добавление иконки в DockPanel
                    dockPanel.Children.Add(border);
                    dockPanel.Children.Add(myTextBlock);

                    mainWindow.accountViewStackPanel.Children.Add(dockPanel);
                }
                // Создаём TextBlock
                var myTextBlock2 = new TextBlock
                {
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 5, 0, 5),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)mainWindow.TryFindResource("PrimaryHueMidBrush"),
                    Style = (Style)mainWindow.FindResource("UnderlineOnHoverTextBlock"),
                    Text = "+ Добавить"
                };
                myTextBlock2.MouseLeftButtonUp += mainWindow.OpenWindowRegistration;
                mainWindow.accountViewStackPanel.Children.Add(myTextBlock2);

                if(result.Success) return (true, "Список пользователей успешно загружен");
                else return (false, "Ошибка получения данных списка пользователей");
            }
            catch (Exception ex)
            {
                await Logger.Log($"Ошибка заполнения пользователей: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // --- Валидация введённых данных при регистрации ---
        public static (bool Success, string Message) ValidateRegistration(UserIDrive userIDrive, LogIn loginWindow)
        {
            var context = new ValidationContext(userIDrive);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = Validator.TryValidateObject(userIDrive, context, results, true);
            if (!isValid)
            {
                StringBuilder errorText = new StringBuilder();
                foreach (var error in results)
                {
                    // Выводим сообщение и свойство, к которому оно относится (если нужно)
                    //string propertyName = error.MemberNames.Any() ? error.MemberNames.First() : "Общая ошибка";
                    //Output.Write($"Ошибка в поле '{propertyName}': {error.ErrorMessage}");
                    errorText.AppendLine(error.ErrorMessage);
                }
                return (false, errorText.ToString());
            }
            else
            {
                return (true, "");
            }
        }
        // --- Создаём главый Bucket - main-bucket, если его нет ----------------------------------------------------------
        public static async Task<(bool Success, string Message)> CreateOrCheckBucket(string nameBucket)
        {
            try
            {
                //var result = await new BucketRequest().GetListBucketsAsync();
                //if (result.listBuckets == null) return (false, "При создании главного Bucket, не получилось получить список Buckets");
                var resultCreating = await new BucketRequest().CreateBucketAsync(nameBucket);
                return(true, "");
                //bool exists = result.listBuckets.Exists(name => name.BucketName == nameBucket);

                //if (exists)
                //{
                //    MessageBox.Show("Bucket найден!");
                //    return (true, "Главный Bucket уже существует!");
                //}
                //else
                //{
                //    MessageBox.Show("Bucket не найден.");
                //    return (true, "Главный Bucket успешно создан!");
                //}
                
            }
            catch (Exception ex)
            {
                return (false, $"Что-то пошло не так, при создании главного Bucket. Ошибка: {ex.Message}");
            }
        }
        // -----------------------------------------------------------------------------------------------------------------
        // Проверка пользователя на существование и на RememberMe (если использовать: Properties.Settings.Default.Properties)
        // Пока, как запасной вариант.
        //private record class UserRememberMe(string Username, string Password, bool RememberMe);
        public static string CheckExistsOrRememberMe(string name)
        {
            var settings = Properties.Settings.Default.Properties;
            int countUsers = settings.Count / 3;

            string[] Usernames = new string[countUsers];
            string[] Passwords = new string[countUsers];
            bool[] RememberMes = new bool[countUsers];
            foreach (SettingsProperty property in settings)
            {
                string[] parts = property.Name.Split('_');
                string key = parts[0];
                int index = int.Parse(parts[1]);

                switch (key)
                {
                    case "Username":
                        Usernames[index] = (string)Properties.Settings.Default[property.Name];
                        break;
                    case "Password":
                        Passwords[index] = (string)Properties.Settings.Default[property.Name];
                        break;
                    case "RememberMe":
                        RememberMes[index] = (bool)Properties.Settings.Default[property.Name];
                        break;
                }
            }
            //for(int a = 0; a < countUsers; a++)
            //{
            //    Output.Write(Usernames[a]);
            //    Output.Write(Passwords[a]);
            //    Output.Write(RememberMes[a].ToString());
            //}

            int index2 = Array.FindIndex(Usernames, i => i == name);
            if (index2 == -1) return null;
            else if (RememberMes[index2]) return Passwords[index2];
            else return "";
        }
    }
}
