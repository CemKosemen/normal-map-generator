
namespace DynamicLighting
{
    public class Pixel
    {
        private int _r, _g, _b, _a;

        ///<summary>Red value of the pixel.</summary>
        public int R
        {
            get { return _r; }
            set { _r = value > byte.MaxValue ? byte.MaxValue : value; }
        }

        ///<summary>Green value of the pixel.</summary>
        public int G
        {
            get { return _g; }
            set { _g = value > byte.MaxValue ? byte.MaxValue : value; }
        }

        ///<summary>Blue value of the pixel.</summary>
        public int B
        {
            get { return _b; }
            set { _b = value > byte.MaxValue ? byte.MaxValue : value; }
        }

        ///<summary>Alpha value of the pixel.</summary>
        public int A
        {
            get { return _a; }
            set { _a = value > byte.MaxValue ? byte.MaxValue : value; }
        }

        ///<summary>Intensity of the pixel which is calculated with the formula: (R+G+B)/3</summary>
        public double Intensity
        {
            get { return (R + G + B) / (3.0 * 255); }
        }

        ///<summary>Constructor.</summary>
        public Pixel(int r, int g, int b, int a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        ///<summary>Converts given number to a value in a range of [0,255].</summary>
        public static byte Map(double value)
        {
            return (byte)((value + 1.0) * (255 / 2.0));
        }

        ///<summary>Converts given number to a value in a range of [-1,1].</summary>
        public static double Unmap(byte value)
        {
            return (value * 2.0 / 255) - 1;
        }

        ///<summary>Calculates the mean value of the given pixels.</summary>
        public static Pixel Mean(Pixel[] pixels)
        {
            int totalR = 0, totalG = 0, totalB = 0, totalA = 0;
            int nullCount = 0;

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == null)
                {
                    nullCount++;
                    continue;
                }

                totalR += pixels[i].R;
                totalG += pixels[i].G;
                totalB += pixels[i].B;
                totalA += pixels[i].A;
            }

            int count = pixels.Length - nullCount; // null values are discarded and does not effect the result.

            return new Pixel((byte)(totalR / count), (byte)(totalG / count), (byte)(totalB / count), (byte)(totalA / count));
        }

        public static Pixel operator *(Pixel a, double b)
        {
            return new Pixel((int)(a.R * b), (int)(a.G * b), (int)(a.B * b), a.A);
        }
    }
}