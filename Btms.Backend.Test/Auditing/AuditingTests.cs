using Btms.Model.Auditing;
using FluentAssertions;

namespace Btms.Backend.Test.Auditing;

public class AuditingTests
{
    public class TestClassOne
    {
        public double NumberValue { get; set; }
    }

    [Fact]
    public void CreateAuditWhenDifferentIsDouble()
    {
        var previous = new TestClassOne { NumberValue = 1.2 };
        var current = new TestClassOne { NumberValue = 2.2 };
        var auditEntry = AuditEntry.CreateUpdated(previous, current, "testid", 1, DateTime.UtcNow);

        auditEntry.Should().NotBeNull();
        auditEntry.Diff.Count.Should().Be(1);
        auditEntry.Diff[0].Value.Should().Be(2.2);
        auditEntry.Diff[0].Op.Should().Be("Replace");
        auditEntry.Diff[0].Path.Should().Be("/NumberValue");

    }


    [Fact]
    public void CreateAuditWhenDifferentIsInt()
    {
        var previous = new TestClassOne { NumberValue = 1 };
        var current = new TestClassOne { NumberValue = 2 };
        var auditEntry = AuditEntry.CreateUpdated(previous, current, "testid", 1, DateTime.UtcNow);

        auditEntry.Should().NotBeNull();
        auditEntry.Diff.Count.Should().Be(1);
        auditEntry.Diff[0].Value.Should().Be(2);
        auditEntry.Diff[0].Op.Should().Be("Replace");
        auditEntry.Diff[0].Path.Should().Be("/NumberValue");

    }
}