namespace TestDataGenerator;


public static class Lens
{
    public static TV Get<TX, TV>(this Lens<TX, TV> lens, TX item)
    {
        return lens.Getter(item);
    }

    public static Lens<TX, TV> Compose<TX, TU, TV>(
        this Lens<TX, TU> lens1,
        Lens<TU, TV> lens2)
    {
        return new Lens<TX, TV>(
            x => lens2.Get(lens1.Get(x)));
    }
}
    
public class Lens<TX, TV>(Func<TX, TV> getter) //, Func<TV, TX, TX> setter
{
    internal Func<TX, TV> Getter { get; } = getter;

    // internal Func<TV, TX, TX> Setter { get; } = setter;
}