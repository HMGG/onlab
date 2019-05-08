using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace App1
{
    internal class FitnessPanel : Panel
    {
        public List<FitnessFunction> functions = new List<FitnessFunction>();

        public PanelFitnessFunction function;

        protected bool newfunction;

        protected override Size MeasureOverride(Size availableSize)
        {
            string name = Name;

            Size retSize = new Size(availableSize.Width, 0);

            //functions are calculated
            if (function != null)
            {
                if (function.matrix != null)
                {
                    int width = (int)availableSize.Width;
                    int height = 0;
                    if (!double.IsInfinity(availableSize.Height))
                    {
                        height = (int)availableSize.Height;
                    }
                    else
                    {
                        double best = 1;
                        height = function.lastVerticalSlope[width];
                        for (int y = function.ymin; y <= function.lastVerticalSlope[width]; y++)
                        {
                            double test = function.matrix[width, y] / (double)y / availableSize.Width;
                            if (test >= 0 && test < best)
                            {
                                best = test;
                                height = y;
                            }
                        }
                        //   height = function.lastVerticalSlope[width];
                    }
                    retSize.Height = height;

                    int takenheight = 0;
                    int i = 0;
                    foreach (UIElement child in Children)
                    {
                        int childheight = 0;
                        //getting separator position
                        if (i < Children.Count - 1)
                        {
                            childheight = function.smatrix[width, height, i];
                        }
                        //last child
                        if (childheight == 0)
                        {
                            childheight = height;
                        }
                        childheight -= takenheight;
                        child.Measure(new Size(width, childheight));
                        i++;
                        takenheight += childheight;
                    }
                    return retSize;
                }
                else
                {
                    return BasicMeasure(availableSize);
                }
            }
            else
            {
                return BasicMeasure(availableSize);
            }
        }

        protected virtual Size BasicMeasure(Size availableSize)
        {
            //return size
            Size retSize = new Size(availableSize.Width, 0);

            newfunction = false;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);

                if (!ContainsFunctionOf(child))
                {
                    AddAndCreateFunction(child);
                }

                retSize.Height += child.DesiredSize.Height;
                //find the widest element
                if (retSize.Width < child.DesiredSize.Width)
                {
                    retSize.Width = child.DesiredSize.Width;
                }
            }

            if (newfunction)
            {
                function = new PanelFitnessFunction(this);
            }

            return retSize;
        }

        protected void AddAndCreateFunction(UIElement child)
        {
            //calculate a fitness function if its a textblock
            if (child.GetType().Name == "TextBlock")
            {
                // add it to the list of functions
                functions.Add(new TextBlockFitnessFunction(child));
                newfunction = true;
            }

            //calculate a fitness function if its a fitnesspanel
            if (child.GetType().Name == "FitnessPanel")
            {
                // add it to the list of functions
                functions.Add(new PanelFitnessFunction(child));
                newfunction = true;
            }

            //calculate a fitness function if its a horizontal fitnesspanel
            if (child.GetType().Name == "HorizontalFitnessPanel")
            {
                // add it to the list of functions
                functions.Add(new HorizontalPanelFitnessFunction(child));
                newfunction = true;
            }

            //calculate a fitness function if its a fitnesspanel
            if (child.GetType().Name == "Image")
            {
                // add it to the list of functions
                functions.Add(new ImageFitnessFunction(child));
                newfunction = true;
            }
        }

        //function for checking if a textblocks fitnessfunction is already contained in the list
        public bool ContainsFunctionOf(object child)
        {
            foreach (FitnessFunction ff in functions)
            {
                if (ff.owner == child)
                {
                    return true;
                }
            }
            return false;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            string name = Name;
            if (function.matrix != null)
            {
                //initalize anchor point
                Point p = new Point(0, 0);

                int width = (int)finalSize.Width;
                int height = (int)finalSize.Height;

                int i = 0;
                foreach (UIElement child in Children)
                {
                    int childheight = 0;
                    //getting separator position
                    if (i < Children.Count - 1)
                    {
                        childheight = function.smatrix[width, height, i];
                    }
                    //last child
                    if (childheight == 0)
                    {
                        childheight = height;
                    }
                    childheight -= (int)p.Y;
                    child.Arrange(new Rect(p, new Size(width, childheight)));
                    i++;
                    p.Y += childheight;
                }
            }
            return finalSize;
        }

        public virtual int FindMaxHeight()
        {
            int r = 0;
            foreach (FitnessFunction ff in functions)
            {
                r += ff.GetMaxHeight();
            }
            return r;
        }

        public void CalcFunctions()
        {
            Debug.WriteLine(Name + " calculating matrices");
            foreach (FitnessFunction ff in functions)
            {
                ff.CalcMatrix();
            }
        }
    }
}
