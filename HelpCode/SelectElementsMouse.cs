using IDriveView.AddWindows;
using IDriveView.Debuging;
using IDriveView.WorkClasses;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDriveView.HelpCode
{
    class SelectElementsMouse
    {
        MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        private Point _startPoint;
        private Border _hoveredBorder = null; // Последний наведенный элемент

        // Обработчик наведения мыши на WrapPanel 
        public void WrapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // Если зажата левая кнопка мыши, ничего не делаем
            if (e.LeftButton == MouseButtonState.Pressed) return;

            Point mousePos = e.GetPosition(mainWindow.wrapPanelView);
            UIElement element = mainWindow.wrapPanelView.InputHitTest(mousePos) as UIElement;

            if (element is Border border)
            {
                // Если навели на новый элемент
                if (_hoveredBorder != border)
                {
                    if (_hoveredBorder != null && _hoveredBorder.Background == Brushes.Gainsboro)
                        _hoveredBorder.Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper"); // Вернуть старый цвет

                    if (border.Background == (Brush)mainWindow.TryFindResource("MaterialDesignPaper"))
                        border.Background = Brushes.Gainsboro; // Выделяем оранжевым

                    mainWindow.pathToElement.Content = Path.GetFileName(border.ToolTip.ToString().TrimEnd('/'));// отображаем имя элемента в шапке окна

                    _hoveredBorder = border;
                }
            }
            else
            {
                // Если ушли с элементов - убираем выделение
                if (_hoveredBorder != null && _hoveredBorder.Background == Brushes.Gainsboro)
                    _hoveredBorder.Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper");

                mainWindow.pathToElement.Content = ""; // скрываем имя элемента в шапке окна

                _hoveredBorder = null;
            }
        }
        public void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = e.GetPosition(mainWindow.selectionCanvas);
                mainWindow.selectionRectangle.Width = 0;
                mainWindow.selectionRectangle.Height = 0;
                mainWindow.selectionRectangle.Visibility = Visibility.Visible;
                Canvas.SetLeft(mainWindow.selectionRectangle, _startPoint.X);
                Canvas.SetTop(mainWindow.selectionRectangle, _startPoint.Y);
            }
        }

        public void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(mainWindow.selectionCanvas);
                double x = Math.Min(_startPoint.X, currentPoint.X);
                double y = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                Canvas.SetLeft(mainWindow.selectionRectangle, x);
                Canvas.SetTop(mainWindow.selectionRectangle, y);
                mainWindow.selectionRectangle.Width = width;
                mainWindow.selectionRectangle.Height = height;
            }
        }

        public async void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                ResetSelection();
            }
            SelectElementsInsideRectangle();
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                FillGapsInSelection();
            }
            mainWindow.selectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void ResetSelection()
        {
            foreach (UIElement element in mainWindow.wrapPanelView.Children)
            {
                if (element is Border border)
                {
                    border.Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper"); // Сброс цвета
                    //border.Tag = false;
                }
            }
        }

        bool doubleClickProtection = false; // Защита от двойного клика

        // Метод для выделения элементов внутри прямоугольника (имитация клика мыши)
        private async void SelectElementsInsideRectangle()
        {
            double rectX = Canvas.GetLeft(mainWindow.selectionRectangle);
            double rectY = Canvas.GetTop(mainWindow.selectionRectangle);
            double rectWidth = mainWindow.selectionRectangle.Width;
            double rectHeight = mainWindow.selectionRectangle.Height;

            Rect selectionBounds = new Rect(rectX, rectY, rectWidth, rectHeight);

            List<Border> borderElems = new List<Border>(); // Элементы, попавшие в пересечение и имеющие цвет выделения (LightBlue)

            foreach (UIElement element in mainWindow.wrapPanelView.Children)
            {
                if (element is Border border)
                {
                    Point elementPosition = border.TranslatePoint(new Point(0, 0), mainWindow.selectionCanvas);
                    Rect elementBounds = new Rect(elementPosition, new Size(border.ActualWidth, border.ActualHeight));

                    // Проверяем пересечение
                    if (selectionBounds.IntersectsWith(elementBounds))
                    {
                        if(border.Background == Brushes.LightBlue) // Собираем выделенные элементы, которые попали в пересечение (для изменения цвета с Ctrl)
                        {
                            borderElems.Add(border);
                            continue;
                        }

                        border.Background = Brushes.LightBlue; // Меняем цвет выделенных элементов
                        //border.Tag = true; // Помечаем как выделенный
                    }
                }
            }
            // Сбрасываем цвет у элементов, попавших в пересечение, но имеющие уже выделение
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (borderElems.Count == 1) borderElems[0].Background = Brushes.Gainsboro;
                else borderElems.ForEach(b => b.Background = (Brush)mainWindow.TryFindResource("MaterialDesignPaper"));
            }

            // ----------------------------- ОТКРЫТЬ ПАПКУ ИЛИ ФАЙЛ --------------------------------------------------------------
            
            // Получаем все Border с фоном LightBlue
            List<Border> borders = mainWindow.wrapPanelView.Children
                .OfType<Border>()
                .Where(b => (b.Background as SolidColorBrush)?.Color == Colors.LightBlue)
                .ToList();

            // если выделен всего один элемент без зажатых кнопок: Ctrl и Shift, то обрабатываем его, как открыть
            if (borders.Count == 1)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    return;
                }
                //Output.WriteLine("one element");
                var elem = StaticVariables.listFoldersAndFiles.Where(i => i.OuterBorder.Name == borders[0].Name).FirstOrDefault();

                if (elem != null)
                {
                    if (elem.Type == "folder")
                    {
                        Output.WriteLine("Folder");
                        // показываем содержимое папки
                        await FillWindow.FillContentWindow((elem.Path).TrimStart('/', '\\'), "/");
                    }
                    else
                    {
                        Output.WriteLine("File");
                        Output.WriteLine(elem.Path);

                        // Если файл является картинкой доступного для просмотра формата
                        if (Services.CheckOnPicture(Path.GetExtension(elem.Path)))
                        {
                            //// Проверяем, есть ли уже открытое окно типа ChildWindow
                            //var existingWindow = Application.Current.Windows
                            //    .OfType<ShowPicturesOne>()
                            //    .FirstOrDefault();

                            //// Если окно найдено, закрываем его
                            //existingWindow?.Close();

                            ShowPicturesOne showPicturesOne = new ShowPicturesOne(elem.Path);
                            showPicturesOne.Show();
                        }
                        // Проверяем, что это видео нужного формата
                        else if (Services.CheckOnVideo(elem.Path))
                        {
                            // Защита от двойного клика
                            if (doubleClickProtection) return;
                            else doubleClickProtection = true;

                            // Проверяем, есть ли уже открытое окно типа ChildWindow
                            var existingWindow = Application.Current.Windows
                                .OfType<IDriveVideo>()
                                .FirstOrDefault();

                            // Если окно найдено, закрываем его
                            existingWindow?.Close();

                            IDriveVideo idriveVideo = new IDriveVideo(elem.Path);
                            idriveVideo.Show();

                            await Task.Delay(200);
                            doubleClickProtection = false;
                        }
                        // Проверяем, что это текстовый файл
                        else if(Services.CheckOnText(elem.Path))
                        {
                            // Защита от двойного клика
                            if (doubleClickProtection) return;
                            else doubleClickProtection = true;
                            await Task.Delay(200);
                            doubleClickProtection = false;

                            if(elem.Size > 20 * 1024 * 1024)
                            {
                               string  messageInternet = "Файл весит больше, чем 20 мб. Пожалуйста, скачайте его на локальный диск и откройте с помощью текстового редактора.";
                                await DialogWindows.InformationWindow(messageInternet);
                                return;
                            }

                            TextFileWindow textFileWindow = new TextFileWindow(elem.Path);
                            textFileWindow.Show();
                        }
                    }
                }
            }
            // ----------------------------- Открыть элемент ----------- Конец ----------------------------------------------
        }

        private void FillGapsInSelection()
        {
            List<Border> selectedBorders = new List<Border>();
            foreach (UIElement element in mainWindow.wrapPanelView.Children)
            {
                if (element is Border border && border.Background == Brushes.LightBlue)
                {
                    selectedBorders.Add(border);
                }
            }

            if (selectedBorders.Count < 2) return;

            // Найти индексы первого и последнего выделенного элемента
            int firstIndex = mainWindow.wrapPanelView.Children.IndexOf(selectedBorders.First());
            int lastIndex = mainWindow.wrapPanelView.Children.IndexOf(selectedBorders.Last());

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                if (mainWindow.wrapPanelView.Children[i] is Border border)
                {
                    border.Background = Brushes.LightBlue;
                    //border.Tag = true;
                }
            }
        }

        // Контекстное меню вызывается у первого выделенного элемента, если выделено несколько. Если выделен один или ни одного,
        // то проверяется элемент под мышью, он выделяется и вызывается его меню (если есть). Если ничего не найдено, меню не открывается.
        public void Canvas_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                return;
            }

            List<Border> selectedBorders = mainWindow.wrapPanelView.Children.OfType<Border>().Where(border => border.Background == Brushes.LightBlue).ToList();

            if (selectedBorders.Count >= 2)
            {
                Border firstSelected = selectedBorders.First();
                if (firstSelected.ContextMenu != null)
                {
                    firstSelected.ContextMenu.PlacementTarget = firstSelected;
                    firstSelected.ContextMenu.IsOpen = true;
                }
            }
            else
            {
                Point mousePosition = e.GetPosition(mainWindow.wrapPanelView);
                Border? hoveredBorder = mainWindow.wrapPanelView.Children.OfType<Border>().FirstOrDefault(border =>
                {
                    Point pos = border.TranslatePoint(new Point(0, 0), mainWindow.wrapPanelView);
                    return new Rect(pos, new Size(border.ActualWidth, border.ActualHeight)).Contains(mousePosition);
                });

                if (hoveredBorder != null)
                {
                    ResetSelection();
                    hoveredBorder.Background = Brushes.LightBlue;
                    if (hoveredBorder.ContextMenu != null)
                    {
                        hoveredBorder.ContextMenu.PlacementTarget = hoveredBorder;
                        hoveredBorder.ContextMenu.IsOpen = true;
                    }
                }
            }
        }
    }
}
