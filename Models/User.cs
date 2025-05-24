namespace IDriveView.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime StartRegistration { get; set; }
        public string PathSavePictures { get; set; }
        public string Tariff { get; set; }
        public string EmployedSpace { get; set; } = string.Empty;
        public DateTime DateTimeStart { get; set; }
        public long SizeDownload { get; set; }
        public long SizeWatch { get; set; }
        public long SizeUpload { get; set; }
        public UsersDownload UserDownloadList { get; set; }
        public WatchDownload WatchDownloadList { get; set; }
        public UsersUpload UserUploadList { get; set; }

        public User()
        {
            UserDownloadList = new UsersDownload();  // Создаём объект DownloadList при создании User
            WatchDownloadList = new WatchDownload();  // Создаём объект WatchDownload при создании User
            UserUploadList = new UsersUpload();  // Создаём объект UploadList при создании User
        }
    }

    public class UsersDownload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<long> DownloadList { get; set; } = new List<long>();
        public User? User { get; set; }
    }

    public class WatchDownload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<long> WatchList { get; set; } = new List<long>();
        public User? User { get; set; }
    }

    public class UsersUpload
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<long> UploadList { get; set; } = new List<long>();
        public User? User { get; set; }
    }


    //public class User
    //{
    //    public int Id { get; set; }
    //    public string? Name { get; set; }
    //    public DateTime DateTimeStart { get; set; }
    //    public long SizeDownload { get; set; }
    //    public long SizeUpload { get; set; }
    //}

    //public class UsersDownload
    //{
    //    public int Id { get; set; }
    //    public string? Name { get; set; }
    //    public List<long> DownloadList { get; set; } = new List<long>();
    //}

    //public class UsersUpload
    //{
    //    public int Id { get; set; }
    //    public string? Name { get; set; }
    //    public List<long> UploadList { get; set; } = new List<long>();
    //}


}
