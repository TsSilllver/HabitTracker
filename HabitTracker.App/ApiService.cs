//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Text.Json;
//using HabitTracker.Shared.Models;

//namespace HabitTracker.App;

//public class ApiService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _baseUrl = "https://localhost:60381";

//    public ApiService()
//    {

//        if (string.IsNullOrEmpty(_baseUrl))
//            throw new InvalidOperationException("Base URL is not configured.");

//        var handler = new HttpClientHandler();
//        // Только для разработки — игнорируем ошибки SSL
//        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

//        _httpClient = new HttpClient(handler);
//        _httpClient.BaseAddress = new Uri(_baseUrl);
//    }

//    public ApiService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//        if (_httpClient.BaseAddress == null)
//            _httpClient.BaseAddress = new Uri(_baseUrl);
//    }

//    //         Habits 
//    public async Task<List<Habit>?> GetHabitsAsync() =>
//        await _httpClient.GetFromJsonAsync<List<Habit>>("api/habits");

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HabitTracker.Shared.Models;

namespace HabitTracker.App;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:60381"; // при необходимости измените

    // Конструктор для реального использования (без параметров)
    public ApiService() : this(new HttpClient()) 
    {
        if (string.IsNullOrEmpty(_baseUrl))
            throw new InvalidOperationException("Base URL is not configured.");

        var handler = new HttpClientHandler();
        // Только для разработки — игнорируем ошибки SSL
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

        _httpClient = new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    // Конструктор для тестов (позволяет передать настроенный HttpClient)
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (_httpClient.BaseAddress == null)
            _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<Habit>?> GetHabitsAsync() =>
        await _httpClient.GetFromJsonAsync<List<Habit>>("api/habits");

    public async Task<Habit?> GetHabitAsync(int id) =>
        await _httpClient.GetFromJsonAsync<Habit>($"api/habits/{id}");

    public async Task<Habit?> PostHabitAsync(Habit habit)
    {
        var response = await _httpClient.PostAsJsonAsync("api/habits", habit);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Habit>();
    }

    public async Task<bool> PutHabitAsync(int id, Habit habit)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/habits/{id}", habit);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteHabitAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/habits/{id}");
        return response.IsSuccessStatusCode;
    }

    //            HabitRecords
    public async Task<List<HabitRecord>?> GetHabitRecordsAsync() =>
        await _httpClient.GetFromJsonAsync<List<HabitRecord>>("api/habitrecords");

    public async Task<HabitRecord?> GetHabitRecordAsync(int id) =>
        await _httpClient.GetFromJsonAsync<HabitRecord>($"api/habitrecords/{id}");

    public async Task<HabitRecord?> PostHabitRecordAsync(HabitRecord record)
    {
        var response = await _httpClient.PostAsJsonAsync("api/habitrecords", record);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HabitRecord>();
    }

    public async Task<bool> PutHabitRecordAsync(int id, HabitRecord record)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/habitrecords/{id}", record);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteHabitRecordAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/habitrecords/{id}");
        return response.IsSuccessStatusCode;
    }

    //            Schedules  
    public async Task<List<Schedule>?> GetSchedulesAsync() =>
        await _httpClient.GetFromJsonAsync<List<Schedule>>("api/schedules");

    public async Task<Schedule?> GetScheduleAsync(int id) =>
        await _httpClient.GetFromJsonAsync<Schedule>($"api/schedules/{id}");

    public async Task<Schedule?> PostScheduleAsync(Schedule schedule)
    {
        var response = await _httpClient.PostAsJsonAsync("api/schedules", schedule);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Schedule>();
    }

    public async Task<bool> PutScheduleAsync(int id, Schedule schedule)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/schedules/{id}", schedule);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteScheduleAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/schedules/{id}");
        return response.IsSuccessStatusCode;
    }
}