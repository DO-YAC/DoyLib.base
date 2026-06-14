using AutoFixture;
using AutoFixture.AutoMoq;
using doylib.Ai.Interfaces;
using doylib.Services.Interfaces;
using doylib.Strategy.Modules;
using DoyVestment.Framework.Models;
using Moq;

namespace doylib.tests.Unit.Strategy.Modules;

[TestClass]
public class ExampleAiModuleTests
{
    private IFixture mFixture = null!;
    private Mock<ICandleWindowService> mCandleWindowServiceMock = null!;
    private ExampleAiModule mSut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mFixture = new Fixture().Customize(new AutoMoqCustomization());
        mCandleWindowServiceMock = mFixture.Freeze<Mock<ICandleWindowService>>();
        mSut = new ExampleAiModule(mCandleWindowServiceMock.Object);
    }

    [TestMethod]
    public void Name_IsExampleAiModule()
    {
        Assert.AreEqual("ExampleAiModule", mSut.Name);
    }

    [TestMethod]
    public void Warmup_DoesNotThrow()
    {
        mSut.Warmup();
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
        mCandleWindowServiceMock.SetupGet(s => s.Window).Returns(new ReadOnlyMemory<Candle>(window));

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => mSut.Evaluate());
    }

    [TestMethod]
    public void AttachAi_RequestsSessionWithExpectedName()
    {
        // Arrange
        var aiMock = mFixture.Freeze<Mock<IAiInferenceService>>();
        aiMock.Setup(a => a.GetSession(It.IsAny<string>())).Returns(Mock.Of<IAiSession>());

        // Act
        mSut.AttachAi(aiMock.Object);

        // Assert
        aiMock.Verify(a => a.GetSession("lstm_eurusd_m1"), Times.Once);
    }
}
