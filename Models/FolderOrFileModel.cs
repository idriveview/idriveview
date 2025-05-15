using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace IDriveView.Models
{
    class FolderOrFileModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ParentDirectory { get; set; }
        public long Size { get; set; }
        public Border OuterBorder { get; set; }
        public Border InnerBorder { get; set; }
        public PackIcon Packicon { get; set; }
        public StackPanel Stackpanel { get; set; }
        public TextBlock Textblock { get; set; }
        public FolderOrFileModel(string type, string name, string path, string parentDirectory, long size)
        {
            Type = type;
            Name = name;
            Path = path;
            ParentDirectory = parentDirectory;
            Size = size;
        }

        public FolderOrFileModel Clone()
        {
            return new FolderOrFileModel(Type, Name, Path, ParentDirectory, Size)
            {
                OuterBorder = this.OuterBorder,
                InnerBorder = this.InnerBorder,
                Packicon = this.Packicon,
                Stackpanel = this.Stackpanel,
                Textblock = this.Textblock
            };
        }
    }
}
