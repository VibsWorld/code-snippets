public static List<string> GetListOfErrorMessagesFromModelState(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
{
    if (modelState is null || modelState.Count == 0 || modelState.IsValid) return new List<string>();

    List<string> errors = new List<string>();

    foreach(var item in modelState.Values)
    {
        foreach(var error in item.Errors)
        {
            errors.Add(error.ErrorMessage);
        }  
    }

    return errors;

    /* 
      //Short Form Notation
       return ModelState.Values.SelectMany(m => m.Errors).Select(x => x.ErrorMessage).ToList()
    */
}
