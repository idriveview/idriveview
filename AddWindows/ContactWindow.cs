using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;

namespace IDriveView.AddWindows
{
    public class ContactWindow : Window
    {
        public ContactWindow(string titleText, string copyableText)
        {
            Width = 700;
            SizeToContent = SizeToContent.Height;
            Background = Brushes.Snow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(10),
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);

            // Внутреннее содержимое окна с фоном и тенью
            var contentBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                //Effect = new System.Windows.Media.Effects.DropShadowEffect
                //{
                //    Color = Colors.Black,
                //    Direction = 270,
                //    ShadowDepth = 0,
                //    Opacity = 0.3,
                //    BlurRadius = 20
                //},
                Padding = new Thickness(20),
                Child = CreateContent(titleText, copyableText)
            };

            Content = contentBorder;

        }

        private UIElement CreateContent(string title, string copyable)
        {
            var notificationText = new TextBlock
            {
                Text = "Скопировано!",
                FontSize = 14,
                Foreground = Brushes.Green,
                Background = Brushes.WhiteSmoke,
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity = 0,
                Margin = new Thickness(0, 10, 0, 0),
                TextAlignment = TextAlignment.Center
            };

            var closeButton = new Button
            {
                Content = "✖",
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                FontSize = 14,
                Foreground = Brushes.Gray,
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => Close();

            var titleBox = new TextBox
            {
                Text = title,
                IsReadOnly = true,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Cursor = Cursors.IBeam,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var copyableBox = new TextBox
            {
                Text = copyable,
                IsReadOnly = true,
                FontSize = 14,
                Margin = new Thickness(0, 20, 0, 10),
                Padding = new Thickness(0, 0, 0, 15),
            };

            var copyButton = new Button
            {
                Content = "Скопировать адрес",
                Width = 160,
                Margin = new Thickness(10, -5, 0, 0),
            };

            copyButton.Click += (s, e) =>
            {
                Clipboard.SetText(copyable);
                //MessageBox.Show("Скопировано!");
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                };

                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(1),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(fadeIn);
                storyboard.Children.Add(fadeOut);

                Storyboard.SetTarget(fadeIn, notificationText);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

                Storyboard.SetTarget(fadeOut, notificationText);
                Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

                storyboard.Begin();
            };

            var adressStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { copyableBox, copyButton }
            };

            return new StackPanel
            {
                Children = { closeButton, titleBox, adressStack, notificationText}
            };
        }
    }
}
