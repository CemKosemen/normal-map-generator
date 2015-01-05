using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace DynamicLighting
{
    public class PixelMap
    {
        ///<summary>2D array to store pixels of the image.</summary>
        private Pixel[,] _pixels;

        ///<summary>Width of the image in pixels.</summary>
        public int Width { get; private set; }

        ///<summary>Height of the image in pixels.</summary>
        public int Height { get; private set; }

        ///<summary>Size of one row of the image in bytes.</summary>
        public int Stride { get { return Width * 4; } }

        ///<summary>Size of the image in bytes.</summary>
        public int Size { get { return Stride * Height; } }

        ///<summary>Get a pixel in the specified coordinates.</summary>
        public Pixel this[int x, int y]
        {
            get
            {
                if (x >= Width || y >= Height || x < 0 || y < 0) { return null; }
                return _pixels[x, y];
            }
            set { _pixels[x, y] = value; }
        }

        ///<summary>Constructor.</summary>
        public PixelMap(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Pixel[Width, Height];
        }

        ///<summary>Constructor.</summary>
        public PixelMap(byte[] data, int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Pixel[Width, Height];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    int index = (i * Stride) + (j * 4);
                    this[j, i] = new Pixel(data[index + 2], data[index + 1], data[index], data[index + 3]);
                }
            }
        }

        ///<summary>Converts to WPF type BitmapImage.</summary>
        public BitmapImage ConvertToBitmapImage()
        {
            var newImgData = new byte[Size];
            var newImg = new BitmapImage();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (this[j, i] == null) continue;

                    int index = (i * Stride) + (j * 4);
                    newImgData[index] = (byte)this[j, i].B;
                    newImgData[index + 1] = (byte)this[j, i].G;
                    newImgData[index + 2] = (byte)this[j, i].R;
                    newImgData[index + 3] = (byte)this[j, i].A;
                }
            }

            var bitmapSource = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Pbgra32, null, newImgData, Stride);
            var encoder = new PngBitmapEncoder();
            var memoryStream = new MemoryStream();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            newImg.BeginInit();
            newImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            newImg.EndInit();

            memoryStream.Close();

            return newImg;
        }

        ///<summary>Smooth effect with mean filtering.</summary>
        public PixelMap Smooth()
        {
            var newMap = new PixelMap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var square = new Pixel[9];
                    square[0] = this[i - 1, j - 1];
                    square[1] = this[i, j - 1];
                    square[2] = this[i + 1, j - 1];
                    square[3] = this[i - 1, j];
                    square[4] = this[i, j];
                    square[5] = this[i + 1, j];
                    square[6] = this[i - 1, j + 1];
                    square[7] = this[i, j + 1];
                    square[8] = this[i + 1, j + 1];

                    newMap[i, j] = Pixel.Mean(square);
                }
            }

            return newMap;
        }

        ///<summary>Gets the normal map of the image with Sobel filter.</summary>
        public PixelMap GetNormalMap(double strength)
        {
            var newMap = new PixelMap(Width, Height);

            for (int i = 1; i < Width - 1; i++)
            {
                for (int j = 1; j < Height - 1; j++)
                {
                    /*
                        [ a b c ]
                        [ d x e ]
                        [ f g h ]
                    */
                    var a = this[i - 1, j - 1].Intensity;
                    var b = this[i, j - 1].Intensity;
                    var c = this[i + 1, j - 1].Intensity;
                    var d = this[i - 1, j].Intensity;
                    var e = this[i + 1, j].Intensity;
                    var f = this[i - 1, j + 1].Intensity;
                    var g = this[i, j + 1].Intensity;
                    var h = this[i + 1, j + 1].Intensity;

                    var v = new Vector3D { X = (f + 2.0 * g + h) - (a + 2.0 * b + c), Y = (c + 2.0 * e + h) - (a + 2.0 * d + f), Z = 1.0 / strength };
                    v.Normalize();

                    newMap[i, j] = new Pixel(Pixel.Map(v.X), Pixel.Map(v.Y), Pixel.Map(v.Z), this[i, j].A);
                }
            }

            return newMap;
        }

        ///<summary>Lighthens the image with given normal map, light vector and eye vector.</summary>
        public PixelMap Lighten(PixelMap normalMap, Vector3D lightVector, Vector3D eyeVector)
        {
            var newMap = new PixelMap(Width, Height);
            var l = lightVector;
            var v = eyeVector;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var pixel = this[i, j];
                    var normalPixel = normalMap[i, j];

                    if (pixel == null || normalPixel == null) { continue; }

                    var n = new Vector3D(Pixel.Unmap((byte)normalPixel.R), Pixel.Unmap((byte)normalPixel.G), Pixel.Unmap((byte)normalPixel.B)); // normal vector
                    var r = l - (2 * n * (Vector3D.DotProduct(n, l))); // reflected vector

                    double ambientLight = 0;
                    double diffuseReflection = Vector3D.DotProduct(n, l);
                    double specularReflection = Vector3D.DotProduct(r, v);
                    double light = ambientLight + diffuseReflection + specularReflection;

                    if (light > 1) { light = 1; }
                    else if (light < 0) { light = 0; }

                    pixel = pixel * light;
                    newMap[i, j] = pixel;
                }
            }

            return newMap;
        }
    }
}