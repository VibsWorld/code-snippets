### Datetime Parsing with specific format example
```csharp
CultureInfo provider = CultureInfo.InvariantCulture;

if (DateTime.TryParseExact(dateValues[0], "MM/dd/yyyy", provider, DateTimeStyles.None, out DateTime startDate) && DateTime.TryParseExact(dateValues[1], "MM/dd/yyyy", provider, DateTimeStyles.None, out DateTime endDate))
{
  //Your code
}
```
