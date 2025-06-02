using DogWalk_Domain.Entities;
using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain_UnitTests.Users;

internal class UserMock
{
    public static readonly string Nombre = "Alfonso";
    public static readonly string Apellido = "Gomez";
    public static readonly Email Email = Email.Create("alfonso@gmail.com");
    public static readonly Password Password = Password.Create("Test1234");

}
