using System.Windows.Controls;

namespace IDriveView.Models
{
    class PathToSelectFolder
    {
        public string Path { get; set; }
        public StackPanel SelectStackPanel {  get; set; }
        public PathToSelectFolder(string path, StackPanel stackPanel)
        {
            Path = path;
            SelectStackPanel = stackPanel;
        }
    }
}
