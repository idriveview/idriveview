using IDriveView.CreateReqest;
using IDriveView.HelpCode;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IDriveView.AddWindows
{
    /// <summary>
    /// Логика взаимодействия для TextFileWindow.xaml
    /// </summary>
    public partial class TextFileWindow : Window
    {
        string _pathFile;
        public TextFileWindow(string pathFile)
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

            _pathFile = pathFile;
            nameFile.Text = Path.GetFileName(pathFile);

            Loaded += TextFileWindow_Loaded;


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

        private bool _isFirstTime = true;
        private async void TextFileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBox.FontSize = Settings.fontSizeTextDocument;
            await new ContentRequest().DownloadFileToMemoryAndDisplay(Settings.mainBucket, _pathFile, textBox);
            textBox.CaretIndex = textBox.Text.Length;
            textBox.Focus(); // Чтобы курсор отобразился
            _isFirstTime = false; // При загрузке файла, не должно быть реакции на изменение текста
        }

        // --- Сигнализирует о том, что появились не сохранённые данные ---------------------------------------
        private bool _isTextChanged = false;
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isTextChanged && !_isFirstTime)
            {
                saveText.Visibility = Visibility.Visible;
                _isTextChanged = true;
            }
        }
        // --- Сохранить изменения в текстовом файле ---------------------------------------
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранить изменения в текстовом файле
            var result = await new ContentRequest().UploadTextBoxContentToCloud(Settings.mainBucket, _pathFile, textBox);

            if (!result)
            {
                await DialogWindows.InformationWindow("Изменения не сохранены. Смотри логи.");
                return;
            }

            // Скрываем кнопку после сохранения
            saveText.Visibility = Visibility.Collapsed;
            _isTextChanged = false;

            textBox.Focus(); // Чтобы курсор отобразился
        }

        // --- Обработка нажатия клавиши Tab для вставки пробелов ---------------------------------------
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата клавиша Tab (или Shift + Tab)
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                // Для обычного Tab (вставляем 4 пробела)
                if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    e.Handled = true;
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, "    ");
                    textBox.CaretIndex = caretIndex + 4; // Перемещаем курсор после вставленных пробелов
                }
                // Для Shift + Tab (удаляем до 4 пробелов слева от курсора)
                else if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                {
                    e.Handled = true;
                    int caretIndex = textBox.CaretIndex;

                    // Убираем пробелы слева от курсора
                    int spaceCount = 0;
                    while (caretIndex > 0 && textBox.Text[caretIndex - 1] == ' ' && spaceCount < 4)
                    {
                        caretIndex--;
                        spaceCount++;
                    }

                    // Если пробелы были удалены, обновляем текст и позицию курсора
                    if (spaceCount > 0)
                    {
                        textBox.Text = textBox.Text.Remove(caretIndex, spaceCount);
                        textBox.CaretIndex = caretIndex;
                    }
                }
            }
        }


        // --- Переопределение закрытия программы -----------------------------------------------
        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_isTextChanged)
            {
                var dialog = new CustomThreeButtonDialog(
                    "Сохранить изменения перед выходом?",
                    "Сохранить",
                    "Не сохранять",
                    "Отменить"
                );

                dialog.Owner = this;
                bool? result = dialog.ShowDialog();

                string choice = dialog.SelectedOption;

                if (choice == "Сохранить")
                {
                    // Сохранить изменения в текстовом файле
                    var result2 = await new ContentRequest().UploadTextBoxContentToCloud(Settings.mainBucket, _pathFile, textBox);

                    if (!result2)
                    {
                        await DialogWindows.InformationWindow("Изменения не сохранены. Смотри логи.");
                        e.Cancel = true;
                        return;
                    }
                }
                else if (choice == "Не сохранять")
                {
                    // ...
                }
                else if (choice == "Отменить")
                {
                    // Пользователь отменил — отменяем закрытие
                    e.Cancel = true;
                    return;
                }
            }
        }

        // Обработка нажатия клавиш
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем Ctrl + A
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                textBox.SelectAll();

                e.Handled = true; // Останавливаем дальнейшую обработку
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true; // Предотвращаем стандартное поведение (например, "звук")

                // Сохранить изменения в текстовом файле
                var result = await new ContentRequest().UploadTextBoxContentToCloud(Settings.mainBucket, _pathFile, textBox);

                if (!result)
                {
                    await DialogWindows.InformationWindow("Изменения не сохранены. Смотри логи.");
                    return;
                }

                // Скрываем кнопку после сохранения
                saveText.Visibility = Visibility.Collapsed;
                _isTextChanged = false;
            }
        }
    }
}
