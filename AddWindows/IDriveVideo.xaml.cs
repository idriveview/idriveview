using Amazon.S3;
using Amazon.S3.Model;
using IDriveView.CreateClient;
using IDriveView.Debuging;
using IDriveView.HelpCode;
using IDriveView.WorkClasses;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops;

namespace IDriveView.AddWindows
{
    /// <summary>
    /// Логика взаимодействия для IDriveVideo.xaml
    /// </summary>
    public partial class IDriveVideo : Window
    {
        private long durationVideo; // теперь не задаём жёстко
        bool _isPaused = true;
        bool _isStoped = false;
        bool _isClosineg = false; // Переменная для проверки закрытия окна
        long _weightOneSecond;
        private DispatcherTimer timer;
        string filePath; // переместим путь в переменную
        ControlControlPanel controlControlPanel;
        public IDriveVideo(string pathFile)
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

            filePath = pathFile;
            nameFile.Text = Path.GetFileName(filePath);

            controlControlPanel = new ControlControlPanel(this); // Передаём MainWindow

            // Подписываемся на Loaded
            Loaded += MainWindow_Loaded;

            // Инициализация таймера
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // Привязка изменение слайдера к отображению текущего времени проигрывания видео
            videoSlider.ValueChanged += (s, e) =>
            {
                var currentTime = videoSlider.Value / 1000;
                CurrentTimeText.Text = $"{TimeSpan.FromSeconds(currentTime):hh\\:mm\\:ss}";
            };

            // Подпишемся на события мыши
            MouseEnter += controlControlPanel.YourElement_MouseEnter;
            MouseLeave += controlControlPanel.YourElement_MouseLeave;
            MouseMove += controlControlPanel.YourElement_MouseMove;

