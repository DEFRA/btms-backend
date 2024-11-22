using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Dsl;

namespace TestDataGenerator;

public abstract class BuilderBase<T, TBuilder>
    where TBuilder : BuilderBase<T, TBuilder> where T : new()
{
    private IPostprocessComposer<T> _composer = null!;

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

    protected Fixture Fixture { get; private set; } = null!;

    public TBuilder With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
    {
        _composer = _composer.With(propertyPicker, value);

        return (TBuilder)this;
    }

    public TBuilder With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<TProperty> valueFactory)
    {
        _composer = _composer.With(propertyPicker, valueFactory);

        return (TBuilder)this;
    }

    public TBuilder Do(Action<T> action)
    {
        _composer = _composer.Do(action);

        return (TBuilder)this;
    }

    protected string CreateRandomString(int length)
    {
        return string.Join("", Fixture.CreateMany<char>(length));
    }

    protected static string CreateRandomInt(int length)
    {
        return CreateRandomInt(Convert.ToInt32(Math.Pow(10, length - 1)), Convert.ToInt32(Math.Pow(10, length) - 1))
            .ToString();
    }

    protected static int CreateRandomInt(int min, int max)
    {
        return Random.Shared.Next(min, max);
    }

    public T Build()
    {
        return _composer.Create();
    }

    public abstract TBuilder Validate();

    public T ValidateAndBuild()
    {
        return Validate().Build();
    }

    private void Setup(T item = default!)
    {
        Fixture = new Fixture();

        item ??= Fixture.Create<T>();

        _composer = Fixture.Build<T>()
            .FromFactory(() => item)
            .OmitAutoProperties();
    }
}