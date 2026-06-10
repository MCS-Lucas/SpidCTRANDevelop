using System;using Microsoft.AspNetCore.Identity;var h = new PasswordHasher<object>();Console.WriteLine(h.HashPassword(new object(), "visual"));
