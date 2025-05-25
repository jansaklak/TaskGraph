namespace Tasks.UnitTest;

[TestClass]
public class TimeAndCostTest {
    [TestMethod]
    [DataRow(100, 200, 1, 2)]
    [DataRow(300, 400, 3, 4)]
    [DataRow(500, 600, 5, 6)]
    [DataRow(700, 800, 7, 8)]
    [DataRow(900, 1000, 9, 10)]
    public void TimeAndCost_Constructors_WorkCorrectly(int time1, int time2, int cost1, int cost2) {
        var t1 = new TimeAndCost(time1, cost1);
        Assert.AreEqual(time1, t1.Times[0]);
        Assert.AreEqual(cost1, t1.Costs[0]);

        var t2 = new TimeAndCost(time2, cost2);
        Assert.AreEqual(time2, t2.Times[0]);
        Assert.AreEqual(cost2, t2.Costs[0]);

        var t3 = new TimeAndCost(new List<int> { time1, time2 }, new List<int> { cost1, cost2 });
        CollectionAssert.AreEqual(new List<int> { time1, time2 }, t3.Times);
        CollectionAssert.AreEqual(new List<int> { cost1, cost2 }, t3.Costs);
    }
}
