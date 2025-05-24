namespace IDriveView.Models
{
    class DataDownlosdFile
    {
        public string PathFileCloud { get; set; }
        public string PathFilePC { get; set; }
        public DataDownlosdFile(string pathFileCloud, string pathFilePC)
        {
            PathFileCloud = pathFileCloud;
            PathFilePC = pathFilePC;
        }
    }
}
