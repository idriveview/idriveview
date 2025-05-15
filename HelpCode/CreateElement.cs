using IDriveView.Debuging;
using IDriveView.Models;
using IDriveView.WorkClasses;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IDriveView.HelpCode
{
    class CreateElement
    {
        // --- Метод создания кнопки в панели: pathToSelectFolder - навигация по пройденным папкам ---------------------
        // применяется в FillWindow.FillContentWindow
        public static async void PathToSelectedFolder(MainWindow mainWindow, WrapPanel wrapPanel, string pathFolder, List<string> pathChaildFolders, bool createButton)
        {
            if (createButton)
            {
                string folderName;
                if (pathFolder == "") folderName = "Drive";
                else folderName = (Path.GetFileName(pathFolder.TrimEnd('/', '\\')));

                pathFolder = pathFolder.TrimStart('/', '\\');// если не сделать, то появляется в окне лишний элемент

                if (folderName.Length > 10) folderName = folderName.Remove(10) + "...";
                //-------------------------------------------------------
                // Создаем StackPanel для размещения кнопки и иконки в ряд
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                //-------------------------------------------------------
                // Создаем кнопку
                Button driveButton = new Button
                {
                    //Name = "drive",
                    Content = folderName,
                    Padding = new Thickness(10, 0, 10, 0),
                    Height = 25,
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontSize = 16,
                    Tag = pathFolder,
                    ToolTip = pathFolder
                };

                // Применяем стиль MaterialDesignFlatButton (если он задан в ресурсах)
                driveButton.SetResourceReference(Control.StyleProperty, "MaterialDesignFlatButton");

                driveButton.Click += async (sender, args) =>
                {
                    Button button = (Button)sender;
                    // Заполняем окно контентом

                    // Блокируем кнопку от повторного нажатия
                    button.IsEnabled = false;
                    try
                    {
                        // Выполняем асинхронную операцию
                        await FillWindow.FillContentWindow(button.Tag.ToString(), "/", false);
                    }
                    finally
                    {
                        // Разблокируем кнопку после завершения операции
                        button.IsEnabled = true;
                    }
                };

                // Создаем контекстное меню
                ContextMenu contextMenu = new ContextMenu();
                // Список заголовков для контекстного меню
                var menuHeaders = pathChaildFolders;
                // Метод для обработки кликов по элементам контекстного меню
                async void OnMenuItemClick(object sender, RoutedEventArgs e)
                {
                    if (sender is MenuItem menuItem)
                    {
                        string path = menuItem.Header.ToString();
                        DeleteButtons(pathFolder);

                        if (menuItem.Parent is ContextMenu contextMenu)
                        {
                            if (contextMenu.PlacementTarget is Button button)
                            {
                                // Заполняем окно контентом
                                await FillWindow.FillContentWindow(button.Tag.ToString() + path + "/", "/", true);
                            }
                        }
                        //await FillWindow.FillContentWindow(pathFolder.TrimStart('/', '\\') + path + "/", "/", true);
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
                driveButton.ContextMenu = contextMenu;
                //-------------------------------------------------------
                // Создаем иконку PackIcon
                PackIcon chevronIcon = new PackIcon
                {
                    Kind = PackIconKind.ChevronRight,
                    Margin = new Thickness(-5, 5, -5, 0)
                };
                //-------------------------------------------------------
                // Добавляем кнопку и иконку в StackPanel
                stackPanel.Children.Add(driveButton);
                stackPanel.Children.Add(chevronIcon);

                // Добавляем StackPanel в контейнер, например, в Grid
                wrapPanel.Children.Add(stackPanel);

                // Добавляем кнопку в список
                StaticVariables.pathToSelectFolders.Push(new PathToSelectFolder(pathFolder, stackPanel));
            }
            else
            {
                DeleteButtons(pathFolder);
            }
            // метод удаления кнопок, которые справа от нажимающей
            void DeleteButtons(string pathFolderClick)
            {
                int count = StaticVariables.pathToSelectFolders.Count;
                for (int a = 0; a < count; a++)
                {
                    PathToSelectFolder element = StaticVariables.pathToSelectFolders.Peek();
                    if (element.Path != pathFolderClick)
                    {
                        wrapPanel.Children.Remove(element.SelectStackPanel);
                        StaticVariables.pathToSelectFolders.Pop();
                    }
                    else break;
                }
            }
        }
        
    }
}
