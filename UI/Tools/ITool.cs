// UI/Tools/ITool.cs
using System.Drawing;
using System.Windows.Forms;

namespace StaticsEngineeringCAD.UI.Tools
{
    public interface ITool
    {
        DrawingPanel Panel { get; }
        Cursor Cursor { get; }

        void Activate();    // Вызывается при активации инструмента
        void Deactivate();  // Вызывается при деактивации инструмента

        void OnMouseDown(MouseEventArgs e);
        void OnMouseMove(MouseEventArgs e);
        void OnMouseUp(MouseEventArgs e);
        void OnKeyDown(KeyEventArgs e);
        void OnKeyUp(KeyEventArgs e);
        void DrawTemporary(Graphics g); // Для отрисовки "фантомов" и т.п.
    }
}