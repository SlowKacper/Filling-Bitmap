using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GK_Pro2
{
    public partial class Form1 : Form
    {
        int W = 5;
        int H = 7;
        Point[] Vertices = new Point[1];
        Triangle[] triangles = new Triangle[1];
        Bitmap MyBitmap = new Bitmap(1, 1);
        string path =@"..\zdj.jpg";
        Bitmap MyPhoto;
        Bitmap Inter;
        Color col = Color.Aquamarine;
        public Form1()
        {
            InitializeComponent();
            MyBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Bitmap tmp = new Bitmap(path);
            MyPhoto = new Bitmap(tmp, pictureBox1.Width, pictureBox1.Height);
            CreateGrid();
            Print();
            SetR();
            pictureBox1.Image = MyBitmap;
        }
        void CreateGrid()
        {
            Vertices = new Point[W * H];
            triangles = new Triangle[2 * (W - 1) * (H - 1) + 1];
            int dx = (pictureBox1.Width - 3) / (W - 1);
            int dy = (pictureBox1.Height - 3) / (H - 1);
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    Vertices[i * W + j] = new Point(j * dx + 1, i * dy + 1);
                }
            }
            int Ctriangle = 1;
            for (int j = 0; j < H - 1; j++)
            {
                for (int i = W * j; i < W * j + W - 1; i++)
                {
                    triangles[Ctriangle] = new Triangle(i, i + W, i + 1);
                    triangles[Ctriangle + 1] = new Triangle(i + W, i + W + 1, i + 1);
                    Ctriangle += 2;
                }
            }
            if (MyPhoto != null)
                MyPhoto = new Bitmap(MyPhoto, pictureBox1.Width, pictureBox1.Height);
            if (NormalMap != null)
                NormalMap = new Bitmap(NormalMap, pictureBox1.Width, pictureBox1.Height);
        }
        void PrintGrid()
        {
            Pen myPen = new Pen(Color.Black);
            Graphics grap = Graphics.FromImage(MyBitmap);

            for (int i = 1; i < triangles.Length; i++)
            {
                grap.DrawLine(myPen, Vertices[triangles[i].Vertex[0]], Vertices[triangles[i].Vertex[1]]);
                grap.DrawLine(myPen, Vertices[triangles[i].Vertex[0]], Vertices[triangles[i].Vertex[2]]);
                grap.DrawLine(myPen, Vertices[triangles[i].Vertex[2]], Vertices[triangles[i].Vertex[1]]);
            }
            for (int i = 0; i < H * W; i++)
            {
                for (int x = Vertices[i].X - 1; x < Vertices[i].X + 2; x++)
                {
                    for (int y = Vertices[i].Y - 1; y < Vertices[i].Y + 2; y++)
                        MyBitmap.SetPixel(x, y, Color.Red);
                }
            }
            pictureBox1.Image = MyBitmap;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            CreateGrid();
            Print();
            if (flag)
                PrintGrid();
        }
        void Print()
        {
            MyBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            for (int i = 1; i < triangles.Length; i++)
            {
                FillTriangle(i);
            }
            Inter = new Bitmap(MyBitmap, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = MyBitmap;
        }
        void FillTriangle(int i)
        {
            int[] Tab = new int[3];
            int ymin;
            triangles[i].Vertex.CopyTo(Tab, 0);
            for (int j = 0; j < 2; j++)
            {
                ymin = Vertices[Tab[j]].Y;
                int ind = j;
                for (int k = j + 1; k < 3; k++)
                {
                    if (ymin > Vertices[Tab[k]].Y)
                    {
                        ind = k;
                        ymin = Vertices[Tab[k]].Y;
                    }
                }
                int tmp = Tab[ind];
                Tab[ind] = Tab[j];
                Tab[j] = tmp;
            }
            ymin = Vertices[Tab[0]].Y;
            int ymax = Vertices[Tab[2]].Y;
            List<(double, double, double)> AET = new List<(double, double, double)>();
            if (los)
            {
                Random rnd = new Random();
                Kd = (double)rnd.Next(0, 10) / 10;
                Ks = (double)rnd.Next(0, 10) / 10;
                m = rnd.Next(0, 100);
            }
            if (DIH == 2)
                Interpolation(triangles[i]);
            if (DIH == 3)
                Hybryda(triangles[i]);
            for (int y = ymin; y <= ymax; y++)
            {
                for (int t = 0; t < Tab.Length; t++)
                {
                    if (y == Vertices[Tab[t]].Y)
                    {

                        int prev = t - 1;
                        if (prev == -1)
                            prev = Tab.Length - 1;

                        if (Vertices[Tab[prev]].Y >= Vertices[Tab[t]].Y)
                        {
                            if ((Vertices[Tab[prev]].Y - Vertices[Tab[t]].Y) != 0)
                                AET.Add((Vertices[Tab[prev]].Y, Vertices[Tab[t]].X, ((double)(Vertices[Tab[prev]].X - Vertices[Tab[t]].X) / (double)(Vertices[Tab[prev]].Y - Vertices[Tab[t]].Y))));
                        }
                        else
                        {
                            for (int z = 0; z < AET.Count(); z++)
                            {
                                if (AET[z].Item1 == y)
                                {
                                    FillPixel((int)AET[z].Item2, y, i);
                                    AET.RemoveAt(z);
                                }
                            }
                        }
                        int next = (t + 1) % Tab.Length;

                        if (Vertices[Tab[next]].Y >= Vertices[Tab[t]].Y)
                        {
                            if ((Vertices[Tab[next]].Y - Vertices[Tab[t]].Y) != 0)
                            {
                                double xd = (Vertices[Tab[next]].X - Vertices[Tab[t]].X);
                                double wx = (Vertices[Tab[next]].Y - Vertices[Tab[t]].Y);
                                double eq = xd / wx;
                                AET.Add((Vertices[Tab[next]].Y, Vertices[Tab[t]].X, ((double)(Vertices[Tab[next]].X - Vertices[Tab[t]].X) / (double)(Vertices[Tab[next]].Y - Vertices[Tab[t]].Y))));
                            }
                        }
                        else
                        {
                            for (int z = 0; z < AET.Count(); z++)
                            {
                                if (AET[z].Item1 == y)
                                {
                                    FillPixel((int)AET[z].Item2, y, i);
                                    AET.RemoveAt(z);
                                }
                            }
                        }
                    }

                }
                for (int j = 0; j < AET.Count() - 1; j++)
                {
                    double xmin = AET[j].Item2;
                    int ind = j;
                    for (int k = j + 1; k < AET.Count(); k++)
                    {
                        if (xmin > AET[k].Item2)
                        {
                            ind = k;
                            xmin = AET[k].Item2;
                        }
                    }
                    List<(double, double, double)> tmp = new List<(double, double, double)>(AET);
                    AET[ind] = AET[j];
                    AET[j] = tmp[ind];
                }

                for (int g = 0; g < AET.Count - 1; g += 2)
                {
                    for (int j = (int)AET[g].Item2; j <= (AET[g + 1].Item2); j++)
                    {
                        FillPixel(j, y, i);
                    }
                }
                for (int j = 0; j < AET.Count(); j++)
                {
                    AET[j] = (AET[j].Item1, AET[j].Item2 + AET[j].Item3, AET[j].Item3);
                }
            }
        }
        void FillPixel(int x, int y, int i)
        {
            Color pixelColor;
            if (MyPhoto != null)
            {
                pixelColor = MyPhoto.GetPixel(x, y);
            }
            else
                pixelColor = col;
            MyBitmap.SetPixel(x, y, FindColor(pixelColor, x, y, triangles[i]));
        }
        double cos((double, double, double) v, (double, double, double) w)
        {
            return v.Item1 * w.Item1 + v.Item2 * w.Item2 + v.Item3 * w.Item3; ;
        }
        (double, double, double) ColorFill(Color pix)
        {
            double cosNL = cos(N, L);
            double cosVR = cos(V, R);
            Math.Max(cosNL, 0);
            Math.Max(cosVR, 0);
            cosVR = Math.Pow(cosVR, m);
            return (Kd * IL.Item1 * pix.R * cosNL + Ks * IL.Item1 * pix.R * cosVR, Kd * IL.Item2 * pix.G * cosNL + Ks * IL.Item2 * pix.G * cosVR, Kd * IL.Item3 * pix.B * cosNL + Ks * IL.Item3 * pix.B * cosVR);
        }
        (double, double, double) Normalize((double, double, double) norma)
        {
            double mian = Math.Sqrt(norma.Item1 * norma.Item1 + norma.Item2 * norma.Item2 + norma.Item3 * norma.Item3);
            norma.Item1 = norma.Item1 / mian;
            norma.Item2 = norma.Item2 / mian;
            norma.Item3 = norma.Item3 / mian;
            return norma;
        }
        void SetR()
        {
            R = (2 * cos(N, L) * N.Item1 - L.Item1, 2 * cos(N, L) * N.Item2 - L.Item2, 2 * cos(N, L) * N.Item3 - L.Item3);
        }
        (double, double, double) GetN(int x, int y)
        {
            Color normal = NormalMap.GetPixel(x, y);
            return ((normal.R / (255 * 0.5) - 1), (normal.G / (255 * 0.5) - 1), (normal.B / (255 * 0.5)));
        }

        (double, double, double)[] Cv;
        (double, double, double)[] Iov;
        (double, double, double)[] Nv;

        void Interpolation(Triangle t)
        {
            Cv = new (double, double, double)[3];
            for (int i = 0; i < 3; i++)
            {
                Color pix = Inter.GetPixel(Vertices[t.Vertex[i]].X, Vertices[t.Vertex[i]].Y);

                if (NormalMap != null)
                {
                    N = GetN(Vertices[t.Vertex[i]].X, Vertices[t.Vertex[i]].Y);
                }
                N = Normalize(N);
                SetR();
                Cv[i] = ColorFill(pix);
            }
        }
        void Hybryda(Triangle t)
        {
            Nv = new (double, double, double)[3];
            Iov = new (double, double, double)[3];
            for (int i = 0; i < 3; i++)
            {
                Color pix = Inter.GetPixel(Vertices[t.Vertex[i]].X, Vertices[t.Vertex[i]].Y);

                if (NormalMap != null)
                {
                    Nv[i] = GetN(Vertices[t.Vertex[i]].X, Vertices[t.Vertex[i]].Y);
                }
                else
                    Nv[i] = (0, 0, 1);
                Nv[i] = Normalize(Nv[i]);
                Iov[i].Item1 = pix.R;
                Iov[i].Item2 = pix.G;
                Iov[i].Item3 = pix.B;
            }
        }
        Color FindColor(Color pix, int x, int y, Triangle t)
        {
            if (DIH == 1)
            {
                if (babelek)
                {
                    double dist = Math.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
                    if (dist < 100)
                    {
                        N.Item1 = px - x;
                        N.Item2 = py - y;
                        N.Item3 = 50;
                        N = Normalize(N);
                        SetR();
                        I = ColorFill(pix);
                    }
                    else
                    {
                        SetN();
                        N = Normalize(N);
                        SetR();
                        I = ColorFill(pix);
                    }

                }
                else if (NormalMap != null)
                {
                    N = GetN(x, y);
                    N = Normalize(N);
                    SetR();
                    I = ColorFill(pix);
                }
                else
                {
                    N = Normalize(N);
                    SetR();
                    I = ColorFill(pix);
                }


            }
            if (DIH == 2)
            {
                double[] Wv = new double[3];
                Wv[0] = ((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (x - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (y - Vertices[t.Vertex[2]].Y)) / (((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (Vertices[t.Vertex[0]].Y - Vertices[t.Vertex[2]].Y)) + 0.000001);
                Wv[1] = ((Vertices[t.Vertex[2]].Y - Vertices[t.Vertex[0]].Y) * (x - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) * (y - Vertices[t.Vertex[2]].Y)) / (((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (Vertices[t.Vertex[0]].Y - Vertices[t.Vertex[2]].Y)) + 0.000001);
                Wv[2] = 1 - Wv[0] - Wv[1];
                double sum = Wv[0] + Wv[1] + Wv[2];
                double c1 = 0, c2 = 0, c3 = 0;
                for (int i = 0; i < 3; i++)
                {
                    c1 += Cv[i].Item1 * (Wv[i]);
                    c2 += Cv[i].Item2 * (Wv[i]);
                    c3 += Cv[i].Item3 * (Wv[i]);
                }
                I = (c1 / sum, c2 / sum, c3 / sum);
            }
            if (DIH == 3)
            {
                double[] Wv = new double[3];
                Wv[0] = ((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (x - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (y - Vertices[t.Vertex[2]].Y)) / (((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (Vertices[t.Vertex[0]].Y - Vertices[t.Vertex[2]].Y)) + 0.000001);
                Wv[1] = ((Vertices[t.Vertex[2]].Y - Vertices[t.Vertex[0]].Y) * (x - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) * (y - Vertices[t.Vertex[2]].Y)) / (((Vertices[t.Vertex[1]].Y - Vertices[t.Vertex[2]].Y) * (Vertices[t.Vertex[0]].X - Vertices[t.Vertex[2]].X) + (Vertices[t.Vertex[2]].X - Vertices[t.Vertex[1]].X) * (Vertices[t.Vertex[0]].Y - Vertices[t.Vertex[2]].Y)) + 0.000001);
                Wv[2] = 1 - Wv[0] - Wv[1];
                double N1 = 0, N2 = 0, N3 = 0;
                double Io1 = 0, Io2 = 0, Io3 = 0;
                double sum = Wv[0] + Wv[1] + Wv[2];
                for (int i = 0; i < 3; i++)
                {
                    Io1 += Iov[i].Item1 * (Wv[i]);
                    Io2 += Iov[i].Item2 * (Wv[i]);
                    Io3 += Iov[i].Item3 * (Wv[i]);
                    N1 += Nv[i].Item1 * (Wv[i]);
                    N2 += Nv[i].Item2 * (Wv[i]);
                    N3 += Nv[i].Item3 * (Wv[i]);
                }
                N = Normalize((N1, N2, N3));
                SetR();

                I = ColorFill(Color.FromArgb((int)(Io1 / sum), (int)(Io2 / sum), (int)(Io3 / sum)));
            }
            I.Item1 = Math.Min(I.Item1, 255);
            I.Item2 = Math.Min(I.Item2, 255);
            I.Item3 = Math.Min(I.Item3, 255);

            I.Item1 = Math.Max(I.Item1, 0);
            I.Item2 = Math.Max(I.Item2, 0);
            I.Item3 = Math.Max(I.Item3, 0);
            return Color.FromArgb((int)I.Item1, (int)I.Item2, (int)I.Item3);
        }
        private void radioButton1_MouseClick(object sender, MouseEventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "jpg files (*.jpg)|*.jpg";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    path = dlg.FileName;
                }
                Bitmap tmp = new Bitmap(path);
                MyPhoto = new Bitmap(tmp, pictureBox1.Width, pictureBox1.Height);
            }
        }
        private void radioButton2_MouseClick(object sender, MouseEventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {

                col = colorDialog1.Color;
                radioButton2.BackColor = col;
                MyPhoto = null;
            }
        }
        void SetN()
        {
            N.Item1 = 0;
            N.Item2 = 0;
            N.Item3 = 1;
        }
        bool move = false;
        int ver = -1;
        Point a;
        int px = 0;
        int py = 0;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (MyBitmap.GetPixel(e.Location.X, e.Location.Y).ToArgb() == Color.Red.ToArgb())
            {
                a = new Point(e.Location.X, e.Location.Y);
                for (int i = 0; i < Vertices.Length; i++)
                {
                    double dist = Math.Sqrt((Vertices[i].X - a.X) * (Vertices[i].X - a.X) + (Vertices[i].Y - a.Y) * (Vertices[i].Y - a.Y));
                    if (dist <= 2)
                    {
                        move = true;
                        ver = i;
                        px = Vertices[ver].X - a.X;
                        py = Vertices[ver].Y - a.Y;

                    }
                }
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (move)
            {
                if (e.Location.X + px <= pictureBox1.Width - 1 && e.Location.Y + py <= pictureBox1.Height - 1 && e.Location.X + px >= 1 && e.Location.Y + py >= 1)
                {
                    Vertices[ver] = new Point(e.Location.X + px, e.Location.Y + py);
                    Print();
                }
            }
            if (babelek)
            {
                px = e.Location.X;
                py = e.Location.Y;
                Print();
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            move = false;
            if (flag)
                PrintGrid();
        }

        int DIH = 1;
        bool los = false;
        double Ks = 0;
        double Kd = 1;
        (double, double, double) IL = (1, 1, 1);
        (double, double, double) L = (0, 0, 1);
        (double, double, double) V = (0, 0, 1);
        (double, double, double) N = (0, 0, 1);
        (double, double, double) R;
        (double, double, double) I;
        double m = 1;
        Bitmap NormalMap = null;

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            Kd = (double)trackBar1.Value / 10;
        }
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            Ks = (double)trackBar2.Value / 10;
        }
        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            m = trackBar3.Value;
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            H = (int)numericUpDown1.Value;
            CreateGrid();
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            W = (int)numericUpDown2.Value;
            CreateGrid();
        }
        private void radioButton5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "jpg files (*.jpg)|*.jpg";
                string ppath = @"..\normal_map.jpg";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ppath = dlg.FileName;
                }
                Bitmap tmp = new Bitmap(ppath);
                NormalMap = new Bitmap(tmp, pictureBox1.Width, pictureBox1.Height);
            }
            babelek = false;
        }
        private void radioButton6_Click(object sender, EventArgs e)
        {
            N = (0, 0, 1);
            NormalMap = null;
            babelek = false;
        }
        private void radioButton3_Click(object sender, EventArgs e)
        {
            Kd = (double)trackBar1.Value / 10;
            Ks = (double)trackBar2.Value / 10;
            m = trackBar3.Value;
            los = false;
        }
        private void radioButton4_Click(object sender, EventArgs e)
        {
            los = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button3.BackColor = colorDialog1.Color;
                IL.Item1 = (double)colorDialog1.Color.R / 255;
                IL.Item2 = (double)colorDialog1.Color.G / 255;
                IL.Item3 = (double)colorDialog1.Color.B / 255;
                numericUpDown3.Value = colorDialog1.Color.R;
                numericUpDown4.Value = colorDialog1.Color.G;
                numericUpDown5.Value = colorDialog1.Color.B;
            }
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            IL.Item1 = (double)numericUpDown3.Value / 255;
            button3.BackColor = Color.FromArgb((int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            IL.Item2 = (double)numericUpDown4.Value / 255;
            button3.BackColor = Color.FromArgb((int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            IL.Item3 = (double)numericUpDown5.Value / 255;
            button3.BackColor = Color.FromArgb((int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
        }
        private void radioButton7_Click(object sender, EventArgs e)
        {
            DIH = 1;
        }
        private void radioButton8_Click(object sender, EventArgs e)
        {
            DIH = 2;
        }
        private void radioButton9_Click(object sender, EventArgs e)
        {
            DIH = 3;
        }

        bool flag = false;

        private void button2_Click(object sender, EventArgs e)
        {
            if (flag)
            {
                button2.Text = "Pokaż siatkę";
                flag = !flag;
                pictureBox1.Image = Inter;
            }
            else
            {
                button2.Text = "Ukryj siatkę";
                flag = !flag;
                PrintGrid();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Print();
            if (flag)
                PrintGrid();
        }

        private int _ticks;
        bool sunMove = false;

        void SetL(int time)
        {
            L.Item3 = (-0.01) * (time - 10) * (time - 10) + 30;
            L.Item1 = (5 * time);
            L.Item2 = (4.35 * time);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (!sunMove)
            {
                _ticks = 0;
                timer1.Start();
                sunMove = !sunMove;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_ticks == 21)
                _ticks = 0;
            SetL(_ticks);
            this.Text = _ticks.ToString();
            L = Normalize(L);
            Print();
            _ticks++;
        }

        private void radioButton10_Click(object sender, EventArgs e)
        {
            if (sunMove)
            {
                timer1.Stop();
                sunMove = !sunMove;
            }
            L = (0, 0, 1);
        }

        bool babelek = false;
        private void radioButton11_Click(object sender, EventArgs e)
        {
            babelek = true; ;
        }
    }
    public class Triangle
    {
        public int[] Vertex = new int[3];
        public Triangle(int v1, int v2, int v3)
        {
            Vertex[0] = v1;
            Vertex[1] = v2;
            Vertex[2] = v3;
        }
    }
}
