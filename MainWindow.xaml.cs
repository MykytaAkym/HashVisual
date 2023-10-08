using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections;

namespace HashDemo
{
    public partial class MainWindow : Window
    {
        StackPanel[] stackPanel; //Array of children for the adder
        //Arrays of window elements for visualizing bits
        Label[] moveLabels; 
        Label[] hashLabels;
        Label[] addFirstLabels;
        Label[] addSecondLabels;
        Label[] addResLabels;
        private Thread thread;

        //Coefficient for adjusting the hashing rate and its visualization
        double speed = 1;
        //Initialize the window and its components
        public MainWindow()
        {
            InitializeComponent();
            Closed += EndProgram;
        }

        //A function that runs when the program ends. Terminates running threads
        private void EndProgram(object sender, EventArgs e)
        {
            if (thread != null)
                thread.Abort();
            Close();
        }

        //Function handler for the event of clicking on the "Calculate Hash" button
        //Starts the hash calculation process in a separate thread.
        //Through form elements, receives the rendering speed value and the source string
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            if (textbox1.Text.Length == 0)
            {
                MessageBox.Show("Ввод пустой строки. Хеш не будет подсчитан!", "Внимание!");
                return;
            }
            SpeedCh.IsEnabled = false;
            Start_Button.IsEnabled = false;
            CleanButton.IsEnabled = false;
            string str = textbox1.Text;
            thread = new Thread(delegate ()
            {
                djb(str);
            });
            thread.Start();

        }


