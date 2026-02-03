
Collects all occurrences of a given property name, such as **errorMessage**, anywhere in the JSON, using `System.Text.Json.Nodes.JsonNode`. 
The function returns a List<string?> with all values found for that key 

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

class Program
{
    static void Main()
    {
        string json = @"[
            {
                ""duplicateResult"": {
                    ""allowSave"": false,
                    ""duplicateRule"": ""Contact_Duplicate_Rule"",
                    ""duplicateRuleEntityType"": ""Contact"",
                    ""errorMessage"": ""Use one of these records?"",
                    ""matchResults"": [
                        {
                            ""entityType"": ""Contact"",
                            ""errors"": [],
                            ""matchEngine"": ""ExactMatchEngine"",
                            ""matchRecords"": [
                                {
                                    ""additionalInformation"": [],
                                    ""fieldDiffs"": [],
                                    ""matchConfidence"": 100,
                                    ""record"": {
                                        ""attributes"": {
                                            ""type"": ""Contact"",
                                            ""url"": ""/services/data/v62.0/sobjects/Contact/003S800000jyTwTIAU""
                                        },
                                        ""Id"": ""003S800000jyTwTIAU""
                                    }
                                }
                            ],
                            ""rule"": ""Contact_Duplicate_Matching_Rule"",
                            ""size"": 1,
                            ""success"": true
                        }
                    ]
                },
                ""errorCode"": ""DUPLICATES_DETECTED"",
                ""message"": ""Use one of these records?""
            }
        ]";

        JsonNode? root = JsonNode.Parse(json);

        var allErrorMessages = FindAllPropertyValues(root, "errorMessage");

        if (allErrorMessages.Count == 0)
        {
            Console.WriteLine("No errorMessage properties found.");
        }
        else
        {
            foreach (var value in allErrorMessages)
            {
                Console.WriteLine($"errorMessage: {value}");
            }
        }
    }

    static List<string?> FindAllPropertyValues(JsonNode? node, string propertyName)
    {
        var results = new List<string?>();
        FindAllPropertyValuesRecursive(node, propertyName, results);
        return results;
    }

    static void FindAllPropertyValuesRecursive(JsonNode? node, string propertyName, List<string?> results)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key == propertyName)
                {
                    results.Add(kvp.Value?.ToString());
                }
                FindAllPropertyValuesRecursive(kvp.Value, propertyName, results);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                FindAllPropertyValuesRecursive(item, propertyName, results);
            }
        }
    }
}
```
How to use
* Replace the sample JSON with your own as needed.
* Call `FindAllPropertyValues(root, "errorMessage")` for any key you want to collect.
* The results list contains all values of all occurrences of that property, regardless of where in the structure.
This is a very flexible and robust approach for "find all values by property name" in nested JSON
