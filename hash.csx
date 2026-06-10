using Microsoft.AspNetCore.Identity;
var hasher = new PasswordHasher<object>();
var hash = hasher.HashPassword(new object(), "visual");
Console.WriteLine(hash);
