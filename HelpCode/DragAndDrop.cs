using IDriveView.WorkClasses;
using System.Windows;

namespace IDriveView.HelpCode
{
    class DragAndDrop
    {
        MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;

        // При наведении файла — показываем оверлей
        public void DropBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                mainWindow.Overlay.Visibility = Visibility.Visible;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        // Когда файл уводят с поля — скрываем оверлей только если мышь не нажата
        public void DropBorder_DragLeave(object sender, DragEventArgs e)
        {
            mainWindow.Overlay.Visibility = Visibility.Collapsed;
        }

        // При Drop — получаем путь и скрываем оверлей
        public async void DropBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Получаем массив путей к файлам или папкам
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Запуск загрузки контента в облако
                //StartUploadToCloud(paths);
                _ = new DownUpContent().UpLoadDragAndDropAsync(paths);

                // Выводим путь в консоль (или можно в TextBlock)
                //Consol.Logtext($"Файл: {paths[0]}");

                // Скрываем оверлей
                mainWindow.Overlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}
