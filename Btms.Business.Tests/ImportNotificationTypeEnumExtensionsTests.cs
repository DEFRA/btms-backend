using Btms.Business.Extensions;
using Btms.Model;
using Btms.Model.Ipaffs;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests;

public class ImportNotificationTypeEnumExtensionsTests
{
    [Theory]
    [InlineData("9115", ImportNotificationTypeEnum.Chedpp)]
    [InlineData("C633", ImportNotificationTypeEnum.Chedpp)]
    [InlineData("N002", ImportNotificationTypeEnum.Chedpp)]
    [InlineData("N851", ImportNotificationTypeEnum.Chedpp)]
    [InlineData("C085", ImportNotificationTypeEnum.Chedpp)]

    [InlineData("N852", ImportNotificationTypeEnum.Ced)]
    [InlineData("C678", ImportNotificationTypeEnum.Ced)]

    [InlineData("C640", ImportNotificationTypeEnum.Cveda)]

    [InlineData("C641", ImportNotificationTypeEnum.Cvedp)]
    [InlineData("C673", ImportNotificationTypeEnum.Cvedp)]
    [InlineData("N853", ImportNotificationTypeEnum.Cvedp)]

    [InlineData("9HCG", null)]
    public void DocumentCode_ToChedType(string documentCode, ImportNotificationTypeEnum? expected)
    {
        var result = documentCode.GetChedType();

        result.Should().Be(expected);
    }
}