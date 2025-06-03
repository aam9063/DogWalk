using System.Net.Http.Json;
using Api_FunctionalTests.Users;
using DogWalk_Application.Contracts.DTOs.Auth;
using Xunit;

namespace Api_FunctionalTests.Infrastructure;

public class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;

    public BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
    }

    protected async Task<string> GetToken()
    {
        var response = await HttpClient.PostAsJsonAsync("api/Auth/login", new LoginDto {
            Email = UserData.RegisterUserRquestTest.Email,
            Password = UserData.RegisterUserRquestTest.Password
        });

        return await response.Content.ReadAsStringAsync();
    }
}
