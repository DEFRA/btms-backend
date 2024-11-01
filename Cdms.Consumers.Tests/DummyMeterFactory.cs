using System.Diagnostics.Metrics;

namespace Cdms.Consumers.Tests;

internal sealed class DummyMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options) => new Meter(options);

    public void Dispose()
    {
    }
}

// Based on https://gist.github.com/asimmon/612b2d54f1a0d2b4e1115590d456e0be.