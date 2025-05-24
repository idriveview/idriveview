using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDriveView.Debuging
{
    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private static DebugWindow _instance;

        public static DebugWindow Instance
        {
            get
            {
                if (_instance == null || _instance.IsClosed)
                {
                    _instance = new DebugWindow();
                }
                return _instance;
            }
        }

        private bool IsClosed { get; set; }
        private bool _isPositionSet = false; // Флаг для установки позиции только один раз
        public DebugWindow()
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
            this.Closing += (s, e) =>
            {
                if (!IsClosed) // Только скрываем, если не вызван явный Close
                {
                    e.Cancel = true;
                    Hide();
                }
            };
            this.Closed += (s, e) => IsClosed = true;

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
        public void WriteLine(string message)
        {
            if (!IsClosed)
            {
                Dispatcher.Invoke(() =>
                {
                    if (!IsClosed)
                    {
                        OutputTextBox.AppendText(message + Environment.NewLine);
                        OutputTextBox.ScrollToEnd();
                        if (!IsVisible)
                        {
                            SetWindowPosition(); // Устанавливаем позицию перед показом
                            Show();
                        }
                    }
                });
            }
        }

        public void ForceClose()
        {
            IsClosed = true;
            Close();
        }
        private void SetWindowPosition()
        {
            if (!_isPositionSet)
            {
                var screen = SystemParameters.WorkArea; // Получаем рабочую область экрана (без панели задач)
                Left = screen.Right - Width;    // Позиция по X (правый край - ширина окна)
                Top = screen.Bottom - Height;   // Позиция по Y (нижний край - высота окна)
                _isPositionSet = true;          // Помечаем, что позиция установлена
            }
        }

        // Обработчик кнопки Wrap (включение/выключение переноса строк)
        private void WrapButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.TextWrapping = OutputTextBox.TextWrapping == TextWrapping.Wrap
                ? TextWrapping.NoWrap
                : TextWrapping.Wrap;
        }

        // Обработчик кнопки Copy (копировать в буфер обмена)
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputTextBox.Text))
            {
                Clipboard.SetText(OutputTextBox.Text);
            }
        }

        // Обработчик кнопки Write (записать в файл log.txt)
        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText("log.txt", OutputTextBox.Text);
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при записи в файл: {ex.Message}");
            }
        }

        // Обработчик кнопки Clear (очистить окно)
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text += "----------------------------------------" + Environment.NewLine;
        }
    }
}
