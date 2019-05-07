using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App1
{
    internal class ImageFitnessFunction : FitnessFunction
    {
        public int height = 0;
        public int width = 0;

        public ImageFitnessFunction(UIElement uie) : base(uie) { }

        public override void CalcMatrix()
        {
            Image image = (Image)owner;

            Debug.WriteLine(image.Name + " calculating matrix");
            
            xmin = height / 2;
            ymin = width / 2;

            InitMatrix();

            // fill matrix with values
            for (int x = xmin; x < xmax; x++)
            {
                for (int y = ymin; y < ymax; y++)
                {
                    matrix[x, y] = GetFitness(x, y);
                    if (lastHorizontalSlope[y] == xmax - 1)
                    {
                        lastHorizontalSlope[y] = width * y / height;
                    }
                }
                lastVerticalSlope[x] = height * x / width;
            }
        }

        private int GetFitness(int x, int y)
        {
            int whitearea = 0;
            //ascpect ratio is wider
            if ((x / (double)y) > (width / (double)height))
            {
                //total area - scaled area (height*y/height * width*y/height)
                whitearea = (int)(x * y - y * y * width / (double)height);
            }
            //aspect ratio is taller
            if ((x / (double)y) < (width / (double)height))
            {
                //total area - scaled area (width*x/width * height*x/width)
                whitearea = (int)(x * y - x * x * height / (double)width);
            }

            return whitearea;
        }

        public override int GetMaxHeight()
        {
            return height * xmax / width;
        }
    }
}
