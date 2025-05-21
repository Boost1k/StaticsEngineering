using System.Drawing;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public class PointElement : DrawingElement
    {
        public float Radius { get; set; } = 5f;

        public PointElement(PointF location)
        {
            Location = location;
        }

        public override void Draw(Graphics g, Grid grid)
        {
            using (Pen pen = GetPen())
            using (Brush brush = new SolidBrush(pen.Color))
            {
                float drawRadius = IsSelected ? Radius + 2 : Radius;
                g.FillEllipse(brush, Location.X - drawRadius, Location.Y - drawRadius, drawRadius * 2, drawRadius * 2);
            }
        }

        public override RectangleF GetBoundingBox()
        {
            return new RectangleF(Location.X - Radius, Location.Y - Radius, Radius * 2, Radius * 2);
        }

        public override bool HitTest(PointF worldPoint, float tolerance)
        {
            float dx = worldPoint.X - Location.X;
            float dy = worldPoint.Y - Location.Y;
            return (dx * dx + dy * dy) <= (Radius + tolerance) * (Radius + tolerance);
        }
    }
}