using IDriveView.Debuging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace IDriveView.AddWindows
{
    /// <summary>
    /// Логика взаимодействия для ContactsWindow.xaml
    /// </summary>
    public partial class ContactsWindow : Window
    {
        string urlToYouTube = "http://idriveview.site/videos/linkToYouTube.txt"; // ссылка не текстовый файл на сервере сайта
        public ContactsWindow()
        {
            InitializeComponent();
            #region действия при изменении размеров окна
            // отследить изменения ширины всего окна
            this.SizeChanged += OnWindowSizeChanged;
            void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (e.WidthChanged)
                {
                    if (this.WindowState == WindowState.Maximized)
                    {
                        nameBorder.BorderThickness = new Thickness(7);
                        labelMaxmin.Content = "❐";
                    }
                    else
                    {
                        nameBorder.BorderThickness = new Thickness(2, 1, 2, 2);
                        labelMaxmin.Content = "☐";
                    }
                }
            }
            #endregion
            Loaded += ContactsWindow_Loaded;
        }
        #region обработка шапки (с кнопками)
        // обрабатывает наведение на кнопку
        private void header_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            if (border.Name == "close")
                border.Background = Brushes.Red;
            else
            {
                border.Background = (Brush)this.TryFindResource("PrimaryHueLightBrush");
                border.Opacity = 0.7;
            }
        }
        // обрабатывает: мыша покидает кнопку
        private void header_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            border.Background = (Brush)this.TryFindResource("PrimaryHueMidBrush");
            border.Opacity = 1;
        }
        // обрабатывает нажатие на кнопку
        private void header_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border.Name == "close")
                border.Background = Brushes.LightPink;
            else
            {
                border.Background = (Brush)this.TryFindResource("PrimaryHueLightBrush");
                border.Opacity = 1;
            }
        }
        // управение действием кнопок: закрыть, на весь экран, маленькое окно, свернуть на панель задач
        private void header_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border.Name == "close")
                this.Close();
            else if ((border.Name == "maxmin"))
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            }
            else
                this.WindowState = WindowState.Minimized;
        }
        #endregion



        private async void ContactsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string linkToYouTube = await GetLinkToYouTube(urlToYouTube);

            linkSite.Text = "https://idriveview.site";
            linkGithub.Text = "https://github.com/idriveview/idriveview/tree/main";
            linkYoutube.Text = linkToYouTube;
            linkEmail.Text = "idriveview@gmail.com";
            //wallet.Text = "0x874784d04c895BfB7a4d544F6b92073baD6A9e3a";
            //GitHubLink.NavigateUri = new Uri(linkGithub.Text);
            //AppWebsiteLink.NavigateUri = new Uri(linkSite.Text);
            //svailableTranslations.Text = "USDT - сети: BSC BNB Smart Chain (BEP20), ARBITRUM Arbitrum One, OPTIMISM Optimism\r\n" +
            //    "ETH - сети: ETH Ethereum (ERC20), BSC BNB Smart Chain (BEP20), ARBITRUM Arbitrum One, OPTIMISM Optimism\r\n" +
            //    "BNB - сеть: BSC BNB Smart Chain (BEP20)\r\n" +
            //    "OP Optimism - сеть: OPTIMISM\r\n" +
            //    "ARB Arbitrum - сеть: ARBITRUM Arbitrum One";
        }

        private void buttonSite_Click(object sender, RoutedEventArgs e)
        {
            OpenLink(linkSite.Text);
        }

        private void buttonGithub_Click(object sender, RoutedEventArgs e)
        {
            OpenLink(linkGithub.Text);
        }

        private void copySile_Click(object sender, RoutedEventArgs e)
        {
            SetToBuffer(linkSite.Text);
        }

        private void copyGithub_Click(object sender, RoutedEventArgs e)
        {
            SetToBuffer(linkGithub.Text);
        }

        private void buttonYoutube_Click(object sender, RoutedEventArgs e)
        {
            OpenLink(linkYoutube.Text);
        }

        private void copyYoutube_Click(object sender, RoutedEventArgs e)
        {
            SetToBuffer(linkYoutube.Text);
        }

        private void copyEmail_Click(object sender, RoutedEventArgs e)
        {
            SetToBuffer(linkEmail.Text);
        }

        //private void copyWallet_Click(object sender, RoutedEventArgs e)
        //{
        //    SetToBuffer(wallet.Text);
        //}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии ссылки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void CopyLink_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var menuItem = sender as MenuItem;
        //        string linkName = menuItem?.Tag?.ToString();
        //        string url = linkName switch
        //        {
        //            "GitHubLink" => GitHubLink.NavigateUri.ToString(),
        //            "AppWebsiteLink" => AppWebsiteLink.NavigateUri.ToString(),
        //            _ => string.Empty
        //        };

        //        if (!string.IsNullOrEmpty(url))
        //        {
        //            Clipboard.SetText(url);
        //            MessageBox.Show("Ссылка скопирована в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при копировании ссылки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void OpenLink(string link)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии ссылки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetToBuffer(string text)
        {
            try
            {
                Clipboard.SetText(text);

                // Показать Chip
                CopyChip.Visibility = Visibility.Visible;

                // Скрыть Chip через 2 секунды
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, args) =>
                {
                    CopyChip.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при копировании: " + ex.Message);
            }
        }

        // Прочитать файл с ссылкой на Youtube с сервера сайта
        private async Task<string> GetLinkToYouTube(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string content = await client.GetStringAsync(url);
                    Output.WriteLine("Содержимое файла:");
                    Output.WriteLine(content);
                    return content;
                }
                catch (HttpRequestException e)
                {
                    Output.WriteLine($"Ошибка при запросе: {e.Message}");
                    return "https://www.youtube.com";
                }
            }
        }
    }
}
