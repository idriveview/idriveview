using IDriveView.CreateClient;
using IDriveView.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace IDriveView.HelpCode
{
    class AddRegistrationWorkJson
    {
        // Путь к JSON файлу
        private static readonly string filePath = Settings.pathConfigRegistr;

        // --- Метод для добавления нового пользователя в список List ------------------------------------------------
        public static async Task<(bool Success, string Message)> AddUser(UserIDrive user)
        {
            try
            {
                // Проверка входного параметра
                if (user == null)
                {
                    return (false, "Пользователь не может быть null");
                }

                // Загрузка существующих пользователей
                var loadResult = await GetListUsersAsync();
                if (!loadResult.Success)
                {
                    return (false, $"Не удалось загрузить пользователей: {loadResult.Message}");
                }
                List<UserIDrive> users = loadResult.userIDrives;

                // Проверка наличия пользователя с таким же именем
                if (users.Any(u => u.Name == user.Name))
                {
                    return (false, "userAlreadyExists");
                }

                // Добавление нового пользователя
                users.Add(user);

                // Сохранение списка
                var saveResult = await SaveUsers(users);
                if (!saveResult.Success)
                {
                    return (false, $"Не удалось сохранить пользователей: {saveResult.Message}");
                }

                return (true, "Пользователь успешно добавлен");
            }
            catch (Exception ex)
            {
                return (false, $"Не удалось добавить пользователя в список List: {ex.Message}");
            }
        }

        // --- Метод для удаления пользователя по Name ----------------------------------------------------
        public static async Task<(bool Success, string Message)> RemoveUser(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return (false, "Имя пользователя не может быть пустым");
                }

                var loadResult = await GetListUsersAsync();
                if (!loadResult.Success)
                {
                    return (false, $"Не удалось загрузить список пользователей: {loadResult.Message}");
                }
                List<UserIDrive> users = loadResult.userIDrives;

                int removedCount = users.RemoveAll(u => u.Name == name);
                if (removedCount == 0)
                {
                    return (false, $"Пользователь с именем '{name}' не найден");
                }

                var saveResult = await SaveUsers(users);
                if (!saveResult.Success)
                {
                    return (false, $"Не удалось сохранить изменения: {saveResult.Message}");
                }

                return (true, $"Пользователь '{name}' успешно удалён");
            }
            catch (Exception ex)
            {
                return (false, $"Не удалось удалить пользователя. Неизвестная ошибка: {ex.Message}");
            }
        }

        // --- Получение списка пользователей из файла --------------------------------------------------------
        public static async Task<(bool Success, string Message, List<UserIDrive> userIDrives)> GetListUsersAsync()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    // Проверка и создание директории, если она не существует
                    string directory = System.IO.Path.GetDirectoryName(filePath);

                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    // Создаем файл
                    using (File.Create(filePath)) { }
                }
                if (!File.Exists(filePath))
                {
                    return (false, "Не получилось создать файл для записи пользователей", new List<UserIDrive>());
                }

                // Чтение файла асинхронно
                string json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrEmpty(json))
                {
                    return (true, "В файле нет записей", new List<UserIDrive>());
                }

                // Десериализация в асинхронном потоке
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
                var users = await JsonSerializer.DeserializeAsync<List<UserIDrive>>(stream);

                return (true, "Получение данных прошло успешно", users ?? new List<UserIDrive>());
            }
            catch (FileNotFoundException)
            {
                return (false, "Файл не найден", new List<UserIDrive>());
            }
            catch (JsonException ex)
            {
                return (false, $"Некорректный JSON: {ex.Message}", new List<UserIDrive>());
            }
            catch (IOException ex)
            {
                return (false, $"Ошибка чтения файла: {ex.Message}", new List<UserIDrive>());
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, $"Нет прав доступа к файлу или директории: {ex.Message}", new List<UserIDrive>());
            }
            catch (Exception ex)
            {
                return (false, $"Неизвестная ошибка при получении пользователя из файла: {ex.Message}", new List<UserIDrive>());
            }
        }

        // --- Сохранение списка пользователей в файл -------------------------------------------------
        public static async Task<(bool Success, string Message)> SaveUsers(List<UserIDrive> users)
        {
            try
            {
                // Проверка входных данных
                if (users == null)
                {
                    return (false, "Список пользователей не может быть null");
                }

                // Сериализация и асинхронная запись
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(users, options);
                await File.WriteAllTextAsync(filePath, json);

                return (true, "Список пользователей успешно сохранён в файл");
            }
            catch (JsonException ex)
            {
                return (false, $"Ошибка сериализации данных: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, $"Нет прав доступа к файлу или директории: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return (false, $"Директория не найдена: {ex.Message}");
            }
            catch (IOException ex)
            {
                return (false, $"Ошибка ввода-вывода при сохранении файла: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Неизвестная ошибка: {ex.Message}");
            }
        }

        // --- Изменить поле: RemaberMe у пользователя ------------------------------------------------
        public static async Task<(bool Success, string Message)> ChengeRemaberMe(UserIDrive user, bool? value)
        {
            // Загрузка существующих пользователей
            var loadResult = await GetListUsersAsync();
            if (!loadResult.Success)
            {
                return (false, $"Не удалось загрузить пользователей: {loadResult.Message}");
            }
            List<UserIDrive> users = loadResult.userIDrives;

            // Получение пользователя
            var userChenge = users.FirstOrDefault(u => u.Name == user.Name);

            var Key = Properties.Settings.Default.KeyCrypt;

            string rememberMe = value == true ? "False" : "True";

            userChenge.RememberMe = await ConfigurationAES.EncryptAcync(rememberMe, Key);

            // Сохранение списка
            var saveResult = await SaveUsers(users);
            if (!saveResult.Success)
            {
                return (false, $"Не удалось изменить данные: {saveResult.Message}");
            }

            return (true, "Данные успешно изменены");
        }
    }
}
