using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App1
{
    internal class TextBlockFitnessFunction : FitnessFunction
    {
        public string[] words;
        public int verticalsize;
        public int horizontalsize;
        protected TextBlock textBlock;

        public TextBlockFitnessFunction(UIElement uie) : base(uie)
        {
            textBlock = (TextBlock)owner;

            words = textBlock.Text.Split(' ');
            verticalsize = 18;
            horizontalsize = 9;
        }

        public override void CalcMatrix()
        {
            Debug.WriteLine(textBlock.Name + " calculating matrix");

            //searching for longest word
            string longest = "";
            foreach (string s in words)
            {
                if (s.Length > longest.Length)
                {
                    longest = s;
                }
            }

            //min width is the longest words's length
            xmin = longest.Length * horizontalsize + (int)textBlock.Padding.Right + (int)textBlock.Padding.Left;

            //but max 500
            if (xmin > 500)
            {
                xmin = 500;
            }

            //min height is the fontsize
            ymin = verticalsize + (int)textBlock.Padding.Bottom + (int)textBlock.Padding.Top;

            InitMatrix();

            //fill matrix with values
            for (int x = xmin; x < xmax; x++)
            {
                for (int y = ymin; y < ymax; y++)
                {
                    //if y is at the last slope there is no need for calculating
                    if (lastVerticalSlope[x] < ymax - 1)
                    {
                        matrix[x, y] = matrix[x, y - 1] + x;
                        if (lastHorizontalSlope[y] > x)
                        {
                            lastHorizontalSlope[y] = x;
                        }
                    }
                    else
                    {
                        int fitness = GetFitness(x, y);
                        matrix[x, y] = fitness;
                        if (fitness > -1)
                        {
                            lastVerticalSlope[x] = y;
                            if (lastHorizontalSlope[y] > x)
                            {
                                lastHorizontalSlope[y] = x;
                            }
                        }
                    }
                }
            }
        }

        private int GetFitness(int x, int y)
        {
            int xremaining = x;
            int yremaining = y - ymin;
            int whiteArea = 0;
            foreach (string word in words)
            {
                int length = word.Length * horizontalsize;
                if (length > xremaining)
                {
                    //word doesn't fit in new row
                    if (verticalsize > yremaining)
                    {
                        //dimension are too small
                        return -1;
                    }
                    else
                    {
                        //new row
                        yremaining -= verticalsize;
                        //adding the remaining space to whitearea
                        whiteArea += (xremaining + 9) * verticalsize;

                        xremaining = x - length - horizontalsize;
                    }
                }
                else
                {
                    //word does fit in the row
                    xremaining -= length + horizontalsize; //extra space is needed
                }
            }
            //space ramaining in the last row
            whiteArea += (xremaining + horizontalsize) * verticalsize;

            //remaining height
            whiteArea += yremaining * x;

            return whiteArea;
        }

        public override int GetMaxHeight()
        {
            return verticalsize * words.Length + (int)textBlock.Padding.Bottom + (int)textBlock.Padding.Top;
        }
    }
}
