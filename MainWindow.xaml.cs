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
            RunButton.ToolTip = "Run the task scheduling simulation.";
            SaveButton.ToolTip = "Save the current task graph to a file.";
            AssignFastestButton.ToolTip = "Assign each task to the hardware that executes it the fastest.";
            AssignCheapestButton.ToolTip = "Assign all tasks to the cheapest hardware.";
            ShowAssignmentsButton.ToolTip = "Display which hardware executes which tasks.";
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
                if (!int.TryParse(TasksTextBox.Text, out int tasks) || tasks < 1 || tasks > 100)
                {
                    MessageBox.Show("Invalid number of tasks. Enter a number between 1 and 100.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(HCTextBox.Text, out int hcs) || hcs < 0 || hcs > 10)
                {
                    MessageBox.Show("Invalid number of HCs. Enter a number between 0 and 10.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(PUTextBox.Text, out int pus) || pus < 0 || pus > 10)
                {
                    MessageBox.Show("Invalid number of PUs. Enter a number between 0 and 10.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(COMTextBox.Text, out int coms) || coms < 0 || coms > 10)
                {
                    MessageBox.Show("Invalid number of COMs. Enter a number between 0 and 10.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(MaxTasksTextBox.Text, out int maxTasks) || maxTasks < 1 || maxTasks > 5)
                {
                    MessageBox.Show("Invalid max tasks per instance. Enter a number between 1 and 5.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(SubtaskProbTextBox.Text, out int subtaskProb) || subtaskProb < 0 || subtaskProb > 100)
                {
                    MessageBox.Show("Invalid subtask probability. Enter a percentage between 0 and 100.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                costList = new CostList(tasks, hcs, pus, coms, maxTasks);
                costList.RandALL();

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

                if (costList.GetHardwares() == null || !costList.GetHardwares().Any())
                {
                    MessageBox.Show("No hardware resources available. Please generate or load a configuration with hardware resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.taskDistribution(0);
                    costList.RunTasks();
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

                if (costList.GetHardwares() == null || !costList.GetHardwares().Any())
                {
                    MessageBox.Show("No hardware resources available. Please generate or load a configuration with hardware resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.AssignEachToFastestHardware();
                });
                await UpdateUIAsync();
                MessageBox.Show("Each task assigned to its fastest hardware.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (costList.GetHardwares() == null || !costList.GetHardwares().Any())
                {
                    MessageBox.Show("No hardware resources available. Please generate or load a configuration with hardware resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await Task.Run(() =>
                {
                    costList.AssignToCheapestHardware();
                });
                await UpdateUIAsync();
                MessageBox.Show("All tasks assigned to the cheapest hardware.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

                if (costList.GetHardwares() == null || !costList.GetHardwares().Any())
                {
                    MessageBox.Show("No hardware resources available. Please generate or load a configuration with hardware resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (costList.GetInstance(0) == null)
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
            UpdateHardwareAndCOMs();
            UpdateGraph();
            await UpdateScheduleAsync();
        }

        private void UpdateHardwareAndCOMs()
        {
            HardwareListBox.Items.Clear();
            var hardwares = costList.GetHardwares();
            if (hardwares == null || !hardwares.Any())
            {
                HardwareListBox.Items.Add("No hardware available.");
                return;
            }
            foreach (var hw in hardwares)
            {
                HardwareListBox.Items.Add($"{hw} (Cost: {hw.GetCost()})");
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

                var hardware = costList.GetHardwares().FirstOrDefault();
                bool hasSubtasks = hardware != null &&
                    costList.GetTimes().GetTimes(i, hardware)?.Count > 1;

                var times = hardware != null ?
                    costList.GetTimes().GetTimes(i, hardware) ?? new List<int>() :
                    new List<int>();

                var costs = hardware != null ?
                    costList.GetTimes().GetCosts(i, hardware) ?? new List<int>() :
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
                        costList.PrintInstances();
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