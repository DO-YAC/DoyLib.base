using AutoFixture;
using AutoFixture.AutoMoq;
using doylib.Services;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace doylib.tests.Unit.Services;

[TestClass]
public class CandleWindowServiceTests
{
    private IFixture mFixture = null!;
    private Mock<IDoyExceptionHandler> mExceptionHandlerMock = null!;
    private CandleWindowService mSut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mFixture = new Fixture().Customize(new AutoMoqCustomization());
        mExceptionHandlerMock = mFixture.Freeze<Mock<IDoyExceptionHandler>>();
        mSut = new CandleWindowService(NullLogger<CandleWindowService>.Instance, mExceptionHandlerMock.Object);
    }

    [TestMethod]
    public void Initialize_WithZeroMaxSize_RoutesToExceptionHandler()
    {
        // Act
        mSut.Initialize(0);

        // Assert
        mExceptionHandlerMock.Verify(
            e => e.HandleException(It.IsAny<DoyVestmentException>(), It.IsAny<ILogger>()),
            Times.Once);
    }

    [TestMethod]
    public void Initialize_WithNegativeMaxSize_RoutesToExceptionHandler()
    {
        // Act
        mSut.Initialize(-5);

        // Assert
        mExceptionHandlerMock.Verify(
            e => e.HandleException(It.IsAny<DoyVestmentException>(), It.IsAny<ILogger>()),
            Times.Once);
    }

    [TestMethod]
    public void Initialize_WithPositiveMaxSize_DoesNotInvokeExceptionHandler()
    {
        // Act
        mSut.Initialize(5);

        // Assert
        mExceptionHandlerMock.Verify(
            e => e.HandleException(It.IsAny<DoyVestmentException>(), It.IsAny<ILogger>()),
            Times.Never);
    }

    [TestMethod]
    public void Window_BeforeAddingAny_IsEmpty()
    {
        // Arrange
        mSut.Initialize(5);

        // Act
        var window = mSut.Window;

        // Assert
        Assert.AreEqual(0, window.Length);
    }

    [TestMethod]
    public void AddCandle_Single_WhenWindowNotFull_AppendsToEnd()
    {
        // Arrange
        mSut.Initialize(3);
        var c1 = MakeCandle(new DateTime(2024, 1, 1, 0, 0, 0));
        var c2 = MakeCandle(new DateTime(2024, 1, 1, 0, 1, 0));

        // Act
        mSut.AddCandle(c1);
        mSut.AddCandle(c2);

        // Assert
        var window = mSut.Window.ToArray();
        Assert.AreEqual(2, window.Length);
        Assert.AreSame(c1, window[0]);
        Assert.AreSame(c2, window[1]);
    }

    [TestMethod]
    public void AddCandle_Single_FillsWindowExactly()
    {
        // Arrange
        mSut.Initialize(3);
        var candles = new[]
        {
            MakeCandle(new DateTime(2024, 1, 1, 0, 0, 0)),
            MakeCandle(new DateTime(2024, 1, 1, 0, 1, 0)),
            MakeCandle(new DateTime(2024, 1, 1, 0, 2, 0)),
        };

        // Act
        foreach (var c in candles) mSut.AddCandle(c);

        // Assert
        var window = mSut.Window.ToArray();
        Assert.AreEqual(3, window.Length);
        CollectionAssert.AreEqual(candles, window);
    }

    [TestMethod]
    public void AddCandle_Single_WhenWindowFull_DropsOldestAndAppendsLatest()
    {
        // Arrange
        mSut.Initialize(3);
        var c1 = MakeCandle(new DateTime(2024, 1, 1, 0, 0, 0));
        var c2 = MakeCandle(new DateTime(2024, 1, 1, 0, 1, 0));
        var c3 = MakeCandle(new DateTime(2024, 1, 1, 0, 2, 0));
        var c4 = MakeCandle(new DateTime(2024, 1, 1, 0, 3, 0));
        mSut.AddCandle(c1);
        mSut.AddCandle(c2);
        mSut.AddCandle(c3);

        // Act
        mSut.AddCandle(c4);

        // Assert
        var window = mSut.Window.ToArray();
        Assert.AreEqual(3, window.Length);
        Assert.AreSame(c2, window[0]);
        Assert.AreSame(c3, window[1]);
        Assert.AreSame(c4, window[2]);
    }

    [TestMethod]
    public void AddCandle_Single_WhenTimestampMatchesLatest_ReplacesInPlace()
    {
        // Arrange
        mSut.Initialize(3);
        var ts = new DateTime(2024, 1, 1, 0, 1, 0);
        var first = MakeCandle(ts, close: 100);
        var update = MakeCandle(ts, close: 105);
        mSut.AddCandle(MakeCandle(new DateTime(2024, 1, 1, 0, 0, 0)));
        mSut.AddCandle(first);

        // Act
        mSut.AddCandle(update);

        // Assert
        var window = mSut.Window.ToArray();
        Assert.AreEqual(2, window.Length);
        Assert.AreSame(update, window[1]);
        Assert.AreEqual(105, window[1].Close);
    }

    [TestMethod]
    public void AddCandle_Array_AddsAllCandlesInOrder()
    {
        // Arrange
        mSut.Initialize(5);
        var candles = new[]
        {
            MakeCandle(new DateTime(2024, 1, 1, 0, 0, 0)),
            MakeCandle(new DateTime(2024, 1, 1, 0, 1, 0)),
            MakeCandle(new DateTime(2024, 1, 1, 0, 2, 0)),
        };

        // Act
        mSut.AddCandle(candles);

        // Assert
        CollectionAssert.AreEqual(candles, mSut.Window.ToArray());
    }

    [TestMethod]
    public void AddCandle_FromMultipleThreads_FinalCountStaysWithinMaxSize()
    {
        // Arrange
        const int maxSize = 50;
        mSut.Initialize(maxSize);
        var candles = Enumerable.Range(0, 500)
            .Select(i => MakeCandle(new DateTime(2024, 1, 1).AddSeconds(i)))
            .ToArray();

        // Act
        Parallel.ForEach(candles, c => mSut.AddCandle(c));

        // Assert
        Assert.AreEqual(maxSize, mSut.Window.Length);
    }

    #region Helpers

    private static Candle MakeCandle(
        DateTime timestamp,
        string symbol = "EURUSD",
        string timeframe = "M1",
        double open = 1.0,
        double high = 1.1,
        double low = 0.9,
        double close = 1.05,
        long volume = 100)
    {
        return new Candle(timestamp, symbol, timeframe, open, high, low, close, volume);
    }

    #endregion
}
