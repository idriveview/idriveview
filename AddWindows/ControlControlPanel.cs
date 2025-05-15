using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace IDriveView.AddWindows
{
    class ControlControlPanel
    {
        private readonly IDriveVideo _mainWindow;
        public DispatcherTimer _timer;
        public bool stopTimer = false; // Переменная для остановки таймера

        private bool _isDisposed = false; // Флаг, чтобы избежать двойной очистки

        public ControlControlPanel(IDriveVideo mainWindow)
        {
            _mainWindow = mainWindow;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public void YourElement_MouseEnter(object sender, MouseEventArgs e)
        {
            _mainWindow.controlPanel.Visibility = Visibility.Visible; // Показываем панель управления
            _mainWindow.controlPanelFon.Visibility = Visibility.Visible; // Показываем панель управления
            Debug.WriteLine("Показать панель управления"); // Для тестирования
        }

        public void YourElement_MouseLeave(object sender, MouseEventArgs e)
        {
            _mainWindow.controlPanel.Visibility = Visibility.Collapsed; // Скрываем панель управления
            _mainWindow.controlPanelFon.Visibility = Visibility.Collapsed; // Скрываем панель управления
        }
        bool _isCloseWindow = false; // Переменная для проверки закрытия окна
        public void YourElement_MouseMove(object sender, MouseEventArgs e)
        {
            _timer.Stop();
            if (_isCloseWindow) // Если окно закрыто, то не показываем панель управления
            {
                return;
            }
            _timer.Start(); // Перезапускаем таймер при движении мыши

            _mainWindow.controlPanel.Visibility = Visibility.Visible; // Показываем панель управления
            _mainWindow.controlPanelFon.Visibility = Visibility.Visible; // Показываем панель управления
            Mouse.OverrideCursor = null; // Показываем курсор мыши
        }

        public void Timer_Tick(object sender, EventArgs e)
        {

            _timer.Stop(); // Останавливаем таймер после скрытия

            _mainWindow.controlPanel.Visibility = Visibility.Collapsed; // Скрываем панель управления
            _mainWindow.controlPanelFon.Visibility = Visibility.Collapsed; // Скрываем панель управления 

            // Проверяем зкрытие окна IDriveVideo, если закрыто, то не скрваем курсор
            // Проверяемя, что курсор находится в окне плеера
            if (!stopTimer && _mainWindow.gridContent.IsMouseOver)
                Mouse.OverrideCursor = Cursors.None; // Скрываем курсор мыши
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            stopTimer = true;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }

            // Отписка от событий мыши
            if (_mainWindow is UIElement element)
            {
                element.MouseEnter -= YourElement_MouseEnter;
                element.MouseLeave -= YourElement_MouseLeave;
                element.MouseMove -= YourElement_MouseMove;
            }

            Debug.WriteLine("ControlControlPanel освобождён");
        }
    }
}
