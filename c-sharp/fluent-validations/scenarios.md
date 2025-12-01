* Fluent Validator example either one of property (A or B) is not null
* Do validation only when one of the object is not null
```csharp
public class MyObject
{
    public string PropertyA { get; set; }
    public string PropertyB { get; set; }
    public string PropertyC { get; set; }
    public TestObject TestObject {get; set;}
}

public class TestObject
{
    public string EmailId {get; set;}
    public PropertyD {get; set;}
}

public class MyObjectValidator : AbstractValidator<MyObject>
{
    public MyObjectValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PropertyA != null || x.PropertyB != null)
            .WithMessage("Either PropertyA or PropertyB must not be null.");
        RuleFor(x => x).
            NotNull()
            .NotEmpty();


        //Custom Rule Validator for the inherited object
        When(x => x.TestObject is not null, () =>
        {
            RuleFor(x => x.TestObject).SetValidator(new TestObjectValidator());
        }
    }
}

public class TestObjectValidator : AbstractValidator<TestObject>
{
    public TestObjectValidator()
    {
        RuleFor(x => x.EmailId)
            .NotEmpty()
            .WithMessage("Email Id is required")
            .EmailAddress()
            .WithMessage("Email ID Id must be a valid email address");
    }
}
```
* Two objects cannot have same boolean value
  ```csharp
  public class Test{
    public bool Value1 {get;set;}
    public bool Value2 {get;set;}
  }
 public class TestValidator : AbstractValidator<Test>
 {
    public TestValidator()
    {
        RuleFor(x => x.Value1)
            .Must(model, validatedValue) => model.Value1 != model.Value2).WithMessage("Value cannot be same for both properties);
    }
 }
  ```


