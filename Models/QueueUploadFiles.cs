namespace IDriveView.Models
{
    class QueueUploadFiles
    {
        // Класс для создания и сортировки очереди загрузки файлов в облако
        public string OpenFolderCloud { get; set; }
        public Dictionary<string, string> FilesForUpload { get; set; }
        public string FileLargeForUpload { get; set; }
        public string PathSaveToPC { get; set; }
        public QueueUploadFiles(string openFolderCloud, Dictionary<string, string> filesForUpload, string fileLargeForUpload, string pathSaveToPC = "")
        {
            OpenFolderCloud = openFolderCloud;
            FilesForUpload = filesForUpload;
            FileLargeForUpload = fileLargeForUpload;
            PathSaveToPC = pathSaveToPC;
        }
    }
}
