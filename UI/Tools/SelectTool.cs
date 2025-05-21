// UI/Tools/SelectTool.cs
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Core;

namespace StaticsEngineeringCAD.UI.Tools
{
    public class SelectTool : ITool
    {
        public DrawingPanel Panel { get; }
        public Cursor Cursor => Cursors.Default;

        private DrawingElement _elementToDrag;
        private PointF _lastMousePosition;
        private bool _isDragging;

        public SelectTool(DrawingPanel panel)
        {
            Panel = panel;
        }

        public void Activate() { }
        public void Deactivate()
        {
            _elementToDrag = null;
            _isDragging = false;
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF worldPoint = e.Location;
                if (Panel.DrawingGrid.SnapToGrid)
                {
                    // Привязка к сетке для точки клика может быть не нужна для выбора,
                    // но может быть полезна для начала перетаскивания от узла.
                    // worldPoint = Panel.DrawingGrid.SnapPoint(worldPoint);
                }

                DrawingElement hitElement = Panel.CurrentScene.HitTest(worldPoint);

                if (hitElement != null)
                {
                    Panel.CurrentScene.SelectElement(hitElement);
                    _elementToDrag = hitElement;
                    _lastMousePosition = worldPoint;
                    _isDragging = true;
                }
                else
                {
                    Panel.CurrentScene.DeselectAll();
                    _elementToDrag = null;
                    _isDragging = false;
                }
                Panel.Invalidate();
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && _elementToDrag != null && e.Button == MouseButtons.Left)
            {
                PointF currentMousePosition = e.Location;
                PointF snappedCurrentMouse = Panel.DrawingGrid.SnapToGrid ?
                    Panel.DrawingGrid.SnapPoint(currentMousePosition) :
                    currentMousePosition;

                PointF snappedLastMouse = Panel.DrawingGrid.SnapToGrid ?
                    Panel.DrawingGrid.SnapPoint(_lastMousePosition) :
                    _lastMousePosition;

                // Если привязка к сетке, то двигаем только если курсор перешел на новый узел сетки
                if (!Panel.DrawingGrid.SnapToGrid || snappedCurrentMouse != snappedLastMouse)
                {
                    PointF delta = new PointF(snappedCurrentMouse.X - snappedLastMouse.X,
                        snappedCurrentMouse.Y - snappedLastMouse.Y);

                    _elementToDrag.Move(delta);
                    _lastMousePosition = currentMousePosition;

                    Panel.Invalidate();
                }
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && Panel.CurrentScene.SelectedElement != null)
            {
                Panel.CurrentScene.RemoveElement(Panel.CurrentScene.SelectedElement);
                Panel.Invalidate();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Panel.CurrentScene.DeselectAll();
                Panel.Invalidate();
            }
        }

        public void OnKeyUp(KeyEventArgs e) {  }

        public void DrawTemporary(Graphics g) {  }
    }
}