using IDriveView.HelpCode;
using System.Diagnostics;
using System.IO;

namespace IDriveView.Debuging
{
    public static class Logger
    {
        private static readonly string logFilePath = Settings.pathLogFile;
        //private static readonly long maxFileSize = 5 * 1024 * 1024; // 5 MB
        private static readonly long maxFileSize = 5 * 1024; // 5 кB

        public static async Task Log(string message)
        {
            try
            {
                string directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Проверка размера файла и обрезка, если нужно
                if (File.Exists(logFilePath))
                {
                    FileInfo fileInfo = new FileInfo(logFilePath);
                    if (fileInfo.Length >= maxFileSize)
                    {
                        await TrimLogFileByBytes();
                    }
                }

                string logEntry = $"{DateTime.Now}: {message}";
                await File.AppendAllTextAsync(logFilePath, logEntry + "\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка логирования: {ex.Message}");
            }
        }

        private static async Task TrimLogFileByBytes()
        {
            try
            {
                // Читаем весь файл как байты
                byte[] content = await File.ReadAllBytesAsync(logFilePath);
                if (content.Length <= 0) return;

                // Вычисляем половину размера
                int halfLength = content.Length / 2;

                // Ищем ближайший перенос строки после половины
                int startIndex = halfLength;
                while (startIndex < content.Length && content[startIndex] != (byte)'\n')
                {
                    startIndex++;
                }
                if (startIndex >= content.Length) startIndex = halfLength; // Если не нашли перенос, берём середину

                // Пропускаем начальный перенос строки или пробелы
                startIndex++; // Переходим за найденный \n
                while (startIndex < content.Length && (content[startIndex] == (byte)'\n' || content[startIndex] == (byte)'\r' || content[startIndex] == (byte)' '))
                {
                    startIndex++; // Пропускаем лишние переносы или пробелы
                }
                if (startIndex >= content.Length) return; // Если после пропуска ничего не осталось, ничего не пишем

                // Оставляем вторую половину, начиная с первой непустой строки
                byte[] trimmedContent = new byte[content.Length - startIndex];
                Array.Copy(content, startIndex, trimmedContent, 0, trimmedContent.Length);

                // Перезаписываем файл
                await File.WriteAllBytesAsync(logFilePath, trimmedContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обрезки лог-файла: {ex.Message}");
            }
        }

        // --- Старый, рабочий, простой ваниант --------------------------
        //private static readonly string logFilePath = Settings.pathLogFile;
        //public static async Task Log(string message)
        //{
        //    try
        //    {
        //        string directory = Path.GetDirectoryName(logFilePath);
        //        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        //        {
        //            Directory.CreateDirectory(directory);
        //        }

        //        string logEntry = $"{DateTime.Now}: {message}";
        //        await File.AppendAllTextAsync(logFilePath, logEntry + "\n");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Временный вывод в консоль, пока нет другого логгера
        //        Debug.WriteLine($"Ошибка логирования: {ex.Message}");
        //    }
        //}
    }
}
