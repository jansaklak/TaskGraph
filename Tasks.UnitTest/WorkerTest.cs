using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasks.UnitTest;

[TestClass]
public class WorkerTest
{
    [TestMethod]
    public void Worker_ContructorAndGetters_Work() {
        var wkr = new Worker(5);
        Assert.AreEqual(0, wkr.GetCost());
        Assert.AreEqual(5, wkr.GetID());
    }

    [TestMethod]
    public void Worker_PrintWkr_WritesCorrectly() {
        var wkr = new Worker(7);
        using var sw = new StringWriter();
        wkr.PrintWkr(sw);
        Assert.AreEqual("0 7", sw.ToString());
    }
}
