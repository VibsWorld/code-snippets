## Authentication and Authorization

**Generate Token (string) from `Claims`**
```csharp
 private string GenerateTokenForInternalUser()
 {
     var claims = new[]
     {
         new Claim("customer_id", CustomerId),
         new Claim("name", CustomerName),
         new Claim("sub", CustomerEmail),
         new Claim("user_relationship", "internal"),
         new Claim("username", CustomerEmail)
     };

     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

     var token = new JwtSecurityToken(
         issuer: nameof(Docker),
         audience: nameof(Docker),
         claims: claims,
         expires: DateTime.UtcNow.AddMinutes(1),
         signingCredentials: creds
     );

     return new JwtSecurityTokenHandler().WriteToken(token);
 }
```
