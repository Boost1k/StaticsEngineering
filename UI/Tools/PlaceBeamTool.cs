// UI/Tools/PlaceBeamTool.cs
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Core;
using StaticsEngineeringCAD.Elements;

namespace StaticsEngineeringCAD.UI.Tools
{
    public class PlaceBeamTool : ITool
    {
        public DrawingPanel Panel { get; }
        public Cursor Cursor => Cursors.Cross;

        private PointF? _firstPointAbs; // Абсолютная первая точка (не обязательно узел)
        private BeamNode _firstPointNode;  // Если первая точка привязана к узлу
        private BeamElement _previewBeam;
        private BeamNode _targetNodePreview; // Узел, к которому привязывается текущая точка (вторая или первая)

        private const float NodeSnapDistance = 15f; // Максимальное расстояние для привязки к узлу

        public PlaceBeamTool(DrawingPanel panel)
        {
            Panel = panel;
        }

        public void Activate()
        {
            _firstPointAbs = null;
            _firstPointNode = null;
            _previewBeam = null;
            _targetNodePreview = null;
        }

        public void Deactivate()
        {
            Activate();
            Panel.Invalidate();
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF worldPoint = e.Location;
                _targetNodePreview = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, worldPoint, NodeSnapDistance);

                PointF currentPoint;
                BeamNode currentNode = null;

                if (_targetNodePreview != null) // Если попали в существующий узел
                {
                    currentPoint = _targetNodePreview.Location;
                    currentNode = _targetNodePreview;
                }
                else if (Panel.DrawingGrid.SnapToGrid) // Иначе, если есть сетка - к сетке
                {
                    currentPoint = Panel.DrawingGrid.SnapPoint(worldPoint);
                }
                else // Иначе - как есть
                {
                    currentPoint = worldPoint;
                }


                if (_firstPointAbs == null && _firstPointNode == null) // Первый клик
                {
                    if (currentNode != null)
                    {
                        _firstPointNode = currentNode; // Запомнили узел
                        _firstPointAbs = currentNode.Location;
                    }
                    else
                    {
                        _firstPointAbs = currentPoint; // Запомнили абсолютную точку
                    }

                    _previewBeam = new BeamElement(_firstPointAbs.Value)
                    {
                        ElementColor = Color.LightCoral,
                        LineWidth = 1.5f
                    };
                }
                else // Второй клик
                {
                    PointF startP = _firstPointNode?.Location ?? _firstPointAbs.Value;
                    PointF endP = currentNode?.Location ?? currentPoint;

                    BeamElement newBeam = new BeamElement(startP, endP);
                    Panel.CurrentScene.AddElement(newBeam);

                    // Если начальная точка была привязана к узлу, связываем элементы (если это разные балки)
                    if (_firstPointNode != null && _firstPointNode.ParentBeam != newBeam)
                    {
                        // пока не релиз
                    }
                    // Если конечная точка привязана к узлу
                    if (currentNode != null && currentNode.ParentBeam != newBeam)
                    {
                        // пока не релиз
                    }

                    Activate(); // Сбрасываем все состояние инструмента
                }
                Panel.Invalidate();
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            PointF currentMousePositionAbs = e.Location;
            _targetNodePreview = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, currentMousePositionAbs, NodeSnapDistance);

            PointF currentSnappingPoint;
            if (_targetNodePreview != null)
            {
                currentSnappingPoint = _targetNodePreview.Location;
            }
            else if (Panel.DrawingGrid.SnapToGrid)
            {
                currentSnappingPoint = Panel.DrawingGrid.SnapPoint(currentMousePositionAbs);
            }
            else
            {
                currentSnappingPoint = currentMousePositionAbs;
            }

            if (_previewBeam != null && (_firstPointAbs != null || _firstPointNode != null))
            {
                PointF previewStart = _firstPointNode?.Location ?? _firstPointAbs.Value;
                _previewBeam.UpdatePoints(previewStart, currentSnappingPoint);
                Panel.Invalidate();
            }
            else if (_firstPointAbs == null && _firstPointNode == null)
            {
                Panel.Invalidate(); // Чтобы перерисовать _targetNodePreview
            }
        }

        public void OnMouseUp(MouseEventArgs e) { }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Activate(); // Сброс инструмента
                Panel.Invalidate();
            }
        }

        public void OnKeyUp(KeyEventArgs e) { }

        public void DrawTemporary(Graphics g)
        {
            if (_previewBeam != null)
            {
                _previewBeam.Draw(g, Panel.DrawingGrid);
            }
            
            _targetNodePreview?.Draw(g, true);
        }
    }
}