        //Function responsible for the hashing process and its visualization,
        //accepts the original string. Returns binary and
        //hexadecimal representation of the hash sum
        private void djb(string str)
        {
            int hash = 5381;
            bool[] a = new BitArray(new int[] { hash }).Cast<bool>().Reverse().ToArray();

            int c;
            int i = 0;
            PrintArray(a);
            HashOut(a);

            while (i < str.Length)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    iterNum.Content = Convert.ToString(i);
                });
                a = new BitArray(new int[] { hash }).Cast<bool>().Reverse().ToArray();
                int buf = hash;
                PrintArray(a);
                c = str[i];
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    move.Fill = Brushes.Red;
                    move2.Fill = Brushes.Red;
                    sum2.Fill = Brushes.White;
                    sumF.Fill = Brushes.White;
                });
                move_ret();
                Thread.Sleep(Convert.ToInt32(1000 / speed));
                hash = (hash << 5);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    move.Fill = Brushes.White;
                    move2.Fill = Brushes.White;
                    sum1.Fill = Brushes.Red;
                    sumF.Fill = Brushes.Red;

                });
                stringSet("Hash << 5", "Hash");
                bit_add(hash, buf);

                Thread.Sleep(Convert.ToInt32(1000 / speed));
                hash += buf;
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    sum1.Fill = Brushes.White;
                    sum2.Fill = Brushes.Red;
                });
                stringSet("(Hash >> 5) + Hash", "Str[i]");
                bit_add(hash, c);
                hash += c;
                Thread.Sleep(Convert.ToInt32(1000/speed));
                i++;

            }
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                label_res.Content = Convert.ToString(Convert.ToInt32(hash), 2);
                label_res_Copy.Content = Convert.ToString(Convert.ToInt32(hash), 16);
                label_sum1.Content = "";
                label_sum2.Content = "";
                sum2.Fill = Brushes.White;
                sumF.Fill = Brushes.White;
                move.Fill = Brushes.White;
                move2.Fill = Brushes.White;
                sumF.Fill = Brushes.White;
                MainPanel.Children.Clear();
                NumPanel_2.Children.Clear();
                Start_Button.IsEnabled = true;
                CleanButton.IsEnabled = true;
                SpeedCh.IsEnabled = true;
            });
            return;

        }

        //The function that is responsible for displaying information about the addition operation,
        //accepts lines describing the current summing elements and displays them in special form fields
        void stringSet(string str_1, string str_2)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                label_sum1.Content = str_1;
                label_sum2.Content = str_2;
                label_sum_res.Content = "Сумма -> Hash";

            });
        }

        //Function responsible for visualizing the shift operation
        private void move_ret()
        {
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(Convert.ToInt32(250 / speed));
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    for (int j = 1; j < 32; j++)
                        moveLabels[j - 1].Background = moveLabels[j].Background;

                    moveLabels[31].Background = Brushes.Black;
                });

               
            }

        }

        //Function responsible for visualizing the summation. In form elements
        //outputs the sum from the adder into the field with the hash of the current iteration
        private void bit_add(int first, int second)
        {
            bool[] fir = new BitArray(new int[] { first }).Cast<bool>().Reverse().ToArray();
            bool[] sec = new BitArray(new int[] { second }).Cast<bool>().Reverse().ToArray();
            bool[] sum = new BitArray(new int[] { first + second }).Cast<bool>().Reverse().ToArray();
            AddPanel(fir, sec, sum);
            Thread.Sleep(Convert.ToInt32(3000 / speed));
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                for (int i = 0; i < 32; i++)
                {
                    if (sum[i]) { hashLabels[i].Background = Brushes.White; }
                    else { hashLabels[i].Background = Brushes.Black; }
                }
            });
        }

        //Outputs the summed parts and the sum to a special area.
        //Displays elements bit by bit, taking Boolean arrays for this
        private void AddPanel(bool[] a, bool[] b, bool[] c)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                MainPanel.Children.Clear();
                stackPanel = new StackPanel[4];
                for (int i = 0; i < 2; i++)
                {
                    stackPanel[i] = new StackPanel();
                    stackPanel[i].Name = ("panel_" + i.ToString());
                    stackPanel[i].Orientation = Orientation.Horizontal;
                    stackPanel[i].HorizontalAlignment = HorizontalAlignment.Left;
                    stackPanel[i].Height = 20;
                    stackPanel[i].Margin = new Thickness(5, 5, 0, 0);
                    stackPanel[i].VerticalAlignment = VerticalAlignment.Top;
                    stackPanel[i].Width = 670;
                    stackPanel[i].Background = Brushes.Aqua;
                    stackPanel[i].RenderSize = new Size(50, 50);
                    stackPanel[i].Visibility = Visibility.Visible;
                    MainPanel.Children.Add(stackPanel[i]);
                }
                stackPanel[2] = new StackPanel();
                stackPanel[3] = new StackPanel();
                stackPanel[2].Height = 20;
                MainPanel.Children.Add(stackPanel[2]);
                stackPanel[0].Children.Clear();
                addFirstLabels = new Label[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    addFirstLabels[i] = new Label();
                    if (a[i]) { addFirstLabels[i].Background = Brushes.Red; }
                    else { addFirstLabels[i].Background = Brushes.Black; }

                    addFirstLabels[i].FontSize = 10;
                    addFirstLabels[i].Width = 20;
                    addFirstLabels[i].BorderThickness = new Thickness(4);
                    stackPanel[0].Children.Add(addFirstLabels[i]);
                }
                stackPanel[1].Children.Clear();
                addSecondLabels = new Label[a.Length];
                for (int i = 0; i < b.Length; i++)
                {
                    addSecondLabels[i] = new Label();
                    if (b[i]) { addSecondLabels[i].Background = Brushes.Red; }
                    else { addSecondLabels[i].Background = Brushes.Black; }

                    addSecondLabels[i].FontSize = 10;
                    addSecondLabels[i].Width = 20;
                    addSecondLabels[i].BorderThickness = new Thickness(4);
                    stackPanel[1].Children.Add(addSecondLabels[i]);
                }

                stackPanel[3].Name = ("panel_" + 3.ToString());
                stackPanel[3].Orientation = Orientation.Horizontal;
                stackPanel[3].HorizontalAlignment = HorizontalAlignment.Left;
                stackPanel[3].Height = 20;
                stackPanel[3].Margin = new Thickness(5, 5, 0, 0);
                stackPanel[3].VerticalAlignment = VerticalAlignment.Top;
                stackPanel[3].Width = 670;
                stackPanel[3].Background = Brushes.Aqua;
                stackPanel[3].RenderSize = new Size(50, 50);
                stackPanel[3].Visibility = Visibility.Visible;
                MainPanel.Children.Add(stackPanel[3]);
                stackPanel[3].Children.Clear();
                addResLabels = new Label[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    addResLabels[i] = new Label();
                    if (c[i]) { addResLabels[i].Background = Brushes.Red; }
                    else { addResLabels[i].Background = Brushes.Black; }

                    addResLabels[i].FontSize = 10;
                    addResLabels[i].Width = 20;
                    addResLabels[i].BorderThickness = new Thickness(4);
                    stackPanel[3].Children.Add(addResLabels[i]);
                }
            });

        }

        //Outputs a boolean array to visualize bit shifts
        private void PrintArray(bool[] a)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                NumPanel_2.Children.Clear();
                moveLabels = new Label[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    moveLabels[i] = new Label();
                    if (a[i]) { moveLabels[i].Background = Brushes.White; }
                    else { moveLabels[i].Background = Brushes.Black; }

                    moveLabels[i].FontSize = 10;
                    moveLabels[i].Width = 20;
                    moveLabels[i].BorderThickness = new Thickness(4);
                    NumPanel_2.Children.Add(moveLabels[i]);
                }
            });
        }

        //Updating the hash value of the current iteration
        private void set_num(bool[] a)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                for (int i = 0; i < a.Length; i++)
                    moveLabels[i].Background = Brushes.Black;
            });


        }


        //Output an empty field for further placement of the hash value there
        private void HashOut(bool[] a)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                NumPanel_1.Children.Clear();
                hashLabels = new Label[a.Length];
                for (int i = 0; i < a.Length; i++)
                {
                    hashLabels[i] = new Label();
                    hashLabels[i].Background = Brushes.Black;
                    hashLabels[i].FontSize = 10;
                    hashLabels[i].Width = 20;
                    hashLabels[i].BorderThickness = new Thickness(4);
                    NumPanel_1.Children.Add(hashLabels[i]);
                }
            });
        }


        //Function handler for clicking the "Reset Result" button Clears the window from
        //results of the previous calculation
        private void Button_Dump_Click(object sender, RoutedEventArgs e)
        {
            NumPanel_1.Children.Clear();
            NumPanel_2.Children.Clear();
            MainPanel.Children.Clear();
            label_res.Content = "";
            label_res_Copy.Content = "";
            sum2.Fill = Brushes.White;
            sumF.Fill = Brushes.White;
            move.Fill = Brushes.White;
            move2.Fill = Brushes.White;
            sumF.Fill = Brushes.White;
            iterNum.Content = "";
            label_sum1.Content = "";
            label_sum2.Content = "";
        }

        //Function handler for moving the speed slider
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speed = SpeedCh.Value;
        }


    }
}
