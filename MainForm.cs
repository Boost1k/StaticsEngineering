// MainForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.UI;
using StaticsEngineeringCAD.UI.Tools;
using StaticsEngineeringCAD.Elements;
using StaticsEngineeringCAD.Analysis;
using StaticsEngineeringCAD.Core;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Linq;

namespace StaticsEngineeringCAD
{
    public partial class MainForm : Form
    {
        private TabMenuPanel tabMenuPanel;
        private DrawingPanel drawingPanelMain;

        private ToolStripButton _currentActiveToolButton = null;

        private List<DrawingElement> _reactionForceElements = new List<DrawingElement>();

        public MainForm()
        {
            InitializeFormLayout();
            InitializeDefaultSettings();
            SubscribeToEvents();

            this.KeyPreview = true;
        }

        private void InitializeFormLayout()
        {
            this.SuspendLayout(); // Приостановить компоновку

            this.Text = "Решение задач статитики | Обучение теоретической механике";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- Создание панели инструментов ---
            tabMenuPanel = new TabMenuPanel(this);

            // --- Создание DrawingPanel ---
            drawingPanelMain = new DrawingPanel
            {
                Name = "drawingPanelMain",
                Dock = DockStyle.Fill
            };

            // --- Добавление контролов на форму ---
            this.Controls.Add(drawingPanelMain);
            this.Controls.Add(tabMenuPanel.TabControlRibbon);

            this.ResumeLayout(false);
            this.PerformLayout();

            drawingPanelMain?.SetTool(new SelectTool(drawingPanelMain));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                if (drawingPanelMain?.ActiveTool != null)
                {
                    drawingPanelMain.ActiveTool.OnKeyDown(e);
                }

                if (_currentActiveToolButton != null && _currentActiveToolButton != tabMenuPanel.ToolBtnSelectTool)
                {
                    _currentActiveToolButton.Checked = false;
                }
                ActivateDefaultTool();
                _currentActiveToolButton = tabMenuPanel.ToolBtnSelectTool;

                e.Handled = true;
            }
        }

        private void InitializeDefaultSettings()
        {
            // Проверяем, что сетка в drawingPanelMain знает о состоянии кнопки
            if (drawingPanelMain != null && drawingPanelMain.DrawingGrid != null)
            {
                drawingPanelMain.DrawingGrid.IsVisible = tabMenuPanel.ToolBtnToggleGrid.Checked;
            }

            // Добавление тестовых элементов
            if (drawingPanelMain != null && drawingPanelMain.CurrentScene != null)
            {
                drawingPanelMain.CurrentScene.AddElement(new PointElement(new PointF(100, 100)));
                drawingPanelMain.CurrentScene.AddElement(new PointElement(new PointF(200, 150)));
                drawingPanelMain.CurrentScene.AddElement(new PointElement(new PointF(150, 250)));
            }
        }

        // --- Слушатели событий ---
        private void SubscribeToEvents()
        {
            if (tabMenuPanel == null) return;

            // --- Слушатели событий инструментов вкладки "Главная" ---

            tabMenuPanel.ToolBtnSelectTool.Click += ToolButton_Click;
            tabMenuPanel.ToolBtnToggleGrid.Click += ToolBtnToggleGrid_Click;
            tabMenuPanel.ToolBtnGridSettings.Click += ToolButton_Click;

            // --- Слушатели событий инструментов вкладки "Элементы" ---

            tabMenuPanel.ToolBtnAddBeam.Click += ToolButton_Click;
            tabMenuPanel.ToolBtnAddPinnedSupport.Click += ToolButton_Click;
            tabMenuPanel.ToolBtnAddRollerSupport.Click += ToolButton_Click;
            tabMenuPanel.ToolBtnAddForce.Click += ToolButton_Click;

            // --- Слушатели событий инструментов вкладки "Анализ" ---
            tabMenuPanel.ToolBtnSolve.Click += ToolBtnCalculate_Click;
        }

        // --- Обработчики событий ---

