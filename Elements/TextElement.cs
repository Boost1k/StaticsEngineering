// Elements/TextElement.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.Elements
{
    public class TextElement : DrawingElement
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    InvalidateBoundingBox();
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        private Font _textFont;
        public Font TextFont
        {
            get => _textFont;
            set
            {
                if (_textFont != value)
                {
                    _textFont = value;
                    InvalidateBoundingBox();
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        /// <summary>
        /// Ссылка на "родительский" графический элемент, к которому привязан этот текст.
        /// Может использоваться для более сложной логики позиционирования или взаимодействия.
        /// </summary>
        public DrawingElement OwnerElement { get; private set; }

        private SizeF _measuredSize = SizeF.Empty;
        private bool _isBoundingBoxValid = false;

        public TextElement(PointF location, string text, DrawingElement owner = null) : base(location)
        {
            _text = text;
            OwnerElement = owner;
            _textFont = new Font("Arial", 10);

            ElementColor = Color.DimGray;
            SelectedColor = Color.DarkOrange;
            LineWidth = 0;
            InvalidateBoundingBox();
        }

        private void InvalidateBoundingBox()
        {
            _isBoundingBoxValid = false;
        }

        private void EnsureBoundingBoxIsValid(Graphics g)
        {
            if (!_isBoundingBoxValid && g != null && !string.IsNullOrEmpty(Text) && TextFont != null)
            {
                _measuredSize = g.MeasureString(Text, TextFont);
                _isBoundingBoxValid = true;
            }
            else if (string.IsNullOrEmpty(Text) || TextFont == null)
            {
                _measuredSize = SizeF.Empty;
                _isBoundingBoxValid = true;
            }
        }

        public override void Draw(Graphics g, Grid grid)
        {
            if (string.IsNullOrEmpty(Text) || TextFont == null) return;

            EnsureBoundingBoxIsValid(g);

            using (Brush brush = new SolidBrush(IsSelected ? SelectedColor : ElementColor))
            {
                g.DrawString(Text, TextFont, brush, Location);
            }

            if (IsSelected && LineWidth > 0)
            {
                RectangleF bounds = _isBoundingBoxValid ? new RectangleF(Location, _measuredSize) : GetBoundingBoxWithTempGraphics();
                using (Pen selectPen = new Pen(SelectedColor, LineWidth) { DashStyle = DashStyle.Dot })
                {
                    g.DrawRectangle(selectPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
            }
        }

        public override RectangleF GetBoundingBox()
        {
            if (!_isBoundingBoxValid)
            {
                return GetBoundingBoxWithTempGraphics();
            }
            return new RectangleF(Location, _measuredSize);
        }

        private RectangleF GetBoundingBoxWithTempGraphics()
        {
            if (string.IsNullOrEmpty(Text) || TextFont == null)
                return new RectangleF(Location, SizeF.Empty);

            SizeF size;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                size = g.MeasureString(Text, TextFont);
            }
            _measuredSize = size;
            _isBoundingBoxValid = true;
            return new RectangleF(Location, size);
        }


        public override bool HitTest(PointF worldPoint, float tolerance)
        {
            RectangleF bounds = GetBoundingBox();
            return bounds.Inflate(tolerance, tolerance).Contains(worldPoint);
        }

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _textFont?.Dispose();
                    _textFont = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~TextElement()
        {
            Dispose(disposing: false);
        }
    }
}