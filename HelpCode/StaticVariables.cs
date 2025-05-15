using IDriveView.AddWindows;
using IDriveView.Debuging;
using IDriveView.Models;
using IDriveView.WorkClasses;
using System.Windows.Controls;

namespace IDriveView.HelpCode
{
    class StaticVariables
    {
        public static UserIDrive IDriveOnDuty { get; set; } // дежурный экземпляр класса UserIDrive для работы с актуальным пользователем при регистрации

        // список папок и файлов в окне программы
        public static List<FolderOrFileModel> listFoldersAndFiles = new List<FolderOrFileModel>();

        // список папок и файлов в окне программы - только картинки
        public static List<FolderOrFileModel> listPicturesFilesOnly = new List<FolderOrFileModel>();

        // список-путь к выбранной папке
        public static Stack<PathToSelectFolder> pathToSelectFolders = new Stack<PathToSelectFolder>();

        // --- Бок загрузки файлов в облако ----------------------
        // активный прогресс-бар загрузки файлов в облако
        public static Grid ActivProugressIndicator;
        public static ProgressBar activProgressBar;
        public static TextBlock activProgressPercentText;
        public static Button activStopButton;
        // сейчас идёт скачивание файлов
        public static bool nowUploadFiles = false;
        // очередь на скачивание файлов
        public static Queue<QueueUploadFiles> queueUploadFiles = new Queue<QueueUploadFiles>();
        // список для отображение очереди на скачивание файлов
        public static Queue<string> showQueueUploadFiles = new Queue<string>();

        // --- Конец --- Бок загрузки файлов в облако ----------------------

        // --- Бок скачивания файлов на ПК ---------------------------------
        // сейчас идёт скачивание файлов
        public static bool nowDownloadFiles = false;
        // очередь на скачивание файлов
        public static Queue<QueueUploadFiles> queueDownloadFiles = new Queue<QueueUploadFiles>();
        // --- Бок скачивания файлов на ПК ----- Конец ------------------

        // текущий пользователь
        public static string currentUser = "";
        // текущий пользователь 
        public static UserInformation currentUserInformation = new UserInformation();

        // операции после зогрузки файлов в облако
        public static async Task ActionPostPartUploadFiles()
        {
            await Task.Delay(100);
            // отключает кнопку отмены скачивания в этом блоке
            StaticVariables.activStopButton.IsEnabled = false;
            // показывает, что в этом блоке скачивание закончилось
            StaticVariables.ActivProugressIndicator.Tag = false;
            // Запуск следующего скачивания (если оно есть)
            if (StaticVariables.queueUploadFiles.Count > 0)
            {
                MainWindow.queueUploadViewModel.Items.RemoveAt(0);
                StaticVariables.showQueueUploadFiles.Dequeue();
                await DownUpContent.UpLoadFilesAsyncContinue(StaticVariables.queueUploadFiles.Dequeue());
            }
            else
            {
                StaticVariables.nowUploadFiles = false;// индикатор: происходит, ли скачивание
                // путь к папке загрузки в облаке (открытая папка в программе)
                string openFolderCloud = StaticVariables.pathToSelectFolders.Peek().Path;
                // перезагрузить отображение текущей папки
                await FillWindow.FillContentWindow(openFolderCloud, "/", false);
            }
        }

        // операции после скачивания файлов на ПК
        public static async Task ActionPostPartDownloadFiles()
        {
            await Task.Delay(100);
            // отключает кнопку отмены скачивания в этом блоке
            //StaticVariables.activStopButton.IsEnabled = false;
            // показывает, что в этом блоке скачивание закончилось
            //StaticVariables.ActivProugressIndicator.Tag = false;
            // Запуск следующего скачивания (если оно есть)
            if (StaticVariables.queueDownloadFiles.Count > 0)
            {
                //MainWindow.queueUploadViewModel.Items.RemoveAt(0);
                //StaticVariables.showQueueDownloadFiles.Dequeue();
                await DownUpContent.DownLoadFilesAsyncContinue(StaticVariables.queueDownloadFiles.Dequeue());
            }
            else
            {
                StaticVariables.nowDownloadFiles = false;// индикатор: происходит, ли скачивание
                //await DialogWindows.InformationWindow("Загрузка файлов закончилась");
            }
        }
    }

}
