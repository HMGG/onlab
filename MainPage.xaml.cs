using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public partial class MainPage : Page
    {
        public static List<object> allfunctions = new List<object>();

        private int imagesloaded = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ShowFunctions(object sender, RoutedEventArgs e)
        {
            if (imagesloaded == 3)
            {
                Start();
            }
            int i = 0;
            foreach (FitnessFunction ff in allfunctions)
            {
                Image graph = new Image();
                Graphs.Children.Add(graph);
                graph.Height = FitnessFunction.ymax;
                graph.Width = FitnessFunction.xmax;
                graph.Tag = ff;
                await ff.ShowGraphAsync(graph);
                i++;
            }
            Graphs.UpdateLayout();
        }

        private void Image1_Loaded(object sender, RoutedEventArgs e)
        {
            Image image = (Image)sender;
            BitmapSource bms = (BitmapSource)image.Source;

            ImageFitnessFunction iff = new ImageFitnessFunction(image)
            {
                width = bms.PixelWidth,
                height = bms.PixelHeight
            };

            column1Panel.functions[0] = iff;
            column1Panel.function = new PanelFitnessFunction(column1Panel);

            imagesloaded++;
        }

        private void Image3_Loaded(object sender, RoutedEventArgs e)
        {
            Image image = (Image)sender;
            BitmapSource bms = (BitmapSource)image.Source;

            ImageFitnessFunction iff = new ImageFitnessFunction(image)
            {
                width = bms.PixelWidth,
                height = bms.PixelHeight
            };

            column2Panel.functions[0] = iff;
            column2Panel.function = new PanelFitnessFunction(column2Panel);

            imagesloaded++;
        }

        private void Image2_Loaded(object sender, RoutedEventArgs e)
        {
            Image image = (Image)sender;
            BitmapSource bms = (BitmapSource)image.Source;

            ImageFitnessFunction iff = new ImageFitnessFunction(image)
            {
                width = bms.PixelWidth,
                height = bms.PixelHeight
            };

            textPanel.functions[3] = iff;
            textPanel.function = new PanelFitnessFunction(textPanel);

            imagesloaded++;
        }

        private void Start()
        {
            PanelFitnessFunction pff = new PanelFitnessFunction(basePanel);
            basePanel.function = pff;

            FitnessFunction.ymax += basePanel.FindMaxHeight();
            pff.CalcMatrix();

            allfunctions.AddRange(textPanel.functions);
            allfunctions.Add(textPanel.function);
            allfunctions.AddRange(column1Panel.functions);
            allfunctions.Add(column1Panel.function);
            allfunctions.AddRange(column2Panel.functions);
            allfunctions.Add(column2Panel.function);
            allfunctions.Add(basePanel.function);

            mainScrollView.UpdateLayout();

            Debug.WriteLine("Done");
        }


        private void ShowValue_Click(object sender, RoutedEventArgs e)
        {
            int x = int.Parse(xBox.Text);
            int n = int.Parse(numberBox.Text);

            FitnessFunction ff = (FitnessFunction)allfunctions[n];
            int v = ff.matrix[x, int.Parse(yBox.Text)];
            valueBox.Text = ff.lastVerticalSlope[x] + " " + v + " " + v / (double)x / double.Parse(yBox.Text);

            if (ff.GetType().ToString().Contains("PanelFitnessFunction"))
            {
                PanelFitnessFunction pff = (PanelFitnessFunction)ff;
                for (int i = 0; i < pff.fitnessPanel.functions.Count - 1; i++)
                {
                    valueBox.Text += "  " + i + " " + pff.smatrix[x, int.Parse(yBox.Text), i];
                }
            }
        }
    }
}

