using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Tasks;

namespace TaskGraphWPF
{
    public partial class MainWindow : Window
    {
        private CostList costList;
        private readonly Random rand = new Random();
        private Dictionary<int, Point> nodePositions;

        public MainWindow()
        {
            InitializeComponent();
            costList = new CostList();
            nodePositions = new Dictionary<int, Point>();
            InitializeToolTips();
        }

        private void InitializeToolTips()
        {
            LoadButton.ToolTip = "Load a task graph from a text file.";
            GenerateButton.ToolTip = "Generate a random task graph.";

            SaveButton.ToolTip = "Save the current task graph to a file.";
            AssignFastestButton.ToolTip = "Assign each task to the worker that executes it the fastest.";
            AssignCheapestButton.ToolTip = "Assign all tasks to the cheapest worker.";
            ShowAssignmentsButton.ToolTip = "Display which worker executes which tasks.";
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string error;
                    int result = costList.LoadFromFile(openFileDialog.FileName, out error);

                    if (result == 1)
                    {
                        await UpdateUIAsync();
                        MessageBox.Show("File loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to parse the file.\n{error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                costList.CreateRandomTasksGraph();
                await UpdateUIAsync();
                MessageBox.Show("Random task graph generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating graph: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (costList.GetGraph() == null || costList.GetGraph().GetVerticesSize() == 0)
                {
                    MessageBox.Show("No task graph available. Please generate or load a graph first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetWorkers() == null || !costList.GetWorkers().Any())
                {
                    MessageBox.Show("No worker resources available. Please generate or load a configuration with worker resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.taskDistribution(0);
                });
                await UpdateUIAsync();
                MessageBox.Show("Simulation completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running simulation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AssignFastestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (costList.GetGraph() == null || costList.GetGraph().GetVerticesSize() == 0)
                {
                    MessageBox.Show("No task graph available. Please generate or load a graph first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetWorkers() == null || !costList.GetWorkers().Any())
                {
                    MessageBox.Show("No worker resources available. Please generate or load a configuration with worker resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.AssignEachToFastestWorker();
                });
                await UpdateUIAsync();
                MessageBox.Show("Each task assigned to its fastest worker.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ButtonAddCompoundTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                costList.AddRandomCompoundTask();
                MessageBox.Show("A new compound task has been added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await UpdateUIAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding a new compound task:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private async void AssignCheapestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (costList.GetGraph() == null || costList.GetGraph().GetVerticesSize() == 0)
                {
                    MessageBox.Show("No task graph available. Please generate or load a graph first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetWorkers() == null || !costList.GetWorkers().Any())
                {
                    MessageBox.Show("No worker resources available. Please generate or load a configuration with worker resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.AssignToCheapestWorker();
                });
                await UpdateUIAsync();
                MessageBox.Show("All tasks assigned to the cheapest worker.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAssignmentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (costList.GetGraph() == null || costList.GetGraph().GetVerticesSize() == 0)
                {
                    MessageBox.Show("No task graph available. Please generate or load a graph first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetWorkers() == null || !costList.GetWorkers().Any())
                {
                    MessageBox.Show("No worker resources available. Please generate or load a configuration with worker resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetAssignment(0) == null)
                {
                    MessageBox.Show("No tasks assigned. Please run simulation or assign tasks first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var assignmentWindow = new TaskAssignmentWindow(costList);
                assignmentWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing assignments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (costList.GetGraph() == null || costList.GetGraph().GetVerticesSize() == 0)
            {
                MessageBox.Show("No task graph available to save. Please generate or load a graph first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "task_graph.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    costList.PrintALL(saveFileDialog.FileName, false);
                    MessageBox.Show("File saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task UpdateUIAsync()
        {
            UpdateWorkerAndCOMs();
            UpdateGraph();
            await UpdateScheduleAsync();
        }

        private void UpdateWorkerAndCOMs()
        {
            WorkerListBox.Items.Clear();
            var workers = costList.GetWorkers();
            if (workers == null || !workers.Any())
            {
                WorkerListBox.Items.Add("No worker available.");
                return;
            }
            foreach (var wkr in workers)
            {
                WorkerListBox.Items.Add($"{wkr} (Cost: {wkr.GetCost()})");
            }

            COMListBox.Items.Clear();
        }

        private void UpdateGraph()
        {
            GraphCanvas.Children.Clear();
            var graph = costList.GetGraph();
            if (graph == null || graph.GetVerticesSize() == 0)
            {
                return;
            }

            int vertexCount = graph.GetVerticesSize();
            nodePositions.Clear();

            double centerX = GraphCanvas.ActualWidth / 2;
            double centerY = GraphCanvas.ActualHeight / 2;
            double radius = Math.Min(GraphCanvas.ActualWidth, GraphCanvas.ActualHeight) / 3;

            for (int i = 0; i < vertexCount; i++)
            {
                double angle = 2 * Math.PI * i / vertexCount;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                nodePositions[i] = new Point(x, y);

                var worker = costList.GetWorkers().FirstOrDefault();
                bool hasSubtasks = worker != null &&
                    costList.GetTimes().GetTimes(i, worker)?.Count > 1;

                var times = worker != null ?
                    costList.GetTimes().GetTimes(i, worker) ?? new List<int>() :
                    new List<int>();

                var costs = worker != null ?
                    costList.GetTimes().GetCosts(i, worker) ?? new List<int>() :
                    new List<int>();

                string tooltip = hasSubtasks
                    ? $"T{i} (Subtasks: Times=[{string.Join(", ", times)}], Costs=[{string.Join(", ", costs)}])"
                    : $"T{i} (Time: {times.FirstOrDefault()}, Cost: {costs.FirstOrDefault()})";

                var node = CreateNode(x, y, hasSubtasks, tooltip);
                GraphCanvas.Children.Add(node.Item1);
                GraphCanvas.Children.Add(node.Item2);
            }

            foreach (var edgeList in graph.GetAdjList())
            {
                foreach (var edge in edgeList)
                {
                    int v1 = edge.GetV1();
                    int v2 = edge.GetV2();
                    if (nodePositions.ContainsKey(v1) && nodePositions.ContainsKey(v2))
                    {
                        Point start = nodePositions[v1];
                        Point end = nodePositions[v2];
                        var line = CreateEdge(start, end, edge.GetWeight());
                        GraphCanvas.Children.Add(line.Item1);
                        GraphCanvas.Children.Add(line.Item2);
                    }
                }
            }
        }

        private (Ellipse, TextBlock) CreateNode(double x, double y, bool hasSubtasks, string tooltip)
        {
            var node = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = hasSubtasks ? Brushes.Red : Brushes.Blue,
                Stroke = Brushes.Black,
                ToolTip = tooltip
            };
            Canvas.SetLeft(node, x - 15);
            Canvas.SetTop(node, y - 15);

            var label = new TextBlock
            {
                Text = $"T{nodePositions.Count}",
                Foreground = Brushes.White,
                ToolTip = tooltip
            };
            Canvas.SetLeft(label, x - 10);
            Canvas.SetTop(label, y - 10);

            return (node, label);
        }

        private (Line, TextBlock) CreateEdge(Point start, Point end, int weight)
        {
            var line = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Red
            };
            Canvas.SetLeft(weightLabel, (start.X + end.X) / 2);
            Canvas.SetTop(weightLabel, (start.Y + end.Y) / 2);

            return (line, weightLabel);
        }

        private async Task UpdateScheduleAsync()
        {
            ScheduleTextBox.Text = "Updating schedule...\n";
            try
            {
                await Task.Run(() =>
                {
                    using (var writer = new StringWriter())
                    {
                        costList.PrintTasks(writer);
                        writer.WriteLine();
                        costList.PrintProc(writer);
                        writer.WriteLine();
                        costList.GetTimes().Show(writer);
                        writer.WriteLine();
                        costList.PrintAssignments();
                        Dispatcher.Invoke(() => ScheduleTextBox.Text = writer.ToString());
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => ScheduleTextBox.Text = $"Error updating schedule: {ex.Message}");
            }
        }
    }
}