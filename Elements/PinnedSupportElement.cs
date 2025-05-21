// Elements/PinnedSupportElement.cs
using System.Drawing;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public class PinnedSupportElement : SupportElement
    {
        public PinnedSupportElement(PointF location) : base(location)
        {
            Type = SupportType.Pinned;
            ElementColor = Color.ForestGreen;
        }
        public override void Draw(Graphics g, Grid grid)
        {
            using (Pen pen = GetPen())
            using (Brush fillBrush = new SolidBrush(IsSelected ? Color.LightGreen : Color.FromArgb(180, Color.PaleGreen)))
            {
                PointF p1 = new PointF(Location.X, Location.Y);
                PointF p2 = new PointF(Location.X - Size / 1.5f, Location.Y + Size);
                PointF p3 = new PointF(Location.X + Size / 1.5f, Location.Y + Size);
                PointF[] trianglePoints = { p1, p2, p3 };
                g.FillPolygon(fillBrush, trianglePoints);
                g.DrawPolygon(pen, trianglePoints);
                float groundY = Location.Y + Size;
                float groundWidth = Size * 1.8f;
                g.DrawLine(pen, Location.X - groundWidth / 2, groundY, Location.X + groundWidth / 2, groundY);
                for (int i = 0; i < 5; i++)
                {
                    float lx = Location.X - groundWidth / 2 + (i * groundWidth / 4);
                    g.DrawLine(pen, lx, groundY, lx - Size * 0.25f, groundY + Size * 0.4f);
                }
                if (IsSelected)
                {
                    g.FillEllipse(Brushes.LightSkyBlue, Location.X - Size * 0.3f, Location.Y - Size * 0.3f, Size * 0.6f, Size * 0.6f);
                    g.DrawEllipse(Pens.DarkBlue, Location.X - Size * 0.3f, Location.Y - Size * 0.3f, Size * 0.6f, Size * 0.6f);
                }
            }
        }
    }
}