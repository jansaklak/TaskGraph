using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Tasks;

namespace TaskGraphWPF
{
    public partial class TaskAssignmentWindow : Window
    {
        private readonly CostList costList;
        private Dictionary<Worker, Point> workerPositions;
        private Dictionary<string, Point> taskPositions;

        public TaskAssignmentWindow(CostList costList)
        {
            InitializeComponent();
            this.costList = costList ?? throw new ArgumentNullException(nameof(costList));
            workerPositions = new Dictionary<Worker, Point>();
            taskPositions = new Dictionary<string, Point>();
            Loaded += TaskAssignmentWindow_Loaded;
            Console.WriteLine("TaskAssignmentWindow initialized.");
        }

        private void TaskAssignmentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("TaskAssignmentWindow Loaded. Forcing layout update...");
            AssignmentCanvas.UpdateLayout();
            DrawAssignmentGraph();
        }

        private void DrawAssignmentGraph()
        {
            Console.WriteLine("Starting DrawAssignmentGraph...");
            AssignmentCanvas.Children.Clear();
            var workers = costList.GetWorkers();
            var instances = costList.GetAssignments();
            if (workers == null || !workers.Any() || instances == null || !instances.Any())
            {
                Console.WriteLine("No workers or Assignments available.");
                MessageBox.Show("No assignments to display.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    Fill = Brushes.Green ,
                    Stroke = Brushes.Black,
                    ToolTip = $"{wkr} (Cost: {wkr.GetCost()})"
                };
                Canvas.SetLeft(node, x - nodeSize / 2);
                Canvas.SetTop(node, wkrY - nodeSize / 2);
                AssignmentCanvas.Children.Add(node);

                var label = new TextBlock
                {
                    Text = wkr.ToString(),
                    Foreground = Brushes.White,
                    ToolTip = node.ToolTip
                };
                Canvas.SetLeft(label, x - 15);
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
                            Canvas.SetLeft(label, wkrX - 20);
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
                            AssignmentCanvas.Children.Add(line);

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
                        Canvas.SetLeft(label, wkrX - 15);
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
                        AssignmentCanvas.Children.Add(line);

                        currentY += taskSpacing;
                    }
                }
            }
            Console.WriteLine("DrawAssignmentGraph completed.");
        }
    }
}