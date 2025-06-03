using DogWalk_Application.Contracts.DTOs.Auth;

namespace Api_FunctionalTests.Users;

internal static class UserData
{
    public static RegisterUserDto RegisterUserRquestTest = new()
    {
        Dni = "48671157J",        
        Nombre = "Albert",
        Apellido = "Alarcón",
        Direccion = "Calle Test 123, Ciudad Test, 12345",   
        Email = "usuario.test@example.com",
        Password = "Test1234!",    
        ConfirmPassword = "Test1234!",  
        Telefono = "600360797"    
    };
}
