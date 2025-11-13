* Fluent Validator either one of property is not null
```csharp
    public class MyObject
{
    public string PropertyA { get; set; }
    public string PropertyB { get; set; }
}

public class MyObjectValidator : AbstractValidator<MyObject>
{
    public MyObjectValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PropertyA != null || x.PropertyB != null)
            .WithMessage("Either PropertyA or PropertyB must not be null.");
    }
}
``` 
