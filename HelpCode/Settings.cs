using IDriveView.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IDriveView.HelpCode
{
    class Settings
    {
        public static string folderSettings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IDriveView");
        //public readonly static string pathConfigRegistr = "configlogin.json"; // путь к файлу с настройками регистрации
        public readonly static string pathConfigRegistr = Path.Combine(folderSettings, "configlogin.json"); // путь к файлу с настройками регистрации
        //public readonly static string pathLogFile = "app.log"; // путь к файлу с логгированием
        public readonly static string pathLogFile = Path.Combine(folderSettings, "app.log");// путь к файлу с логгированием
        public static string mainBucket = ""; // название главного бакета (получаем при входе пользователя)
        public readonly static string namePicturesFolder = "IDriveViewSave"; // имя папки для сохранения картинок для превью и просмотра
        public readonly static string pathFolderSavePicturesDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), namePicturesFolder); // путь к папке сохранение картинок для превью и просмотра
        public static string pathFolderSavePictures = pathFolderSavePicturesDefault; // путь к папке сохранение картинок для превью и просмотра
        public readonly static string EmployedSpace = "0 байт"; // занятое место в облаке
        public readonly static string tariff = "10 Гб"; // тариф
        public readonly static byte fontSizeTextDocument = 16; // Размер шрифта текста в просмотре текстовых документов

        public static int transitionSecondLine = SetGridView().IconHeight * 2;

        // Для распольжения элементов в окне программы: сеткой
        public static ViewWindowModel SetGridView()
        {
            //Double WidthOuterBorder = Double.NaN; // ширина элемента 
            Double WidthOuterBorder = 190; // ширина элемента
            HorizontalAlignment HorizontalInnerBorder = HorizontalAlignment.Center; // горизонтальное расположение элемента
            Orientation OrientStackPanel = Orientation.Vertical;
            int NameItemLength = 10; // длина имени элемента
            int IconWidth = 150;
            int IconHeight = 100;
            Thickness MarginTextBlock = new Thickness(0, 5, 0, 0);
            Visibility VisibilityStackSize = Visibility.Collapsed;

            return new ViewWindowModel(WidthOuterBorder, HorizontalInnerBorder, OrientStackPanel, NameItemLength, IconWidth, IconHeight, MarginTextBlock, VisibilityStackSize);
        }
        // Для распольжения элементов в окне программы: линиями
        public static ViewWindowModel SetLineView()
        {
            Double WidthOuterBorder = 10000; // ширина элемента
            HorizontalAlignment HorizontalInnerBorder = HorizontalAlignment.Left; // горизонтальное расположение элемента
            Orientation OrientStackPanel = Orientation.Horizontal;
            int NameItemLength = 10000; // длина имени элемента
            int IconWidth = 50;
            int IconHeight = 30;
            Thickness MarginTextBlock = new Thickness(10, 5, 0, 0);
            Visibility VisibilityStackSize = Visibility.Visible;

            return new ViewWindowModel(WidthOuterBorder, HorizontalInnerBorder, OrientStackPanel, NameItemLength, IconWidth, IconHeight, MarginTextBlock, VisibilityStackSize);
        }
    }

}
