// Elements/BeamElement.cs
using System;
using System.Drawing;
using System.Collections.Generic; // Для списка связанных элементов
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public enum BeamNodeType { Start, End } // Типы узлов на балке

    public class BeamNode
    {
        public BeamElement ParentBeam { get; }
        public BeamNodeType NodeType { get; }
        public PointF Location => NodeType == BeamNodeType.Start ? ParentBeam.StartPoint : ParentBeam.EndPoint;
        public List<DrawingElement> AttachedElements { get; } = new List<DrawingElement>(); // Элементы, привязанные к этому узлу

        public BeamNode(BeamElement parent, BeamNodeType type)
        {
            ParentBeam = parent;
            NodeType = type;
        }

        // Метод для отрисовки узла (например, если он выбран или для отладки)
        public void Draw(Graphics g, bool isHot) // isHot - если курсор над узлом
        {
            float radius = isHot ? 6f : 4f;
            using (Brush brush = new SolidBrush(isHot ? Color.OrangeRed : Color.LightSkyBlue))
            {
                g.FillEllipse(brush, Location.X - radius, Location.Y - radius, radius * 2, radius * 2);
            }
        }
    }

    public class BeamElement : DrawingElement
    {
        public PointF StartPoint { get; set; }
        public PointF EndPoint { get; set; }

        public BeamNode NodeStart { get; set; }
        public BeamNode NodeEnd { get; set; }

        public float Length => (float)Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2));

        public BeamElement(PointF startPoint) : this(startPoint, startPoint) {  }

        public TextElement StartNodeLabel { get; set; }
        public TextElement EndNodeLabel { get; set; }


        public BeamElement(PointF startPoint, PointF endPoint, MainForm mainFormRef = null) : base(startPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            MainFormReference = mainFormRef; // Сохраняем ссылку
            ElementColor = Color.DarkSlateGray;
            NodeStart = new BeamNode(this, BeamNodeType.Start);
            NodeEnd = new BeamNode(this, BeamNodeType.End);

            // Создаем текстовые метки для узлов, если есть ссылка на MainForm
            if (MainFormReference != null)
            {
                CreateNodeLabels();
            }
        }

        private void CreateNodeLabels()
        {
            if (MainFormReference == null) return;

            // TODO: Нужна логика для именования узлов (A, B, C... или 1, 2, 3...)
            // Пока просто "S" и "E"
            Font nodeFont = new Font("Arial", 9, FontStyle.Bold);

            StartNodeLabel = new TextElement(PointF.Empty, "S", this) { TextFont = nodeFont, ElementColor = Color.DarkRed };
            UpdateNodeLabelPosition(NodeStart, StartNodeLabel);
            MainFormReference.drawingPanelMain.CurrentScene.AddElement(StartNodeLabel); // Добавляем на сцену

            EndNodeLabel = new TextElement(PointF.Empty, "E", this) { TextFont = nodeFont, ElementColor = Color.DarkRed };
            UpdateNodeLabelPosition(NodeEnd, EndNodeLabel);
            MainFormReference.drawingPanelMain.CurrentScene.AddElement(EndNodeLabel); // Добавляем на сцену
        }

        private void UpdateNodeLabelPosition(BeamNode node, TextElement label)
        {
            if (label == null || MainFormReference == null) return;
            SizeF textSize = MainFormReference.MeasureText(label.Text, label.TextFont);
            // Размещаем текст немного смещенным от узла
            float offsetX = (node.Location.X < (StartPoint.X + EndPoint.X) / 2) ? -textSize.Width - 5 : 5; // Левее для левого, правее для правого
            float offsetY = -textSize.Height / 2; // По центру вертикально
            if (StartPoint.Y == EndPoint.Y) // Горизонтальная балка
            {
                offsetY = -textSize.Height - 5; // Выше балки
                offsetX = node.Location.X - textSize.Width / 2; // По центру узла
            }


            label.Location = new PointF(node.Location.X + offsetX, node.Location.Y + offsetY);
        }


        public override void Move(PointF delta)
        {
            base.Move(delta); // Перемещает Location (StartPoint) и вызывает обновление текста через SelectTool
            // Обновляем EndPoint
            EndPoint = new PointF(EndPoint.X + delta.X, EndPoint.Y + delta.Y);

            // Перемещаем привязанные к узлам элементы (опоры, силы)
            MoveAttachedElements(NodeStart, delta);
            MoveAttachedElements(NodeEnd, delta);

            // Обновляем позиции текстовых меток узлов
            UpdateNodeLabelPosition(NodeStart, StartNodeLabel);
            UpdateNodeLabelPosition(NodeEnd, EndNodeLabel);
        }

        private void MoveAttachedElements(BeamNode node, PointF delta)
        {
            // ... (код как был, но внутри него обновление текста для ForceElement)
            var attachedCopy = new List<DrawingElement>(node.AttachedElements);
            foreach (var attachedElement in attachedCopy)
            {
                attachedElement.Move(delta); // Вызываем Move для опоры/силы
                // Обновляем позицию текста для ForceElement (если он привязан)
                if (attachedElement is ForceElement fe)
                {
                    fe.UpdateAssociatedTextPosition(this.MainFormReference);
                }
                else if (attachedElement is SupportElement se) // И для SupportElement
                {
                    se.UpdateAssociatedTextPosition(this.MainFormReference);
                }
            }
        }

        public void UpdatePoints(PointF newStart, PointF newEnd)
        {
            StartPoint = newStart;
            EndPoint = newEnd;
            Location = StartPoint;

            UpdateAttachedElementsLocation(NodeStart);
            UpdateAttachedElementsLocation(NodeEnd);

            UpdateNodeLabelPosition(NodeStart, StartNodeLabel);
            UpdateNodeLabelPosition(NodeEnd, EndNodeLabel);
        }

        private void UpdateAttachedElementsLocation(BeamNode node)
        {
            // ... (код как был, но внутри него обновление текста для ForceElement)
            var attachedCopy = new List<DrawingElement>(node.AttachedElements);
            foreach (var attachedElement in attachedCopy)
            {
                attachedElement.Location = node.Location;
                if (attachedElement is ForceElement fe)
                {
                    fe.UpdateAssociatedTextPosition(this.MainFormReference);
                }
                else if (attachedElement is SupportElement se)
                {
                    se.UpdateAssociatedTextPosition(this.MainFormReference);
                }
            }
        }
        // Draw(), HitTestNode(), HitTest(), GetBoundingBox(), Attach/Detach остаются как были
        public override void Draw(Graphics g, Grid grid)
        {
            using (Pen pen = GetPen())
            {
                g.DrawLine(pen, StartPoint, EndPoint);
                if (IsSelected)
                {
                    NodeStart.Draw(g, false);
                    NodeEnd.Draw(g, false);
                }
            }
        }
        public BeamNode HitTestNode(PointF worldPoint, float tolerance)
        {
            float distToStart = (float)Math.Sqrt(Math.Pow(worldPoint.X - StartPoint.X, 2) + Math.Pow(worldPoint.Y - StartPoint.Y, 2));
            if (distToStart <= tolerance) return NodeStart;
            float distToEnd = (float)Math.Sqrt(Math.Pow(worldPoint.X - EndPoint.X, 2) + Math.Pow(worldPoint.Y - EndPoint.Y, 2));
            if (distToEnd <= tolerance) return NodeEnd;
            return null;
        }
        public override bool HitTest(PointF worldPoint, float tolerance)
        {
            if (HitTestNode(worldPoint, tolerance + LineWidth) != null) return true;
            float dx = EndPoint.X - StartPoint.X;
            float dy = EndPoint.Y - StartPoint.Y;
            if (dx == 0 && dy == 0)
            {
                return Math.Sqrt(Math.Pow(worldPoint.X - StartPoint.X, 2) + Math.Pow(worldPoint.Y - StartPoint.Y, 2)) <= tolerance;
            }
            float t = ((worldPoint.X - StartPoint.X) * dx + (worldPoint.Y - StartPoint.Y) * dy) / (dx * dx + dy * dy);
            PointF closestPoint;
            if (t < 0) closestPoint = StartPoint;
            else if (t > 1) closestPoint = EndPoint;
            else closestPoint = new PointF(StartPoint.X + t * dx, StartPoint.Y + t * dy);
            float distance = (float)Math.Sqrt(Math.Pow(worldPoint.X - closestPoint.X, 2) + Math.Pow(worldPoint.Y - closestPoint.Y, 2));
            return distance <= tolerance + LineWidth / 2;
        }
        public override RectangleF GetBoundingBox()
        {
            float minX = Math.Min(StartPoint.X, EndPoint.X);
            float minY = Math.Min(StartPoint.Y, EndPoint.Y);
            float maxX = Math.Max(StartPoint.X, EndPoint.X);
            float maxY = Math.Max(StartPoint.Y, EndPoint.Y);
            float padding = LineWidth + 5;
            return new RectangleF(minX - padding, minY - padding, (maxX - minX) + 2 * padding, (maxY - minY) + 2 * padding);
        }
        public void AttachElementToNode(BeamNode node, DrawingElement element)
        {
            if (node != null && element != null && !node.AttachedElements.Contains(element))
            {
                node.AttachedElements.Add(element);
                if (element is SupportElement se) se.AttachedToNode = node;
                if (element is ForceElement fe) fe.AttachedToNode = node;
            }
        }
        public void DetachElementFromNode(BeamNode node, DrawingElement element)
        {
            if (node != null && element != null)
            {
                node.AttachedElements.Remove(element);
                if (element is SupportElement se && se.AttachedToNode == node) se.AttachedToNode = null;
                if (element is ForceElement fe && fe.AttachedToNode == node) fe.AttachedToNode = null;
            }
        }
    }
}