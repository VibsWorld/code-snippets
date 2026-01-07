## Performing Deserialization using JSON Nodes (`System.Text.Json`)

### Working options
```csharp
 private static readonly JsonSerializerOptions optionsJsonSerializer =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
```
### Simple JsonNode Example
```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

string jsonString = """
    {
      "Id": 101,
      "Name": "Widget",
      "Price": 19.99
    }
    """;

// 1. Parse the JSON string into a JsonNode (which will be a JsonObject in this case)
JsonNode rootNode = JsonNode.Parse(jsonString);

// 2. Use the Deserialize extension method to convert the JsonNode to your model
Product product = rootNode.Deserialize<Product>();

// You can also pass in JsonSerializerOptions if needed
// var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
// Product productWithOptions = rootNode.Deserialize<Product>(options);

// 3. Use the deserialized object
Console.WriteLine($"Product Name: {product.Name}, Price: {product.Price}");
```

## Common Deserialization errors and their handling in options
* Case Mismatch in Property Names
use
`var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);`
OR
`var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
OR
```public class User
{
    [JsonPropertyName("id")] // Matches "id" in JSON
    public int Id { get; set; }
}
```
* Handling Numbers
```
var options = new JsonSerializerOptions {
    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
};
```
* Handling Enums
```
var options = new JsonSerializerOptions {
    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
};
```
* Invalid JSON Payload (Trailing Commas, Comments) 
```
var options = new JsonSerializerOptions {
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};
```
* Pick Property from JsonPath
```
    var createIncidentResponseJsonNode = JsonNode.Parse(createIncidentResponseStream);
        var incidentId = createIncidentResponseJsonNode["incidentId"].ToString();
        incidentId.Should().NotBeNullOrEmpty();
```
* Pick Array from Json Path
```
 var incidentDetailsStream =
            await incidentDetailsWithCommentForUserTypeAsCustomer.ResponseMessage.Content.ReadAsStreamAsync();
        var incidentDetailsJsonNode = JsonNode.Parse(incidentDetailsStream);
        var comments = incidentDetailsJsonNode["comments"];
        comments.Should().NotBeNull();
        comments.AsArray().Count.Should().Be(1);
        ShipmentIncidentCommentView[] shipmentIncidentCommentView =
            comments.Deserialize<ShipmentIncidentCommentView[]>(optionsJsonSerializer);
        Assert.True(shipmentIncidentCommentView.All(c => c.IsPublic));
```
