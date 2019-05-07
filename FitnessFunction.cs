using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace App1
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    internal abstract class FitnessFunction
    {
        public int[,] matrix;
        public static int xmax = 1921;
        public static int ymax = 0;
        public int xmin;
        public int ymin;
        public UIElement owner;
        public int[] lastHorizontalSlope;
        public int[] lastVerticalSlope = new int[xmax];

        public FitnessFunction(UIElement uie)
        {
            owner = uie;
        }

        public void InitMatrix()
        {
            matrix = new int[xmax, ymax];

            //fill invalid places with -1
            for (int x = 0; x < xmax; x++)
            {
                for (int y = 0; y < ymin; y++)
                {
                    matrix[x, y] = -1;
                }
            }
            for (int x = 0; x < xmin; x++)
            {
                for (int y = 0; y < ymax; y++)
                {
                    matrix[x, y] = -1;
                }
            }

            //init slope vectors
            lastHorizontalSlope = new int[ymax];
            for (int i = ymin; i < ymax; i++)
            {
                lastHorizontalSlope[i] = xmax - 1;
            }

            for (int i = 0; i < xmax; i++)
            {
                lastVerticalSlope[i] = ymax - 1;
            }
        }

        public abstract void CalcMatrix();

        //for debugging
        public async Task ShowGraphAsync(Image image)
        {
            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, xmax - xmin, ymax - ymin, BitmapAlphaMode.Premultiplied);

            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    unsafe
                    {
                        ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* dataInBytes, out uint capacity);

                        // Fill-in the BGRA plane
                        BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                        int y = ymin;
                        for (int i = 0; i < bufferLayout.Height - 1; i++)
                        {
                            int x = xmin;
                            for (int j = 0; j < bufferLayout.Width - 1; j++)
                            {
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = 0;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = 0;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = 0;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3] = 255;
                                if (x < xmax && y < ymax)
                                {
                                    /* if (x == y)
                                     {
                                         Debug.WriteLine(image.Name + " " + x + " " + y + " " + matrix[x, y]);
                                     }*/
                                    if (matrix[x, y] >= 0)
                                    {
                                        byte value = (byte)(255 - matrix[x, y] / ((double)(x * y)) * 255);
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = value;
                                    }
                                    if (matrix[x, y] == 0)
                                    {
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = 255;
                                        dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = 255;
                                    }
                                }
                                x++;
                            }
                            y++;
                        }
                    }
                }
            }
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            image.Source = source;
            image.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
        }

        public abstract int GetMaxHeight();
    }
}
