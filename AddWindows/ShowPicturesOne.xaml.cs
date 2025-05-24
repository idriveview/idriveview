using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IDriveView.AddWindows
{
    /// <summary>
    /// Логика взаимодействия для ShowPicturesOne.xaml
    /// </summary>
    public partial class ShowPicturesOne : Window
    {
        string _pathFileClick; // Путь к первому файлу просмотра
        int _index = 0; // Индекс просматриваемого файла
        int _countListFiles; // Количество картинок в открытой папке
        double _widthMonitor; // ширина монитора
        double _heightMonitor; // высота монитора
        bool _imageHeightAsMonitorHeight; // указывает, как будет показывается изображение: подстраиваться под размер 1080 или в своём размере

        // для перетаскивания картинки в окне
        private bool isDragging = false;
        private Point startPoint;
        private Point dragStartPoint; // Добавляем для отслеживания начальной точки перетаскивания
        private List<FolderOrFileModel> _listPictures = new List<FolderOrFileModel>();

        Cursor cursorLeft = new Cursor("Resources\\cursorLeft.cur");
        Cursor cursorLeftEnd = new Cursor(@"Resources\cursorLeftEnd.cur");
        Cursor cursorRight = new Cursor("Resources\\cursorRight.cur");
        Cursor cursorRightEnd = new Cursor("Resources\\cursorRightEnd.cur");
        public ShowPicturesOne(string pathFileClick)
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

                ImageToSizeOfWindow();// подгонка изображения под размер окна
            }
            #endregion
            Loaded += ShowPicturesOne_Loaded;
            this.Closing += ShowPicturesOne_Closed;
            _widthMonitor = SystemParameters.PrimaryScreenWidth;
            _heightMonitor = SystemParameters.PrimaryScreenHeight;
            _pathFileClick = pathFileClick;
            _listPictures = StaticVariables.listPicturesFilesOnly.Select(item => item.Clone()).ToList();// копируем список
            _countListFiles = _listPictures.Count;
            totalFiles.Text = _countListFiles.ToString();
            _imageHeightAsMonitorHeight = Properties.Settings.Default.ImageHeightAsMonitorHeight;

            scrollNext_Click.MouseLeftButtonUp += ScrollNext_Click_MouseLeftButtonUp; // перемотка вперёд
            scrollBack_Click.MouseLeftButtonUp += ScrollBack_Click_MouseLeftButtonUp; // перемотка назад
            scrollNext_Click.PreviewMouseWheel += SecondBorder_PreviewMouseWheel; // переопределяет кручение колёсика мыши на ScrollViewer
            scrollBack_Click.PreviewMouseWheel += SecondBorder_PreviewMouseWheel; // переопределяет кручение колёсика мыши на ScrollViewer           

            showBorder.MouseLeftButtonUp += ShowBorder_MouseLeftButtonUp; // меняем подстраиваемость изобажения под размеры окна

            // скролл мышью
            showBorder.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            showBorder.MouseMove += Border_MouseMove;
            showBorder.MouseLeftButtonUp += Border_MouseLeftButtonUp;
            // масштабирование мышкой
            showBorder.MouseWheel += Border_MouseWheel;

            if (Properties.Settings.Default.ImageHeightAsMonitorHeight) menuSizeMonitor.Foreground = Brushes.Blue;
            else menuSizeOriginal.Foreground = Brushes.Blue;
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
        // управение действием кнопок: закрыть, на весь экран, маленькое окно, свернуть на панель задач checkClickButtonWindowState
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
            else this.WindowState = WindowState.Minimized;
        }
        #endregion

        private async void ShowPicturesOne_Loaded(object sender, RoutedEventArgs e)
        {
            // находим индекс загружаемой картинки
            _index = _listPictures.FindIndex(item => item.Path == _pathFileClick);
            if (_index == 0)
            {
                scrollNext_Click.Cursor = cursorRight;
                scrollBack_Click.Cursor = cursorLeftEnd;
            }
            else if (_index == _countListFiles - 1)
            {
                scrollNext_Click.Cursor = cursorRightEnd;
                scrollBack_Click.Cursor = cursorLeft;
            }
            else
            {
                scrollNext_Click.Cursor = cursorRight;
                scrollBack_Click.Cursor = cursorLeft;
            }
 
            await NextBackShowImages(_index);
        }
        // Метод перемотки показа изображение
        public async Task NextBackShowImages(int index)
        {
            // Полный путь к картинке на ПК
            string fullPathFileClickToPC = Path.Combine(Settings.pathFolderSavePictures, _listPictures[index].Path);
            cuontFile.Text = (index + 1).ToString();
            nameFile.Text = _listPictures[index].Name;
            // Показываем картинку
            await ShowImage(fullPathFileClickToPC);
        }

        #region Перемотка картинок вперёд и назад
        // перемотка вперёд
        private async void ScrollNext_Click_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_index < _countListFiles-1)
            {
                scrollNext_Click.Cursor = cursorRight;
                scrollBack_Click.Cursor = cursorLeft;

                ImageToSizeOfWindow();// подгонка изображения под размер окна
                _index = _index + 1;
                await NextBackShowImages(_index);
            }
            else
            {
                scrollNext_Click.Cursor = cursorRightEnd;
            }
        }
        // перемотка назад
        private async void ScrollBack_Click_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_index > 0)
            {
                scrollNext_Click.Cursor = cursorRight;
                scrollBack_Click.Cursor = cursorLeft;

                ImageToSizeOfWindow();// подгонка изображения под размер окна
                _index = _index - 1;
                await NextBackShowImages(_index);
            }
            else
            {
                scrollBack_Click.Cursor = cursorLeftEnd;
            }
        }
        #endregion Перемотка картинок вперёд и назад
        #region Перемещение большой картинки мышью
        // ---  Перемещение большой картинки мышью ---
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                isDragging = true;
                startPoint = e.GetPosition(scrollViewer);
                dragStartPoint = startPoint; // Сохраняем начальную точку перетаскивания
                showBorder.CaptureMouse();
            }
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPoint = e.GetPosition(scrollViewer);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                // Обновляем позицию прокрутки
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - offsetX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offsetY);

                // Обновляем начальную точку для следующего движения
                startPoint = currentPoint;
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                showBorder.ReleaseMouseCapture();
            }
        }

        // --- Конец ---  Перемещение большой картинки мышью ---
        #endregion
        #region Меняем подстраиваемость изобажения под размеры окна или под размер изображения
        // --- Меняем подстраиваемость изобажения под размеры окна или под размер изображения ---
        private void ShowBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // блок, отвечающий за получение размера передвижения мыши по окну
            // если меньше 5 пкс, то картинка подстраивается под размер окна,
            // если больше, то в данном методе ничего не происходит, но в другом, картинка перемещается в окне
            Point endPoint = e.GetPosition(scrollViewer);
            double distanceX = Math.Abs(endPoint.X - dragStartPoint.X);
            double distanceY = Math.Abs(endPoint.Y - dragStartPoint.Y);
            double totalDistance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
            //---------------------------------------------------------------------- конец блока ----------

            // Проверяем, было ли перемещение меньше 5 пикселей
            if (totalDistance < 5)
            {
                // два режима: под размер окна и под размер изображения
                if (showBorder.Tag.ToString() == "true")
                {
                    ImageToSizeOfWindow();// подгонка изображения под размер окна
                }
                else
                {
                    if (showBorder.Child is Image image)
                    {
                        // Оригинальные размеры
                        if (image.Source is BitmapImage bitmapImage)
                        {
                            // Запоминаем положение мыши относительно изображения ДО изменения размера
                            Point mousePos = e.GetPosition(showBorder);
                            double relativeX = mousePos.X / showBorder.Width;
                            double relativeY = mousePos.Y / showBorder.Height;

                            int originalWidth = bitmapImage.PixelWidth;
                            int originalHeight = bitmapImage.PixelHeight;
                            //Output.WriteLine($"Оригинальные размеры: {originalWidth}x{originalHeight}");
                            showBorder.Width = originalWidth;
                            showBorder.Height = originalHeight;

                            // Вычисляем новую позицию мыши после изменения размера
                            double newMouseX = relativeX * showBorder.Width;
                            double newMouseY = relativeY * showBorder.Height;

                            // Корректируем положение ScrollViewer, чтобы мышь осталась над той же точкой изображения
                            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + newMouseX - mousePos.X);
                            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + newMouseY - mousePos.Y);
                        }
                    }
                    showBorder.Tag = "true";
                }
            }
            // Если перемещение больше 5 пикселей, ничего не меняем
        }
        #endregion Меняем подстраиваемость изобажения под размеры окна или под размер изображения
        #region Метод помещает картинку в окно программы
        // --- Метод помещает картинку в окно программы ---------------------------
        private static CancellationTokenSource cts = new CancellationTokenSource();
        public async Task ShowImage(string pathImage, bool highQuality = true)
        {
            // ---------------------------------------
            // если картинка долго скачивается или отсутствует, то при переходе на следующую, 
            // предыдущее отслеживание картинки отменяется
            CancellationToken token;

            lock (typeof(ShowPicturesOne)) // Используем тип как объект блокировки
            {
                // Если cts уже существует, отменяем предыдущий
                if (cts != null)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                // Создаём новый CancellationTokenSource
                cts = new CancellationTokenSource();
                token = cts.Token;
            }
            //-----------------------------------------

            showBorder.Child = null;

            var dynamicImage = new Image
            {
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = highQuality ? true : false, // Включаем точное отображение пикселей
                UseLayoutRounding = highQuality ? true : false // Исправляем проблемы с округлением пикселей
            };

            int maxAttempts = 200;
            int delay = 1000; // 500 мс между попытками
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        //Output.WriteLine("операция была отменена");
                        return;
                    }

                    // Загружаем изображение
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(pathImage, UriKind.RelativeOrAbsolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    if (!highQuality)
                    {
                        // Можно добавить уменьшенное разрешение для экономии ресурсов
                        // bitmap.DecodePixelHeight = 100; 
                        
                    }
                    // параметр сохранён в параметры Setting приложения wpf
                    // меняется из контектсного меню окна программы
                    if (_imageHeightAsMonitorHeight)
                    {
                        bitmap.DecodePixelHeight = 1080;
                    }

                    bitmap.EndInit();

                    // по умолчанию показывает картинку в высоком качестве
                    if (highQuality)
                    {
                        // Улучшаем качество рендеринга
                        RenderOptions.SetBitmapScalingMode(dynamicImage, BitmapScalingMode.HighQuality);
                    }

                    dynamicImage.Source = bitmap;
                    showBorder.Child = dynamicImage;

                    return;
                }
                catch (Exception ex)
                {
                    await Task.Delay(delay);
                }
            }      
        }
        #endregion Метод помещает картинку в окно программы

        // --- Масштабирование колёсиком мыши ------------
        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double scaleStep = 1.1; // Коэффициент увеличения/уменьшения
            double zoomFactor = e.Delta > 0 ? scaleStep : 1 / scaleStep; // Определяем направление масштабирования

            // Получаем текущие размеры
            double newWidth = showBorder.Width * zoomFactor;
            double newHeight = showBorder.Height * zoomFactor;

            // Ограничение минимального размера
            if (newWidth < 100 || newHeight < 100)
                return;

            // Получаем позицию мыши в ScrollViewer перед масштабированием
            Point mousePos = e.GetPosition(scrollViewer);
            double relativeX = mousePos.X / showBorder.Width;
            double relativeY = mousePos.Y / showBorder.Height;

            // Устанавливаем новые размеры
            showBorder.Width = newWidth;
            showBorder.Height = newHeight;

            // Корректируем смещение ScrollViewer для сохранения точки под мышью
            scrollViewer.ScrollToHorizontalOffset((scrollViewer.HorizontalOffset + mousePos.X) * zoomFactor - mousePos.X);
            scrollViewer.ScrollToVerticalOffset((scrollViewer.VerticalOffset + mousePos.Y) * zoomFactor - mousePos.Y);

            showBorder.Tag = "true";
        }

        // --- Перенаправление колёсика мыши с кнопок перемотки на общий скролл окна -------------------
        private void SecondBorder_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Создаём новое событие прокрутки для firstBorder
            var newEventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent, // Указываем тип события
                Source = showBorder // Задаем firstBorder как источник события
            };

            showBorder.RaiseEvent(newEventArgs); // Передаем событие прокрутки первому Border
            e.Handled = true; // Блокируем обработку события secondBorder, но клики всё ещё работают!
        }

        // --- Метод подгонки изображения под размер окна ---------------------------
        private void ImageToSizeOfWindow()
        {
            if (this.WindowState != WindowState.Maximized)
            {
                showBorder.Width = Width - 25;
                showBorder.Height = Height - 45;
            }
            else
            {
                showBorder.Width = _widthMonitor - 20;
                showBorder.Height = _heightMonitor - 75;
            }
            showBorder.Tag = "folse";
        }

        #region Методы контекстного меню
        // --- Контекстное меню --------- оригинальный размер ---------------------------
        private async void SizeOriginal(object sender, RoutedEventArgs e)
        {
            _imageHeightAsMonitorHeight = false;

            // перезагружаем картинку
            if (showBorder.Child is Image image && image.Source is BitmapImage bitmap)
            {
                string imagePath = bitmap.UriSource?.LocalPath;
                Output.WriteLine(imagePath);
                if (File.Exists(imagePath))
                {
                    await ShowImage(imagePath);
                }
            }

            // меняем цвет текска в контекстном меню во всех окнах
            menuSizeOriginal.Foreground = Brushes.Blue;
            menuSizeMonitor.Foreground = Brushes.Black;
        }
        // --- Контекстное меню --------- под заданный размер ---------------------------
        private async void SizeMonitor(object sender, RoutedEventArgs e)
        {
            _imageHeightAsMonitorHeight = true;

            // перезагружаем картинку
            if (showBorder.Child is Image image && image.Source is BitmapImage bitmap)
            {
                string imagePath = bitmap.UriSource?.LocalPath;
                if (File.Exists(imagePath))
                {
                    await ShowImage(imagePath);
                }
            }

            // меняем цвет текска в контекстном меню во всех окнах
            menuSizeOriginal.Foreground = Brushes.Black;
            menuSizeMonitor.Foreground = Brushes.Blue;
        }
        // --- Контекстное меню --------- закрыть текущее окно ---------------------------
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        // --- Контекстное меню --------- закрыть приложение ---------------------------
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // --- Контекстное меню ------ Конец --------------------------------------
        #endregion Методы контекстного меню
        #region Обработка нажатия клавиш на клавиатуре
        // Обработка нажатия клавиш
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // При нажжатии Пробел дублируем действие кнопки Play
            if (e.Key == Key.Left)
            {
                ScrollBack_Click_MouseLeftButtonUp(null, null); // перемотка назад
            }
            else if (e.Key == Key.Right)
            {
                ScrollNext_Click_MouseLeftButtonUp(null, null); // перемотка вперёд
            }
        }
        #endregion

        // --- Переопределение закрытия окна -------------------------------------
        private void ShowPicturesOne_Closed(object sender, EventArgs e)
        {
            // сохраняем, как будет показывается изображение: подстраиваться под размер 1080 или в своём размере
            Properties.Settings.Default.ImageHeightAsMonitorHeight = _imageHeightAsMonitorHeight;
            Properties.Settings.Default.Save();
        }
    }

}
