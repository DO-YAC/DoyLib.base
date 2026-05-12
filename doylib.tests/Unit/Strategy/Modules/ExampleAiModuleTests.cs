using doylib.Ai.Interfaces;
using doylib.Services.Interfaces;
using doylib.Strategy.Modules;
using DoyVestment.Framework.Models;
using Moq;

namespace doylib.tests.Unit.Strategy.Modules;

[TestClass]
public class ExampleAiModuleTests
{
    [TestMethod]
    public void Name_IsExampleAiModule()
    {
        var sut = new ExampleAiModule(Mock.Of<ICandleWindowService>());

        Assert.AreEqual("ExampleAiModule", sut.Name);
    }

    [TestMethod]
    public void Warmup_DoesNotThrow()
    {
        var sut = new ExampleAiModule(Mock.Of<ICandleWindowService>());

        sut.Warmup();
    }

    [TestMethod]
    public void Evaluate_BeforeAttachAi_Throws()
    {
        // Arrange — feed the candle window enough candles to pass the slicing step
        var window = new Candle[60];
        for (int i = 0; i < window.Length; i++)
        {
            window[i] = new Candle(new DateTime(2024, 1, 1).AddMinutes(i),
                "EURUSD", "M1", 1, 1.1, 0.9, 1.05, 100);
        }
        var candleSvc = new Mock<ICandleWindowService>();
        candleSvc.SetupGet(s => s.Window).Returns(new ReadOnlyMemory<Candle>(window));

        var sut = new ExampleAiModule(candleSvc.Object);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Evaluate());
    }

    [TestMethod]
    public void AttachAi_RequestsSessionWithExpectedName()
    {
        // Arrange
        var aiMock = new Mock<IAiInferenceService>();
        aiMock.Setup(a => a.GetSession(It.IsAny<string>())).Returns(Mock.Of<IAiSession>());

        var sut = new ExampleAiModule(Mock.Of<ICandleWindowService>());

        // Act
        sut.AttachAi(aiMock.Object);

        // Assert
        aiMock.Verify(a => a.GetSession("lstm_eurusd_m1"), Times.Once);
    }
}
