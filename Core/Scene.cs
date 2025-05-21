// Core/Scene.cs
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StaticsEngineeringCAD.UI;

namespace StaticsEngineeringCAD.Core
{
    public class Scene
    {
        public List<DrawingElement> Elements { get; private set; }
        public DrawingElement SelectedElement { get; private set; }
        private DrawingPanel _drawingPanel;

        public Scene(DrawingPanel ownerPanel)
        {
            Elements = new List<DrawingElement>();
            _drawingPanel = ownerPanel;
        }

        public void AddElement(DrawingElement element)
        {
            if (element != null && !Elements.Contains(element))
            {
                Elements.Add(element);
                _drawingPanel?.SubscribeToElementEvents(element);
            }
        }

        public void RemoveElement(DrawingElement element)
        {
            if (element != null && Elements.Contains(element))
            {
                _drawingPanel?.UnsubscribeFromElementEvents(element);
                Elements.Remove(element);

                if (SelectedElement == element)
                {
                    SelectedElement = null;
                }
            }
        }

        public void Draw(Graphics g, Grid grid)
        {
            foreach (var element in Elements)
            {
                element.Draw(g, grid);
            }
        }

        public DrawingElement HitTest(PointF worldPoint, float tolerance = 5f)
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                if (Elements[i].HitTest(worldPoint, tolerance))
                {
                    return Elements[i];
                }
            }
            return null;
        }

        public void SelectElement(DrawingElement element)
        {
            if (SelectedElement != null)
            {
                SelectedElement.IsSelected = false;
            }
            SelectedElement = element;
            if (SelectedElement != null)
            {
                SelectedElement.IsSelected = true;
            }
        }

        public void DeselectAll()
        {
            if (SelectedElement != null)
            {
                SelectedElement.IsSelected = false;
            }
            SelectedElement = null;
        }

        public void Clear()
        {
            var elementsToRemove = new List<DrawingElement>(Elements);
            foreach (var element in elementsToRemove)
            {
                RemoveElement(element);
            }
            SelectedElement = null;
        }
    }
}