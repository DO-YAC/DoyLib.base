using doylib.Services.Interfaces;
using doylib.Strategy.Modules;
using DoyVestment.Framework.Models.Enums;
using Moq;

namespace doylib.tests.Unit.Strategy.Modules;

[TestClass]
public class ExampleModuleTests
{
    private ExampleModule mSut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mSut = new ExampleModule(Mock.Of<ICandleWindowService>());
    }

    [TestMethod]
    public void Name_IsExampleModule()
    {
        Assert.AreEqual("ExampleModule", mSut.Name);
    }

    [TestMethod]
    public void Evaluate_ReturnsOneOfNoneBuyOrSell()
    {
        for (int i = 0; i < 25; i++)
        {
            var action = mSut.Evaluate();
            Assert.IsTrue(
                action is TradeAction.NONE or TradeAction.BUY or TradeAction.SELL,
                $"Unexpected action: {action}");
        }
    }

    [TestMethod]
    public void Warmup_DoesNotThrow()
    {
        mSut.Warmup();
    }
}
