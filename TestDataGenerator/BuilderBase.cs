using System.Composition;
using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator;

public abstract class BuilderBase<T, TBuilder>
    where TBuilder : BuilderBase<T, TBuilder> where T : new()
{
    protected Fixture Fixture { get; private set; }

    private IPostprocessComposer<T> _composer;

    protected BuilderBase()
    {
        Setup();
    }

    protected BuilderBase(string filePath)
    {
        var json = File.ReadAllText(filePath);

        var n = JsonSerializer.Deserialize<T>(json)!;

        Setup(n);
    }

    public TBuilder With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
    {
        _composer = _composer.With(propertyPicker, value);

        return (TBuilder)this;
    }

    public TBuilder Do(Action<T> action)
    {
        _composer = _composer.Do(action);

        return (TBuilder)this;
    }

    public TBuilder With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<TProperty> valueFactory)
    {
        _composer = _composer.With(propertyPicker, valueFactory);

        return (TBuilder)this;
    }

    protected string CreateRandomString(int length) => string.Join("", Fixture.CreateMany<char>(length));

    protected string CreateRandomInt(int length) =>
        CreateRandomInt(Convert.ToInt32(Math.Pow(10, length - 1)), Convert.ToInt32(Math.Pow(10, length) - 1))
            .ToString();

    protected int CreateRandomInt(int min, int max) => new Random().Next(min, max);

    public T Build() => _composer.Create();

    private void Setup(T item = default)
    {
        Fixture = new Fixture();

        item ??= Fixture.Create<T>();

        _composer = Fixture.Build<T>()
            .FromFactory(() => item)
            .OmitAutoProperties();
    }
}