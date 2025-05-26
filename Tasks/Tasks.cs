using System.IO;
using System.Text.RegularExpressions;


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

    public class Worker
    {
        private int cost = 0;
        private int id;

        public Worker(int id)
        {
            this.cost = 0;
            this.id = id;
        }

        public int GetCost() => cost;
        public int GetID() => id;

        public void PrintWkr(TextWriter writer)
        {
            writer.Write($"{cost} {id}");
        }
    }

    public class Assignment : IComparable<Assignment>
    {
        private int id;
        private Worker worker;
        private HashSet<int> taskSet = new HashSet<int>();

        public Assignment(int id, Worker worker)
        {
            this.id = id;
            this.worker = worker;
        }

        public Worker GetWorkerPtr() => worker;
        public HashSet<int> GetTaskSet() => taskSet;

        public void AddTask(int taskId)
        {
            taskSet.Add(taskId);
        }

        public void RemoveTask(int taskId)
        {
            taskSet.Remove(taskId);
        }

        public override string ToString()
        {
            return $"{worker}{id}";
        }

        public int CompareTo(Assignment other)
        {
            if (other == null) return 1;
            // Najpierw sortuj po ID worker, potem po ID instancji
            int cmp = worker.GetID().CompareTo(other.worker.GetID());
            if (cmp != 0) return cmp;
            return id.CompareTo(other.id);
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
        private List<Worker> wkrs = new List<Worker>();
        public List<List<TimeAndCost>> TimeAndCosts => timesAndCosts;
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

        public void LoadWkr(List<Worker> workers)
        {
            wkrs = new List<Worker>(workers);
            foreach (var wkrList in timesAndCosts)
            {
                wkrList.Clear();
                foreach (var wkr in wkrs)
                {
                    wkrList.Add(new TimeAndCost());
                }
            }
        }

        public void SetRandomTimesAndCosts(int subtaskProbability = 50, int minSubtasks = 2, int maxSubtasks = 4, int minValue = 1, int maxValue = SCALE)
        {
            for (int t = 0; t < timesAndCosts.Count; t++)
            {
                bool hasSubtasks = random.Next(0, 100) < subtaskProbability;
                int subtaskCount = hasSubtasks ? random.Next(minSubtasks, maxSubtasks + 1) : 1;

                for (int h = 0; h < wkrs.Count; h++)
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

                        timesAndCosts[t][h] = new TimeAndCost(times, costs);
                    }
                    else
                    {
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

        public List<int> GetTimes(int taskId, Worker wkr)
        {
            if (taskId >= timesAndCosts.Count || wkr.GetID() >= timesAndCosts[taskId].Count)
                return new List<int>();
            return timesAndCosts[taskId][wkr.GetID()].Times;
        }

        public List<int> GetCosts(int taskId, Worker wkr)
        {
            if (taskId >= timesAndCosts.Count || wkr.GetID() >= timesAndCosts[taskId].Count)
                return new List<int>();
            return timesAndCosts[taskId][wkr.GetID()].Costs;
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
        private List<Worker> workers = new List<Worker>();
        public Times times = new Times();
        private Graf taskGraph = new Graf();
        private Dictionary<int, int> workerAssignmentsCount = new Dictionary<int, int>(); 
        private List<Assignment> Assignments = new List<Assignment>();
        private Dictionary<int, Assignment> taskAssignmentMap = new Dictionary<int, Assignment>();
        private Dictionary<int, Tuple<int, int>> taskSchedule = new Dictionary<int, Tuple<int, int>>();
        private List<Worker> workerToTasks = new List<Worker>();
        private int totalCost = 0;
        private const int INF = 2000000000;
        private const int SCALE = 100;
        private static Random random = new Random();

        private int jeepAmount;
        private int tasksAmount;
        private int withCost;

        public Times GetTimes() => times;

        public List<Assignment> GetAssignments()
        {
            return Assignments;
        }

        public CostList()
        {
            times = new Times();
            taskGraph = new Graf();
            workers = new List<Worker>();
            Assignments = new List<Assignment>();
            taskAssignmentMap = new Dictionary<int, Assignment>();
            taskSchedule = new Dictionary<int, Tuple<int, int>>();

            totalCost = 0;
            tasksAmount = 0;
        }

        public CostList(int tasks, int jeeps, int maxTasks)
        {
            tasksAmount = tasks;
            times = new Times(tasks);
            taskGraph = new Graf(tasks, maxTasks);
            workers = new List<Worker>();
            Assignments = new List<Assignment>();
            taskAssignmentMap = new Dictionary<int, Assignment>();
            taskSchedule = new Dictionary<int, Tuple<int, int>>();
            jeepAmount = jeeps;
            totalCost = 0;
            withCost = 1;
        }

        public void AddRandomCompoundTask(int minSubtasks = 2, int maxSubtasks = 5)
        {

            if (times == null || times.TimeAndCosts == null || times.TimeAndCosts.Count == 0 || workers == null || workers.Count == 0)
                return;

            int numSubtasks = random.Next(minSubtasks, maxSubtasks + 1);

            var picked = new List<(int taskIdx, int subIdx)>();

            for (int i = 0; i < numSubtasks; i++)
            {
                int taskCount = times.TimeAndCosts.Count;
                int taskIdx = random.Next(taskCount);

                int subCount = times.TimeAndCosts[taskIdx][0].Times.Count;
                if (subCount == 0) subCount = 1;
                int subIdx = random.Next(subCount);

                picked.Add((taskIdx, subIdx));
            }

            var newTask = new List<TimeAndCost>();
            for (int w = 0; w < workers.Count; w++)
            {
                var subTimes = new List<int>();
                var subCosts = new List<int>();
                foreach (var (taskIdx, subIdx) in picked)
                {
                    var tc = times.TimeAndCosts[taskIdx][w];
                    int t = (tc.Times.Count > subIdx) ? tc.Times[subIdx] : (tc.Times.Count > 0 ? tc.Times[0] : 0);
                    int c = (tc.Costs.Count > subIdx) ? tc.Costs[subIdx] : (tc.Costs.Count > 0 ? tc.Costs[0] : 0);
                    subTimes.Add(t);
                    subCosts.Add(c);
                }
                newTask.Add(new TimeAndCost(subTimes, subCosts));
            }

            times.TimeAndCosts.Add(newTask);

            tasksAmount += 1;

            taskGraph.GetAdjList().Add(new List<Edge>());

        }



        public void AssignEachToFastestWorker()
        {
            if (taskGraph == null || workers == null || workers.Count == 0)
            {
                Console.Error.WriteLine("Invalid task graph or worker list.");
                return;
            }

            Assignments.Clear();
            taskAssignmentMap.Clear();
            workerAssignmentsCount.Clear();
            taskSchedule.Clear();

            Dictionary<Worker, Assignment> workerAssignments = new Dictionary<Worker, Assignment>();
            int estimatedCost = 0;
            int estimatedTime = 0;
            int numAllocated = 0;

            for (int i = 0; i < taskGraph.GetVerticesSize(); i++)
            {
                Worker fastestWorker = null;
                int minTime = int.MaxValue;

                foreach (var wkr in workers)
                {
                    var allTaskTimes = times.GetTimes(i, wkr) ?? new List<int>();
                    int taskTime = allTaskTimes.Count > 1 ? allTaskTimes.Max() : allTaskTimes.FirstOrDefault();
                    if (taskTime < minTime)
                    {
                        minTime = taskTime;
                        fastestWorker = wkr;
                    }
                }

                if (fastestWorker == null)
                {
                    Console.Error.WriteLine($"No suitable worker found for task {i}.");
                    continue;
                }

                if (!workerAssignments.TryGetValue(fastestWorker, out var inst))
                {
                    int instId = workerAssignmentsCount.ContainsKey(fastestWorker.GetID()) ? workerAssignmentsCount[fastestWorker.GetID()] : 0;
                    inst = new Assignment(instId, fastestWorker);
                    Assignments.Add(inst);
                    workerAssignments[fastestWorker] = inst;
                    workerAssignmentsCount[fastestWorker.GetID()] = instId + 1;
                }

                var taskTimes = times.GetTimes(i, fastestWorker) ?? new List<int>();
                var taskCosts = times.GetCosts(i, fastestWorker) ?? new List<int>();

                if (taskTimes.Count > 1)
                {
                    estimatedTime += taskTimes.Max();
                    estimatedCost += taskCosts.Sum();
                }
                else
                {
                    estimatedTime += taskTimes.FirstOrDefault();
                    estimatedCost += taskCosts.FirstOrDefault();
                }

                inst.AddTask(i);
                taskAssignmentMap[i] = inst;
                numAllocated++;
            }

            totalCost = estimatedCost;
            Console.WriteLine($"Assigned {numAllocated} tasks to their fastest worker. Estimated time: {estimatedTime}, Estimated cost: {estimatedCost}");
        }

        public void AssignToCheapestWorker()
        {
            {
                if (taskGraph == null || workers == null || workers.Count == 0)
                {
                    Console.Error.WriteLine("Invalid task graph or worker list.");
                    return;
                }

                
                Assignments.Clear();
                taskAssignmentMap.Clear();
                workerAssignmentsCount.Clear();
                taskSchedule.Clear();

             
                Dictionary<int, Assignment> workerAssignments = new Dictionary<int, Assignment>();

                for (int i = 0; i < taskGraph.GetVerticesSize(); i++)
                {
                    Worker cheapestWorker = null;
                    int minCost = int.MaxValue;

                    foreach (var wkr in workers)
                    {
                        var taskCosts = times.GetCosts(i, wkr) ?? new List<int>();
                        int cost = taskCosts.Count > 1 ? taskCosts.Sum() : (taskCosts.Count == 1 ? taskCosts[0] : int.MaxValue);
                        if (cost < minCost)
                        {
                            minCost = cost;
                            cheapestWorker = wkr;
                        }
                    }

                    if (cheapestWorker != null)
                    {
                        Assignment inst;
                        if (!workerAssignments.TryGetValue(cheapestWorker.GetID(), out inst))
                        {
                            int instId = workerAssignmentsCount.ContainsKey(cheapestWorker.GetID()) ? workerAssignmentsCount[cheapestWorker.GetID()] : 0;
                            inst = new Assignment(instId, cheapestWorker);
                            Assignments.Add(inst);
                            workerAssignments[cheapestWorker.GetID()] = inst;
                            workerAssignmentsCount[cheapestWorker.GetID()] = instId + 1;
                        }
                        inst.AddTask(i);
                        taskAssignmentMap[i] = inst;
                    }
                    else
                    {
                        Console.Error.WriteLine($"No suitable worker found for task {i}.");
                    }
                }
            }
        }

        public int GetAssignmentStartingTime(Assignment inst)
        {
            int startingTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                if (GetStartingTime(i) > startingTime) // Ensure parentheses
                    startingTime = GetStartingTime(i);
            }
            return startingTime;
        }

        public int GetAssignmentEndingTime(Assignment inst)
        {
            int endingTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                if (GetEndingTime(i) > endingTime) // Ensure parentheses
                    endingTime = GetEndingTime(i);
            }
            return endingTime;
        }

        public int GetTimeRunning(Assignment inst)
        {
            int totalTime = 0;
            foreach (int i in inst.GetTaskSet())
            {
                totalTime += GetEndingTime(i) - GetStartingTime(i);
            }
            return totalTime;
        }

        public int GetIdleTime(Assignment inst, int timeStop)
        {
            return timeStop - GetTimeRunning(inst);
        }

        public Assignment GetLongestRunningAssignment()
        {
            int longestRunning = int.MinValue;
            Assignment longest = null;

            foreach (var inst in Assignments)
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

        public Assignment GetShortestRunningAssignment()
        {
            int shortestRunning = int.MaxValue;
            Assignment shortest = null;

            foreach (var inst in Assignments)
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

        public List<Worker> GetWorkers() => workers;
        public Graf GetGraph() => taskGraph;

        public void Clear()
        {
            workers.Clear();
            times = new Times();
            taskGraph = new Graf();
            workerAssignmentsCount.Clear();
            Assignments.Clear();
            taskAssignmentMap.Clear();
            workerToTasks.Clear();
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
                    int tasksAmount = 0;
                    int procCount = 0;
                    var timesMatrix = new List<List<string>>();
                    var costsMatrix = new List<List<string>>();
                    var loaded = new Graf();

                    while ((line = file.ReadLine()) != null)
                    {
                        lineNumber++;
                        line = line.Trim();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                        if (line.StartsWith("@tasks"))
                        {
                            section = 0;
                            var match = Regex.Match(line, @"@tasks\s+(\d+)");
                            if (match.Success)
                                tasksAmount = int.Parse(match.Groups[1].Value);
                            loaded = new Graf(tasksAmount, 0);
                            continue;
                        }
                        if (line.StartsWith("@proc"))
                        {
                            section = 1;
                            var match = Regex.Match(line, @"@proc\s+(\d+)");
                            if (match.Success)
                                procCount = int.Parse(match.Groups[1].Value);
                            workers.Clear();
                            for (int i = 0; i < procCount; i++)
                                workers.Add(new Worker(i));
                            continue;
                        }
                        if (line.StartsWith("@times")) { section = 2; continue; }
                        if (line.StartsWith("@cost")) { section = 3; continue; }

                        switch (section)
                        {
                            case 2:
                                timesMatrix.Add(ParseMatrixLine(line));
                                break;
                            case 3:
                                costsMatrix.Add(ParseMatrixLine(line));
                                break;
                                // Ignore anything else
                        }
                    }

                    if (timesMatrix.Count != tasksAmount || costsMatrix.Count != tasksAmount)
                    {
                        errorMessage = "Liczba wierszy w @times/@cost nie zgadza się z liczbą zadań.";
                        return -1;
                    }
                    if (timesMatrix[0].Count != procCount || costsMatrix[0].Count != procCount)
                    {
                        errorMessage = "Liczba kolumn (wykonawców) nie zgadza się z liczbą w @proc.";
                        return -1;
                    }

                    times = new Times(tasksAmount);
                    times.LoadWkr(workers);
                    times.SetTimesMatrix(timesMatrix);
                    times.SetCostsMatrix(costsMatrix);

                    taskGraph = loaded;
                    this.tasksAmount = tasksAmount;

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

            int minTasks = 3, maxTasks = 10;
            int minWorkers = 2, maxWorkers = 5;
            int minSubtasks = 2, maxSubtasks = 5;
            int minTime = 5, maxTime = 40;
            int minCost = 1, maxCost = 100;

            var random = new Random();

            tasksAmount = random.Next(minTasks, maxTasks + 1);
            jeepAmount = random.Next(minWorkers, maxWorkers + 1);

            workers = new List<Worker>();
            for (int i = 0; i < jeepAmount; i++)
                workers.Add(new Worker(i));

            taskGraph = new Graf(tasksAmount, maxSubtasks);

            times = new Times(tasksAmount);
            times.LoadWkr(workers);

            times.SetRandomTimesAndCosts(
                subtaskProbability: 100,
                minSubtasks: minSubtasks,
                maxSubtasks: maxSubtasks,
                minValue: minTime,
                maxValue: maxTime
            );
        }


        public void PrintTasks(TextWriter writer = null)
        {
            var output = writer ?? Console.Out;
            output.WriteLine($"@tasks {tasksAmount}");
        }

        public void PrintProc(TextWriter writer = null)
        {
            var output = writer ?? Console.Out;
            output.WriteLine($"@proc {workers.Count}");
        }


        public void PrintAssignments()
        {
            Assignments.Sort();
            Console.WriteLine($"Created {Assignments.Count} components");

            int criticalTime = 0;
            if (taskAssignmentMap != null)
            {
                foreach (var pair in taskAssignmentMap)
                {
                    int taskId = pair.Key;
                    int endingTime = GetEndingTime(taskId);
                    if (endingTime > criticalTime)
                    {
                        criticalTime = endingTime;
                    }
                }
            }

            foreach (var inst in Assignments ?? Enumerable.Empty<Assignment>())
            {
                int expTime = 0;
                int expCost = 0;
                var taskIds = inst.GetTaskSet()?.ToList() ?? new List<int>();

                foreach (int taskID in taskIds)
                {
                    var taskTimes = this.times.GetTimes(taskID, inst.GetWorkerPtr()) ?? new List<int>();
                    var taskCosts = this.times.GetCosts(taskID, inst.GetWorkerPtr()) ?? new List<int>();

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

                expCost += inst.GetWorkerPtr()?.GetCost() ?? 0;
                Console.WriteLine($"{inst} Tasks: {taskIds.Count} Expected time: {expTime} Idle time: {GetIdleTime(inst, criticalTime)} Cost: {expCost} Including initial: {inst.GetWorkerPtr()?.GetCost() ?? 0}");
                foreach (int taskID in taskIds)
                {
                    var taskTimes = this.times.GetTimes(taskID, inst.GetWorkerPtr()) ?? new List<int>();
                    var taskCosts = this.times.GetCosts(taskID, inst.GetWorkerPtr()) ?? new List<int>();

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
                    var assignment = GetAssignment(pair.Key);
                    // Use ternary operator to handle null assignment
                    Console.WriteLine($"T{pair.Key}\ton {(assignment != null ? assignment.ToString() : "None")} from: {pair.Value.Item1} to: {pair.Value.Item2}");
                }
            }

            Console.WriteLine($"Critical path time: {criticalTime}");
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

            for (int i = 0; i < jeepAmount; i++)
            {
                workers.Add(new Worker(workers.Count));
            }

            return 1;
        }



        public Worker GetLowestTimeWorker(int taskId, int timeCost) 
        {
            int minValue = INF;
            Worker minWorker = null;

            foreach (var wkr in workers)
            {
                var values = (timeCost == 0) ? times.GetTimes(taskId, wkr) : times.GetCosts(taskId, wkr);
                // For subtasks, use max time (parallel execution) or sum of costs
                int value = values.Count > 1 ? (timeCost == 0 ? values.Max() : values.Sum()) : values.FirstOrDefault();
                if (value < minValue)
                {
                    minValue = value;
                    minWorker = wkr;
                }
            }

            return minWorker;
        }

        public void createAssignment(int taskId, Worker worker)
        {
            int instId = workerAssignmentsCount.ContainsKey(worker.GetID()) ? workerAssignmentsCount[worker.GetID()] : 0;
            var inst = new Assignment(instId, worker);
            inst.AddTask(taskId);
            Assignments.Add(inst);
            taskAssignmentMap[taskId] = inst;
            workerAssignmentsCount[worker.GetID()] = instId + 1;
        }

        public void addTaskToAssignment(int taskId, Assignment inst)
        {
            inst.AddTask(taskId);
            taskAssignmentMap[taskId] = inst;
        }

        public void removeTaskFromAssignment(int taskId)
        {
            Assignment oldInst = taskAssignmentMap[taskId];
            oldInst.RemoveTask(taskId);
            if(oldInst.GetTaskSet().Count == 0)
            {
                Assignments.Remove(oldInst);
            }
            else
            {
                taskAssignmentMap[taskId] = oldInst;
            }
            taskAssignmentMap.Remove(taskId);
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

        public Assignment GetAssignment(int taskId)
        {
            if (taskAssignmentMap.ContainsKey(taskId))
            {
                return taskAssignmentMap[taskId];
            }
            return null;
        }

        public void taskDistribution(int rule) // narazie lepiej zostawić
        {
            if (taskGraph == null || workers == null || workers.Count == 0)
            {
                Console.Error.WriteLine("Invalid task graph or worker list.");
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
                var taskTimesList = this.times.GetTimes(i, workers[0]) ?? new List<int>();

                if (taskTimesList.Count > 1) // Subtasks: allocate each subtask to best worker
                {
                    for (int s = 0; s < taskTimesList.Count; s++)
                    {
                        Worker bestWkr = null;
                        int minValue = INF;

                        foreach (var wkr in workers)
                        {
                            var wkrTimes = this.times.GetTimes(i, wkr) ?? new List<int>();
                            var wkrCosts = this.times.GetCosts(i, wkr) ?? new List<int>();

                            if (s >= wkrTimes.Count || s >= wkrCosts.Count)
                                continue;

                            int value = (rule == 0) ? wkrTimes[s] : wkrCosts[s];
                            if (value < minValue)
                            {
                                minValue = value;
                                bestWkr = wkr;
                            }
                        }

                        if (bestWkr != null)
                        {
                            var bestWkreCosts = this.times.GetCosts(i, bestWkr) ?? new List<int>();
                            var bestWkrTimes = this.times.GetTimes(i, bestWkr) ?? new List<int>();

                            if (s < bestWkreCosts.Count && s < bestWkrTimes.Count)
                            {
                                estimatedCost += bestWkreCosts[s];
                                estimatedTime += bestWkrTimes[s];
                                totalCost += bestWkreCosts[s];

                                createAssignment(i, bestWkr);
                                addTaskToAssignment(i, Assignments[Assignments.Count - 1]);
                                if (allocatedTasks[i] == 0) // Count task only once
                                {
                                    allocatedTasks[i] = 1; // Mark task as allocated
                                    numAllocated++;
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"No suitable worker found for task {i}, subtask {s}.");
                        }
                    }
                }
                else // Single task: allocate to best worker
                {
                    var wkr = GetLowestTimeWorker(i, rule);
                    if (wkr != null)
                    {
                        var taskCosts = this.times.GetCosts(i, wkr) ?? new List<int>();
                        var taskTimes = this.times.GetTimes(i, wkr) ?? new List<int>();

                        estimatedCost += taskCosts.Count > 0 ? taskCosts[0] : 0;
                        estimatedTime += taskTimes.Count > 0 ? taskTimes[0] : 0;
                        totalCost += taskCosts.Count > 0 ? taskCosts[0] : 0;

                        createAssignment(i, wkr);
                        allocatedTasks[i] = 1; // Mark task as allocated
                        numAllocated++;
                    }
                    else
                    {
                        Console.Error.WriteLine($"No suitable worker found for task {i}.");
                    }
                }
            }

            Console.WriteLine($"Allocated {numAllocated} tasks. Estimated time: {estimatedTime}, Estimated cost: {estimatedCost}");
        }


    }
}
