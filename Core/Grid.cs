using System;
using System.Drawing;

namespace StaticsEngineeringCAD.Core
{
    public class Grid
    {
        public int StepSize { get; set; } = 20; // Шаг сетки в пикселях
        public Color GridColor { get; set; } = Color.LightGray;
        public bool IsVisible { get; set; } = true;
        public bool SnapToGrid { get; set; } = true;

        public Grid() { }

        public void Draw(Graphics g, Rectangle clientRect)
        {
            if (!IsVisible || StepSize <= 0) return;

            using (Pen gridPen = new Pen(GridColor))
            {
                gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot; // Точечная сетка

                // Вертикальные линии
                for (int x = 0; x < clientRect.Width; x += StepSize)
                {
                    g.DrawLine(gridPen, x, 0, x, clientRect.Height);
                }

                // Горизонтальные линии
                for (int y = 0; y < clientRect.Height; y += StepSize)
                {
                    g.DrawLine(gridPen, 0, y, clientRect.Width, y);
                }
            }
        }

        public Point SnapPoint(PointF worldPoint)
        {
            if (!SnapToGrid || StepSize <= 0)
            {
                return Point.Round(worldPoint);
            }

            int snappedX = (int)(Math.Round(worldPoint.X / StepSize) * StepSize);
            int snappedY = (int)(Math.Round(worldPoint.Y / StepSize) * StepSize);

            return new Point(snappedX, snappedY);
        }

        // Перегрузка для Point
        public Point SnapPoint(Point worldPoint)
        {
            return SnapPoint(new PointF(worldPoint.X, worldPoint.Y));
        }
    }
}