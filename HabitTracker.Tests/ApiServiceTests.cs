using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HabitTracker.App;
using HabitTracker.Shared.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace HabitTracker.Tests;

public class ApiServiceTests
{
    [Fact]
    public async Task GetHabitsAsync_ReturnsHabits_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedHabits = new List<Habit>
        {
            new Habit { Id = 1, Name = "Test Habit", TargetValue = 10, Unit = "times" }
        };

        var json = JsonSerializer.Serialize(expectedHabits);

        // Создаём мок для HttpMessageHandler
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });

        // Создаем HttpClient с мок-обработчиком и базовым адресом
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:60381/")
        };

        // Используем конструктор ApiService, принимающий HttpClient
        var apiService = new ApiService(httpClient);

        // Act
        var result = await apiService.GetHabitsAsync();

        // Assert: проверяем, что SendAsync был вызван один раз
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        // Проверяем результат
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result[0].Name.Should().Be("Test Habit");
    }
}