        // Обработчик кнопок-инструментов
        private void ToolButton_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton clickedButton)) return;
            if (drawingPanelMain == null) return;

            // Проверяем, является ли нажатая кнопка уже активной
            if (clickedButton == _currentActiveToolButton && clickedButton.Checked)
            {
                clickedButton.Checked = false;
                _currentActiveToolButton = null;
                drawingPanelMain.SetTool(new SelectTool(drawingPanelMain));
                ActivateDefaultTool();
            }
            else
            {
                // Клик по новой кнопке или по неактивной кнопке: активируем инструмент
                ITool newTool = GetToolForButton(clickedButton);
                if (newTool != null)
                {
                    if (_currentActiveToolButton != null && _currentActiveToolButton != clickedButton)
                    {
                        _currentActiveToolButton.Checked = false;
                    }

                    drawingPanelMain.SetTool(newTool);
                    clickedButton.Checked = true;
                    _currentActiveToolButton = clickedButton;
                }
                else
                {
                    if (_currentActiveToolButton != null) _currentActiveToolButton.Checked = false;
                    _currentActiveToolButton = null;
                    ActivateDefaultTool();
                }
            }
        }

        private ITool GetToolForButton(ToolStripButton button)
        {
            if (button == tabMenuPanel.ToolBtnSelectTool)
                return new SelectTool(drawingPanelMain);
            if (button == tabMenuPanel.ToolBtnAddBeam)
                return new PlaceBeamTool(drawingPanelMain);
            if (button == tabMenuPanel.ToolBtnAddPinnedSupport)
                return new PlaceSupportTool(drawingPanelMain, SupportType.Pinned);
            if (button == tabMenuPanel.ToolBtnAddRollerSupport)
                return new PlaceSupportTool(drawingPanelMain, SupportType.Roller);
            if (button == tabMenuPanel.ToolBtnAddForce)
                return new PlaceForceTool(drawingPanelMain);
            return null;
        }

        private void ActivateDefaultTool()
        {
            if (drawingPanelMain != null && tabMenuPanel?.ToolBtnSelectTool != null)
            {
                drawingPanelMain.SetTool(new SelectTool(drawingPanelMain));
                tabMenuPanel.ToolBtnSelectTool.Checked = true;
                _currentActiveToolButton = tabMenuPanel.ToolBtnSelectTool;
            }
        }

        // Обработчик инструмента "Показать/Скрыть сетку"
        private void ToolBtnToggleGrid_Click(object sender, EventArgs e)
        {
            if (drawingPanelMain != null && drawingPanelMain.DrawingGrid != null && tabMenuPanel.ToolBtnToggleGrid != null)
            {
                drawingPanelMain.DrawingGrid.IsVisible = !tabMenuPanel.ToolBtnToggleGrid.Checked;
                tabMenuPanel.ToolBtnToggleGrid.Checked = !tabMenuPanel.ToolBtnToggleGrid.Checked;
                drawingPanelMain.Invalidate();
            }
        }

        // Обработчик инструмента "Выполнить расчет"
        private void ToolBtnCalculate_Click(object sender, EventArgs e)
        {
            if (drawingPanelMain == null || drawingPanelMain.CurrentScene == null) return;

            foreach (var reactionElement in _reactionForceElements)
            {
                drawingPanelMain.CurrentScene.RemoveElement(reactionElement);
            }
            _reactionForceElements.Clear();

            StaticsSolver solver = new StaticsSolver();
            GlobalCalculactionResult results = solver.CalculateReactions(drawingPanelMain.CurrentScene);

            if (results.IsSolved)
            {
                string resultText = "Расчет успешно выполнен.\n";
                foreach (var resForSupport in results.SupportReactions)
                {
                    string supportName = "Опора"; // Имя по умолчанию

                    // Пытаемся дать опоре имя (A, B, C...) на основе ее порядка на балке
                    if (resForSupport.Support.AttachedToNode?.ParentBeam != null)
                    {
                        BeamElement currentBeam = resForSupport.Support.AttachedToNode.ParentBeam;

                        // Находим все опоры, привязанные к ЭТОЙ ЖЕ балке
                        var supportsOnThisBeam = drawingPanelMain.CurrentScene.Elements
                            .OfType<SupportElement>()
                            .Where(s => s.AttachedToNode?.ParentBeam == currentBeam)
                            .OrderBy(s => s.Location.X) // Упорядочиваем по X
                            .ToList();

                        if (supportsOnThisBeam.Count > 0)
                        {
                            int supportIndex = supportsOnThisBeam.IndexOf(resForSupport.Support);
                            if (supportIndex != -1) // Если опора найдена в списке (должна быть)
                            {
                                supportName = "R" + ((char)('A' + supportIndex)).ToString(); // Имя типа RA, RB
                            }
                        }
                    }

                    resultText += $"{supportName}x: {resForSupport.Rx:F2} Н, {supportName}y: {resForSupport.Ry:F2} Н";
                    if (Math.Abs(resForSupport.Mz) > 0.01f)
                    {
                        resultText += $", {supportName}z: {resForSupport.Mz:F2} Н*м";
                    }
                    resultText += "\n";
                }

                MessageBox.Show(resultText, "Результаты расчета", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayReactionsOnPanel(results); // Передаем results, а не отдельные значения
                drawingPanelMain.Invalidate();
            }
            else
            {
                MessageBox.Show("Ошибка или схема не может быть решена текущим методом: \n" + results.Message,
                                "Ошибка расчета", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayReactionsOnPanel(GlobalCalculactionResult results)
        {
            float reactionArrowScreenLength = 35f;
            Color reactionColor = Color.Crimson;

            foreach (var resForSupport in results.SupportReactions)
            {
                PointF supportLocation = resForSupport.Support.Location;
                string supportLabelPrefix = "R"; // Базовый префикс

                if (resForSupport.Support.AttachedToNode?.ParentBeam != null)
                {
                    BeamElement currentBeam = resForSupport.Support.AttachedToNode.ParentBeam;
                    var supportsOnThisBeam = drawingPanelMain.CurrentScene.Elements
                        .OfType<SupportElement>()
                        .Where(s => s.AttachedToNode?.ParentBeam == currentBeam)
                        .OrderBy(s => s.Location.X)
                        .ToList();

                    if (supportsOnThisBeam.Any()) // Проверка, что список не пуст
                    {
                        int index = supportsOnThisBeam.IndexOf(resForSupport.Support);
                        if (index != -1)
                        {
                            supportLabelPrefix = "R" + ((char)('A' + index)).ToString();
                        }
                    }
                }

                // --- Реакция Rx ---

                if (Math.Abs(resForSupport.Rx) > 0.01f)
                {
                    float angleRx = resForSupport.Rx >= 0 ? 0 : 180;

                    ForceElement rxElement = new ForceElement(supportLocation, Math.Abs(resForSupport.Rx), angleRx)
                    {
                        ElementColor = reactionColor,
                        SelectedColor = reactionColor,
                        ArrowScreenLength = reactionArrowScreenLength,
                        LineWidth = 1.5f
                    };

                    drawingPanelMain.CurrentScene.AddElement(rxElement);
                    _reactionForceElements.Add(rxElement);

                    string rxText = $"{supportLabelPrefix}x: {resForSupport.Rx:F1}";
                    PointF rxTextLocation = CalculateTextPositionForForce(rxElement, rxText, new Font("Arial", 10));
                    TextElement rxTextElement = new TextElement(rxTextLocation, rxText)
                    {
                        ElementColor = reactionColor,
                        SelectedColor = reactionColor
                    };

                    drawingPanelMain.CurrentScene.AddElement(rxTextElement);
                    _reactionForceElements.Add(rxTextElement);
                }

                // ---

                // --- Реакция Ry --- 

                if (Math.Abs(resForSupport.Ry) > 0.01f)
                {
                    float angleRy = resForSupport.Ry >= 0 ? 90 : 270;

                    ForceElement ryElement = new ForceElement(supportLocation, Math.Abs(resForSupport.Ry), angleRy)
                    {
                        ElementColor = reactionColor,
                        SelectedColor = reactionColor,
                        ArrowScreenLength = reactionArrowScreenLength,
                        LineWidth = 1.5f
                    };

                    drawingPanelMain.CurrentScene.AddElement(ryElement);
                    _reactionForceElements.Add(ryElement);

                    string ryText = $"{supportLabelPrefix}y: {resForSupport.Ry:F1}";
                    PointF ryTextLocation = CalculateTextPositionForForce(ryElement, ryText, new Font("Arial", 10));
                    TextElement ryTextElement = new TextElement(ryTextLocation, ryText)
                    {
                        ElementColor = reactionColor,
                        SelectedColor = reactionColor
                    };
                    drawingPanelMain.CurrentScene.AddElement(ryTextElement);
                    _reactionForceElements.Add(ryTextElement);
                }

                // ---

                // Реактивный момент Mz
                if (Math.Abs(resForSupport.Mz) > 0.01f)
                {
                    
                }

                // ---
            }
        }

        public PointF CalculateTextPositionForForce(ForceElement forceElement, string text, Font font)
        {
            if (forceElement == null || string.IsNullOrEmpty(text))
                return PointF.Empty;

            SizeF textSize;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics gMeasure = Graphics.FromImage(bmp))
            {
                textSize = gMeasure.MeasureString(text, font);
            }

            PointF anchorPoint = forceElement.GetTailPoint(); // Размещаем текст у хвоста стрелки
            float vectorAngleDegrees = forceElement.AngleDegrees;
            float offset = forceElement.LineWidth + 5; // Отступ от стрелки

            // Нормализуем угол к 0-360
            vectorAngleDegrees = (vectorAngleDegrees % 360 + 360) % 360;

            if (vectorAngleDegrees >= 315 || vectorAngleDegrees < 45) // Вектор вправо (текст слева от хвоста)
                return new PointF(anchorPoint.X - textSize.Width - offset, anchorPoint.Y - textSize.Height / 2f);
            else if (vectorAngleDegrees >= 45 && vectorAngleDegrees < 135) // Вектор вверх (текст снизу от хвоста)
                return new PointF(anchorPoint.X - textSize.Width / 2f, anchorPoint.Y + offset);
            else if (vectorAngleDegrees >= 135 && vectorAngleDegrees < 225) // Вектор влево (текст справа от хвоста)
                return new PointF(anchorPoint.X + offset, anchorPoint.Y - textSize.Height / 2f);
            else // (vectorAngleDegrees >= 225 && vectorAngleDegrees < 315) // Вектор вниз (текст сверху от хвоста)
                return new PointF(anchorPoint.X - textSize.Width / 2f, anchorPoint.Y - textSize.Height - offset);
        }
    }

    public class CustomColors : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => SystemColors.Control;
        public override Color ToolStripGradientMiddle => SystemColors.Control;
        public override Color ToolStripGradientEnd => SystemColors.Control;
        public override Color MenuStripGradientBegin => SystemColors.Control;
        public override Color MenuStripGradientEnd => SystemColors.Control;
        public override Color MenuItemBorder => SystemColors.ControlDark;
        public override Color MenuItemSelectedGradientBegin => SystemColors.Highlight;
        public override Color MenuItemSelectedGradientEnd => SystemColors.Highlight;
        public override Color ButtonSelectedHighlight => SystemColors.Highlight;
        public override Color ButtonSelectedGradientBegin => SystemColors.Highlight;
        public override Color ButtonSelectedGradientEnd => SystemColors.Highlight;
        public override Color ButtonPressedHighlight => SystemColors.HotTrack;
        public override Color ButtonPressedGradientBegin => SystemColors.HotTrack;
        public override Color ButtonPressedGradientEnd => SystemColors.HotTrack;
    }
}