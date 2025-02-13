namespace Btms.Business.Services.Linking;

public record LinkerResult<TFrom, TTo>(IReadOnlyList<TFrom> From, TTo To) 
    where TFrom : class 
    where TTo : class;