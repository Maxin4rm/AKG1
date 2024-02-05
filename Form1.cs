using AKG1.Models;
using System.Drawing.Imaging;
using System.Numerics;

namespace AKG1
{
    public partial class Form1 : Form
    {
        List<Tuple<int, int, int>> projections = new();
        const float cameraZ = 500.0f;
        
        const string filePath = "..\\..\\..\\model1.obj";

        ObjFileParser parser = new();

        const int height = 900;
        const int width = 1200;
        
        public Form1()
        {
            InitializeComponent();   
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            parser.ParseOBJFile(filePath);
        }

        private static Vector3 MultiplyVectorOnMatrix(Matrix4x4 matrix, Vector3 vector)
        {
            Vector3 result = new(
                matrix.M11 * vector.X + matrix.M21 * vector.Y + matrix.M31 * vector.Z + matrix.M41,
                matrix.M12 * vector.X + matrix.M22 * vector.Y + matrix.M32 * vector.Z + matrix.M42,
                matrix.M13 * vector.X + matrix.M23 * vector.Y + matrix.M33 * vector.Z + matrix.M43
            );
            return result;
        }

        

        private List<Tuple<int, int>> DrawLine(int p1, int p2)
        {
            int x0 = projections[p1 - 1].Item1;
            int x1 = projections[p2 - 1].Item1;
            int y0 = projections[p1 - 1].Item2;
            int y1 = projections[p2 - 1].Item2;
            
            List<Tuple<int, int>> points = new();
            
            //Изменения координат
            int dx = (x1 > x0) ? (x1 - x0) : (x0 - x1);
            int dy = (y1 > y0) ? (y1 - y0) : (y0 - y1);
            //Направление приращения
            int sx = (x1 >= x0) ? (1) : (-1);
            int sy = (y1 >= y0) ? (1) : (-1);

            if (dy < dx)
            {
                int d = (dy << 1) - dx;
                int d1 = dy << 1;
                int d2 = (dy - dx) << 1;
                points.Add(new Tuple<int, int>(x0, y0));
                int x = x0 + sx;
                int y = y0;
                for (int i = 1; i <= dx; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        y += sy;
                    }
                    else
                        d += d1;
                    points.Add(new Tuple<int, int>(x, y));
                    x += sx;
                }
            }
            else
            {
                int d = (dx << 1) - dy;
                int d1 = dx << 1;
                int d2 = (dx - dy) << 1;
                points.Add(new Tuple<int, int>(x0, y0));
                int x = x0;
                int y = y0 + sy;
                for (int i = 1; i <= dy; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        x += sx;
                    }
                    else
                        d += d1;
                    points.Add(new Tuple<int, int>(x, y));
                    y += sy;
                }
            }
            return points;
        }

        private void CalculateProjections(Pivot model)
        {
            
            Matrix4x4 modelMatrix = model.CreateModelMatrix();
            
            /*
            var camera = new Pivot
            {
                Translation = new Vector3(1, 2, 3),
                Rotation = new Vector3(0, 0, 0)
            };
            Matrix4x4 cameraMatrix = camera.CreateCameraMatrix();
            */

            projections.Clear();
            for (int i = 0; i < parser._vertices.Count; i++)
            {
                var curr = parser._vertices[i];
                Vector3 currPointVector = MultiplyVectorOnMatrix(modelMatrix, curr.Coordinates);
                float scale = cameraZ / (cameraZ + currPointVector.Z); 
                parser._vertices[i].Coordinates = currPointVector;
                var point = new Tuple<int, int, int>((int)((currPointVector.X * scale) + width / 2), (int)((currPointVector.Y * (-scale)) + height / 2), (int)currPointVector.Z);
                projections.Add(point);     
            }
        }

        private void OutputImage()
        {
            Bitmap bmp = new Bitmap(1200, 900);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] screenMatrix = new byte[bytes];
            Array.Fill<byte>(screenMatrix, 255);
            int screenMatrixWidth = Math.Abs(bmpData.Stride);

            foreach (var curr in projections)
            {
                if (curr.Item1 < width && curr.Item1 > 0 && curr.Item2 < height && curr.Item2 > 0)
                {
                    int pos = (curr.Item1) * 4 + (curr.Item2) * screenMatrixWidth;
                    screenMatrix[pos] = 0;
                    screenMatrix[pos + 1] = 0;
                    screenMatrix[pos + 2] = 0;
                    screenMatrix[pos + 3] = 255;
                }
            }

            Parallel.ForEach(
                    parser._faces,
                   polygon =>
                   {
                       for (int i = 0; i < 3; i++)
                       {
                           foreach (var curr in DrawLine(polygon.Indexes[0], polygon.Indexes[(i + 1) % 3]))
                           {
                               SetColorToPixel(ref screenMatrix, 0, 0, 0, 255, curr.Item1, curr.Item2, screenMatrixWidth);
                           }
                       }
                   }
            );
   
            System.Runtime.InteropServices.Marshal.Copy(screenMatrix, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);

            pictureBox.Image = bmp;
            Refresh();
        }

    

        private void DrawButton_Click(object sender, EventArgs e)
        {
            CalculateProjections(new Pivot
            {
                Scale = new Vector3(50, 50, 50),
                Translation = new Vector3(0, -250, 0),
                Rotation = new Vector3(0.0f, 0.0f, 0.0f)
            });
            
            while (true)
            {
                CalculateProjections(new Pivot
                {
                    Scale = new Vector3(1, 1, 1),
                    Translation = new Vector3(0, 0, 0),
                    Rotation = new Vector3(0.05f, 0.0f, 0.0f)
                });
                OutputImage();
                
            }  
        }

        private static int MatrixPositionCalculation(int x, int y, int screenMatrixWidth)
        {
            if (x < width && x > 0 && y < height && y > 0)
            {
                return x * 4 + y * screenMatrixWidth;
            }
            else
            {
                return -1;
            }
        }

        private static void SetColorToPixel(ref byte[] screenMatrix, byte r, byte g, byte b, byte alpha, int x, int y, int screenMatrixWidth)
        {
            int pos = MatrixPositionCalculation(x, y, screenMatrixWidth);
            if (pos != -1 && screenMatrix[pos] != b) {
                screenMatrix[pos] = b;
                screenMatrix[pos + 1] = g;
                screenMatrix[pos + 2] = r;
                screenMatrix[pos + 3] = alpha;
            }
        }
    }
}