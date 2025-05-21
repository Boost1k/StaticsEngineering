// UI/DrawingPanel.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Core;
using StaticsEngineeringCAD.UI.Tools; // Добавим позже

namespace StaticsEngineeringCAD.UI
{
    public class DrawingPanel : Panel
    {
        public Grid DrawingGrid { get; private set; }
        public Scene CurrentScene { get; private set; }
        public ITool ActiveTool { get; set; } // Текущий активный инструмент

        public DrawingPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            DrawingGrid = new Grid();
            CurrentScene = new Scene();

            ActiveTool = new SelectTool(this); // Раскомментируем, когда создадим SelectTool
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (DrawingGrid != null && DrawingGrid.IsVisible)
            {
                DrawingGrid.Draw(g, this.ClientRectangle);
            }

            if (CurrentScene != null)
            {
                CurrentScene.Draw(g, DrawingGrid);
            }

            // Отрисовка временной графики от активного инструмента (например, "фантом" элемента)
            ActiveTool?.DrawTemporary(g);
        }

        // Передача событий мыши активному инструменту
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus(); // Для получения событий клавиатуры
            ActiveTool?.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ActiveTool?.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            ActiveTool?.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            ActiveTool?.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            ActiveTool?.OnKeyUp(e);
        }

        // Метод для смены инструмента
        public void SetTool(ITool tool)
        {
            ActiveTool?.Deactivate(); // Даем старому инструменту возможность "прибраться"
            ActiveTool = tool;
            ActiveTool?.Activate();   // Даем новому инструменту возможность инициализироваться

            this.Cursor = ActiveTool?.Cursor ?? Cursors.Default;
            this.Invalidate();
        }
    }
}