            fullScreen.Click += FullScreen_Click;
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
        // управение действием кнопок: закрыть, на весь экран, маленькое окно, свернуть на панель задач checkClickButtonWindowState
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
            else this.WindowState = WindowState.Minimized;
        }
        #endregion

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем длительность видео из метаданных
                durationVideo = await GetVideoDurationAsync(filePath);
                videoSlider.Maximum = durationVideo;
                DurationTimeText.Text = $"{TimeSpan.FromSeconds(durationVideo / 1000):hh\\:mm\\:ss}";
            }
            catch
            {
                durationVideo = 0;
            }

            // Инициализация VLC (Библиотека перенесена в корень программы плеера)
            //vlcPlayer.SourceProvider.CreatePlayer(new DirectoryInfo(@"VideoLAN\VLC"));
            // Инициализация происходит один раз при загрузке окна, потому что объект нельзя остановить правильно
            if (vlcPlayer.SourceProvider.MediaPlayer == null)
            {
                vlcPlayer.SourceProvider.CreatePlayer(new DirectoryInfo(@"VideoLAN\VLC"));
            }

            // При изменении позиции видео обновляем слайдер
            vlcPlayer.SourceProvider.MediaPlayer.PositionChanged += (s, args) =>
            {
                var durationLostVideo = vlcPlayer.SourceProvider.MediaPlayer.Time;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Mouse.LeftButton != MouseButtonState.Pressed && !_isPaused)
                    {
                        videoSlider.Value = durationLostVideo;
                    }
                }));
            };
            // При остановке видео обновляем кнопки
            vlcPlayer.SourceProvider.MediaPlayer.Stopped += (s, args) =>
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _isPaused = true;
                    _isStoped = true; // останавливаем видео
                    //pause.Content = "Play";
                    pauseIcon.Kind = PackIconKind.Play;
                }));
                //MessageBox.Show("Воспроизведение остановлено.");
                // проблема в том, что метод Stop(), который вызывается при закрытии окна, даёт исключение
                if (_isClosineg) vlcPlayer.SourceProvider.MediaPlayer.Dispose(); // Освобождаем ресурсы плеера перед закрытием окна
            };
            // При клике на видео вызываем паузу
            vlcPlayer.MouseLeftButtonUp += vlcPlayer_MouseLeftButtonUp;
            // Ошибка плеера
            vlcPlayer.SourceProvider.MediaPlayer.EncounteredError += (s, args) =>
            {
                MessageBox.Show("Ошибка в VLC!");
            };

            timer.Start(); // запуск после полной инициализации

            PlayButton_Click(null, null); // запускаем видео сразу после загрузки окна
        }

        // Таймер для отслеживания времени воспроизведения видео для того, чтобы высчитывать количество скачанных байт
        private long playbackSeconds = 0;
        private long saveCounter = 0;
        private long countTime = 5; // каждые 5 секунд сохраняем в настройках

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Проверяем, воспроизводится ли видео
            if (vlcPlayer.SourceProvider.MediaPlayer.IsPlaying() && !_isPaused)
            {
                // нужно только для тестирования (времени воспроизведения)
                // для тестирования включить в Visibility в XAML
                playbackSeconds++;
                PlaybackTimer.Text = $"{playbackSeconds} seconds";

                saveCounter++;
                if (saveCounter >= countTime) // Сохраняем каждые 5 секунд
                {
                    try
                    {
                        Properties.Settings.Default.LastPlaybackByte += saveCounter * _weightOneSecond; // Сохраняем количество скачанных байт
                        Properties.Settings.Default.Save(); // Записываем на диск
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}");
                    }
                    saveCounter = 0;
                }
            }
        }

        // Получение продолжительности видео из метоаданных файла
        public async Task<long> GetVideoDurationAsync(string filePath)
        {
            var _s3Client = S3ClientFactory.GetS3Client().s3Client;

            var headDestRequest = new GetObjectMetadataRequest
            {
                BucketName = Settings.mainBucket,
                Key = filePath
            };
            try
            {
                var metadata = await _s3Client.GetObjectMetadataAsync(headDestRequest);
                long duration = 0;
                long.TryParse(metadata.Metadata["x-amz-meta-durationVideo"], out duration);// длительность видео
                long bytes = metadata.Headers.ContentLength; // размер видео в байтах
                _weightOneSecond = bytes / (duration / 1000); // вес одного секунды видео
                return duration; // Возвращаем длительность видео
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения метаданных: {ex.Message}");
            }
            return 0; // Возвращаем 0, если произошла ошибка
        }

        // После завершения перетаскивания слайдера
        private async void VideoSlider25_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            long newPosition = (long)videoSlider.Value;

            // еслви видео закончилось, то запускаем его с начала
            if (_isStoped)
            {
                PlayButton_Click(null, null); // Запускаем видео
                return;
            }

            RewindVideo(newPosition); //Перемотка видео
        }

        // Перемотка видео
        private async Task RewindVideo(long newPosition)
        {
            // если видео воспроизводится, то ставим его на паузу, чтобы потом запустить с нужной позиции
            if (vlcPlayer.SourceProvider.MediaPlayer.IsPlaying())
            {
                vlcPlayer.SourceProvider.MediaPlayer.Pause(); // Останавливаем и ждем
                await Task.Delay(500);
            }
            vlcPlayer.SourceProvider.MediaPlayer.Time = newPosition; // Устанавливаем позицию
            vlcPlayer.SourceProvider.MediaPlayer.Pause(); // Вторая Pause (запускает видео)
            vlcPlayer.SourceProvider.MediaPlayer.Play(); // Дополнительный Play (добавлен экспериментально)

            //pause.Content = "Pause";
            pauseIcon.Kind = PackIconKind.Pause;
            _isPaused = false;
        }

        // Получение подписанной ссылки на видео из S3
        public async Task<string> GetSignedUrlFromS3Async(string filePath)
        {
            var _s3Client = S3ClientFactory.GetS3Client().s3Client;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = Settings.mainBucket,
                Key = filePath,
                Expires = DateTime.UtcNow.AddDays(1), // В продакшен будет: Один день. Максимум 7 дней
                //Expires = DateTime.UtcNow.AddMinutes(10), // URL действителен 10 минут (для тестирования)
                Verb = HttpVerb.GET // Метод HTTP (GET для скачивания)
            };

            return _s3Client.GetPreSignedURL(request);
        }

        // Кнопка Play
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                image.Visibility = Visibility.Collapsed; // скрываем картинку-заставку
                _isPaused = false;
                _isStoped = false; // сбрасываем флаг
                //pause.Content = "Pause";
                pauseIcon.Kind = PackIconKind.Pause;

                // Получаем подписанную ссылку на видео
                //string filePath = "second/rocnroll.mp4";
                string videoUrl = await GetSignedUrlFromS3Async(filePath);
                //Output.WriteLine(videoUrl);
                // Устанавливаем медиа и начинаем воспроизведение
                vlcPlayer.SourceProvider.MediaPlayer.SetMedia(new Uri(videoUrl));
                vlcPlayer.SourceProvider.MediaPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в PlayButton_Click: {ex.Message}");
            }
        }

        // Кнопка Pause
        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isStoped)
            {
                PlayButton_Click(null, null); // запускаем видео с начала
            }
            else
            {
                vlcPlayer.SourceProvider.MediaPlayer.Pause(); 
                if (pauseIcon.Kind == PackIconKind.Pause)
                {
                    _isPaused = true;
                    pauseIcon.Kind = PackIconKind.Play;
                }
                else
                {
                    _isPaused = false;
                    pauseIcon.Kind = PackIconKind.Pause;
                }
            }
        }

        // При клике на видео вызываем паузу
        private void vlcPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PauseButton_Click(null, null);
        }

        // При клике на кнопку "На весь экран"

        WindowState windowState;
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (iconFullscreen.Kind == PackIconKind.Fullscreen)
            {
                windowState = WindowState; // сохраняем состояние окна в переменную

                // Устанавливаем окно на весь экран
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
  
                titlePanel.Visibility = Visibility.Collapsed; // скрываем панель заголовка
                iconFullscreen.Kind = PackIconKind.FullscreenExit; // меняем иконку кнопки

                if (windowState == WindowState.Maximized)
                {
                    nameBorder.BorderThickness = new Thickness(7);
                }
                else
                {
                    nameBorder.BorderThickness = new Thickness(0);
                }
            }
            else
            {
                // если окно было развёрнуто из полноэкранного режима
                if (windowState == WindowState.Maximized)
                {
                    nameBorder.BorderThickness = new Thickness(7);
                    labelMaxmin.Content = "❐";
                }
                // если окно было развёрнуто из нормального режима
                else
                {
                    WindowState = WindowState.Normal;
                    nameBorder.BorderThickness = new Thickness(2, 1, 2, 2);
                    labelMaxmin.Content = "☐";
                }
                titlePanel.Visibility = Visibility.Visible; // показываем панель заголовка
                iconFullscreen.Kind = PackIconKind.Fullscreen; // меняем иконку кнопки

                // Возвращаем нормальные настройки окна
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
            }
        }

        // Обработка нажатия клавиш
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // При нажжатии Пробел дублируем действие кнопки Play
            if (e.Key == Key.Space)
            {
                PauseButton_Click(null, null);
                e.Handled = true; // Останавливаем дальнейшую обработку
            }
            else if (e.Key == Key.Left)
            {
                if (vlcPlayer.SourceProvider.MediaPlayer.Time < 5000) return; // если меньше 5 секунд, то не перематываем назад

                var newPosition = vlcPlayer.SourceProvider.MediaPlayer.Time - 5000; // Устанавливаем позицию
                RewindVideo(newPosition); //Перемотка видео назад
            }
            else if (e.Key == Key.Right)
            {
                if (vlcPlayer.SourceProvider.MediaPlayer.Time > videoSlider.Maximum - 10000) return; // если осталось меньше 10 секунд, то не перематываем вперёд

                var newPosition = vlcPlayer.SourceProvider.MediaPlayer.Time + 5000; // Устанавливаем позицию
                RewindVideo(newPosition); //Перемотка видео вперёд
            }
        }

        // При клике на кнопку закрытия окна
        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            _isClosineg = true;// Устанавливаем флаг закрытия окна для освобождения ресурсов плеера

            // Останавливаем управение панелью управления
            controlControlPanel?.Dispose();
            controlControlPanel = null;


            Properties.Settings.Default.LastPlaybackByte += saveCounter * _weightOneSecond; // Сохраняем значение
            Properties.Settings.Default.Save(); // Записываем на диск

            // проблема в том, что метод Stop() для остановки плеера, даёт исключение - это обходной путь
            if (vlcPlayer.SourceProvider.MediaPlayer.IsPlaying())
            {
                vlcPlayer.SourceProvider.MediaPlayer.Pause(); // Останавливаем и ждем
            }

            // Добавляем скачанные bytes при просмотре видео в Базу данных
            await WorkDateBase.AddByteVideo(Properties.Settings.Default.LastPlaybackByte);

            //double sizeMb = Properties.Settings.Default.LastPlaybackByte / 1024.0 / 1024.0;
            //MessageBox.Show(sizeMb.ToString("F2") + " mb");

            await Task.Delay(500); // Ждем 0.5 секунды
            Properties.Settings.Default.LastPlaybackByte = 0; // Обнуляем значение
            Properties.Settings.Default.Save();
        }
    }
}
