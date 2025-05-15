using IDriveView.Debuging;
using System.Diagnostics;
using System.Net.Http;

namespace IDriveView.HelpCode
{
    internal class InternetAvailability
    {
        // --- Проверка интернета через продключение к сайтам в трёх общедоступных регионах -----------------------------------
        public static async Task<(bool Success, string Message)> CheckInternet(bool checkInternetSpecific = false, string Endpoint="")
        {
            string[] urls = { "https://www.yahoo.com", "https://ya.ru", "https://www.google.com" };
            
            // Если хоть один из адресов работает, то проверяем конкретное облако (если нужно)
            foreach (string url in urls)
            {
                try
                {
                    var result2 = await CheckWebsiteAvailabilityAsync(url, 7);
                    Output.WriteLine(url + " " + result2.ToString());
                    if (result2)
                    {
                        if (checkInternetSpecific)
                        {
                            var result = await CheckInternetSpecific(Endpoint);
                            if (result.Success)
                            {
                                return (true, result.Message);
                            }
                            else
                            {
                                return (false, result.Message);
                            }
                        }
                        else return (true, "Интернет доступен");
                    }
                }
                catch (Exception ex)
                {
                    await Logger.Log($"Ошибка при проверке {url}: {ex.Message}");
                }
            }

            return (false, "Интернет недоступен. Проверьте соединение.");
        }

        // --- Проверка соединения с облаком ----------------------------------------------------------------------------------
        public static async Task<(bool Success, string Message)> CheckInternetSpecific(string Endpoint)
        {
            string? Url = Endpoint switch
            {
                string e when e.Contains("idrivee2") => "https://app.idrivee2.com/signin",
                string e when e.Contains("selcloud") => "https://my.selectel.ru",
                string e when e.Contains("yandex") => "https://yandex.cloud",
                _ => null
            };
            if(Url == null)
            {
                await Logger.Log($"Неопознанное облако. Нет проверки доступа к сайту.");
                return (true, "Неопознанное облако. Нет проверки доступа к сайту.");
            }
            // Проверяем idrive.com с тайм-аутом 10 секунд
            var result = await CheckWebsiteAvailabilityAsync(Url, 10);
            try
            {
                if (result)
                {
                    return (true, "Интернет доступен");
                }
            }
            catch (Exception ex)
            {
                await Logger.Log($"Ошибка при проверке соединения с облаком: {ex.Message}");
            }
            return (false, "Ошибка при проверке соединения с облаком. См. логи.");
        }

        // --- Проверка соединения с конкретным сайтом ------------------------------------------------------------------------
        public static async Task<bool> CheckWebsiteAvailabilityAsync(string url, int timeOut)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeOut);
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (TaskCanceledException)
            {
                await Logger.Log($"Тайм-аут при проверке {url}");
                return false;
            }
            catch (HttpRequestException ex)
            {
                await Logger.Log($"Сетевая ошибка при проверке {url}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                await Logger.Log($"Неизвестная ошибка при проверке {url}: {ex.Message}");
                return false;
            }
        }
    }
}
