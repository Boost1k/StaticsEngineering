// Core/TextPositioningService.cs
using System;
using System.Drawing;
using StaticsEngineeringCAD.Elements;

namespace StaticsEngineeringCAD.Core
{
    public static class TextPositioningService
    {
        /// <summary>
        /// Измеряет размер строки текста с указанным шрифтом.
        /// Требует объект Graphics для выполнения измерения.
        /// </summary>
        public static SizeF MeasureText(Graphics g, string text, Font font)
        {
            if (g == null || string.IsNullOrEmpty(text) || font == null)
            {
                return SizeF.Empty;
            }
            return g.MeasureString(text, font);
        }

        /// <summary>
        /// Рассчитывает позицию для текста, связанного с элементом ForceElement.
        /// Текст обычно размещается у "хвоста" стрелки.
        /// </summary>
        public static PointF CalculatePositionForForceText(Graphics g, ForceElement force, string text, Font font)
        {
            if (g == null || force == null || string.IsNullOrEmpty(text) || font == null) return PointF.Empty;

            SizeF textSize = MeasureText(g, text, font);
            PointF anchorPoint = force.GetTailPoint();
            float vectorAngleDegrees = force.AngleDegrees;
            float offsetFromAnchor = 8f; // Отступ от стрелки

            vectorAngleDegrees = (vectorAngleDegrees % 360 + 360) % 360;

            // Рассчитываем позицию так, чтобы текст был снаружи стрелки
            if (vectorAngleDegrees >= 315 || vectorAngleDegrees < 45) // Вектор направлен вправо
                return new PointF(anchorPoint.X - textSize.Width - offsetFromAnchor, anchorPoint.Y - textSize.Height / 2f);
            else if (vectorAngleDegrees >= 45 && vectorAngleDegrees < 135) // Вектор направлен вверх
                return new PointF(anchorPoint.X - textSize.Width / 2f, anchorPoint.Y + offsetFromAnchor);
            else if (vectorAngleDegrees >= 135 && vectorAngleDegrees < 225) // Вектор направлен влево
                return new PointF(anchorPoint.X + offsetFromAnchor, anchorPoint.Y - textSize.Height / 2f);
            else // (vectorAngleDegrees >= 225 && vectorAngleDegrees < 315) // Вектор направлен вниз
                return new PointF(anchorPoint.X - textSize.Width / 2f, anchorPoint.Y - textSize.Height - offsetFromAnchor);
        }

        /// <summary>
        /// Рассчитывает позицию для текста, связанного с элементом SupportElement.
        /// Текст обычно размещается над опорой.
        /// </summary>
        public static PointF CalculatePositionForSupportText(Graphics g, SupportElement support, string text, Font font)
        {
            if (g == null || support == null || string.IsNullOrEmpty(text) || font == null) return PointF.Empty;

            SizeF textSize = MeasureText(g, text, font);
            // Текст немного выше и по центру Location опоры
            float verticalOffset = 5f; // Отступ над опорой
            return new PointF(
                support.Location.X - textSize.Width / 2f,
                support.Location.Y - textSize.Height - verticalOffset
            );
        }

        /// <summary>
        /// Рассчитывает позицию для текста, связанного с узлом балки (BeamNode).
        /// </summary>
        public static PointF CalculatePositionForBeamNodeText(Graphics g, BeamNode node, string text, Font font, BeamElement beam)
        {
            if (g == null || node == null || string.IsNullOrEmpty(text) || font == null || beam == null) return PointF.Empty;

            SizeF textSize = MeasureText(g, text, font);
            PointF nodeLocation = node.Location;
            float horizontalOffset = 7f;
            float verticalOffset = 7f;

            // По умолчанию, позиционируем в зависимости от того, левый это узел или правый
            bool isStartNodeApproximately = Math.Abs(nodeLocation.X - beam.StartPoint.X) < 0.1 && Math.Abs(nodeLocation.Y - beam.StartPoint.Y) < 0.1;
            bool isEndNodeApproximately = Math.Abs(nodeLocation.X - beam.EndPoint.X) < 0.1 && Math.Abs(nodeLocation.Y - beam.EndPoint.Y) < 0.1;

            // Пытаемся определить, ближе ли узел к началу или концу по X координате,
            // если это не очевидно из NodeType (на случай, если StartPoint правее EndPoint)
            bool nodeIsLefterThanBeamCenter = nodeLocation.X < (beam.StartPoint.X + beam.EndPoint.X) / 2f;


            float finalX = nodeLocation.X;
            float finalY = nodeLocation.Y;

            // Если балка преимущественно горизонтальна
            if (Math.Abs(beam.StartPoint.Y - beam.EndPoint.Y) < Math.Abs(beam.StartPoint.X - beam.EndPoint.X) * 0.5)
            {
                finalX = nodeLocation.X - textSize.Width / 2f; // Центрируем по X
                finalY = nodeLocation.Y - textSize.Height - verticalOffset; // Над узлом
                // Если верхняя часть панели занята, можно разместить под узлом
                if (nodeLocation.Y < textSize.Height + verticalOffset + 10) // 10 - небольшой отступ от верха панели
                {
                    finalY = nodeLocation.Y + verticalOffset;
                }
            }
            // Если балка преимущественно вертикальна
            else if (Math.Abs(beam.StartPoint.X - beam.EndPoint.X) < Math.Abs(beam.StartPoint.Y - beam.EndPoint.Y) * 0.5)
            {
                finalY = nodeLocation.Y - textSize.Height / 2f; // Центрируем по Y
                finalX = nodeLocation.X - textSize.Width - horizontalOffset; // Слева от узла
                // Если левая часть панели занята, можно разместить справа
                if (nodeLocation.X < textSize.Width + horizontalOffset + 10)
                {
                    finalX = nodeLocation.X + horizontalOffset;
                }
            }
            // Для наклонных балок
            else
            {
                if (nodeIsLefterThanBeamCenter) // Если узел левее центра балки
                {
                    finalX = nodeLocation.X - textSize.Width - horizontalOffset; // Слева
                }
                else // Если узел правее центра балки
                {
                    finalX = nodeLocation.X + horizontalOffset; // Справа
                }
                finalY = nodeLocation.Y - textSize.Height / 2f; // По центру Y узла
            }

            return new PointF(finalX, finalY);
        }
    }
}