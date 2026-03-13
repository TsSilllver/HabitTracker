using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using HabitTracker.Shared.Models;

namespace HabitTracker.App
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        // Используем HTTPS, но если сервер слушает HTTP, измените на http://localhost:60380
        private readonly string _baseUrl = "https://localhost:60381";

        public ApiService() : this(CreateHttpClient()) { }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            // Игнорируем ошибки SSL для локального сертификата (только для разработки)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30); // Таймаут 30 секунд
            System.Diagnostics.Debug.WriteLine("HttpClient created with timeout 30s");
            return client;
        }

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.BaseAddress == null)
                _httpClient.BaseAddress = new Uri(_baseUrl);
            System.Diagnostics.Debug.WriteLine($"ApiService base address: {_httpClient.BaseAddress}");
        }

        public string GetBaseUrl() => _httpClient.BaseAddress?.ToString() ?? "не задан";

        // ----- Habits -----
        public async Task<List<Habit>?> GetHabitsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Habit>>("api/habits");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetHabitsAsync error: {ex}");
                return null;
            }
        }

        public async Task<Habit?> GetHabitAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Habit>($"api/habits/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Habit?> PostHabitAsync(Habit habit)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/habits", habit);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Habit>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PutHabitAsync(int id, Habit habit)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/habits/{id}", habit);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteHabitAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/habits/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ----- HabitRecords -----
        public async Task<List<HabitRecord>?> GetHabitRecordsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<HabitRecord>>("api/habitrecords");
            }
            catch
            {
                return null;
            }
        }

        public async Task<HabitRecord?> GetHabitRecordAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HabitRecord>($"api/habitrecords/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<HabitRecord?> PostHabitRecordAsync(HabitRecord record)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/habitrecords", record);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<HabitRecord>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PutHabitRecordAsync(int id, HabitRecord record)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/habitrecords/{id}", record);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteHabitRecordAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/habitrecords/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ----- Schedules -----
        public async Task<List<Schedule>?> GetSchedulesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Schedule>>("api/schedules");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Schedule?> GetScheduleAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Schedule>($"api/schedules/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Schedule?> PostScheduleAsync(Schedule schedule)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/schedules", schedule);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Schedule>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> PutScheduleAsync(int id, Schedule schedule)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/schedules/{id}", schedule);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/schedules/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ----- Sync methods -----
        public async Task<DataExport?> GetAllDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GET api/sync started");
                var result = await _httpClient.GetFromJsonAsync<DataExport>("api/sync");
                System.Diagnostics.Debug.WriteLine("GET api/sync completed");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GET api/sync error: {ex}");
                MessageBox.Show($"Ошибка при получении данных: {ex.Message}", "Ошибка");
                return null;
            }
        }

        public async Task<bool> SendAllDataAsync(DataExport data)
        {
            try
            {
                // Сериализуем для проверки размера
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(data, options);
                System.Diagnostics.Debug.WriteLine($"Sending JSON length: {json.Length}");
                MessageBox.Show($"Размер JSON: {json.Length} символов. Отправка...", "Отладка");

                // Выполняем POST
                var response = await _httpClient.PostAsJsonAsync("api/sync", data, options);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Данные успешно отправлены на сервер.", "Успех");
                    return true;
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {response.StatusCode}\n{content}", "Ошибка");
                    return false;
                }
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TaskCanceledException: {ex}");
                MessageBox.Show($"Превышен таймаут ожидания ответа от сервера. Возможно, сервер не отвечает.\n{ex.Message}", "Ошибка");
                return false;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HttpRequestException: {ex}");
                MessageBox.Show($"Ошибка HTTP-запроса: {ex.Message}", "Ошибка");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                MessageBox.Show($"Необработанное исключение: {ex.Message}", "Ошибка");
                return false;
            }
        }
    }
}