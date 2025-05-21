// Elements/SupportElement.cs
using System.Drawing;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public enum SupportType
    {
        Pinned,     // Шарнирно-неподвижная
        Roller,     // Шарнирно-подвижная
        Fixed       // Жесткая заделка
    }

    public abstract class SupportElement : DrawingElement
    {
        public SupportType Type { get; protected set; }
        public float Size { get; set; } = 15f;
        public BeamNode AttachedToNode { get; set; }

        protected SupportElement(PointF location) : base(location)
        {
            ElementColor = Color.DarkGreen;
        }

        public override RectangleF GetBoundingBox()
        {
            return new RectangleF(Location.X - Size, Location.Y - Size / 2f, Size * 2, Size * 1.5f);
        }

        public override bool HitTest(PointF worldPoint, float tolerance)
        {
            float effectiveRadius = Size * 0.75f;
            PointF centerOfSupport = new PointF(Location.X, Location.Y + Size / 2f);
            float dx = worldPoint.X - centerOfSupport.X;
            float dy = worldPoint.Y - centerOfSupport.Y;
            return (dx * dx + dy * dy) <= (effectiveRadius + tolerance) * (effectiveRadius + tolerance);
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

        public override void UpdateAssociatedTextPosition(MainForm mainFormInstance)
        {
            if (AssociatedTextLabel == null || mainFormInstance == null) return;
            SizeF textSize = mainFormInstance.MeasureText(AssociatedTextLabel.Text, AssociatedTextLabel.TextFont);
            AssociatedTextLabel.Location = new PointF(
                this.Location.X - textSize.Width / 2f,
                this.Location.Y - textSize.Height - 5
            );
        }
    }
}