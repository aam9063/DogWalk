

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api_FunctionalTests.Infrastructure;
using DogWalk_Application.Contracts.DTOs.Auth;
using Xunit;

namespace Api_FunctionalTests.Users;

public class GetUserSessionTest : BaseFunctionalTest
{
    public GetUserSessionTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserSession_ReturnsUserSession()
    {
        await HttpClient.PostAsJsonAsync("/api/Usuario/register", UserData.RegisterUserRquestTest);

        var loginResponse = await HttpClient.PostAsJsonAsync("/api/Auth/login", new
        {
            Email = UserData.RegisterUserRquestTest.Email,
            Password = UserData.RegisterUserRquestTest.Password
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        // 3. Configurar el token para la siguiente llamada
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult?.Token);

        // Act
        var response = await HttpClient.GetAsync("/api/Usuario/profile");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var userSession = await response.Content.ReadFromJsonAsync<UserSessionDto>();
        Assert.NotNull(userSession);
        Assert.Equal(UserData.RegisterUserRquestTest.Email, userSession.Email);
    }

}
