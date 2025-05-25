namespace Tasks.UnitTest;

[TestClass]
public class TimesTest {
    [TestMethod]
    public void Times_SetAndGetTimesAndCosts_Works() {
        var wkr = new Worker(0);
        var times = new Times(1);

        times.LoadWkr(new List<Worker> { wkr });
        times.SetRandomTimesAndCosts();

        var t = times.GetTimes(0, wkr);
        var c = times.GetCosts(0, wkr);
        Assert.AreEqual(t.Count, c.Count);
    }
}
