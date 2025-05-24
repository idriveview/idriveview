using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace IDriveView.Models
{
    // Модель настроек порядка отображения иконок контента в окне программы: сеткой или линиями
    internal class ViewWindowModel
    {
        public Double WidthOuterBorder { get; set; }
        public HorizontalAlignment HorizontalInnerBorder { get; set; }
        public Orientation OrientStackPanel { get; set; }
        public int NameItemLength { get; set; }
        public int IconWidth { get; set; }
        public int IconHeight { get; set; }
        public Thickness MarginTextBlock { get; set; }
        public Visibility VisibilityStackSize { get; set; }
        public ViewWindowModel(Double width, HorizontalAlignment horizontal, Orientation orientation, int nameLength, int iconWidth, int iconHeight, Thickness margin, Visibility visibilityStackSize)
        {
            WidthOuterBorder = width;
            HorizontalInnerBorder = horizontal;
            OrientStackPanel = orientation;
            NameItemLength = nameLength;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            MarginTextBlock = margin;
            VisibilityStackSize = visibilityStackSize;
        }
    }
}
