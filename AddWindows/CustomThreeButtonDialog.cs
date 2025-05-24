using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace IDriveView.AddWindows
{
    public class CustomThreeButtonDialog : Window
    {
        public string SelectedOption { get; private set; } = null;

        public CustomThreeButtonDialog(string textMessage, string button1Text, string button2Text, string button3Text)
        {
            Width = 500;
            MinHeight = 180;
            Background = System.Windows.Media.Brushes.Snow;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // WindowChrome
            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(10),
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);

            var message = new TextBlock
            {
                Text = textMessage,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Margin = new Thickness(10),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var button1 = new Button
            {
                Content = button1Text,
                Width = 120,
                Margin = new Thickness(5)
            };
            button1.Click += (s, e) =>
            {
                SelectedOption = button1Text;
                DialogResult = true;
                Close();
            };

            var button2 = new Button
            {
                Content = button2Text,
                Width = 120,
                Margin = new Thickness(5)
            };
            button2.Click += (s, e) =>
            {
                SelectedOption = button2Text;
                DialogResult = true;
                Close();
            };

            var button3 = new Button
            {
                Content = button3Text,
                Width = 120,
                Margin = new Thickness(5)
            };
            button3.Click += (s, e) =>
            {
                SelectedOption = button3Text;
                DialogResult = false; // можно оставить false для "Отмена"
                Close();
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { button1, button2, button3 }
            };

            var layout = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(35),
                Children = { message, buttonPanel }
            };

            Content = layout;
        }
    }

}
