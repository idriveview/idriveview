using System.Windows;

namespace IDriveView;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Mutex для предотвращения запуска нескольких экземпляров приложения
    private static Mutex mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        bool isNewInstance;
        string mutexName = "IDriveViewMutex";

        mutex = new Mutex(true, mutexName, out isNewInstance);

        if (!isNewInstance)
        {
            MessageBox.Show("Программа уже запущена.", "IDriveView");
            Shutdown(); // Завершаем текущий экземпляр
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (mutex != null)
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }

        base.OnExit(e);
    }
}

