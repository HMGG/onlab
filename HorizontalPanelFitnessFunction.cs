using System;
using Windows.UI.Xaml;

namespace App1
{
    internal class HorizontalPanelFitnessFunction : PanelFitnessFunction
    {
        public HorizontalPanelFitnessFunction(UIElement uie) : base(uie)
        {
            fitnessPanel = (HorizontalFitnessPanel)owner;
        }

        protected override void FindMinSize()
        {
            xmin = fitnessPanel.functions[0].xmin;
            foreach (FitnessFunction ff in fitnessPanel.functions)
            {
                if (ff.xmin < xmin)
                {
                    xmin = ff.xmin;
                }
                //min height is the largest min height
                if (ff.ymin > ymin)
                {
                    ymin = ff.ymin;
                }
            }
            if (xmin > 500)
            {
                xmin = 500;
            }
        }

        protected override void Convolve(FitnessFunction f1, FitnessFunction f2, int s)
        {
            int[,] matrix1 = f1.matrix;
            int[,] matrix2 = f2.matrix;

            for (int x = xmax - 1; x >= f1.xmin + f2.xmin; x--)
            {
                bool firstenter = false;
                for (int y = ymin; y < ymax; y++)
                {
                    //only if both matrices are at a legal value
                    if (matrix1[x, y] > -1 && matrix2[x, y] > -1)
                    {
                        //if both whiteareas are only increasing from now on and there's already a legal pairing
                        if (f1.lastVerticalSlope[x] < y && f2.lastVerticalSlope[x] < y && matrix[x, y - 1] != -1)
                        {
                            //no need for convolution
                            matrix[x, y] = matrix[x, y - 1] + x;
                            smatrix[x, y, s] = smatrix[x, y - 1, s];
                            UpdateSeparators(x, y, smatrix[x, y, s], s - 1);
                            //if lastslope doesnt have a value yet
                            if (!firstenter)
                            {
                                firstenter = true;
                                lastVerticalSlope[x] = y;
                            }
                        }
                        else
                        {
                            //finding the best pairing of width
                            int best = int.MaxValue;
                            //where to separate the two contents
                            int separator = 0;
                            for (int xconv = x - f2.xmin; xconv >= f1.xmin; xconv--)
                            {
                                int w1 = matrix1[xconv, y];
                                int w2 = matrix2[x - xconv, y];
                                //only if the separator is good for both contents
                                if (w1 > -1 && w2 > -1)
                                {
                                    int test = w1 + w2;
                                    if (test < best)
                                    {
                                        best = test;
                                        separator = xconv;
                                    }
                                }
                            }
                            //no legal convolution at this size
                            if (best > xmax * ymax || best < 0)
                            {
                                best = -1;
                                separator = x;
                            }
                            else
                            {
                                UpdateSeparators(x, y, separator, s - 1);
                            }
                            matrix[x, y] = best;
                            smatrix[x, y, s] = separator;
                        }
                    }
                    else
                    {
                        if (s == 0)
                        {
                            matrix[x, y] = -1;
                        }
                        smatrix[x, y, s] = y;
                    }

                    if (x == xmin)
                    {
                        lastHorizontalSlope[y] = Math.Max(f1.lastHorizontalSlope[y], f2.lastHorizontalSlope[y]);
                    }
                }
            }

            xmin = f1.xmin + f2.xmin;
            if (xmin > 500)
            {
                xmin = 500;
            }
        }

        protected void UpdateSeparators(int x, int y, int separator, int s)
        {
            if (s >= 0)
            {
                smatrix[x, y, s] = smatrix[separator, y, s];
                UpdateSeparators(x, y, smatrix[x, y, s], s - 1);
            }
        }
    }
}
