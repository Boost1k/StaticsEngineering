// UI/Tools/PlaceForceTool.cs
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Core;
using StaticsEngineeringCAD.Elements;
using StaticsEngineeringCAD.UI.Dialogs;

namespace StaticsEngineeringCAD.UI.Tools
{
    public class PlaceForceTool : ITool
    {
        private BeamNode _targetNodePreview;

        public DrawingPanel Panel { get; }
        public Cursor Cursor => Cursors.Cross;

        private ForceElement _previewForce; // Фантом для стрелки
        private TextElement _previewText; // Фантом для текста
        private Font _defaultTextFont = new Font("Arial", 10);

        private float _defaultMagnitude = 100f; // Значения по умолчанию для диалога
        private float _defaultAngle = 270f;

        public PlaceForceTool(DrawingPanel panel)
        {
            Panel = panel;
        }

        public void Activate()
        {
            _previewForce = new ForceElement(PointF.Empty, _defaultMagnitude, _defaultAngle)
            {
                ElementColor = Color.FromArgb(128, Color.BlueViolet),
                LineWidth = 1f
            };

            string previewForceText = $"{_defaultMagnitude:F0}Н";
            PointF initialTextPos = Panel.FindForm() is MainForm mf ?
                mf.CalculateTextPositionForForce(_previewForce, previewForceText, _defaultTextFont) :
                PointF.Empty;

            _previewText = new TextElement(initialTextPos, previewForceText)
            {
                ElementColor = _previewForce.ElementColor,
                LineWidth = 0
            };
            _previewForce.AssociatedTextElement = _previewText;
        }

        public void Deactivate()
        {
            _previewForce = null;
            _targetNodePreview = null;
            Panel.Invalidate();
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF worldPoint = e.Location;
                BeamNode targetNode = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, worldPoint, 15f);
                PointF placementPoint;

                if (targetNode != null)
                {
                    placementPoint = targetNode.Location;
                }
                else if (Panel.DrawingGrid.SnapToGrid)
                {
                    placementPoint = Panel.DrawingGrid.SnapPoint(worldPoint);
                }
                else
                {
                    placementPoint = worldPoint;
                }

                using (ForceInputDialog dialog = new ForceInputDialog(_defaultMagnitude, _defaultAngle))
                {
                    if (dialog.ShowDialog(Panel.FindForm()) == DialogResult.OK)
                    {
                        // Создаем реальный ForceElement
                        ForceElement newForce = new ForceElement(placementPoint, dialog.ForceMagnitude, dialog.ForceAngleDegrees);
                        Panel.CurrentScene.AddElement(newForce);
                        if (targetNode != null)
                        {
                            targetNode.ParentBeam.AttachElementToNode(targetNode, newForce);
                        }

                        // Создаем реальный TextElement для этой силы
                        string forceText = $"{newForce.Magnitude:F0}Н";
                        PointF textLocation = Panel.FindForm() is MainForm mainForm ? 
                            mainForm.CalculateTextPositionForForce(newForce, forceText, _defaultTextFont) :
                            new PointF(placementPoint.X + 10, placementPoint.Y - 10);

                        TextElement newText = new TextElement(textLocation, forceText)
                        {
                            ElementColor = newForce.ElementColor,
                            SelectedColor = newForce.SelectedColor
                        };
                        Panel.CurrentScene.AddElement(newText);

                        // Связываем их
                        newForce.AssociatedTextElement = newText;

                        _defaultMagnitude = dialog.ForceMagnitude;
                        _defaultAngle = dialog.ForceAngleDegrees;

                        // Обновляем фантомы для следующего раза
                        if (_previewForce != null)
                        {
                            _previewForce.Magnitude = _defaultMagnitude;
                            _previewForce.AngleDegrees = _defaultAngle;
                            if (_previewText != null)
                            {
                                _previewText.Text = $"{_defaultMagnitude:F0}Н";
                            }
                        }
                        Panel.Invalidate();
                    }
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            PointF currentMousePosition = e.Location;
            _targetNodePreview = SceneHelpers.FindClosestBeamNode(Panel.CurrentScene, currentMousePosition, 15f);

            if (_previewForce != null)
            {
                PointF previewForceLocation;
                if (_targetNodePreview != null)
                {
                    previewForceLocation = _targetNodePreview.Location;
                }
                else if (Panel.DrawingGrid.SnapToGrid)
                {
                    previewForceLocation = Panel.DrawingGrid.SnapPoint(currentMousePosition);
                }
                else
                {
                    previewForceLocation = currentMousePosition;
                }
                _previewForce.Location = previewForceLocation;

                if (_previewText != null)
                {
                    _previewText.Text = $"{_previewForce.Magnitude:F0}Н";

                    _previewText.Location = Panel.FindForm() is MainForm mainForm ?
                        mainForm.CalculateTextPositionForForce(_previewForce, _previewText.Text, _previewForce.AssociatedTextElement.TextFont) :
                        new PointF(previewForceLocation.X + 10, previewForceLocation.Y - 10);
                }
                Panel.Invalidate();
            }
        }

        public void DrawTemporary(Graphics g)
        {
            _previewForce?.Draw(g, Panel.DrawingGrid);
            _previewText?.Draw(g, Panel.DrawingGrid);
            _targetNodePreview?.Draw(g, true);
        }

        public void OnMouseUp(MouseEventArgs e) {  }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _previewForce = null;
                Panel.Invalidate();
            }
        }

        public void OnKeyUp(KeyEventArgs e) {  }
    }
}