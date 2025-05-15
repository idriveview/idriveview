using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace IDriveView.AddWindows
{
    public class SmallDialog : Window
    {
        // Диалоговое окно принятия решения: Да или Нет
        public SmallDialog(string textMessage)
        {
            Width = 400;
            MinHeight = 180;
            Background = System.Windows.Media.Brushes.Snow;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Применим WindowChrome
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

            var continueButton = new Button
            {
                Content = "Продолжить",
                Width = 120,
                Margin = new Thickness(5)
            };
            continueButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button
            {
                Content = "Отменить",
                Width = 120,
                Margin = new Thickness(5)
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { continueButton, cancelButton }
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
