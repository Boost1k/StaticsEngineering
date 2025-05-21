// UI/Tools/PlaceSupportTool.cs
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Core;
using StaticsEngineeringCAD.Elements;

namespace StaticsEngineeringCAD.UI.Tools
{
    public class PlaceSupportTool : ITool
    {
        private BeamNode _targetNodePreview; // Для подсветки узла, к которому будет привязка

        public DrawingPanel Panel { get; }
        public Cursor Cursor => Cursors.Hand; // Рука или другой подходящий курсор

        private SupportType _supportTypeToPlace;
        private SupportElement _previewSupport; // Для предварительного просмотра

        public PlaceSupportTool(DrawingPanel panel, SupportType supportType)
        {
            Panel = panel;
            _supportTypeToPlace = supportType;
        }

        public void Activate()
        {
            _previewSupport = CreateSupportElement(PointF.Empty, _supportTypeToPlace); // Создаем фантом
            if (_previewSupport != null)
            {
                _previewSupport.ElementColor = Color.FromArgb(128, _previewSupport.ElementColor); // Полупрозрачный
                _previewSupport.LineWidth = 1f;
            }

            _targetNodePreview = null;
        }

        public void Deactivate()
        {
            _previewSupport = null;
            _targetNodePreview = null;
            Panel.Invalidate();
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF worldPoint = e.Location; // Не привязываем к сетке, если ищем узел
                BeamNode targetNode = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, worldPoint, 15f); // 15f - макс. дистанция для привязки

                PointF placementPoint;
                SupportElement newSupport;

                if (targetNode != null)
                {
                    placementPoint = targetNode.Location; // Ставим точно на узел
                    newSupport = CreateSupportElement(placementPoint, _supportTypeToPlace);
                    if (newSupport != null)
                    {
                        Panel.CurrentScene.AddElement(newSupport);
                        targetNode.ParentBeam.AttachElementToNode(targetNode, newSupport); // Привязываем
                    }
                }
                else // Если узел не найден, ставим по сетке, как раньше
                {
                    if (Panel.DrawingGrid.SnapToGrid)
                    {
                        worldPoint = Panel.DrawingGrid.SnapPoint(e.Location); // Теперь используем e.Location
                    }
                    placementPoint = worldPoint;
                    newSupport = CreateSupportElement(placementPoint, _supportTypeToPlace);
                    if (newSupport != null)
                    {
                        Panel.CurrentScene.AddElement(newSupport);
                    }
                }
                Panel.Invalidate();
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            PointF currentMousePosition = e.Location;
            _targetNodePreview = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, currentMousePosition, 15f);

            if (_previewSupport != null)
            {
                if (_targetNodePreview != null)
                {
                    _previewSupport.Location = _targetNodePreview.Location;
                }
                else
                {
                    if (Panel.DrawingGrid.SnapToGrid)
                    {
                        currentMousePosition = Panel.DrawingGrid.SnapPoint(e.Location);
                    }
                    _previewSupport.Location = currentMousePosition;
                }
                Panel.Invalidate();
            }
        }

        public void OnMouseUp(MouseEventArgs e) {  }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _previewSupport = null;
                Panel.Invalidate();
            }
        }

        public void OnKeyUp(KeyEventArgs e) {  }

        public void DrawTemporary(Graphics g)
        {
            if (_previewSupport != null)
            {
                _previewSupport.Draw(g, Panel.DrawingGrid);
            }

            _targetNodePreview?.Draw(g, true);
        }

        private SupportElement CreateSupportElement(PointF location, SupportType type)
        {
            switch (type)
            {
                case SupportType.Pinned:
                    return new PinnedSupportElement(location);
                case SupportType.Roller:
                    return new RollerSupportElement(location);
                // case SupportType.Fixed: // Для будущего
                // return new FixedSupportElement(location);
                default:
                    return null;
            }
        }
    }
}