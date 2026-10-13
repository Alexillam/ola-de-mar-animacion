using System.Drawing.Drawing2D;

namespace ANIMACION
{
    public partial class Form1 : Form
    {
        private float desplazamientoFase = 0f;
        private float nubeX = 100f;

        // Nombre completo para evitar ambigüedad con System.Threading.Timer en .NET 6+
        private readonly System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;
            ClientSize = new Size(800, 500);
            Text = "Simulador de Olas de Mar en Windows Forms";

            timer1.Interval = 30;           // ~33 cuadros por segundo
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            desplazamientoFase += 0.1f;

            nubeX += 0.6f;
            if (nubeX > ClientSize.Width + 150) nubeX = -200;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int ancho = ClientSize.Width;
            int alto = ClientSize.Height;
            if (ancho <= 0 || alto <= 0) return;

            // Cielo
            using (var cielo = new LinearGradientBrush(
                new Point(0, 0), new Point(0, alto / 2),
                Color.LightSkyBlue, Color.AliceBlue))
            {
                g.FillRectangle(cielo, 0, 0, ancho, alto / 2);
            }

            // Sol con resplandor
            float solX = ancho * 0.8f, solY = alto * 0.15f;
            using (var brillo = new SolidBrush(Color.FromArgb(70, Color.Gold)))
                g.FillEllipse(brillo, solX - 55, solY - 55, 110, 110);
            using (var sol = new SolidBrush(Color.Gold))
                g.FillEllipse(sol, solX - 35, solY - 35, 70, 70);

            // Nubes
            DibujarNube(g, nubeX, alto * 0.12f);
            DibujarNube(g, nubeX * 0.6f + ancho * 0.4f, alto * 0.25f);

            // Fondo del mar
            using (var mar = new SolidBrush(Color.MidnightBlue))
                g.FillRectangle(mar, 0, alto / 2, ancho, alto / 2);

            // Ola trasera
            DibujarOla(g, ancho, alto, desplazamientoFase * 0.5f, 25, 0.015f, alto / 2 + 30, Color.DodgerBlue);

            // Ola media + barco encima
            DibujarOla(g, ancho, alto, desplazamientoFase, 35, 0.020f, alto / 2 + 60, Color.DeepSkyBlue);
            DibujarBarco(g, ancho * 0.35f, desplazamientoFase, 35, 0.020f, alto / 2 + 60);

            // Ola delantera (tapa un poco el casco del barco)
            DibujarOla(g, ancho, alto, desplazamientoFase * 1.5f, 45, 0.025f, alto / 2 + 90, Color.Cyan);
        }

        private void DibujarNube(Graphics g, float x, float y)
        {
            using (var b = new SolidBrush(Color.FromArgb(220, Color.White)))
            {
                g.FillEllipse(b, x, y, 90, 35);
                g.FillEllipse(b, x + 20, y - 18, 55, 40);
                g.FillEllipse(b, x + 45, y - 8, 60, 35);
            }
        }

        private void DibujarBarco(Graphics g, float x, float fase, float amplitud, float frecuencia, int nivelBaseY)
        {
            // Posición y inclinación según la pendiente de la ola en x
            float y = nivelBaseY + (float)(amplitud * Math.Sin(frecuencia * x - fase));
            float pendiente = amplitud * frecuencia * (float)Math.Cos(frecuencia * x - fase);
            float angulo = (float)(Math.Atan(pendiente) * 180.0 / Math.PI);

            GraphicsState estado = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(angulo);

            // Casco
            PointF[] casco =
            {
                new PointF(-50, -5), new PointF(50, -5),
                new PointF(35, 20),  new PointF(-35, 20)
            };
            using (var b = new SolidBrush(Color.SaddleBrown))
                g.FillPolygon(b, casco);

            // Mástil y vela
            using (var pen = new Pen(Color.Black, 3))
                g.DrawLine(pen, 0, -5, 0, -70);
            PointF[] vela = { new PointF(0, -68), new PointF(0, -12), new PointF(38, -12) };
            using (var b = new SolidBrush(Color.WhiteSmoke))
                g.FillPolygon(b, vela);

            g.Restore(estado);
        }

        private void DibujarOla(Graphics g, int ancho, int alto, float fase,
                                float amplitud, float frecuencia, int nivelBaseY, Color colorOla)
        {
            const int resolucion = 10;
            int totalPuntos = (ancho / resolucion) + 2;

            PointF[] cresta = new PointF[totalPuntos];
            for (int i = 0; i < totalPuntos; i++)
            {
                float x = i * resolucion;
                float y = nivelBaseY + (float)(amplitud * Math.Sin(frecuencia * x - fase));
                cresta[i] = new PointF(x, y);
            }

            PointF[] poligono = new PointF[totalPuntos + 2];
            Array.Copy(cresta, poligono, totalPuntos);
            poligono[totalPuntos] = new PointF(cresta[totalPuntos - 1].X, alto);
            poligono[totalPuntos + 1] = new PointF(0, alto);

            using (var brush = new SolidBrush(Color.FromArgb(180, colorOla)))
                g.FillPolygon(brush, poligono);

            using (var pen = new Pen(Color.White, 2))
                g.DrawCurve(pen, cresta);
        }
    }
}
           
        
    

