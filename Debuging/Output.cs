namespace IDriveView.Debuging
{
    public static class Output
    {
        private static readonly DebugWindow _debugWindow = DebugWindow.Instance;
        private static bool IsDebugEnabled { get; set; } = false; // По умолчанию выключено в продакшене

        static Output()
        {
#if DEBUG
            IsDebugEnabled = true; // Включено только в Debug-сборке
#endif
        }

        public static void WriteLine(string message)
        {
            if (IsDebugEnabled)
                _debugWindow.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : " + message);
        }

        public static void Write(string message)
        {
            if (IsDebugEnabled)
                _debugWindow.WriteLine(message);
        }

        public static void Show()
        {
            if (IsDebugEnabled)
                _debugWindow.Show();
        }

        public static void Hide()
        {
            if (IsDebugEnabled)
                _debugWindow.Hide();
        }
        // Метод для включения/выключения отладки в runtime
        public static void SetDebugEnabled(bool enabled)
        {
            IsDebugEnabled = enabled;
        }
    }
}
