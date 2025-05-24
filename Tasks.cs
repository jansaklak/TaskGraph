using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace Tasks
{
    public class Edge
    {
        private int v1;
        private int v2;
        private int weight;

        public Edge(int v1, int v2, int weight = 1)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.weight = weight;
        }

        public int GetV1() => v1;
        public int GetV2() => v2;
        public int GetWeight() => weight;
    }

    public class Graf
    {
        private List<List<Edge>> adjList = new List<List<Edge>>();

        public Graf()
        {
        }

        public Graf(int vertices, int maxTasks)
        {
            // Initialize adjacency list with 'vertices' empty lists
            for (int i = 0; i < vertices; i++)
            {
                adjList.Add(new List<Edge>());
            }
        }

        public void AddEdge(int v1, int v2, int weight = 1)
        {
            while (adjList.Count <= Math.Max(v1, v2))
            {
                adjList.Add(new List<Edge>());
            }
            adjList[v1].Add(new Edge(v1, v2, weight));
        }

        public bool CheckEdge(int v1, int v2)
        {
            if (v1 >= adjList.Count) return false;

            foreach (var edge in adjList[v1])
            {
                if (edge.GetV2() == v2)
                    return true;
            }
            return false;
        }

        public int GetVerticesSize() => adjList.Count;

        public List<int> GetOutNeighbourIndices(int vertex)
        {
            var neighbors = new List<int>();
            if (vertex < adjList.Count)
            {
                foreach (var edge in adjList[vertex])
                {
                    neighbors.Add(edge.GetV2());
                }
            }
            return neighbors;
        }

        public List<List<Edge>> GetAdjList() => adjList;

        public int GetWeightEdge(int v1, int v2)
        {
            if (v1 >= adjList.Count) return 0;

            foreach (var edge in adjList[v1])
            {
                if (edge.GetV2() == v2)
                    return edge.GetWeight();
            }
            return 0;
        }

        public List<int> BFS()
        {
            var result = new List<int>();
            if (adjList.Count == 0) return result;

            var visited = new bool[adjList.Count];
            var queue = new Queue<int>();

            queue.Enqueue(0);
            visited[0] = true;

            while (queue.Count > 0)
            {
                int vertex = queue.Dequeue();
                result.Add(vertex);

                foreach (var edge in adjList[vertex])
                {
                    int neighbor = edge.GetV2();
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        private void DFSUtil(int start, int end, List<int> path, List<List<int>> allPaths, bool[] visited)
        {
            visited[start] = true;
            path.Add(start);

            if (start == end)
            {
                allPaths.Add(new List<int>(path));
            }
            else
            {
                foreach (var edge in adjList[start])
                {
                    int neighbor = edge.GetV2();
                    if (!visited[neighbor])
                    {
                        DFSUtil(neighbor, end, path, allPaths, visited);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            visited[start] = false;
        }

        public List<List<int>> DFS(int start, int end)
        {
            var allPaths = new List<List<int>>();
            var path = new List<int>();
            var visited = new bool[adjList.Count];

            DFSUtil(start, end, path, allPaths, visited);
            return allPaths;
        }
    }

    public enum HardwareType
    {
        HC = 1, // Hardware Core
        PE = 2  // Processing Element
    }



    public class Hardware : IComparable<Hardware>
    {
        private int cost;
        private HardwareType type;
        private int id;

        public Hardware(int cost, HardwareType type, int id)
        {
            this.cost = cost;
            this.type = type;
            this.id = id;
        }

        public int GetCost() => cost;
        public HardwareType GetType() => type;
        public int GetID() => id;

        public void PrintHW(TextWriter writer)
        {
            writer.Write($"{cost} {(int)type} {id}");
        }

        public int CompareTo(Hardware other)
        {
            if (type != other.type)
                return type.CompareTo(other.type);
            return id.CompareTo(other.id);
        }

        public override string ToString()
        {
            return $"{(type == HardwareType.HC ? "HC" : "PE")}{id}";
        }
    }

    public class Instance : IComparable<Instance>
    {
        private int id;
        private Hardware hardware;
        private HashSet<int> taskSet = new HashSet<int>();

        public Instance(int id, Hardware hardware)
        {
            this.id = id;
            this.hardware = hardware;
        }

        public Hardware GetHardwarePtr() => hardware;
        public HashSet<int> GetTaskSet() => taskSet;

        public void AddTask(int taskId)
        {
            taskSet.Add(taskId);
        }

        public void RemoveTask(int taskId)
        {
            taskSet.Remove(taskId);
        }

        public int CompareTo(Instance other)
        {
            if (hardware.GetType() != other.hardware.GetType())
                return hardware.GetType().CompareTo(other.hardware.GetType());
            return id.CompareTo(other.id);
        }

        public override string ToString()
        {
            return $"{hardware}{id}";
        }
    }

    public class TimeAndCost
    {
        public List<int> Times { get; set; } = new List<int>();
        public List<int> Costs { get; set; } = new List<int>();

        public TimeAndCost()
        {
        }

        public TimeAndCost(int time, int cost)
        {
            Times.Add(time);
            Costs.Add(cost);
        }

        public TimeAndCost(List<int> times, List<int> costs)
        {
            Times = times ?? new List<int>();
            Costs = costs ?? new List<int>();
        }
    }

    public class Times
    {
        private List<List<TimeAndCost>> timesAndCosts = new List<List<TimeAndCost>>();
        private List<Hardware> hardwares = new List<Hardware>();
        private static Random random = new Random();
        private const int SCALE = 100;

        public Times()
        {
        }

        public Times(int tasksAmount)
        {
            for (int i = 0; i < tasksAmount; i++)
            {
                timesAndCosts.Add(new List<TimeAndCost>());
            }
        }

        public void LoadHW(List<Hardware> hws)
        {
            hardwares = new List<Hardware>(hws);
            foreach (var hwList in timesAndCosts)
            {
                hwList.Clear();
                foreach (var hw in hardwares)
                {
                    hwList.Add(new TimeAndCost()); // Empty TimeAndCost, ready for single or subtask values
                }
            }
        }

        public void SetRandomTimesAndCosts(int subtaskProbability = 50, int minSubtasks = 2, int maxSubtasks = 4, int minValue = 1, int maxValue = SCALE)
        {
            for (int t = 0; t < timesAndCosts.Count; t++)
            {
                bool hasSubtasks = random.Next(0, 100) < subtaskProbability;
                int subtaskCount = hasSubtasks ? random.Next(minSubtasks, maxSubtasks + 1) : 1;

                for (int h = 0; h < hardwares.Count; h++)
                {
                    if (hasSubtasks)
                    {
                        var times = new List<int>();
                        var costs = new List<int>();

                        for (int s = 0; s < subtaskCount; s++)
                        {
                            times.Add(random.Next(minValue, maxValue + 1));
                            costs.Add(random.Next(minValue, maxValue + 1));
                        }

                        // Use the vector version of times and costs for subtasks
                        timesAndCosts[t][h] = new TimeAndCost(times, costs);
                    }
                    else
                    {
                        // Single value for non-subtask tasks
                        int time = random.Next(minValue, maxValue + 1);
                        int cost = random.Next(minValue, maxValue + 1);
                        timesAndCosts[t][h] = new TimeAndCost(time, cost);
                    }
                }
            }
        }

        public void SetTimesMatrix(List<List<string>> timesMatrix)
        {
            for (int t = 0; t < timesMatrix.Count; t++)
            {
                if (t >= timesAndCosts.Count)
                {
                    timesAndCosts.Add(new List<TimeAndCost>());
                }
                timesAndCosts[t].Clear();
                for (int h = 0; h < timesMatrix[t].Count; h++)
                {
                    string value = timesMatrix[t][h].Trim();
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        var subtaskValues = value.Trim('[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse).ToList();
                        timesAndCosts[t].Add(new TimeAndCost(subtaskValues, new List<int>(new int[subtaskValues.Count])));
                    }
                    else
                    {
                        int time = int.Parse(value);
                        timesAndCosts[t].Add(new TimeAndCost(time, 0));
                    }
                }
            }
        }

        public void SetCostsMatrix(List<List<string>> costsMatrix)
        {
            for (int t = 0; t < costsMatrix.Count; t++)
            {
                if (t >= timesAndCosts.Count)
                {
                    timesAndCosts.Add(new List<TimeAndCost>());
                }
                for (int h = 0; h < costsMatrix[t].Count && h < timesAndCosts[t].Count; h++)
                {
                    string value = costsMatrix[t][h].Trim();
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        var subtaskValues = value.Trim('[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse).ToList();
                        timesAndCosts[t][h].Costs = subtaskValues;
                    }
                    else
                    {
                        int cost = int.Parse(value);
                        timesAndCosts[t][h].Costs = new List<int> { cost };
                    }
                }
            }
        }

        public List<int> GetTimes(int taskId, Hardware hw)
        {
            if (taskId >= timesAndCosts.Count || hw.GetID() >= timesAndCosts[taskId].Count)
                return new List<int>();
            return timesAndCosts[taskId][hw.GetID()].Times;
        }

        public List<int> GetCosts(int taskId, Hardware hw)
        {
            if (taskId >= timesAndCosts.Count || hw.GetID() >= timesAndCosts[taskId].Count)
                return new List<int>();
            return timesAndCosts[taskId][hw.GetID()].Costs;
        }

        public void Show(TextWriter writer = null)
        {
            var output = writer ?? Console.Out;

            output.WriteLine("@times");
            for (int t = 0; t < timesAndCosts.Count; t++)
            {
                for (int h = 0; h < timesAndCosts[t].Count; h++)
                {
                    var times = timesAndCosts[t][h].Times;
                    if (times.Count > 1)
                        output.Write($"[{string.Join(" ", times)}] ");
                    else
                        output.Write($"{times[0]} ");
                }
                output.WriteLine();
            }

            output.WriteLine("@cost");
            for (int t = 0; t < timesAndCosts.Count; t++)
            {
                for (int h = 0; h < timesAndCosts[t].Count; h++)
                {
                    var costs = timesAndCosts[t][h].Costs;
                    if (costs.Count > 1)
                        output.Write($"[{string.Join(" ", costs)}] ");
                    else
                        output.Write($"{costs[0]} ");
                }
                output.WriteLine();
            }
        }
    }

    public class CostList
    {
        private List<Hardware> hardwares = new List<Hardware>();
        public Times times = new Times();
        private Graf taskGraph = new Graf();
        private Dictionary<int, int> hwInstancesCount = new Dictionary<int, int>();
        private List<Instance> instances = new List<Instance>();
        private Dictionary<int, Instance> taskInstanceMap = new Dictionary<int, Instance>();
        private Dictionary<int, Tuple<int, int>> taskSchedule = new Dictionary<int, Tuple<int, int>>();
        private List<Hardware> hwToTasks = new List<Hardware>();
        private List<int> progress = new List<int>();
        private int simulationTimeScale = 1;
        private int totalCost = 0;
        private const int INF = 2000000000;
        private const int SCALE = 100;
        private static Random random = new Random();

        private int tasksAmount;
        private int hardwareCoresAmount;
        private int processingUnitAmount;
        private int channelsAmount;
        private int withCost;

        public Times GetTimes() => times;

        public List<Instance> GetInstances()
        {
            return instances;
        }

        public CostList()
        {
            times = new Times();
            taskGraph = new Graf();
            hardwares = new List<Hardware>();
            instances = new List<Instance>();
            taskInstanceMap = new Dictionary<int, Instance>();
            taskSchedule = new Dictionary<int, Tuple<int, int>>();
            progress = new List<int>();
            totalCost = 0;
            tasksAmount = 0;
        }

        public CostList(int tasks, int hcs, int pes, int coms, int maxTasks)
        {
            tasksAmount = tasks;
            times = new Times(tasks);
            taskGraph = new Graf(tasks, maxTasks);
            hardwares = new List<Hardware>();
            instances = new List<Instance>();
            taskInstanceMap = new Dictionary<int, Instance>();
            taskSchedule = new Dictionary<int, Tuple<int, int>>();
            progress = new List<int>(new int[tasks]);
            totalCost = 0;
            hardwareCoresAmount = hcs;
            processingUnitAmount = pes;
            channelsAmount = coms;
            withCost = 1;
        }


        public void AssignEachToFastestHardware()
        {
            if (taskGraph == null || hardwares == null || hardwares.Count == 0)
            {
                Console.Error.WriteLine("Invalid task graph or hardware list.");
                return;
            }

            // Clear existing instances and mappings
            instances.Clear();
            taskInstanceMap.Clear();
            hwInstancesCount.Clear();
            taskSchedule.Clear();

            // Dictionary to track instances per hardware
            Dictionary<Hardware, Instance> hardwareInstances = new Dictionary<Hardware, Instance>();
            int estimatedCost = 0;
            int estimatedTime = 0;
            int numAllocated = 0;

            // Assign each task to the hardware with the lowest execution time
            for (int i = 0; i < taskGraph.GetVerticesSize(); i++)
            {
                Hardware fastestHw = null;
                int minTime = int.MaxValue;

                // Find the fastest hardware for this task
                foreach (var hw in hardwares)
                {
                    var allTaskTimes = times.GetTimes(i, hw) ?? new List<int>();
                    int taskTime = allTaskTimes.Count > 1 ? allTaskTimes.Max() : allTaskTimes.FirstOrDefault();
                    if (taskTime < minTime)
                    {
                        minTime = taskTime;
                        fastestHw = hw;
                    }
                }

                if (fastestHw == null)
                {
                    Console.Error.WriteLine($"No suitable hardware found for task {i}.");
                    continue;
                }

                // Get or create an instance for this hardware
                if (!hardwareInstances.TryGetValue(fastestHw, out var inst))
                {
                    int instId = hwInstancesCount.ContainsKey(fastestHw.GetID()) ? hwInstancesCount[fastestHw.GetID()] : 0;
                    inst = new Instance(instId, fastestHw);
                    instances.Add(inst);
                    hardwareInstances[fastestHw] = inst;
                    hwInstancesCount[fastestHw.GetID()] = instId + 1;
                }

                // Assign task to the instance
                var taskTimes = times.GetTimes(i, fastestHw) ?? new List<int>();
                var taskCosts = times.GetCosts(i, fastestHw) ?? new List<int>();

                if (taskTimes.Count > 1) // Subtasks
                {
                    estimatedTime += taskTimes.Max();
                    estimatedCost += taskCosts.Sum();
                }
                else // Single task
                {
                    estimatedTime += taskTimes.FirstOrDefault();
                    estimatedCost += taskCosts.FirstOrDefault();
                }

                inst.AddTask(i);
                taskInstanceMap[i] = inst;
                numAllocated++;
            }

            totalCost = estimatedCost;
            Console.WriteLine($"Assigned {numAllocated} tasks to their fastest hardware. Estimated time: {estimatedTime}, Estimated cost: {estimatedCost}");
        }

        public void AssignToCheapestHardware()
        {
            if (taskGraph == null || hardwares == null || hardwares.Count == 0)
            {
                Console.Error.WriteLine("Invalid task graph or hardware list.");
                return;
            }

            // Clear existing instances and mappings
            instances.Clear();
            taskInstanceMap.Clear();
            hwInstancesCount.Clear();
            taskSchedule.Clear();

            // Find the hardware with the lowest total cost
            Hardware cheapestHw = null;
            int minTotalCost = int.MaxValue;

            foreach (var hw in hardwares)
            {
                int totalCost = 0;
                for (int i = 0; i < taskGraph.GetVerticesSize(); i++)
                {
                    var taskCosts = times.GetCosts(i, hw) ?? new List<int>();
                    totalCost += taskCosts.Count > 1 ? taskCosts.Sum() : taskCosts.FirstOrDefault();
                }
                if (totalCost < minTotalCost)
                {
                    minTotalCost = totalCost;
                    cheapestHw = hw;
                }
            }

            if (cheapestHw == null)
            {
                Console.Error.WriteLine("No suitable hardware found.");
                return;
            }

            // Assign all tasks to the cheapest hardware
            int estimatedCost = 0;
            int estimatedTime = 0;
            int numAllocated = 0;

            // Create a single instance for the cheapest hardware
            int instId = hwInstancesCount.ContainsKey(cheapestHw.GetID()) ? hwInstancesCount[cheapestHw.GetID()] : 0;
            var inst = new Instance(instId, cheapestHw);
            instances.Add(inst);
            hwInstancesCount[cheapestHw.GetID()] = instId + 1;

            for (int i = 0; i < taskGraph.GetVerticesSize(); i++)
            {
                var taskTimes = times.GetTimes(i, cheapestHw) ?? new List<int>();
                var taskCosts = times.GetCosts(i, cheapestHw) ?? new List<int>();

                if (taskTimes.Count > 1) // Subtasks
                {
                    estimatedTime += taskTimes.Max();
                    estimatedCost += taskCosts.Sum();
                }
                else // Single task
                {
                    estimatedTime += taskTimes.FirstOrDefault();
                    estimatedCost += taskCosts.FirstOrDefault();
                }

                inst.AddTask(i);
                taskInstanceMap[i] = inst;
                numAllocated++;
            }

            totalCost = estimatedCost;
            Console.WriteLine($"Assigned {numAllocated} tasks to {cheapestHw}. Estimated time: {estimatedTime}, Estimated cost: {estimatedCost}");
        }




        private void CountTimer(ref bool stop, ref int time)
        {
            var start = Stopwatch.StartNew();
            while (!stop)
            {
                time = (int)start.ElapsedMilliseconds;
                Thread.Sleep(1);
            }
        }

        public int GetInstanceStartingTime(Instance inst)
        {
            int startingTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                if (GetStartingTime(i) > startingTime) // Ensure parentheses
                    startingTime = GetStartingTime(i);
            }
            return startingTime;
        }

        public int GetInstanceEndingTime(Instance inst)
        {
            int endingTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                if (GetEndingTime(i) > endingTime) // Ensure parentheses
                    endingTime = GetEndingTime(i);
            }
            return endingTime;
        }

        public int GetTimeRunning(Instance inst)
        {
            int totalTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                totalTime += GetEndingTime(i) - GetStartingTime(i);
            }
            return totalTime;
        }

        public int GetIdleTime(Instance inst, int timeStop)
        {
            return timeStop - GetTimeRunning(inst);
        }

        public Instance GetLongestRunningInstance()
        {
            int longestRunning = int.MinValue;
            Instance longest = null;

            foreach (var inst in instances)
            {
                int runningTime = GetTimeRunning(inst);
                if (runningTime > longestRunning)
                {
                    longestRunning = runningTime;
                    longest = inst;
                }
            }
            return longest;
        }

        public Instance GetShortestRunningInstance()
        {
            int shortestRunning = int.MaxValue;
            Instance shortest = null;

            foreach (var inst in instances)
            {
                int runningTime = GetTimeRunning(inst);
                if (runningTime < shortestRunning)
                {
                    shortestRunning = runningTime;
                    shortest = inst;
                }
            }
            return shortest;
        }

        private static bool RandomBool(int prob)
        {
            prob++;
            return random.Next(prob) == 0;
        }

        private static int GetRand(int max)
        {
            if (max < 1) max = 1;
            return random.Next(max);
        }

        public List<Hardware> GetHardwares() => hardwares;
        public Graf GetGraph() => taskGraph;

        public void Clear()
        {
            hardwares.Clear();
            times = new Times();
            taskGraph = new Graf();
            hwInstancesCount.Clear();
            instances.Clear();
            taskInstanceMap.Clear();
            hwToTasks.Clear();
            taskSchedule.Clear();
        }

        private List<string> ParseMatrixLine(string line)
        {
            var result = new List<string>();

            // Jeśli linia zawiera nawiasy kwadratowe
            if (line.Contains("[") && line.Contains("]"))
            {
                var matches = Regex.Matches(line, @"\[[^\[\]]+\]|\d+");

                foreach (Match match in matches)
                {
                    result.Add(match.Value);
                }
            }
            else
            {
                result.AddRange(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            return result;
        }

        public int LoadFromFile(string filename, out string errorMessage)
        {
            errorMessage = "";
            int lineNumber = 0;

            try
            {
                using (var file = new StreamReader(filename))
                {
                    string line;
                    int section = -1;
                    int tnum, weight, to, tasks;
                    int hwCost, hwType;
                    var timesMatrix = new List<List<string>>();
                    var costsMatrix = new List<List<string>>();
                    var loaded = new Graf();
                    int hwIdCounter = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        lineNumber++;
                        line = line.Trim();

                        try
                        {
                            if (line.StartsWith("@tasks")) section = 0;
                            else if (line.StartsWith("@proc")) section = 1;
                            else if (line.StartsWith("@times")) section = 2;
                            else if (line.StartsWith("@cost")) section = 3;
                            else if (line.StartsWith("@comm")) section = 4;
                            else if (string.IsNullOrWhiteSpace(line)) continue;
                            else
                            {
                                switch (section)
                                {
                                    case 0:
                                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        if (parts.Length >= 2 && parts[0].StartsWith("T") && int.TryParse(parts[0][1..], out tnum))
                                        {
                                            if (!int.TryParse(parts[1], out tasks)) throw new Exception("Invalid task count.");
                                            for (int i = 2; i < parts.Length; i++)
                                            {
                                                var token = parts[i];
                                                if (token.Contains('(') && token.Contains(')'))
                                                {
                                                    var inner = token.Split('(');
                                                    if (inner.Length != 2) throw new Exception($"Invalid edge format: {token}");
                                                    to = int.Parse(inner[0]);
                                                    weight = int.Parse(inner[1].TrimEnd(')'));
                                                    if (weight == 0) weight = 1;
                                                    loaded.AddEdge(tnum, to, weight);
                                                    tasksAmount++;
                                                }
                                            }
                                        }
                                        break;

                                    case 1:
                                        var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        if (p.Length >= 3)
                                        {
                                            hwCost = int.Parse(p[0]);
                                            hwType = int.Parse(p[1]);
                                            int hwId = int.Parse(p[2]);
                                            var hw = new Hardware(hwCost, (HardwareType)hwType, hwIdCounter++);
                                            hardwares.Add(hw);
                                        }
                                        break;

                                    case 2:
                                        timesMatrix.Add(ParseMatrixLine(line));
                                        break;

                                    case 3:
                                        costsMatrix.Add(ParseMatrixLine(line));
                                        break;


                                    default:
                                        throw new Exception("Unexpected section or malformed file.");
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            errorMessage = $"Błąd w linii {lineNumber}: \"{line}\"\nSzczegóły: {innerEx.Message}";
                            return -1;
                        }
                    }

                    if (timesMatrix.Count != costsMatrix.Count)
                    {
                        errorMessage = "Liczba wierszy w sekcji @times i @cost nie jest taka sama.";
                        return -1;
                    }

                    // ⬇️ Przekazanie danych do klasy Times
                    times.SetTimesMatrix(timesMatrix);
                    times.SetCostsMatrix(costsMatrix);

                    taskGraph = loaded;
                    tasksAmount--;

                    return 1;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd przy odczycie pliku: {ex.Message} (linia {lineNumber})";
                return -1;
            }
        }




        public void CreateRandomTasksGraph()
        {
            if (tasksAmount <= 1)
            {
                Console.Error.WriteLine("Invalid number of tasks");
                return;
            }

            var visited = new HashSet<int>();
            var g = new Graf();

            if (withCost > 0)
            {
                g.AddEdge(0, 1, GetRand(SCALE));
            }
            else
            {
                g.AddEdge(0, 1);
            }

            visited.Add(0);
            var visitedList = visited.ToList();

            foreach (int i in visitedList)
            {
                for (int j = 0; j < tasksAmount; j++)
                {
                    if (RandomBool(tasksAmount / 6) && !visited.Contains(j))
                    {
                        if (withCost > 0)
                        {
                            g.AddEdge(i, j, GetRand(SCALE));
                        }
                        else
                        {
                            g.AddEdge(i, j);
                        }
                        visited.Add(j);
                    }
                }
            }

            for (int i = 0; i < tasksAmount; i++)
            {
                if (!visited.Contains(i))
                {
                    int idx = random.Next(visited.Count);
                    int randomVisitedTask = visited.ElementAt(idx);

                    if (!g.CheckEdge(randomVisitedTask, i))
                    {
                        if (withCost > 0)
                        {
                            g.AddEdge(randomVisitedTask, i, GetRand(SCALE));
                        }
                        else
                        {
                            g.AddEdge(randomVisitedTask, i);
                        }
                        visited.Add(i);
                    }
                }
            }

            visited.Clear();
            taskGraph = g;
        }

        public void PrintTasks(TextWriter writer = null)
        {
            var output = writer ?? Console.Out;
            output.WriteLine($"@tasks {tasksAmount}");

            var outIdx = new List<int>();
            var size = tasksAmount;

            for (int i = 0; i < size; i++)
            {
                outIdx = taskGraph.GetOutNeighbourIndices(i);
                output.Write($"T{i} ");
                output.Write($"{outIdx.Count} ");

                foreach (int j in outIdx)
                {
                    output.Write($"{j}({taskGraph.GetWeightEdge(i, j)}) ");
                }
                output.WriteLine();
            }
        }

        public void PrintProc(TextWriter writer = null)
        {
            var output = writer ?? Console.Out;
            output.WriteLine($"@proc {hardwares.Count}");

            foreach (var h in hardwares)
            {
                h.PrintHW(output);
                output.WriteLine();
            }
        }

        public void PrintInstances()
        {
            instances.Sort();
            Console.WriteLine($"Created {instances.Count} components");

            int criticalTime = 0;
            if (taskInstanceMap != null)
            {
                foreach (var pair in taskInstanceMap)
                {
                    int taskId = pair.Key;
                    int endingTime = GetEndingTime(taskId);
                    if (endingTime > criticalTime)
                    {
                        criticalTime = endingTime;
                    }
                }
            }

            foreach (var inst in instances ?? Enumerable.Empty<Instance>())
            {
                int expTime = 0;
                int expCost = 0;
                var taskIds = inst.GetTaskSet()?.ToList() ?? new List<int>();

                foreach (int taskID in taskIds)
                {
                    var taskTimes = this.times.GetTimes(taskID, inst.GetHardwarePtr()) ?? new List<int>();
                    var taskCosts = this.times.GetCosts(taskID, inst.GetHardwarePtr()) ?? new List<int>();

                    if (taskTimes.Count > 1) // Subtasks: use max time, sum costs
                    {
                        expTime += taskTimes.Any() ? taskTimes.Max() : 0;
                        expCost += taskCosts.Any() ? taskCosts.Sum() : 0;
                    }
                    else // Single task
                    {
                        expTime += taskTimes.Count > 0 ? taskTimes[0] : 0;
                        expCost += taskCosts.Count > 0 ? taskCosts[0] : 0;
                    }
                }

                expCost += inst.GetHardwarePtr()?.GetCost() ?? 0;
                Console.WriteLine($"{inst} Tasks: {taskIds.Count} Expected time: {expTime} Idle time: {GetIdleTime(inst, criticalTime)} Cost: {expCost} Including initial: {inst.GetHardwarePtr()?.GetCost() ?? 0}");
                foreach (int taskID in taskIds)
                {
                    var taskTimes = this.times.GetTimes(taskID, inst.GetHardwarePtr()) ?? new List<int>();
                    var taskCosts = this.times.GetCosts(taskID, inst.GetHardwarePtr()) ?? new List<int>();

                    if (taskTimes.Count > 1)
                    {
                        Console.WriteLine($"  T{taskID} (Subtasks: Times={string.Join(", ", taskTimes)}, Costs={string.Join(", ", taskCosts)})");
                    }
                    else
                    {
                        Console.WriteLine($"  T{taskID} (Time: {(taskTimes.Count > 0 ? taskTimes[0] : 0)}, Cost: {(taskCosts.Count > 0 ? taskCosts[0] : 0)})");
                    }
                }
            }

            Console.WriteLine("\nTask schedule:");
            if (taskSchedule != null)
            {
                foreach (var pair in taskSchedule)
                {
                    var instance = GetInstance(pair.Key);
                    // Use ternary operator to handle null instance
                    Console.WriteLine($"T{pair.Key}\ton {(instance != null ? instance.ToString() : "None")} from: {pair.Value.Item1} to: {pair.Value.Item2}");
                }
            }

            Console.WriteLine($"Critical path time: {criticalTime}");
        }

        public void RandALL()
        {
            times = new Times(tasksAmount); // Initialize the field
            CreateRandomTasksGraph();
            GetRandomProc(hardwareCoresAmount, processingUnitAmount);
            times.LoadHW(hardwares);
            times.SetRandomTimesAndCosts();
        }

        public void PrintALL(string filename, bool toScreen = false)
        {
            try
            {
                using (var outputFile = new StreamWriter(filename))
                {
                    // @tasks
                    if (toScreen) PrintTasks();
                    PrintTasks(outputFile);

                    // @proc
                    if (toScreen) PrintProc();
                    PrintProc(outputFile);

                    // @times @cost
                    if (toScreen) times.Show();
                    times.Show(outputFile);

                }

                Console.WriteLine($"Saved list to file {filename}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cannot open file for writing: {ex.Message}");
            }
        }

        public int GetRandomProc()
        {
            hardwares.Clear();

            if (hardwareCoresAmount < 1 || processingUnitAmount < 1)
            {
                Console.Error.WriteLine("Invalid number of HC or PU");
                return -1;
            }

            for (int i = 0; i < hardwareCoresAmount; i++)
            {
                hardwares.Add(new Hardware(GetRand(5) + 1, HardwareType.HC, hardwares.Count));
            }

            int hcSize = hardwareCoresAmount;
            for (int j = 0; j < processingUnitAmount; j++)
            {
                hardwares.Add(new Hardware(GetRand(5) + 1, HardwareType.PE, hardwares.Count - hcSize));
            }

            return 1;
        }

        public int GetRandomProc(int hcs, int pes)
        {
            hardwares.Clear();

            if (hcs < 1 || pes < 1)
            {
                Console.Error.WriteLine("Invalid number of HC or PU");
                return -1;
            }

            for (int i = 0; i < hcs; i++)
            {
                hardwares.Add(new Hardware(GetRand(5) + 1, HardwareType.HC, hardwares.Count));
            }

            int hcSize = hcs;
            for (int j = 0; j < pes; j++)
            {
                hardwares.Add(new Hardware(GetRand(5) + 1, HardwareType.PE, hardwares.Count - hcSize));
            }

            return 1;
        }

        public void RunTasks()
        {
            int totalCost = 0;
            simulationTimeScale = 1;

            Console.WriteLine($"\nRunning tasks in scale x{simulationTimeScale}:");

            progress = Enumerable.Repeat(-2, taskGraph.GetVerticesSize()).ToList(); // -2 - can't be done flag
            progress[0] = -1;

            var tasks = new List<Task>();
            int numThreads = instances.Count;
            bool stop = false;
            int time = 0;

            var counterTask = Task.Run(() => CountTimer(ref stop, ref time));

            foreach (var inst in instances)
            {
                var instanceCopy = inst;
                tasks.Add(Task.Run(() => TaskRunner(instanceCopy)));
            }

            Task.WaitAll(tasks.ToArray());
            stop = true;
            counterTask.Wait();

            Console.WriteLine($"\n\nProgram execution time: {time} milliseconds. (scale x{simulationTimeScale})\n\n");
        }

        public Hardware GetLowestTimeHardware(int taskId, int timeCost)
        {
            int minValue = INF;
            Hardware minHardware = null;

            foreach (var hw in hardwares)
            {
                var values = (timeCost == 0) ? times.GetTimes(taskId, hw) : times.GetCosts(taskId, hw);
                // For subtasks, use max time (parallel execution) or sum of costs
                int value = values.Count > 1 ? (timeCost == 0 ? values.Max() : values.Sum()) : values.FirstOrDefault();
                if (value < minValue)
                {
                    minValue = value;
                    minHardware = hw;
                }
            }

            return minHardware;
        }

        public void createInstance(int taskId, Hardware hw)
        {
            int instId = hwInstancesCount.ContainsKey(hw.GetID()) ? hwInstancesCount[hw.GetID()] : 0;
            var inst = new Instance(instId, hw);
            inst.AddTask(taskId);
            instances.Add(inst);
            taskInstanceMap[taskId] = inst;
            hwInstancesCount[hw.GetID()] = instId + 1;
        }

        public void addTaskToInstance(int taskId, Instance inst)
        {
            inst.AddTask(taskId);
            taskInstanceMap[taskId] = inst;
        }

        public void removeTaskFromInstance(int taskId)
        {
            Instance oldInst = taskInstanceMap[taskId];
            oldInst.RemoveTask(taskId);
            if(oldInst.GetTaskSet().Count == 0)
            {
                instances.Remove(oldInst);
            }
            else
            {
                taskInstanceMap[taskId] = oldInst;
            }
            taskInstanceMap.Remove(taskId);
        }

        public void TaskRunner(Instance inst)
        {
            if (inst == null || inst.GetHardwarePtr() == null)
            {
                Console.Error.WriteLine("Invalid instance or hardware.");
                return;
            }

            while (true)
            {
                int taskId = -1;
                lock (progress)
                {
                    taskId = GetNextTask();
                    if (taskId == -1) // No more tasks to process
                        break;

                    if (progress[taskId] == -1) // Task is ready to start
                    {
                        progress[taskId] = 0; // Mark as in progress
                    }
                    else
                    {
                        continue; // Task is not ready or already processed
                    }
                }

                // Get times and costs for the task using the CostList.times field
                var taskTimes = times.GetTimes(taskId, inst.GetHardwarePtr()) ?? new List<int>();
                var taskCosts = times.GetCosts(taskId, inst.GetHardwarePtr()) ?? new List<int>();
                int maxTime = 0;
                int startTime = GetInstanceStartingTime(inst);

                try
                {
                    if (taskTimes.Count > 1) // Handle subtasks (parallel execution)
                    {
                        var subtaskEndTimes = new List<int>();
                        var subtaskTasks = new List<Task>();

                        for (int s = 0; s < taskTimes.Count; s++)
                        {
                            int subtaskIndex = s;
                            int subtaskTime = taskTimes[subtaskIndex];
                            if (subtaskTime < 0)
                            {
                                Console.Error.WriteLine($"Invalid subtask time for task {taskId}, subtask {subtaskIndex}.");
                                continue;
                            }

                            var subtask = Task.Run(() =>
                            {
                                Thread.Sleep(subtaskTime * simulationTimeScale);
                                lock (subtaskEndTimes)
                                {
                                    subtaskEndTimes.Add(startTime + subtaskTime);
                                }
                            });
                            subtaskTasks.Add(subtask);
                        }

                        // Wait for all subtasks to complete
                        Task.WhenAll(subtaskTasks).Wait();

                        maxTime = subtaskEndTimes.Any() ? subtaskEndTimes.Max() : startTime;
                    }
                    else // Handle single task
                    {
                        int timeCost = taskTimes.Count > 0 ? taskTimes[0] : 0;
                        if (timeCost < 0)
                        {
                            Console.Error.WriteLine($"Invalid time for task {taskId}.");
                            timeCost = 0;
                        }
                        Thread.Sleep(timeCost * simulationTimeScale);
                        maxTime = startTime + timeCost;
                    }

                    lock (progress)
                    {
                        progress[taskId] = -2; // Mark task as completed
                        taskSchedule[taskId] = new Tuple<int, int>(startTime, maxTime);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing task {taskId}: {ex.Message}");
                    lock (progress)
                    {
                        progress[taskId] = -2; // Mark as completed to avoid stalling
                    }
                }
            }
        }

        void PrintSchedule()
        {
            Console.WriteLine("Task schedule:");
            foreach (var pair in taskSchedule)
            {
                Console.WriteLine($"T{pair.Key} from {pair.Value.Item1} to {pair.Value.Item2}");
            }
        }

        public int GetStartingTime(int taskId)
        {
            if (taskSchedule.ContainsKey(taskId))
            {
                return taskSchedule[taskId].Item1;
            }
            return 0;
        }

        public int GetEndingTime(int taskId)
        {
            if (taskSchedule.ContainsKey(taskId))
            {
                return taskSchedule[taskId].Item2;
            }
            return 0;
        }

        public int GetNextTask()
        {
            for (int i = 0; i < progress.Count; i++)
            {
                if (progress[i] == -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public Instance GetInstance(int taskId)
        {
            if (taskInstanceMap.ContainsKey(taskId))
            {
                return taskInstanceMap[taskId];
            }
            return null;
        }

        public void taskDistribution(int rule)
        {
            if (taskGraph == null || hardwares == null || hardwares.Count == 0)
            {
                Console.Error.WriteLine("Invalid task graph or hardware list.");
                return;
            }

            int estimatedCost = 0;
            int estimatedTime = 0;
            totalCost = 0;
            int taskAmount = taskGraph.GetVerticesSize();
            List<int> allocatedTasks = new List<int>(Enumerable.Repeat(0, taskAmount)); // Track allocated task IDs
            int numAllocated = 0;

            for (int i = 0; i < taskAmount; i++)
            {
                // Use this.times to explicitly access the CostList.times field
                var taskTimesList = this.times.GetTimes(i, hardwares[0]) ?? new List<int>();

                if (taskTimesList.Count > 1) // Subtasks: allocate each subtask to best hardware
                {
                    for (int s = 0; s < taskTimesList.Count; s++)
                    {
                        Hardware bestHw = null;
                        int minValue = INF;

                        foreach (var hw in hardwares)
                        {
                            var hwTimes = this.times.GetTimes(i, hw) ?? new List<int>();
                            var hwCosts = this.times.GetCosts(i, hw) ?? new List<int>();

                            if (s >= hwTimes.Count || s >= hwCosts.Count)
                                continue;

                            int value = (rule == 0) ? hwTimes[s] : hwCosts[s];
                            if (value < minValue)
                            {
                                minValue = value;
                                bestHw = hw;
                            }
                        }

                        if (bestHw != null)
                        {
                            var bestHwCosts = this.times.GetCosts(i, bestHw) ?? new List<int>();
                            var bestHwTimes = this.times.GetTimes(i, bestHw) ?? new List<int>();

                            if (s < bestHwCosts.Count && s < bestHwTimes.Count)
                            {
                                estimatedCost += bestHwCosts[s];
                                estimatedTime += bestHwTimes[s];
                                totalCost += bestHwCosts[s];

                                createInstance(i, bestHw);
                                addTaskToInstance(i, instances[instances.Count - 1]);
                                if (allocatedTasks[i] == 0) // Count task only once
                                {
                                    allocatedTasks[i] = 1; // Mark task as allocated
                                    numAllocated++;
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"No suitable hardware found for task {i}, subtask {s}.");
                        }
                    }
                }
                else // Single task: allocate to best hardware
                {
                    var hw = GetLowestTimeHardware(i, rule);
                    if (hw != null)
                    {
                        var taskCosts = this.times.GetCosts(i, hw) ?? new List<int>();
                        var taskTimes = this.times.GetTimes(i, hw) ?? new List<int>();

                        estimatedCost += taskCosts.Count > 0 ? taskCosts[0] : 0;
                        estimatedTime += taskTimes.Count > 0 ? taskTimes[0] : 0;
                        totalCost += taskCosts.Count > 0 ? taskCosts[0] : 0;

                        createInstance(i, hw);
                        allocatedTasks[i] = 1; // Mark task as allocated
                        numAllocated++;
                    }
                    else
                    {
                        Console.Error.WriteLine($"No suitable hardware found for task {i}.");
                    }
                }
            }

            Console.WriteLine($"Allocated {numAllocated} tasks. Estimated time: {estimatedTime}, Estimated cost: {estimatedCost}");
        }


    }
}
