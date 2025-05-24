using IDriveView.AddWindows;
using IDriveView.Debuging;
using IDriveView.WorkClasses;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Shell;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace IDriveView.HelpCode
{
    class Services
    {
        static MainWindow mainWindow = Application.Current.Windows[0] as MainWindow;
        public static string BiteToKbToMbToGb(string type, long bite)
        {
            if (type == "folder") return "---";

            long resultKb = bite / 1024;
            long resultMb = resultKb / 1024;
            float resultMbF = resultKb / 1024f;
            if (bite.ToString().Length < 4) return bite.ToString() + " B";
            else if (resultKb.ToString().Length < 4) return resultKb.ToString() + " kB";
            else if (resultMb.ToString().Length < 4) return resultMbF.ToString("F2") + " mB";
            else return (resultMbF / 1024f).ToString("F2") + " gB";
        }
        public static int KbMbGbToBite(string space)
        {
            string weight = space.Split(' ')[1]; 
            if (weight == "B") return (int)float.Parse(space.Split(' ')[0]);
            else if (weight == "kB") return (int)float.Parse(space.Split(' ')[0]) * 1024;
            else if (weight == "mB") return (int)float.Parse(space.Split(' ')[0]) * 1024 * 1024;
            else if (weight == "gB") return (int)float.Parse(space.Split(' ')[0]) * 1024 * 1024 * 1024;
            else return 0;
        }
        // --- Выбор иконки для файла -------------------------------------------------
        public static PackIconKind ChooseFileIcon(string type, string path)
        {
            // Получаем расширение файла
            string extension = Path.GetExtension(path).ToLower();

            //string pathImage = "..\\Resources\\folder.png";

            PackIconKind iconKind = PackIconKind.Folder;
            if (type != "folder")
            {
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp" || extension == ".avif")
                {
                    //pathImage = "..\\Resources\\image.png";
                    iconKind = PackIconKind.FileImage;
                }
                else if (extension == ".txt" || extension == ".doc")
                {
                    iconKind = PackIconKind.FileDocument;
                }
                else if (extension == ".mp4" || extension == ".mpeg" || extension == ".mkv" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".flv" || extension == ".webm")
                {
                    iconKind = PackIconKind.FileVideo;
                }
                else if (extension == ".mp3" || extension == ".wav" || extension == ".flac" || extension == ".aac" || extension == ".ogg" || extension == ".aiff" || extension == ".wma" || extension == ".opus" || extension == ".m4a")
                {
                    iconKind = PackIconKind.FileMusic;
                }
                else if(extension == ".rar" || extension == ".zip")
                {
                    iconKind = PackIconKind.FolderZip;
                }
                else if (extension == ".docx")
                {
                    iconKind = PackIconKind.FileWord;
                }
                else if (extension == ".xlsx")
                {
                    iconKind = PackIconKind.FileExcel;
                }
                else if (extension == ".pdf")
                {
                    iconKind = PackIconKind.FilePdfBox;
                }
                else
                {
                    iconKind = PackIconKind.FileOutline;
                }
            }
            return iconKind;
        }
        // Проверка файла, что это картинка нужного формата
        public static bool CheckOnPicture(string ext)
        {
            if (ext.ToLower() == ".jpg" || ext.ToLower() == ".jpeg" || ext.ToLower() == ".png" || ext.ToLower() == ".webp")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // Проверка файла, что это видео нужного формата
        public static bool CheckOnVideo(string pathFile)
        {
            // Получаем расширение файла
            string extension = Path.GetExtension(pathFile).ToLower();
            if (extension == ".mp4" || extension == ".mpeg" || extension == ".mkv" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".flv" || extension == ".webm" || extension == ".mp3")
            {
                return true;
            }
            return false;
        }
        // Проверка файла, что это текстовый файл нужного формата
        public static bool CheckOnText(string pathFile)
        {
            // Получаем расширение файла
            string extension = Path.GetExtension(pathFile).ToLower();
            if (extension == ".txt" || extension == ".json" || extension == ".cs" || extension == ".html" || extension == ".css" || extension == ".js" || extension == ".jsx" || extension == ".py" || extension == ".idv")
            {
                return true;
            }
            return false;
        }

        // Создание папки для хранения картинок для превью на ПК
        public static void CreateFolderPreview()    
        {
            string folderPath = Settings.pathFolderSavePictures; // Укажите путь к папке

            try
            {
                if (Directory.Exists(folderPath))
                {
                    // Получаем все файлы в папке
                    //string[] files = Directory.GetFiles(folderPath);

                    //foreach (string file in files)
                    //{
                    //    File.Delete(file); // Удаляем каждый файл
                    //}
                    Directory.Delete(folderPath, true);

                    Output.WriteLine("Папка и все файлы успешно удалены.");
                }
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Output.WriteLine("Папка для файлов превью создана.");
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Ошибка при создании папки: {ex.Message}");
            }
        }
        // Удаление папки для хранения картинок для превью на ПК
        public static void DeleteFolderPreview()
        {
            string folderPath = Settings.pathFolderSavePictures; // Укажите путь к папке

            try
            {
                if (Directory.Exists(folderPath))
                {
                    // Получаем все файлы в папке
                    //string[] files = Directory.GetFiles(folderPath);

                    //foreach (string file in files)
                    //{
                    //    File.Delete(file); // Удаляем каждый файл
                    //}
                    Directory.Delete(folderPath, true);

                    Output.WriteLine("Папка и все файлы успешно удалены.");
                }
                else
                {
                    Output.WriteLine("Папка не найдена.");
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Ошибка при удалении файлов: {ex.Message}");
            }
        }

        // Очищение папки для хранения картинок для превью на ПК
        public static void ClearFolderPreview()
        {
            string folderPath = Settings.pathFolderSavePictures; // Укажите путь к папке
            try
            {
                // Удаляем директорию со всем содержимым, если она существует
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); // true для рекурсивного удаления
                }

                // Создаём директорию заново
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        //// Очищение папки для хранения картинок для превью на ПК
        //public static void ClearFolderPreview()
        //{
        //    string folderPath = Settings.pathFolderSavePictures; // Укажите путь к папке
        //    try
        //    {
        //        if (Directory.Exists(folderPath))
        //        {
        //            // Удаляем все файлы
        //            foreach (string file in Directory.GetFiles(folderPath))
        //            {
        //                File.Delete(file);
        //            }

        //            // Удаляем все подкаталоги
        //            foreach (string dir in Directory.GetDirectories(folderPath))
        //            {
        //                Directory.Delete(dir, true);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка: {ex.Message}");
        //    }
        //}
        // Открытие папки с картинками для превью и просмотра на ПК
        public static async void OpenFolderPreview(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            // Блокируем кнопку от повторного нажатия
            button.IsHitTestVisible = false;
            try
            {
                // путь к папке загрузки в облаке (открытая папка в программе)
                string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path.TrimEnd('/');
                // путь к папке на ПК, который соответствует папке, открытой в облаке
                string pathFolderPC = Path.Combine(Settings.pathFolderSavePictures, openFolderCloud).Replace('/', '\\');

                // Открыть папку в проводнике
                if (Directory.Exists(pathFolderPC))
                {
                    Process.Start("explorer.exe", pathFolderPC);
                }
                else
                {
                    await DialogWindows.InformationWindow("Папка создаётся при наличии картинок. Такая папка ещё не создана");
                }
            }
            finally
            {
                await Task.Delay(300);
                // Разблокируем кнопку после завершения операции
                button.IsHitTestVisible = true;
            }
              
        }

        // --- Естественная (человеческая) сортировка порядка списка, как в окне Windows ---
        public static int NaturalSortComparer(string x, string y)
        {
            if (x == y) return 0;

            var regex = new Regex("(\\d+)|([^\\d]+)");

            var xParts = regex.Matches(x);
            var yParts = regex.Matches(y);

            for (int i = 0; i < Math.Min(xParts.Count, yParts.Count); i++)
            {
                var xPart = xParts[i].Value;
                var yPart = yParts[i].Value;

                // Попытка сравнить числовые части
                if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
                {
                    int result = xNum.CompareTo(yNum);
                    if (result != 0) return result;
                }
                else // Сравнение строковых частей
                {
                    int result = string.Compare(xPart, yPart, StringComparison.OrdinalIgnoreCase);
                    if (result != 0) return result;
                }
            }

            return xParts.Count.CompareTo(yParts.Count);
        }

        // --- Получение продолжительности, если файл видео ---
        public static string GetDutationVideo(string pathFile)
        {
            // Проверяем, что это видео нужного формата
            if (CheckOnVideo(pathFile))
            {
                try
                {
                    var file = ShellFile.FromFilePath(pathFile);
                    var durationProp = file.Properties.System.Media.Duration;

                    if (durationProp?.Value != null)
                    {
                        var durationTicks = durationProp.Value.Value;
                        long milliseconds = (long)TimeSpan.FromTicks((long)durationTicks).TotalMilliseconds;
                        return milliseconds.ToString();
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибки или логирование
                    return "0";
                }
            }
            return "0";
        }

        // --- Windows урезает длинные имена файлов (в урезанных именах содержится "~") ---
        // метод проверяет на тильду и восстанавливает имя, если оно было обрезано 
        public static Dictionary<string, string> TrimLongFileNames(Dictionary<string, string> filesToUpload)
        {
            var result = new Dictionary<string, string>();

            foreach (var kvp in filesToUpload)
            {
                //string value = kvp.Value;

                //if (value.Contains("~"))
                //{
                //    FileInfo fileInfo = new FileInfo(kvp.Key);
                //    string fullFileName = Path.GetFileName(fileInfo.FullName);
                //    string diretoryCloud = Path.GetDirectoryName(kvp.Value);
                //    string newPathFileCloud = Path.Combine(diretoryCloud, fullFileName).Replace('\\', '/');
                //    Output.WriteLine(newPathFileCloud);
                //    result[kvp.Key] = newPathFileCloud;
                //}
                //else
                //{
                //    result[kvp.Key] = kvp.Value;
                //}
                FileInfo fileInfo = new FileInfo(kvp.Key);
                string fullFileName = Path.GetFileName(fileInfo.FullName);
                string diretoryCloud = Path.GetDirectoryName(kvp.Value);
                Output.WriteLine(diretoryCloud);
                string newPathFileCloud = Path.Combine(diretoryCloud, fullFileName).Replace('\\', '/');
                Output.WriteLine(newPathFileCloud);
                result[kvp.Key] = newPathFileCloud;
                Output.WriteLine("1111111111111111");
            }
            
            return result;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLongPathName(string shortPath, StringBuilder longPath, int bufferLength);

        public static string GetFullLongPath(string shortPath)
        {
            const int MAX_PATH = 32767;
            var buffer = new StringBuilder(MAX_PATH);

            int result = GetLongPathName(shortPath, buffer, MAX_PATH);

            if (result > 0 && result < MAX_PATH)
            {
                return buffer.ToString();
            }

            // Если не удалось — возвращаем оригинал
            return shortPath;
        }
    }
}

