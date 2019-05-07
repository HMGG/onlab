using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace App1
{
    internal class PanelFitnessFunction : FitnessFunction
    {
        public FitnessPanel fitnessPanel;

        public int[,,] smatrix;

        public PanelFitnessFunction(UIElement uie) : base(uie)
        {
            fitnessPanel = (FitnessPanel)owner;
        }

        public override int GetMaxHeight()
        {
            return fitnessPanel.FindMaxHeight();
        }

        public override void CalcMatrix()
        {
            fitnessPanel.CalcFunctions();

            Debug.WriteLine(fitnessPanel.Name + " calculating matrix");

            FindMinSize();

            InitMatrix();

            for (int x = xmin; x < xmax; x++)
            {
                for (int y = ymin; y > ymax; y++)
                {
                    matrix[x, y] = -1;
                }
            }

            if (fitnessPanel.functions.Count == 1)
            {
                matrix = fitnessPanel.functions[0].matrix;
                lastVerticalSlope = fitnessPanel.functions[0].lastVerticalSlope;
                lastHorizontalSlope = fitnessPanel.functions[0].lastHorizontalSlope;
            }
            else if (fitnessPanel.functions.Count >= 2)
            {
                smatrix = new int[xmax, ymax, fitnessPanel.functions.Count - 1];

                Debug.WriteLine("Convolving 1");
                Convolve(fitnessPanel.functions[0], fitnessPanel.functions[1], 0);

                if (fitnessPanel.functions.Count > 2)
                {
                    for (int f = 2; f < fitnessPanel.functions.Count; f++)
                    {
                        Debug.WriteLine("Convolving " + f);
                        Convolve(this, fitnessPanel.functions[f], f - 1);
                    }
                }
            }

            fitnessPanel.function = this;
        }

        protected virtual void FindMinSize()
        {
            xmin = 0;
            ymin = fitnessPanel.functions[0].ymin;
            foreach (FitnessFunction ff in fitnessPanel.functions)
            {
                if (ff.ymin < ymin)
                {
                    ymin = ff.ymin;
                }
                //min width is the largest min width
                if (ff.xmin > xmin)
                {
                    xmin = ff.xmin;
                }
            }
        }

        protected virtual void Convolve(FitnessFunction f1, FitnessFunction f2, int s)
        {
            //visszacsinálni?
            int[,] matrix1 = f1.matrix;
            int[,] matrix2 = f2.matrix;

            //a matrix to work on
            int[,] panelmatrix = new int[xmax, ymax];

            for (int x = 0; x < xmax; x++)
            {
                for (int y = 0; y < f1.ymin + f2.ymin; y++)
                {
                    panelmatrix[x, y] = -1;
                }
            }
            for (int x = 0; x < xmin; x++)
            {
                for (int y = 0; y < ymax; y++)
                {
                    panelmatrix[x, y] = -1;
                }
            }

            for (int x = xmin; x < xmax; x++)
            {
                bool slopeset = false;
                for (int y = f1.ymin + f2.ymin; y < ymax; y++)
                {
                    //only if both matrices are at a legal value
                    if (matrix1[x, y] > -1 && matrix2[x, y] > -1)
                    {
                        //if both whiteareas are only increasing in vertical direction and there's already a legal pairing
                        if (f1.lastVerticalSlope[x] + f2.lastVerticalSlope[x] <= y && panelmatrix[x, y - 1] != -1)
                        {
                            int separator = smatrix[x, y - 1, s];
                            //no need for convolution
                            panelmatrix[x, y] = matrix1[x, separator] + matrix2[x, y - separator];
                            smatrix[x, y, s] = separator;
                            //if lastslope doesnt have a value yet
                            if (!slopeset)
                            {
                                lastVerticalSlope[x] = y;
                                slopeset = true;
                            }
                        }
                        else
                        {
                            //finding the best pairing of height
                            int best = int.MaxValue;
                            //where to separate the two contents
                            int separator = 0;
                            for (int yconv = f1.ymin; yconv <= y - f2.ymin; yconv++)
                            {
                                int w1 = matrix1[x, yconv];
                                int w2 = matrix2[x, y - yconv];
                                //only if the separator is good for both contents
                                if (w1 > -1 && w2 > -1)
                                {
                                    int test = w1 + w2;
                                    if (test < best)
                                    {
                                        best = test;
                                        separator = yconv;
                                    }
                                }
                            }
                            //no legal convolution at this size
                            if (best > xmax * ymax || best < 0 || separator == 0)
                            {
                                best = -1;
                                separator = y;
                            }
                            panelmatrix[x, y] = best;
                            smatrix[x, y, s] = separator;
                        }
                    }
                    else
                    {
                        panelmatrix[x, y] = -1;
                        smatrix[x, y, s] = y;
                    }
                }
                lastHorizontalSlope[x] = Math.Max(f1.lastHorizontalSlope[x], f2.lastHorizontalSlope[x]);
            }

            ymin = f1.ymin + f2.ymin;

            matrix = panelmatrix;
        }
    }
}
