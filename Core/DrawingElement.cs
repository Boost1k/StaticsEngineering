// Core/DrawingElement.cs
using System;
using System.Drawing;
using StaticsEngineeringCAD.Elements;


namespace StaticsEngineeringCAD.Core
{
    public abstract class DrawingElement
    {
        private PointF _location;
        public PointF Location
        {
            get => _location;
            set
            {
                if (Math.Abs(_location.X - value.X) > 0.001f || Math.Abs(_location.Y - value.Y) > 0.001f)
                {
                    _location = value;
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        private Color _elementColor = Color.Black;
        public Color ElementColor
        {
            get => _elementColor;
            set
            {
                if (_elementColor != value)
                {
                    _elementColor = value;
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        private Color _selectedColor = Color.DodgerBlue;
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    // Если элемент выбран, и мы меняем цвет выбора, нужно перерисовать
                    if (IsSelected) OnPositionOrAppearanceChanged();
                }
            }
        }

        private float _lineWidth = 2f;
        public float LineWidth
        {
            get => _lineWidth;
            set
            {
                if (Math.Abs(_lineWidth - value) > 0.001f)
                {
                    _lineWidth = value;
                    OnPositionOrAppearanceChanged();
                }
            }
        }

        public object Tag { get; set; }

        /// <summary>
        /// Связанный текстовый элемент для отображения подписи/названия.
        /// </summary>
        public TextElement AssociatedTextLabel { get; set; }

        /// <summary>
        /// Событие, возникающее при изменении позиции или внешнего вида элемента,
        /// что может потребовать обновления связанных элементов (например, текстовых меток).
        /// </summary>
        public event EventHandler PositionOrAppearanceChanged;

        protected DrawingElement() { }

        protected DrawingElement(PointF location)
        {
            Location = location;
        }

        /// <summary>
        /// Генерирует событие PositionOrAppearanceChanged.
        /// Вызывается при изменении свойств, влияющих на отображение элемента или его текста.
        /// </summary>
        protected virtual void OnPositionOrAppearanceChanged()
        {
            PositionOrAppearanceChanged?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Draw(Graphics g, Grid grid);
        public abstract bool HitTest(PointF worldPoint, float tolerance);
        public abstract RectangleF GetBoundingBox();

        /// <summary>
        /// Перемещает элемент на указанный вектор.
        /// </summary>
        /// <param name="delta">Вектор смещения.</param>
        public virtual void Move(PointF delta)
        {
            // Сеттер Location вызовет OnPositionOrAppearanceChanged
            Location = new PointF(Location.X + delta.X, Location.Y + delta.Y);
        }

        /// <summary>
        /// Возвращает перо для отрисовки с учетом состояния выбора.
        /// </summary>
        protected Pen GetPen()
        {
            return new Pen(IsSelected ? SelectedColor : ElementColor, LineWidth);
        }
    }
}