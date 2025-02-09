using System.Linq.Expressions;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Dsl;
using Btms.Common.Extensions;

namespace TestDataGenerator;

public interface IBaseBuilder
{
    // public DateTime? Created { get; }
    // public string? Id { get; }

    // public T ValidateAndBuild();
}

public abstract class BuilderBase<T, TBuilder> :
    IBaseBuilder
    where TBuilder : BuilderBase<T, TBuilder> ///where T : new()

{
    private IPostprocessComposer<T> _composer = null!;
    protected Fixture Fixture { get; set; } = null!;

    protected BuilderBase(string? filePath = null,  string? itemJson = null, JsonSerializerOptions? options = null, T? item = default(T)) : base()
    {
        Fixture = new Fixture();

        if (filePath.HasValue())
        {
            var json = File.ReadAllText(filePath);
            item = JsonSerializer.Deserialize<T>(json, options)!;
        }
        else if (itemJson.HasValue())
        {
            item = JsonSerializer.Deserialize<T>(itemJson, options)!;
        }
        else
        {
            item = Fixture.Create<T>();
        }

        _composer = Fixture.Build<T>()
            .FromFactory(() => item!)
            .OmitAutoProperties();
    }

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

    public T Build()
    {
        return _composer.Create();
    }

    public T ValidateAndBuild()
    {
        return Validate().Build();
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
    protected abstract TBuilder Validate();

}
