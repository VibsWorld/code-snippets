### Install Appsettings in console app

1. Install following nuget
```
      Microsoft.Extensions.Configuration
      Microsoft.Extensions.Configuration.Binder
      Microsoft.Extensions.Configuration.Json
```
2. Load configuration as shown below
```csharp
 var builder = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var config = builder.Build();
AppSettings appSettings = config.Get<AppSettings>();

public class AppSettings
{
    public CustomModel CustomModel { get; set; }
}
```
uses following JSON
```json
{
  "CustomModel": {
    "ConnectionSettings": {
      "HostName": "localhost",
      "UserName": "guest",
      "Password": "guest",
      "Port": 5672,
      "VirtualHost": "/"
    }
  }
}
```
  
