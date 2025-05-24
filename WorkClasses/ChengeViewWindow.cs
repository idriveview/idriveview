
using IDriveView.CreateReqest;
using IDriveView.HelpCode;
using IDriveView.Models;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IDriveView.WorkClasses
{
    class ChengeViewWindow
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        // --- Метод изменения иконки: Grid - Liner с расположение картинок: сеткой или линиями --------
        public async void ChangeIconKind(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // проверяет значёк переключения расположения
                if (button.Name == "viewGridOutline")
                {
                    await ManagementContentsWindow.VeiwLineWindow();
                    await ChengeView(Settings.SetLineView());
                }
                else
                {
                    await ManagementContentsWindow.VeiwGridWindow();
                    await ChengeView(Settings.SetGridView());
                }
            }
        }
        // --- Вспомогоательный метод для изменения расположения объектов в окне в зависимости от: сетка и линии ---------------
        async Task ChengeView(ViewWindowModel viewWindowModel)
        {
            foreach (var item in StaticVariables.listFoldersAndFiles)
            {
                item.OuterBorder.Width = viewWindowModel.WidthOuterBorder;
                item.InnerBorder.Width = viewWindowModel.IconWidth;
                item.InnerBorder.Height = viewWindowModel.IconHeight;
                item.InnerBorder.HorizontalAlignment = viewWindowModel.HorizontalInnerBorder;
                item.Stackpanel.Orientation = viewWindowModel.OrientStackPanel;
                item.Packicon.Width = viewWindowModel.IconWidth;
                item.Packicon.Height = viewWindowModel.IconHeight;
                string nameItem = item.Name;
                if (nameItem.Length > viewWindowModel.NameItemLength) nameItem = nameItem.Remove(viewWindowModel.NameItemLength) + "...";
                item.Textblock.Text = nameItem;
                item.Textblock.Margin = viewWindowModel.MarginTextBlock;
                mainWindow.stackPanelView.Visibility = viewWindowModel.VisibilityStackSize;
            }
        }

        // --- Метод заполнения окна контентом -------------------------------------------
        public static async Task ViewContenGridOrLinetWindow(ViewWindowModel viewWindowModel)
        {
            // Пройтись по списку и создать кнопки
            foreach (var item in StaticVariables.listFoldersAndFiles)
            {
                string nameItem = item.Name;
                // назначаем соответствующие иконки
                PackIconKind iconKind = Services.ChooseFileIcon(item.Type, nameItem);

                string nameItemToolTip = nameItem;
                // обрезаем имя элемента до viewWindowModel.NameItemLength знаков
                if (nameItem.Length > viewWindowModel.NameItemLength) nameItem = nameItem.Remove(viewWindowModel.NameItemLength) + "...";

                // генерируем уникальное имя для outerBorder
                Random random = new Random();
                string unicName = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz", 8).Select(s => s[random.Next(s.Length)]).ToArray());
                // Основной Borderborder. (Brush)this.TryFindResource("MaterialDesignPaper");
                var outerBorder = new Border
                {
                    Name = unicName,
                    Width = viewWindowModel.WidthOuterBorder,
                    Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper"),
                    Padding = new Thickness(5, 5, 5, 5),
                    Margin = new Thickness(0, 0, 5, 5),
                    ToolTip = item.Path
                };
                item.OuterBorder = outerBorder;//----|||||-----|||||----- ListFolderOrFileModel

                // Стек для текста и вложенного бордера
                var stackPanel = new StackPanel
                {
                    Orientation = viewWindowModel.OrientStackPanel
                };
                item.Stackpanel = stackPanel;// ----||||| -----||||| ----- ListFolderOrFileModel

                // Вложенный Border
                var innerBorder = new Border
                {
                    Width = viewWindowModel.IconWidth,
                    Height = viewWindowModel.IconHeight,
                    HorizontalAlignment = viewWindowModel.HorizontalInnerBorder,
                    VerticalAlignment = VerticalAlignment.Center
                };
                item.InnerBorder = innerBorder;// ----||||| -----||||| ----- ListFolderOrFileModel

                // ----------------------------------------------
                //// Создаем Image
                //Image dynamicImage = new Image
                //{
                //    Stretch = Stretch.Uniform, // Устанавливаем, чтобы изображение подстраивалось под размеры
                //    HorizontalAlignment = HorizontalAlignment.Center,
                //    VerticalAlignment = VerticalAlignment.Center
                //};

                //// Загружаем изображение
                //BitmapImage bitmap = new BitmapImage();
                //bitmap.BeginInit();
                //bitmap.UriSource = new Uri(pathImage, UriKind.RelativeOrAbsolute); // Укажите путь к изображению
                //bitmap.EndInit();
                //dynamicImage.Source = bitmap;

                //// Добавляем Image внутрь Border
                //innerBorder.Child = dynamicImage;

                // ----------------------------------------------
                // Создание PackIcon
                var packIcon = new PackIcon
                {
                    Kind = iconKind, // Иконка "Папка"
                    Width = viewWindowModel.IconWidth,
                    Height = viewWindowModel.IconHeight,
                    //Foreground = (Brush)Application.Current.Resources["PrimaryHueLightBrush"], // Динамический ресурс
                    //Foreground = (Brush)window.TryFindResource("PrimaryHueMidBrush"), // Динамический ресурс
                    Foreground = Brushes.RoyalBlue, // Цвет подобран вручную, по другому не красиво
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false
                };
                item.Packicon = packIcon;// ----||||| -----||||| ----- ListFolderOrFileModel

                // Вставляем PackIcon внутрь Border
                innerBorder.Child = packIcon;
                // ------------------------------------------------

                // Текст под Border
                var textBlock = new TextBlock
                {
                    Text = nameItem,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = viewWindowModel.MarginTextBlock,
                    FontSize = 16,
                    ToolTip = nameItemToolTip,
                    IsHitTestVisible = false
                };
                item.Textblock = textBlock;// ----||||| -----||||| ----- ListFolderOrFileModel

                // Border для записи размера файла
                var borderSize = new Border
                {
                    Height = 40, // подстраивается вручную под высоту outerBorder
                    //Padding = new Thickness(0, 14, 0, 0), // положение текста
                    Margin = new Thickness(0, 0, 0, 5) // промежутки между строками
                };
                // Текст под StackPanel для записи размера файла
                var textBlockSize = new TextBlock
                {
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = Services.BiteToKbToMbToGb(item.Type, item.Size),
                };
                borderSize.Child = textBlockSize;
                //borderSize.Children.Add(textBlockSize);
                // Привязка свойства Background второго Border к первому
                Binding backgroundBinding = new Binding("Background")
                {
                    Source = outerBorder
                };
                borderSize.SetBinding(Border.BackgroundProperty, backgroundBinding);

                // Добавляем события для изменения цвета при наведении и клике на элемент
                var originalBrush = (Brush)mainWindow.TryFindResource("MaterialDesignPaper");
                var hoverBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                var clickBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));

                outerBorder.MouseEnter += (s, e) => outerBorder.Background = hoverBrush;
                outerBorder.MouseLeave += (s, e) => outerBorder.Background = originalBrush;
                outerBorder.MouseLeftButtonDown += (s, e) => outerBorder.Background = clickBrush;
                //outerBorder.MouseLeftButtonUp += (s, e) => outerBorder.Background = hoverBrush;
                //outerBorder.MouseRightButtonUp += (s, e) => MessageBox.Show("Right mouse button released!");

                // Добавляем обработчик события MouseRightButtonUp
                //outerBorder.MouseRightButtonUp += (sender, args) =>
                //{
                //    var clickedBorder = sender as Border;
                //    if (clickedBorder != null)
                //    {
                //        // Получаем привязанный объект
                //        var data = clickedBorder.Tag as ListFolderOrFileModel;
                //        if (data != null)
                //        {
                //            Consol.Logtext($"Type: {data.Type}\nPlainName: {data.Name}\nUuid: {data.Href}");
                //        }
                //    }
                //};

                // Создаем контекстное меню
                ContextMenu contextMenu = new ContextMenu();
                // Список заголовков для контекстного меню
                List<string> menuHeaders;
                if(item.Type == "folder")
                    menuHeaders = new List<string> { "Скачать на ПК", "Переименовать", "Удалить", new('-', 50), "Снять выделение", "Скопировать имя в буфер", "Закрыть меню" };
                else
                    menuHeaders = new List<string> { "Скачать на ПК", "Переименовать", "Удалить", new('-', 50), "Снять выделение", "Скопировать имя в буфер", "Получить ссылку", "Закрыть меню" };
                // Метод для обработки кликов по элементам контекстного меню
                async void OnMenuItemClick(object sender, RoutedEventArgs e)
                {
                    if (sender is MenuItem menuItem)
                    {
                        if (menuItem.Header.ToString() == "Скачать на ПК")
                        {
                            await new DownUpContent().DownLoadFilesAsync(item.ParentDirectory.Replace('\\', '/'));
                        }
                        else if (menuItem.Header.ToString() == "Переименовать") await new ContentRequestAdvanced().RenameObjectAdvancedAsync(item.Type, item.Path, item.ParentDirectory);
                        else if (menuItem.Header.ToString() == "Удалить") await new ContentRequestAdvanced().DeleteFolderOrFileAdvancedAsync();
                        else if(menuItem.Header.ToString() == "Снять выделение")
                        {
                            foreach (var child in mainWindow.wrapPanelView.Children)
                            {
                                if (child is Border control)
                                {
                                    control.Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper"); // сбрасываем выделение всех элементов
                                }
                            }
                        }
                        else if (menuItem.Header.ToString() == "Скопировать имя в буфер") Clipboard.SetText(Path.GetFileName(item.Path.TrimEnd('/')));
                        else if (menuItem.Header.ToString() == "Получить ссылку") Clipboard.SetText(await new ContentRequest().GetSignedUrlFromS3Async(item.Path.TrimEnd('/')));
                        else if (menuItem.Header.ToString() == "Закрыть меню") return;
                    }
                }
                // Добавляем элементы в контекстное меню через foreach
                foreach (var header in menuHeaders)
                {
                    MenuItem menuItem = new MenuItem { Header = header };
                    menuItem.Click += OnMenuItemClick;
                    contextMenu.Items.Add(menuItem);
                }

                // Привязываем контекстное меню к кнопке
                outerBorder.ContextMenu = contextMenu;

                // Добавляем обработчик события MouseLeftButtonUp
                //outerBorder.MouseLeftButtonUp += async (sender, args) =>
                //{
                //    var clickedBorder = sender as Border;
                //    if (clickedBorder != null)
                //    {
                //        // Получаем привязанный объект
                //        var data = clickedBorder.Tag as FolderOrFileModel;
                //        if (data != null)
                //        {
                //            if (data.Type == "folder")
                //            {
                //                Output.WriteLine("Folder");
                //                // показываем содержимое папки
                //                await FillWindow.FillContentWindow((data.Path).TrimStart('/', '\\'), "/");
                //                // здесь можно получить окончание загрузки картинок
                //            }
                //            else
                //            {
                //                Output.WriteLine("File");
                //                //new OpenShowPicturesOneWindow().OpenSecondWindow(data.Href);
                //            }
                //        }
                //    }
                //};

                // Связываем объект с Tag
                outerBorder.Tag = item;

                // Добавляем элементы в стек
                stackPanel.Children.Add(innerBorder);
                stackPanel.Children.Add(textBlock);

                // Добавляем стек в основной Border
                outerBorder.Child = stackPanel;

                // Добавляем Border в WrapPanel
                mainWindow.wrapPanelView.Children.Add(outerBorder);
                mainWindow.stackPanelView.Children.Add(borderSize);
            }
        }

        // --- Метод заполнения превью картинок в окне программы ---------------------------
        public static async Task FillElemWindowPictures(string pathFileCloud)
        {
            var filePicture = StaticVariables.listFoldersAndFiles.Where(i => i.Path == pathFileCloud).FirstOrDefault();

            string pathImage = Path.Combine(Settings.pathFolderSavePictures, filePicture.Path);

            // Создаем Image
            Image dynamicImage = new Image
            {
                Stretch = Stretch.Uniform, // Устанавливаем, чтобы изображение подстраивалось под размеры
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };

            // Загружаем изображение
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pathImage, UriKind.RelativeOrAbsolute); // Укажите путь к изображению

            bitmap.DecodePixelHeight = 100; // Указываем высоту для превью
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Загружаем сразу в память

            bitmap.EndInit();
            dynamicImage.Source = bitmap;

            // Добавляем Image внутрь Border
            filePicture.InnerBorder.Child = dynamicImage;

        }
    }
}
