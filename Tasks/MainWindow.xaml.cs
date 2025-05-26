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
        private bool isSpeedOrdered = false;
        private bool isPriceOrdered = false;
        private readonly Random rand = new Random();
        private Dictionary<int, Point> nodePositions;
        private Dictionary<Worker, Point> workerPositions;
        private Dictionary<string, Point> taskPositions;

        public MainWindow()
        {
            InitializeComponent();
            costList = new CostList();
            nodePositions = new Dictionary<int, Point>();
            InitializeToolTips();
        }

        private void InitializeToolTips()
        {
            LoadButton.ToolTip = "Wczytaj graf zadań z pliku tekstowego.";
            GenerateButton.ToolTip = "Wygeneruj losowy graf zadań.";
            AddCompoundTaskButton.ToolTip = "Dodaj nieprzewidziane zadanie";
            SaveButton.ToolTip = "Zapisz graf zadań do pliku tekstowego.";
            AssignFastestButton.ToolTip = "Przypisz wszystkie zadania do najszybszych pracowników.";
            AssignCheapestButton.ToolTip = "Przypisz wszystkie zadania do najtańszych pracowników.";
            
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating graph: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    isSpeedOrdered = true;
                    isPriceOrdered = false;
                });
                await UpdateUIAsync();
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
                    isPriceOrdered = true;
                    isSpeedOrdered = false;
                });
                await UpdateUIAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            UpdateTaskTable();
            if(isPriceOrdered)
                await Task.Run(() =>
                {
                    costList.AssignToCheapestWorker();
                });
            else if(isSpeedOrdered)
                await Task.Run(() =>
                {
                    costList.AssignEachToFastestWorker();
                });
            CreateTaskAssignment();
            await UpdateScheduleAsync();
        }

        private void UpdateWorkerAndCOMs()
        {
            var workers = costList.GetWorkers();
            //WorkerCount.Text = "Ilość pracowników: " + (workers?.Count ?? 0);
        }

        private void UpdateTaskTable()
        {
            var graph = costList.GetGraph();
            var workers = costList.GetWorkers();

            if (graph == null || graph.GetVerticesSize() == 0 || workers == null || workers.Count == 0)
            {
                TaskGrid.ItemsSource = null;
                return;
            }

            var displayItems = new List<TaskDisplayItem>();

            foreach (var worker in workers)
            {
                for (int i = 0; i < graph.GetVerticesSize(); i++)
                {
                    var times = costList.GetTimes().GetTimes(i, worker);
                    var costs = costList.GetTimes().GetCosts(i, worker);

                    if (times == null || costs == null || times.Count == 0)
                        continue;

                    // Dodaj główne zadanie
                    displayItems.Add(new TaskDisplayItem
                    {
                        TaskId = $"T{i}",
                        Time = times[0],
                        Cost = costs.ElementAtOrDefault(0),
                        Worker = "Pracownik " + worker.GetID()
                    });

                    // Dodaj podzadania
                    for (int j = 1; j < times.Count; j++)
                    {
                        displayItems.Add(new TaskDisplayItem
                        {
                            TaskId = $"T{i}_{j}",
                            Time = times[j],
                            Cost = costs.ElementAtOrDefault(j),
                            Worker = "Pracownik " + worker.GetID()
                        });
                    }
                }
            }

            // Sortuj po numerze głównego zadania, potem po nazwie TaskId
            var sortedItems = displayItems
                .OrderBy(item => GetMainTaskIndex(item.TaskId))
                .ThenBy(item => item.TaskId)
                .ToList();

            TaskGrid.ItemsSource = sortedItems;
        }

        private int GetMainTaskIndex(string taskId)
        {
            // Wydobywa numer głównego zadania z "T0" albo "T0_1" => 0
            var match = System.Text.RegularExpressions.Regex.Match(taskId, @"T(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                return index;
            }
            return int.MaxValue; // Na wypadek błędnego formatu
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

        public void CreateTaskAssignment()
        {
            workerPositions = new Dictionary<Worker, Point>();
            taskPositions = new Dictionary<string, Point>();
            AssignmentCanvas.UpdateLayout();
            DrawAssignmentGraph();
            Console.WriteLine("TaskAssignmentWindow initialized.");
        }

        private void DrawAssignmentGraph()
        {
            Console.WriteLine("Starting DrawAssignmentGraph...");
            AssignmentCanvas.Children.Clear();
            var workers = costList.GetWorkers();
            var instances = costList.GetAssignments();
            if (workers == null || !workers.Any() || instances == null || !instances.Any())
            {
                return;
            }

            double canvasWidth = AssignmentCanvas.ActualWidth > 0 ? AssignmentCanvas.ActualWidth : 800;
            double canvasHeight = AssignmentCanvas.ActualHeight > 0 ? AssignmentCanvas.ActualHeight : 600;
            double wkrSpacing = canvasWidth / (workers.Count + 1);
            double taskSpacing = 60;
            double wkrY = 50;
            double nodeSize = 40;

            Console.WriteLine($"Drawing {workers.Count} worker nodes...");
            int wkrIndex = 0;
            foreach (var wkr in workers.OrderBy(h => h.GetType()).ThenBy(h => h.GetID()))
            {
                double x = (wkrIndex + 1) * wkrSpacing;
                workerPositions[wkr] = new Point(x, wkrY);

                var node = new Rectangle
                {
                    Width = nodeSize,
                    Height = nodeSize,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Black,
                    ToolTip = $"{wkr} (Cost: {wkr.GetCost()})",
                    
                };
                Canvas.SetLeft(node, x - nodeSize / 2);
                Canvas.SetTop(node, wkrY - nodeSize / 2);
                AssignmentCanvas.Children.Add(node);

                var label = new TextBlock
                {
                    Text = wkr.GetID().ToString(),
                    Foreground = Brushes.White,
                    ToolTip = node.ToolTip
                };
                Canvas.SetLeft(label, x - 5);
                Canvas.SetTop(label, wkrY - 10);
                AssignmentCanvas.Children.Add(label);

                wkrIndex++;
            }

            Console.WriteLine($"Drawing tasks for {instances.Count} Assignments...");
            foreach (var instance in instances.OrderBy(i => i.GetWorkerPtr().GetType()).ThenBy(i => i.GetWorkerPtr().GetID()))
            {
                var wkr = instance.GetWorkerPtr();
                if (!workerPositions.ContainsKey(wkr))
                {
                    Console.WriteLine($"Worker {wkr} not found in positions.");
                    continue;
                }

                var tasks = instance.GetTaskSet().ToList();
                double wkrX = workerPositions[wkr].X;
                double currentY = wkrY + nodeSize + 50;

                foreach (int taskId in tasks)
                {
                    var times = costList.GetTimes().GetTimes(taskId, wkr) ?? new List<int>();
                    var costs = costList.GetTimes().GetCosts(taskId, wkr) ?? new List<int>();
                    bool hasSubtasks = times.Count > 1;

                    if (hasSubtasks)
                    {
                        // Draw a node for each subtask
                        for (int subtaskIndex = 0; subtaskIndex < times.Count; subtaskIndex++)
                        {
                            string subtaskLabel = $"T{taskId}_{subtaskIndex}";
                            double taskY = currentY;
                            taskPositions[subtaskLabel] = new Point(wkrX, taskY);

                            string tooltip = $"Subtask {subtaskLabel} (Time: {times[subtaskIndex]}, Cost: {costs[subtaskIndex]})";

                            var node = new Ellipse
                            {
                                Width = nodeSize,
                                Height = nodeSize,
                                Fill = Brushes.Red,
                                Stroke = Brushes.Black,
                                ToolTip = tooltip
                            };
                            Canvas.SetLeft(node, wkrX - nodeSize / 2);
                            Canvas.SetTop(node, taskY - nodeSize / 2);
                            AssignmentCanvas.Children.Add(node);

                            var label = new TextBlock
                            {
                                Text = subtaskLabel,
                                Foreground = Brushes.White,
                                ToolTip = tooltip
                            };
                            Canvas.SetLeft(label, wkrX - 13);
                            Canvas.SetTop(label, taskY - 10);
                            AssignmentCanvas.Children.Add(label);

                            var line = new Line
                            {
                                X1 = wkrX,
                                Y1 = wkrY + nodeSize / 2,
                                X2 = wkrX,
                                Y2 = taskY - nodeSize / 2,
                                Stroke = Brushes.Black,
                                StrokeThickness = 2
                            };
                            AssignmentCanvas.Children.Insert(0, line);

                            currentY += taskSpacing;
                        }
                    }
                    else
                    {
                        // Draw a single task node
                        string taskLabel = $"T{taskId}";
                        double taskY = currentY;
                        taskPositions[taskLabel] = new Point(wkrX, taskY);

                        string tooltip = $"Task {taskLabel} (Time: {times.FirstOrDefault()}, Cost: {costs.FirstOrDefault()})";

                        var node = new Ellipse
                        {
                            Width = nodeSize,
                            Height = nodeSize,
                            Fill = Brushes.Blue,
                            Stroke = Brushes.Black,
                            ToolTip = tooltip
                        };
                        Canvas.SetLeft(node, wkrX - nodeSize / 2);
                        Canvas.SetTop(node, taskY - nodeSize / 2);
                        AssignmentCanvas.Children.Add(node);

                        var label = new TextBlock
                        {
                            Text = taskLabel,
                            Foreground = Brushes.White,
                            ToolTip = tooltip
                        };
                        Canvas.SetLeft(label, wkrX - 5);
                        Canvas.SetTop(label, taskY - 10);
                        AssignmentCanvas.Children.Add(label);

                        var line = new Line
                        {
                            X1 = wkrX,
                            Y1 = wkrY + nodeSize / 2,
                            X2 = wkrX,
                            Y2 = taskY - nodeSize / 2,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2
                        };
                        AssignmentCanvas.Children.Insert(0, line);

                        currentY += taskSpacing;
                    }
                }
            }

            double extraMargin = 50;

            double maxY = 0;
            foreach (UIElement child in AssignmentCanvas.Children)
            {
                double childBottom = Canvas.GetTop(child) + ((FrameworkElement)child).ActualHeight;
                if (childBottom > maxY) maxY = childBottom;
            }

            double maxX = 0;
            foreach (UIElement child in AssignmentCanvas.Children)
            {
                double childRight = Canvas.GetLeft(child) + ((FrameworkElement)child).ActualWidth;
                if (childRight > maxX) maxX = childRight;
            }

            AssignmentCanvas.Width = Math.Max(maxX, AssignmentBorder.ActualWidth);
            AssignmentCanvas.Height = Math.Max(maxY + extraMargin, AssignmentBorder.ActualHeight);


            Console.WriteLine("DrawAssignmentGraph completed.");
        }

        private class TaskDisplayItem
        {
            public string TaskId { get; set; }
            public int Time { get; set; }
            public int Cost { get; set; }
            public string Worker { get; set; }
        }
    }
}