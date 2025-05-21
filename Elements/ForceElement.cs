// Elements/ForceElement.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public class ForceElement : DrawingElement
    {
        public float Magnitude { get; set; } = 100f;
        public float AngleDegrees { get; set; } = 270f;
        public float ArrowScreenLength { get; set; } = 40f;
        private const float ArrowHeadSize = 8f;
        public BeamNode AttachedToNode { get; set; }

        public ForceElement(PointF location) : base(location)
        {
            ElementColor = Color.BlueViolet;
            SelectedColor = Color.MediumOrchid;
        }

        public ForceElement(PointF location, float magnitude, float angleDegrees) : base(location)
        {
            Magnitude = magnitude;
            AngleDegrees = angleDegrees;
            ElementColor = Color.BlueViolet;
            SelectedColor = Color.MediumOrchid;
        }

        public override void Draw(Graphics g, Grid grid)
        {
            using (Pen pen = GetPen())
            {
                float angleRad = AngleDegrees * (float)Math.PI / 180f;
                float dx = ArrowScreenLength * (float)Math.Cos(angleRad);
                float dy = ArrowScreenLength * (float)Math.Sin(angleRad);
                PointF headPoint = Location;
                PointF tailPoint = new PointF(Location.X - dx, Location.Y - dy);

                using (AdjustableArrowCap customCap = new AdjustableArrowCap(ArrowHeadSize / 2, ArrowHeadSize, true))
                using (Pen arrowPen = (Pen)pen.Clone())
                {
                    arrowPen.CustomEndCap = customCap;
                    g.DrawLine(arrowPen, tailPoint, headPoint);
                }

                if (IsSelected)
                {
                    float markerRadius = LineWidth * 2f;
                    using (Brush markerBrush = new SolidBrush(Color.FromArgb(150, SelectedColor)))
                    {
                        g.FillEllipse(markerBrush, Location.X - markerRadius, Location.Y - markerRadius, markerRadius * 2, markerRadius * 2);
                    }
                    using (Pen markerPen = new Pen(SelectedColor, 1))
                    {
                        g.DrawEllipse(markerPen, Location.X - markerRadius, Location.Y - markerRadius, markerRadius * 2, markerRadius * 2);
                    }
                }
            }
        }
        public override void Move(PointF delta)
        {
            if (AttachedToNode != null)
            {
                Location = new PointF(Location.X + delta.X, Location.Y + delta.Y);
            }
            else
            {
                base.Move(delta);
            }
        }
        public PointF GetHeadPoint() => Location;
        public PointF GetTailPoint()
        {
            float angleRad = AngleDegrees * (float)Math.PI / 180f;
            float dx = ArrowScreenLength * (float)Math.Cos(angleRad);
            float dy = ArrowScreenLength * (float)Math.Sin(angleRad);
            return new PointF(Location.X - dx, Location.Y - dy);
        }
        public override RectangleF GetBoundingBox()
        {
            float angleRad = AngleDegrees * (float)Math.PI / 180f;
            float dx = ArrowScreenLength * (float)Math.Cos(angleRad);
            float dy = ArrowScreenLength * (float)Math.Sin(angleRad);
            PointF tailPoint = new PointF(Location.X - dx, Location.Y - dy);
            float minX = Math.Min(Location.X, tailPoint.X);
            float minY = Math.Min(Location.Y, tailPoint.Y);
            float maxX = Math.Max(Location.X, tailPoint.X);
            float maxY = Math.Max(Location.Y, tailPoint.Y);
            float padding = ArrowHeadSize;
            return new RectangleF(minX - padding, minY - padding, (maxX - minX) + 2 * padding, (maxY - minY) + 2 * padding);
        }
        public override bool HitTest(PointF worldPoint, float tolerance)
        {
            float dxApp = worldPoint.X - Location.X;
            float dyApp = worldPoint.Y - Location.Y;
            if ((dxApp * dxApp + dyApp * dyApp) <= (ArrowHeadSize / 1.5f + tolerance) * (ArrowHeadSize / 1.5f + tolerance))
            {
                return true;
            }
            float angleRad = AngleDegrees * (float)Math.PI / 180f;
            float dxLine = ArrowScreenLength * (float)Math.Cos(angleRad);
            float dyLine = ArrowScreenLength * (float)Math.Sin(angleRad);
            PointF tailPoint = new PointF(Location.X - dxLine, Location.Y - dyLine);
            PointF headPoint = Location;
            float L2 = (headPoint.X - tailPoint.X) * (headPoint.X - tailPoint.X) +
                       (headPoint.Y - tailPoint.Y) * (headPoint.Y - tailPoint.Y);
            if (L2 == 0)
                return (Math.Sqrt(dxApp * dxApp + dyApp * dyApp) <= tolerance);
            float t = ((worldPoint.X - tailPoint.X) * (headPoint.X - tailPoint.X) +
                       (worldPoint.Y - tailPoint.Y) * (headPoint.Y - tailPoint.Y)) / L2;
            t = Math.Max(0, Math.Min(1, t));
            PointF projection = new PointF(tailPoint.X + t * (headPoint.X - tailPoint.X),
                                           tailPoint.Y + t * (headPoint.Y - tailPoint.Y));
            float distToLine = (float)Math.Sqrt((worldPoint.X - projection.X) * (worldPoint.X - projection.X) +
                                                (worldPoint.Y - projection.Y) * (worldPoint.Y - projection.Y));
            return distToLine <= LineWidth / 2f + tolerance;
        }

        public override void UpdateAssociatedTextPosition(MainForm mainFormInstance)
        {
            if (AssociatedTextLabel == null || mainFormInstance == null) return;
            AssociatedTextLabel.Location = mainFormInstance.CalculateTextPositionForForce(this, AssociatedTextLabel.Text, AssociatedTextLabel.TextFont);
        }
    }
}