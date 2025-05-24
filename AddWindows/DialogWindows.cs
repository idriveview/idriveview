using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shell;

namespace IDriveView.AddWindows
{
    class DialogWindows
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;      

        // --- Создание окна информации --------------------------------------------------------
        public static async Task InformationWindow(string message, string nameWindow = "MainWindow")
        {
            Grid parentContainer = mainWindow.GridMain;
            if (nameWindow == "LogIn")
            {
                LogIn logInWindow = Application.Current.Windows.OfType<LogIn>().FirstOrDefault();
                parentContainer = logInWindow.GridMain;
            }

            // Создаем Border
            Border informationWindow = new Border
            {
                Name = "Information",
                Background = Brushes.White,
                Width = 300,
                MinHeight = 155,
                CornerRadius = new CornerRadius(10),
                Visibility = Visibility.Visible,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 2,
                    Direction = -90,
                    Opacity = 0.5
                }
            };

            // Создаем DockPanel
            DockPanel dockPanel = new DockPanel();

            // Создаем кнопку
            Button closeThisBorder = new Button
            {
                Content = "Понятно",
                Margin = new Thickness(20, 20, 20, 30),
                Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style
            };
            closeThisBorder.Click += (sender, args) =>
            {
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, "Modal");
                // Удаляем созданное нами окно
                DeleteBorder(parentContainer, "Information");
            };

            // Создаем контейнер для кнопки
            Border buttonContainer = new Border
            {
                Child = closeThisBorder
            };
            DockPanel.SetDock(buttonContainer, Dock.Bottom);

            // Создаем TextBlock
            TextBlock messageInformation = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(20, 30, 20, 0)
            };

            // Добавляем элементы в DockPanel
            dockPanel.Children.Add(buttonContainer);
            dockPanel.Children.Add(messageInformation);

            // Добавляем DockPanel в Border
            informationWindow.Child = dockPanel;

            // Создаём "модальность" окну
            AddBorderModal(parentContainer);

            // Добавляем Border окна в родительский контейнер
            parentContainer.Children.Add(informationWindow);

            closeThisBorder.Focus();

        }
        // -- Этот метод рекурсивно ищет первого потомка указанного типа --
        public static Border FindChildBorder(DependencyObject parent, string childBorderName)
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            // Проверяем текущий элемент
            if (parent is Border border && border.Name?.ToString() == childBorderName)
            {
                return border;
            }

            // Рекурсивно проверяем всех потомков
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                Border result = FindChildBorder(child, childBorderName);

                if (result != null)
                {
                    return result; // Нашли нужный Border
                }
            }

            return null;
        }
        // -- Этот метод удаляем Border-элемент из дерева программы --
        private static void DeleteBorder(Grid parentContainer, string deleteBorderName)
        {
            Border border = FindChildBorder(parentContainer, deleteBorderName);
            if (border != null)
            {
                parentContainer.Children.Remove(border);
            }
        }
        // -- Этот метод добавляет Border-элемент "модальности" определённому Grid-элементу
        private static void AddBorderModal(Grid parentContainer, string nameModal = "Modal")
        {
            Border borderModal = new Border
            {
                Name = nameModal,
                Background = Brushes.AliceBlue,
                Opacity = 0.2
            };
            parentContainer.Children.Add(borderModal);
        }
        // --- Создание окна для введения паролля -----------------------------------------------------
        public static async Task PasswordWindow(Func<string, Task> enter)
        {
            Grid parentContainer = mainWindow.GridMain;

            // Создаем основной Border
            Border enteringPasswordWindow = new Border
            {
                Name = "enteringPasswordWindow",
                Background = Brushes.White,
                Width = 300,
                CornerRadius = new CornerRadius(10),
                Visibility = Visibility.Visible,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Добавляем эффект тени
            enteringPasswordWindow.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 2,
                Direction = -90,
                Opacity = 0.5
            };

            // Создаем DockPanel
            DockPanel dockPanel = new DockPanel
            {
                LastChildFill = true
            };

            // Создаем нижний Border с кнопкой
            Border bottomBorder = new Border();

            DockPanel.SetDock(bottomBorder, Dock.Bottom);

            Button applyButton = new Button
            {
                Name = "applyEnteringPasswordWindow",
                Margin = new Thickness(20, 20, 20, 30),
                Content = "Применить"
            };
            // Предполагается, что стиль MaterialDesignRaisedLightButton определен в ресурсах
            applyButton.Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style;

            // Применить пароль и удалить окно и его "модальность"
            applyButton.Click += async (sender, args) =>
            {
                // Находим PasswordBox в структуре элементов
                PasswordBox pwdBox = dockPanel.Children.OfType<PasswordBox>().FirstOrDefault();
                if (pwdBox != null)
                {
                    await enter(pwdBox.Password);
                    // Выводим пароль в консоль
                    //Output.WriteLine($"Введенный пароль: {pwdBox.Password}");
                }

                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, "Modal");
                // Удаляем созданное окно
                DeleteBorder(parentContainer, "enteringPasswordWindow");
            };
            bottomBorder.Child = applyButton;

            // Создаем верхний Border с кнопкой закрытия
            Border closeBorder = new Border
            {
                Name = "closeEnteringPasswordWindow",
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 30,
                Height = 30
            };
            // Предполагается, что стиль ColorChangeHoveringClose определен в ресурсах
            closeBorder.Style = Application.Current.FindResource("ColorChangeHoveringClose") as Style;

            //Удалить созданное окно (закрытие на крестик Х)
            closeBorder.MouseLeftButtonUp += (sender, args) =>
            {
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, "Modal");
                // Удаляем созданное окно
                DeleteBorder(parentContainer, "enteringPasswordWindow");
            };
            DockPanel.SetDock(closeBorder, Dock.Top);

            TextBlock closeText = new TextBlock
            {
                Text = "✕",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };
            closeBorder.Child = closeText;

            // Создаем PasswordBox
            PasswordBox passwordBox = new PasswordBox
            {
                Name = "passwordBox",
                Margin = new Thickness(30, 0, 30, 0),
                Padding = new Thickness(0, 3, 0, 3),
                FontSize = 16
            };
            // Добавляем свойства MaterialDesign (предполагая, что вы используете MaterialDesignThemes)
            MaterialDesignThemes.Wpf.HintAssist.SetHint(passwordBox, "Password");
            MaterialDesignThemes.Wpf.TextFieldAssist.SetHasClearButton(passwordBox, true);
            passwordBox.Style = Application.Current.FindResource("MaterialDesignFloatingHintRevealPasswordBox") as Style;

            // Собираем все вместе
            dockPanel.Children.Add(closeBorder);
            dockPanel.Children.Add(bottomBorder);
            dockPanel.Children.Add(passwordBox);
            enteringPasswordWindow.Child = dockPanel;

            // Создаём "модальность" окну
            AddBorderModal(parentContainer);

            // Добавляем Border в родительский контейнер
            parentContainer.Children.Add(enteringPasswordWindow);
        }

        // --- Создание окна для ввода данных (останавливает код, пока не кликнуть на кнопку "Применить") ----------------------------
        private TaskCompletionSource<string> _tcs;
        private Border enterDataWindow;
        private TextBox textEnterData;
        private List<string> _listName;
        private string _nameModalWindowBlur = "modalEnterText";
        public async Task<string> EnterDataWindowWait(List<string> listName, string textHint)
        {

            _tcs = new TaskCompletionSource<string>();
            _listName = listName;

            Grid parentContainer = mainWindow.GridMain;

            // Создаем основной Border
            enterDataWindow = new Border
            {
                Name = "enterDataWindow",
                Background = Brushes.White,
                Width = 300,
                CornerRadius = new CornerRadius(10),
                Visibility = Visibility.Visible,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Добавляем эффект тени
            enterDataWindow.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 2,
                Direction = -90,
                Opacity = 0.5
            };

            // Создаем DockPanel
            DockPanel dockPanel = new DockPanel
            {
                LastChildFill = true
            };

            // Создаем нижний Border с кнопкой
            Border bottomBorder = new Border();

            DockPanel.SetDock(bottomBorder, Dock.Bottom);

            // Создаем UniformGrid
            UniformGrid uniformGrid = new UniformGrid { Columns = 2 };

            Button applyButton = new Button
            {
                Name = "apply",
                Margin = new Thickness(20, 25, 10, 30),
                Content = "Применить"
            };
            applyButton.Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style;

            Button cencelButton = new Button
            {
                Name = "cencel",
                Margin = new Thickness(10, 25, 20, 30),
                Content = "Отменить"
            };
            cencelButton.Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style;

            cencelButton.Click += (s, e) =>
            {
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, _nameModalWindowBlur);
                // Удаляем созданное окно
                DeleteBorder(parentContainer, "enterDataWindow");
            };

            bottomBorder.Child = uniformGrid;
            uniformGrid.Children.Add(applyButton);
            uniformGrid.Children.Add(cencelButton);


            // Создаем TextBox
            textEnterData = new TextBox
            {
                Name = "text",
                Margin = new Thickness(30, 30, 30, 0),
                Padding = new Thickness(0, 3, 0, 3),
                FontSize = 16
            };
            // Добавляем свойства MaterialDesign (предполагая, что вы используете MaterialDesignThemes)
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textEnterData, textHint);
            MaterialDesignThemes.Wpf.TextFieldAssist.SetHasClearButton(textEnterData, true);
            textEnterData.Style = Application.Current.FindResource("MaterialDesignFloatingHintTextBox") as Style;
            textEnterData.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));// нажатие на кнопку
                    args.Handled = true;
                }
            };

            // Применить пароль и удалить окно и его "модальность"
            applyButton.Click += CloseDialog; // Обработчик клика

            // Собираем все вместе
            dockPanel.Children.Add(bottomBorder);
            dockPanel.Children.Add(textEnterData);
            enterDataWindow.Child = dockPanel;

            // Создаём "модальность" окну
            AddBorderModal(parentContainer, _nameModalWindowBlur);

            // Добавляем Border в родительский контейнер
            parentContainer.Children.Add(enterDataWindow);

            // Устанавливаем фокус на TextBox после загрузки
            textEnterData.Focus();
            textEnterData.Select(0, 0);

            // Ждём завершения
            return await _tcs.Task;
        }
        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            // Получаем введённый текст
            string name = textEnterData.Text;

            foreach (var path in _listName)
            {
                if (Path.GetFileName(path.TrimEnd('/', '\\')) == name)
                {
                    Output.WriteLine("Cовпадение");
                    DialogWindows.InformationWindow("Такое имя уже существует");
                    return;
                }
            }

            // Завершаем ожидание и передаём введённое слово
            _tcs.SetResult(name);

            // Удаляем окно из родительского контейнера
            if (enterDataWindow.Parent is Panel parentPanel)
            {
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder((Grid)parentPanel, _nameModalWindowBlur);

                parentPanel.Children.Remove(enterDataWindow);
            }
        }

        // --- Создание окна для ввода данных (помещает введённые данные в метод, принимающий агруменом: string) ----------------------------
        public async Task EnterDataWindow(Func<string, Task> Enter ,string textHint)
        {
            Grid parentContainer = mainWindow.GridMain;

            // Создаем основной Border
            Border enterDataWindow = new Border
            {
                Name = "enterDataWindow",
                Background = Brushes.White,
                Width = 300,
                CornerRadius = new CornerRadius(10),
                Visibility = Visibility.Visible,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Добавляем эффект тени
            enterDataWindow.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 2,
                Direction = -90,
                Opacity = 0.5
            };

            // Создаем DockPanel
            DockPanel dockPanel = new DockPanel
            {
                LastChildFill = true
            };

            // Создаем нижний Border с кнопкой
            Border bottomBorder = new Border();

            DockPanel.SetDock(bottomBorder, Dock.Bottom);

            // Создаем UniformGrid
            UniformGrid uniformGrid = new UniformGrid { Columns = 2 };

            Button applyButton = new Button
            {
                Name = "apply",
                Margin = new Thickness(20, 25, 10, 30),
                Content = "Применить"
            };
            applyButton.Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style;

            Button cencelButton = new Button
            {
                Name = "cencel",
                Margin = new Thickness(10, 25, 20, 30),
                Content = "Отменить"
            };
            cencelButton.Style = Application.Current.FindResource("MaterialDesignRaisedLightButton") as Style;

            cencelButton.Click += (s, e) =>
            {
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, "Modal");
                // Удаляем созданное окно
                DeleteBorder(parentContainer, "enterDataWindow");
            };


            // Применить пароль и удалить окно и его "модальность"
            applyButton.Click += CloseDialog; // Обработчик клика

            bottomBorder.Child = uniformGrid;
            uniformGrid.Children.Add(applyButton);
            uniformGrid.Children.Add(cencelButton);


            // Создаем TextBox
            TextBox textEnterData = new TextBox
            {
                Name = "text",
                Margin = new Thickness(30, 30, 30, 0),
                Padding = new Thickness(0, 3, 0, 3),
                FontSize = 16
            };
            // Добавляем свойства MaterialDesign (предполагая, что вы используете MaterialDesignThemes)
            MaterialDesignThemes.Wpf.HintAssist.SetHint(textEnterData, textHint);
            MaterialDesignThemes.Wpf.TextFieldAssist.SetHasClearButton(textEnterData, true);
            textEnterData.Style = Application.Current.FindResource("MaterialDesignFloatingHintTextBox") as Style;

            applyButton.Click += (s, e) =>
            {
                Enter(textEnterData.Text);
                // Находим Border "модальности" в структуре элементов и удаляем его
                DeleteBorder(parentContainer, "Modal");
                // Удаляем созданное окно
                DeleteBorder(parentContainer, "enterDataWindow");
            };

            // Собираем все вместе
            dockPanel.Children.Add(bottomBorder);
            dockPanel.Children.Add(textEnterData);
            enterDataWindow.Child = dockPanel;

            // Создаём "модальность" окну
            AddBorderModal(parentContainer);

            // Добавляем Border в родительский контейнер
            parentContainer.Children.Add(enterDataWindow);

        }

        // --- Создание прогресса чего угодно -------------------------------
        public static async Task CreateProgressWindow(string text = "Загрузка...", string nameWindow = "MainWindow")
        {
            Grid parentContainer = mainWindow.GridMain;
            if (nameWindow == "LogIn")
            {
                LogIn logInWindow = Application.Current.Windows.OfType<LogIn>().FirstOrDefault();
                parentContainer = logInWindow.GridMain;
            }

            // Создаем Border
            Border progressWindow = new Border
            {
                Name = "Progress",
                Width = 150,
                Height = 150,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Visibility = Visibility.Visible,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 20,
                    ShadowDepth = 3,
                    Direction = 270,
                    Opacity = 0.5
                }
            };

            // Создаем Grid
            Grid grid = new Grid();

            // Создаем ProgressBar
            ProgressBar progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 50,
                Height = 50,
                Value = 75,
                Visibility = Visibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            // Применяем стиль MaterialDesignCircularProgressBar
            progressBar.SetResourceReference(FrameworkElement.StyleProperty, "MaterialDesignCircularProgressBar"); 

            // Создаем TextBlock
            TextBlock progressText = new TextBlock
            {
                Margin = new Thickness(8, 0, 0, 5),
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = text // Текст по умолчанию
            };

            // Добавляем элементы в Grid
            grid.Children.Add(progressBar);
            grid.Children.Add(progressText);

            // Добавляем Grid в Border
            progressWindow.Child = grid;

            // Создаём "модальность" окну
            AddBorderModal(parentContainer);

            // Добавляем Border в основное окно (например, в Grid с именем "MainGrid")
            parentContainer.Children.Add(progressWindow);
        }
        // --- Удаление прогресса чего угодно -------------------------------
        public static async Task DeleteProgressWindow(string nameWindow = "MainWindow")
        {
            Grid parentContainer = mainWindow.GridMain;
            if (nameWindow == "LogIn")
            {
                LogIn logInWindow = Application.Current.Windows.OfType<LogIn>().FirstOrDefault();
                parentContainer = logInWindow.GridMain;
            }

            // Находим Border "модальности" в структуре элементов и удаляем его
            DeleteBorder(parentContainer, "Modal");
            // Удаляем созданное окно
            DeleteBorder(parentContainer, "Progress");
        }

        // --- Создание блока окна загрузки файлов ----------------------------
        public static void CreateProgressBlock(Border mainBorder, StackPanel parentStackPanel ,string fileName)
        {
            mainBorder.Visibility = Visibility.Visible;

            Grid grid = new Grid
            {
                Margin = new Thickness(0, 15, 0, 0),
                Tag = true
            };
            StaticVariables.ActivProugressIndicator = grid;

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            TextBlock nameFileProgress = new TextBlock
            {
                Name = "nameFileProgress",
                Cursor = Cursors.Hand,
                Text = fileName,
                ToolTip = fileName
            };
            Grid.SetRow(nameFileProgress, 0);
            Grid.SetColumn(nameFileProgress, 0);

            TextBlock percentProgress = new TextBlock
            {
                Name = "percentProgress",
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            StaticVariables.activProgressPercentText = percentProgress;
            Grid.SetRow(percentProgress, 0);
            Grid.SetColumn(percentProgress, 1);

            TextBlock percentSymbol = new TextBlock
            {
                Text = "%"
            };
            Grid.SetRow(percentSymbol, 0);
            Grid.SetColumn(percentSymbol, 2);

            ProgressBar progressBar = new ProgressBar
            {
                Height = 5,
                Value = 0
            };
            StaticVariables.activProgressBar = progressBar;// присваиваем значение статичекой переменной, для управления из вне
            Grid.SetRow(progressBar, 1);
            Grid.SetColumn(progressBar, 0);
            Grid.SetColumnSpan(progressBar, 3);

            Button stopButton = new Button
            {
                Name = "stopProgressFiles",
                Height = 25,
                Padding = new Thickness(0),
                Margin = new Thickness(10, 0, 0, 0),
                Style = (Style)Application.Current.FindResource("MaterialDesignFlatButton")
            };
            stopButton.Click += (s, e) =>
            {
                new ContentRequestAdvanced().CancelUploadFilesAdvancedAsync(null, null);
            };
            StaticVariables.activStopButton = stopButton;
            StaticVariables.activStopButton.IsEnabled = true;

            PackIcon closeIcon = new PackIcon
            {
                Kind = PackIconKind.Close,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stopButton.Content = closeIcon;

            Grid.SetRow(stopButton, 1);
            Grid.SetColumn(stopButton, 3);

            grid.Children.Add(nameFileProgress);
            grid.Children.Add(percentProgress);
            grid.Children.Add(percentSymbol);
            grid.Children.Add(progressBar);
            grid.Children.Add(stopButton);

            parentStackPanel.Children.Add(grid);
        }

        // --- Окно: Продолжать или отменить ----------
        //public static bool ShowConfirmationDialog(Window parent)
        //{
        //    // Создаём окно
        //    Window dialog = new Window
        //    {
        //        Width = 300,
        //        Height = 150,
        //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        //        ResizeMode = ResizeMode.NoResize,
        //        WindowStyle = WindowStyle.None,           // Без шапки
        //        AllowsTransparency = true,                // Разрешаем прозрачность
        //        Background = Brushes.Transparent,         // Прозрачный фон
        //        Owner = parent
        //    };

        //    // Визуальный контейнер с закруглёнными углами
        //    Border border = new Border
        //    {
        //        Background = Brushes.White,
        //        CornerRadius = new CornerRadius(10),      // Закругления
        //        BorderBrush = Brushes.Gray,
        //        BorderThickness = new Thickness(1),
        //        Padding = new Thickness(10)
        //    };

        //    // Сетка внутри окна
        //    Grid grid = new Grid();
        //    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        //    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        //    // Сообщение
        //    TextBlock message = new TextBlock
        //    {
        //        Text = "Вы уверены, что хотите продолжить?",
        //        FontSize = 14,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        VerticalAlignment = VerticalAlignment.Center,
        //        TextWrapping = TextWrapping.Wrap
        //    };
        //    Grid.SetRow(message, 0);
        //    grid.Children.Add(message);

        //    // Кнопки
        //    StackPanel buttonPanel = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        Margin = new Thickness(10)
        //    };

        //    Button continueButton = new Button
        //    {
        //        Content = "Продолжить",
        //        Width = 100,
        //        Margin = new Thickness(5)
        //    };
        //    continueButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

        //    Button cancelButton = new Button
        //    {
        //        Content = "Отмена",
        //        Width = 100,
        //        Margin = new Thickness(5)
        //    };
        //    cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

        //    buttonPanel.Children.Add(continueButton);
        //    buttonPanel.Children.Add(cancelButton);
        //    Grid.SetRow(buttonPanel, 1);
        //    grid.Children.Add(buttonPanel);

        //    // Вставляем контент внутрь границы
        //    border.Child = grid;
        //    dialog.Content = border;

        //    // Показываем окно модально
        //    return dialog.ShowDialog() == true;
        //}
        public static bool ShowConfirmationDialog(Window parent)
        {
            var dialog = new Window
            {
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.White,           // Фон без прозрачности
                ShowInTaskbar = false,
                Owner = parent
            };

            // Применим WindowChrome
            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(10),
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(dialog, chrome);

            // Контент как раньше
            var message = new TextBlock
            {
                Text = "Вы уверены, что хотите продолжить?",
                Margin = new Thickness(20),
                TextAlignment = TextAlignment.Center
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var continueButton = new Button { Content = "Продолжить", Margin = new Thickness(5), Width = 100 };
            continueButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            var cancelButton = new Button { Content = "Отмена", Margin = new Thickness(5), Width = 100 };
            cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(continueButton);
            buttonPanel.Children.Add(cancelButton);

            var grid = new StackPanel();
            grid.Children.Add(message);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;

            return dialog.ShowDialog() == true;
        }

    }
}
