// Elements/RollerSupportElement.cs
using System.Drawing;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public class RollerSupportElement : SupportElement
    {
        public RollerSupportElement(PointF location) : base(location)
        {
            Type = SupportType.Roller;
            ElementColor = Color.SteelBlue;
        }
        public override void Draw(Graphics g, Grid grid)
        {
            using (Pen pen = GetPen())
            using (Brush fillBrush = new SolidBrush(IsSelected ? Color.LightBlue : Color.FromArgb(180, Color.LightSteelBlue)))
            {
                PointF p1 = new PointF(Location.X, Location.Y);
                PointF p2 = new PointF(Location.X - Size / 1.5f, Location.Y + Size);
                PointF p3 = new PointF(Location.X + Size / 1.5f, Location.Y + Size);
                PointF[] trianglePoints = { p1, p2, p3 };
                g.FillPolygon(fillBrush, trianglePoints);
                g.DrawPolygon(pen, trianglePoints);
                float rollerBaseY = Location.Y + Size;
                float rollerRadius = Size * 0.2f;
                g.FillEllipse(Brushes.DimGray, Location.X - Size * 0.5f - rollerRadius, rollerBaseY, rollerRadius * 2, rollerRadius * 2);
                g.FillEllipse(Brushes.DimGray, Location.X + Size * 0.5f - rollerRadius, rollerBaseY, rollerRadius * 2, rollerRadius * 2);
                float groundY = rollerBaseY + rollerRadius * 2 + 1;
                float groundWidth = Size * 1.8f;
                g.DrawLine(pen, Location.X - groundWidth / 2, groundY, Location.X + groundWidth / 2, groundY);
                if (IsSelected)
                {
                    g.FillEllipse(Brushes.LightSkyBlue, Location.X - Size * 0.3f, Location.Y - Size * 0.3f, Size * 0.6f, Size * 0.6f);
                    g.DrawEllipse(Pens.DarkBlue, Location.X - Size * 0.3f, Location.Y - Size * 0.3f, Size * 0.6f, Size * 0.6f);
                }
            }
        }
    }
}