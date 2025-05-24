using IDriveView.AddWindows;
using IDriveView.CreateReqest;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using IDriveView.WorkClasses;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace IDriveView;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static QueueUploadViewModel queueUploadViewModel = new QueueUploadViewModel();

    public static ApplicationContext dataBase = new ApplicationContext();

    public MainWindow()
    {
        InitializeComponent();
        #region действия при изменении размеров окна
        // отследить изменения ширины всего окна
        this.SizeChanged += OnWindowSizeChanged;
        void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    nameBorder.BorderThickness = new Thickness(7);
                    labelMaxmin.Content = "❐";
                }
                else
                {
                    nameBorder.BorderThickness = new Thickness(2, 1, 2, 2);
                    labelMaxmin.Content = "☐";
                }
            }
        }
        #endregion

        Directory.CreateDirectory(Settings.folderSettings);// создание папки для сохранения конфигураций и данных
        Services.CreateFolderPreview(); // Создаём папку для превью

        DataContext = queueUploadViewModel;
        dataBase.Database.EnsureCreated(); // гарантируем, что база данных создана

        this.Closed += MainWindow_Closed;// переопределение закрытия окна
        Loaded += MainWindow_Loaded;
        wrapPanelView.SizeChanged += WrapPanelView_SizeChanged;

        var downUpContent = new DownUpContent();

        loginButton.Click += OpenWindowRegistration;// открыть окно регистрации
        accountView.MouseEnter += accountViewShow;
        accountView.MouseLeave += accountViewClose;
        accountViewBorder.MouseEnter += accountViewShow;
        accountViewBorder.MouseLeave += accountViewClose;
        viewGridOutline.Click += new ChengeViewWindow().ChangeIconKind; // изменить иконку расположения элементов в окне программы: сетка или линия
        viewAgendaOutline.Click += new ChengeViewWindow().ChangeIconKind; // изменить иконку расположения элементов в окне программы: сетка или линия

        addFolderButton.Click += new ContentRequestAdvanced().CreateFolderAdvancedAsync;// создание папки

        uploadFilesButton.Click += downUpContent.UpLoadFilesAsync; // загрузка файлов в облако
        uploadFolderButton.Click += downUpContent.UpLoadFolderAsync; // загрузка папки в облако
        clearFinishedUpload.Click += DownUpContent.DeleteProugressFinishedFiles; // удаление прогресса загруженных файлов

        SelectElementsMouse selectElementsMouse = new SelectElementsMouse();
        selectionCanvas.MouseDown += selectElementsMouse.Canvas_MouseDown;
        selectionCanvas.MouseMove += selectElementsMouse.Canvas_MouseMove;
        selectionCanvas.MouseMove += selectElementsMouse.WrapPanel_MouseMove;
        selectionCanvas.MouseUp += selectElementsMouse.Canvas_MouseUp;
        selectionCanvas.MouseRightButtonDown += selectElementsMouse.Canvas_RightClick;

        // перетаскивание в окно программы
        DropGrid.DragEnter += new DragAndDrop().DropBorder_DragEnter;
        DropGrid.DragLeave += new DragAndDrop().DropBorder_DragLeave;
        DropGrid.Drop += new DragAndDrop().DropBorder_Drop;

        openFolderSavePreview.Click += Services.OpenFolderPreview; // открытие папки с картинками-превью на ПК

        // обработака нажатия на прогресс занятого пространства в облаке
        // расчёт занятого пространства в облаке
        calculationSpaceCloud.Click += WorkDateBase.ProgressSpaceBorder_Click;

        // открыть - закрыть личный кабинет
        personalOffice.Click += OpenPersonalOfficeUser.OpenPersonalOffice;
        closeUserInformation.Click += OpenPersonalOfficeUser.ClosePersonalOffice;
        resetInformationButton.Click += WorkDateBase.ResetData; // сбросить данные о трафике пользователя
        restartUserInformation.Click += OpenPersonalOfficeUser.OpenPersonalOffice; // перезапустить личный кабинет
        exitSession.Click += OpenPersonalOfficeUser.ExitSessionUser; // выйти из сессии
        enterPassword.Click += OpenPersonalOfficeUser.ChengeEnterPassword; // вход по паролю
        saveChengeTariff.Click += WorkDateBase.SaveChengeTariff_Click; // сохранить изменения тарифа
        //choosePathFolderSavePictures.Click += WorkDateBase.ChoosePathFolderSavePictures_Click; // выбрать путь к папке для сохранения картинок-превью
        //defaultPathFolderSavePictures.Click += WorkDateBase.ChoosePathFolderSavePictures_Click; // выбрать путь к папке для сохранения картинок-превью по умолчанию

        Output.WriteLine("Программа запущена");
        _ = Logger.Log("Программа запущена");
    }

    #region обработка шапки (с кнопками)
    // обрабатывает наведение на кнопку
    private void header_MouseEnter(object sender, MouseEventArgs e)
    {
        Border border = sender as Border;
        if (border.Name == "close")
            border.Background = Brushes.Red;
        else
        {
            border.Background = (Brush)this.TryFindResource("PrimaryHueLightBrush");
            border.Opacity = 0.7;
        }
    }
    // обрабатывает: мыша покидает кнопку
    private void header_MouseLeave(object sender, MouseEventArgs e)
    {
        Border border = sender as Border;
        border.Background = (Brush)this.TryFindResource("PrimaryHueMidBrush");
        border.Opacity = 1;
    }
    // обрабатывает нажатие на кнопку
    private void header_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Border border = sender as Border;
        if (border.Name == "close")
            border.Background = Brushes.LightPink;
        else
        {
            border.Background = (Brush)this.TryFindResource("PrimaryHueLightBrush");
            border.Opacity = 1;
        }
    }
    // управение действием кнопок: закрыть, на весь экран, маленькое окно, свернуть на панель задач
    private void header_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        Border border = sender as Border;
        if (border.Name == "close")
            this.Close();
        else if ((border.Name == "maxmin"))
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
        else
            this.WindowState = WindowState.Minimized;
    }
    #endregion
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await DialogWindows.CreateProgressWindow("Проверка доступа...");// окно прогресса - открыто

        // Проверяем соединение с интернетом (проверяем доступность сайта IDrive и других)
        var (successInternet, messageInternet) = await InternetAvailability.CheckInternet();
        if (!successInternet)
        {
            await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто
            await Logger.Log($"Ошибка проверки интернета: {messageInternet}");
            await DialogWindows.InformationWindow(messageInternet);
            return;
        }

        // Извлекаем список пользователей и создаём окно пользователей
        var (success, message) = await AddRegistrationLogIn.FillListUsers();
        await Logger.Log(success ? $"Успешно: {message}" : $"Ошибка: {message}");
        Output.WriteLine(success ? $"Успешно: {message}" : $"Ошибка: {message}");

        //// Создаём папку для превью
        //Services.CreateFolderPreview();

        await DialogWindows.DeleteProgressWindow();// окно прогресса - закрыто

    }
    // --- Открыть окно с авторизованными пользователями IDriveView -------------
    private void accountViewShow(object sender, MouseEventArgs e)
    {
        accountViewBorder.Visibility = Visibility.Visible;
    }
    // --- Закрыть окно с авторизованными пользователями IDriveView -------------
    private void accountViewClose(object sender, MouseEventArgs e)
    {
        accountViewBorder.Visibility = Visibility.Collapsed;
    }
    // --- Открыть окно регистации в приложении --------------------------------------
    public void OpenWindowRegistration(object sender, RoutedEventArgs e)
    {
        var login = new LogIn(this);
        login.Owner = this;
        login.ShowDialog();
    }
    // --- Автоматическое позиционирование элементов в окне программы:  -------------------
    // если меньше одной строки, то слева, если больше, то по центру
    private void WrapPanelView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //Output.WriteLine("Высота = " +  e.NewSize.Height.ToString());
        if (viewGridOutline.Visibility == Visibility.Visible)
        {
            if (e.NewSize.Height > Settings.transitionSecondLine) wrapPanelView.HorizontalAlignment = HorizontalAlignment.Center;
            else wrapPanelView.HorizontalAlignment = HorizontalAlignment.Left;
        }
    }
  



    // --- Переопределение закрытия программы -----------------------------------------------
    private void MainWindow_Closed(object sender, EventArgs e)
    {
        // Удаляем папку для превью вместе с файлами
        Services.DeleteFolderPreview();

        // получаем данные о трафике пользователя (этот метод ещё упорядочивает данные в Базе данных)
        WorkDateBase.GetData(null, null);

        DebugWindow.Instance.ForceClose(); // Принудительно закрываем окно DebugWindow
        Application.Current.Shutdown();
        _ = Logger.Log("Программа завершила работу");
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = (ToggleButton)sender;
        if ((bool)toggleButton.IsChecked) progressFilesBorder.Visibility = Visibility.Visible;
        else progressFilesBorder.Visibility = Visibility.Collapsed;

    }
    // Обработка нажатия клавиш
    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Проверяем Ctrl + A
        if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            foreach (var child in wrapPanelView.Children)
            {
                if (child is Border control)
                {
                    control.Background = Brushes.LightBlue; // Меняем фон
                }
            }
            e.Handled = true; // Останавливаем дальнейшую обработку
        }
        else if (e.Key == Key.Delete)
        {
            // Удаляет выделенные элементы в окне программы
            await new ContentRequestAdvanced().DeleteFolderOrFileAdvancedAsync();
        }
    }

    private async void ShowContacts_Click(object sender, RoutedEventArgs e)
    {
        var contactsWindow = new ContactsWindow();
        contactsWindow.Owner = this;
        contactsWindow.Show();
    }

}