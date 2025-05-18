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
        private Dictionary<Hardware, Point> hardwarePositions;
        private Dictionary<string, Point> taskPositions;

        public TaskAssignmentWindow(CostList costList)
        {
            InitializeComponent();
            this.costList = costList ?? throw new ArgumentNullException(nameof(costList));
            hardwarePositions = new Dictionary<Hardware, Point>();
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
            var hardwares = costList.GetHardwares();
            var instances = costList.GetInstances();
            if (hardwares == null || !hardwares.Any() || instances == null || !instances.Any())
            {
                Console.WriteLine("No hardwares or instances available.");
                MessageBox.Show("No assignments to display.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double canvasWidth = AssignmentCanvas.ActualWidth > 0 ? AssignmentCanvas.ActualWidth : 800;
            double canvasHeight = AssignmentCanvas.ActualHeight > 0 ? AssignmentCanvas.ActualHeight : 600;
            double hwSpacing = canvasWidth / (hardwares.Count + 1);
            double taskSpacing = 60;
            double hwY = 50;
            double nodeSize = 40;

            Console.WriteLine($"Drawing {hardwares.Count} hardware nodes...");
            int hwIndex = 0;
            foreach (var hw in hardwares.OrderBy(h => h.GetType()).ThenBy(h => h.GetID()))
            {
                double x = (hwIndex + 1) * hwSpacing;
                hardwarePositions[hw] = new Point(x, hwY);

                var node = new Rectangle
                {
                    Width = nodeSize,
                    Height = nodeSize,
                    Fill = hw.GetType() == HardwareType.HC ? Brushes.Green : Brushes.Orange,
                    Stroke = Brushes.Black,
                    ToolTip = $"{hw} (Cost: {hw.GetCost()})"
                };
                Canvas.SetLeft(node, x - nodeSize / 2);
                Canvas.SetTop(node, hwY - nodeSize / 2);
                AssignmentCanvas.Children.Add(node);

                var label = new TextBlock
                {
                    Text = hw.ToString(),
                    Foreground = Brushes.White,
                    ToolTip = node.ToolTip
                };
                Canvas.SetLeft(label, x - 15);
                Canvas.SetTop(label, hwY - 10);
                AssignmentCanvas.Children.Add(label);

                hwIndex++;
            }

            Console.WriteLine($"Drawing tasks for {instances.Count} instances...");
            foreach (var instance in instances.OrderBy(i => i.GetHardwarePtr().GetType()).ThenBy(i => i.GetHardwarePtr().GetID()))
            {
                var hw = instance.GetHardwarePtr();
                if (!hardwarePositions.ContainsKey(hw))
                {
                    Console.WriteLine($"Hardware {hw} not found in positions.");
                    continue;
                }

                var tasks = instance.GetTaskSet().ToList();
                double hwX = hardwarePositions[hw].X;
                double currentY = hwY + nodeSize + 50;

                foreach (int taskId in tasks)
                {
                    var times = costList.GetTimes().GetTimes(taskId, hw) ?? new List<int>();
                    var costs = costList.GetTimes().GetCosts(taskId, hw) ?? new List<int>();
                    bool hasSubtasks = times.Count > 1;

                    if (hasSubtasks)
                    {
                        // Draw a node for each subtask
                        for (int subtaskIndex = 0; subtaskIndex < times.Count; subtaskIndex++)
                        {
                            string subtaskLabel = $"T{taskId}_{subtaskIndex}";
                            double taskY = currentY;
                            taskPositions[subtaskLabel] = new Point(hwX, taskY);

                            string tooltip = $"Subtask {subtaskLabel} (Time: {times[subtaskIndex]}, Cost: {costs[subtaskIndex]})";

                            var node = new Ellipse
                            {
                                Width = nodeSize,
                                Height = nodeSize,
                                Fill = Brushes.Red,
                                Stroke = Brushes.Black,
                                ToolTip = tooltip
                            };
                            Canvas.SetLeft(node, hwX - nodeSize / 2);
                            Canvas.SetTop(node, taskY - nodeSize / 2);
                            AssignmentCanvas.Children.Add(node);

                            var label = new TextBlock
                            {
                                Text = subtaskLabel,
                                Foreground = Brushes.White,
                                ToolTip = tooltip
                            };
                            Canvas.SetLeft(label, hwX - 20);
                            Canvas.SetTop(label, taskY - 10);
                            AssignmentCanvas.Children.Add(label);

                            var line = new Line
                            {
                                X1 = hwX,
                                Y1 = hwY + nodeSize / 2,
                                X2 = hwX,
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
                        taskPositions[taskLabel] = new Point(hwX, taskY);

                        string tooltip = $"Task {taskLabel} (Time: {times.FirstOrDefault()}, Cost: {costs.FirstOrDefault()})";

                        var node = new Ellipse
                        {
                            Width = nodeSize,
                            Height = nodeSize,
                            Fill = Brushes.Blue,
                            Stroke = Brushes.Black,
                            ToolTip = tooltip
                        };
                        Canvas.SetLeft(node, hwX - nodeSize / 2);
                        Canvas.SetTop(node, taskY - nodeSize / 2);
                        AssignmentCanvas.Children.Add(node);

                        var label = new TextBlock
                        {
                            Text = taskLabel,
                            Foreground = Brushes.White,
                            ToolTip = tooltip
                        };
                        Canvas.SetLeft(label, hwX - 15);
                        Canvas.SetTop(label, taskY - 10);
                        AssignmentCanvas.Children.Add(label);

                        var line = new Line
                        {
                            X1 = hwX,
                            Y1 = hwY + nodeSize / 2,
                            X2 = hwX,
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