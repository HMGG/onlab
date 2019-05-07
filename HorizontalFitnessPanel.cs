using Windows.Foundation;
using Windows.UI.Xaml;

namespace App1
{
    internal class HorizontalFitnessPanel : FitnessPanel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size retSize = new Size(availableSize.Width, 0);

            //functions are calculated
            if (function != null)
            {
                if (function.matrix != null)
                {
                    int width = (int)availableSize.Width;
                    int height;
                    if (!double.IsInfinity(availableSize.Height))
                    {
                        height = (int)availableSize.Height;
                    }
                    else
                    {
                        height = FitnessFunction.ymax - 1;
                        int best = int.MaxValue;
                        for (int y = function.ymin; y <= function.lastVerticalSlope[width] + 1; y++)
                        {
                            int test = function.matrix[width, y];
                            if (test >= 0 && test < best)
                            {
                                best = test;
                                height = y;
                            }
                        }
                    }
                    retSize.Height = height;

                    int takenwidth = 0;
                    int i = 0;
                    foreach (UIElement child in Children)
                    {
                        int childwidth = 0;
                        //getting separator position
                        if (i < Children.Count - 1)
                        {
                            childwidth = function.smatrix[width, height, i];
                        }
                        //last child
                        if (childwidth == 0)
                        {
                            childwidth = width;
                        }
                        childwidth -= takenwidth;
                        child.Measure(new Size(childwidth, height));
                        i++;
                        takenwidth += childwidth;
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

        protected override Size BasicMeasure(Size availableSize)
        {
            Size retSize = new Size(availableSize.Width, 0);

            newfunction = false;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);

                if (!ContainsFunctionOf(child))
                {
                    AddAndCreateFunction(child);
                }
                //finding highest element
                if (retSize.Height < child.DesiredSize.Height)
                {
                    retSize.Height = child.DesiredSize.Height;
                }
            }

            if (newfunction)
            {
                function = new HorizontalPanelFitnessFunction(this);
            }

            return retSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //functions are calculated
            if (function.matrix != null)
            {
                //initalize anchor point
                Point p = new Point(0, 0);

                int width = (int)finalSize.Width;
                int height = (int)finalSize.Height;

                int i = 0;
                foreach (UIElement child in Children)
                {
                    int childwidth = 0;
                    //getting separator position
                    if (i < Children.Count - 1)
                    {
                        childwidth = function.smatrix[width, height, i];
                    }
                    //last child
                    if (childwidth == 0)
                    {
                        childwidth = width;
                    }
                    childwidth -= (int)p.X;
                    child.Arrange(new Rect(p, new Size(childwidth, height)));
                    i++;
                    p.X += childwidth;
                }
            }
            return finalSize;
        }

        public override int FindMaxHeight()
        {
            int r = 0;
            foreach (FitnessFunction ff in functions)
            {
                int h = ff.GetMaxHeight();
                if (h > r)
                {
                    r = h;
                }
            }
            return r;
        }
    }
}
