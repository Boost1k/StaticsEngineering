// UI/RibbonFactory.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using StaticsEngineeringCAD.Properties;

namespace StaticsEngineeringCAD.UI
{
    public class TabMenuPanel
    {
        private MainForm mainForm;

        public TabControl TabControlRibbon { get; private set; }

        // --- Вкладки ---
        public ToolStrip ToolStripHome { get; private set; } // Вкладка "Главная"
        public ToolStrip ToolStripElements { get; private set; } // Вкладка "Элементы"
        public ToolStrip ToolStripAnalysis { get; private set; } // Вкладка "Анализ"

        // --- Кнопки вкладки "Главная" ---
        public ToolStripButton ToolBtnSelectTool { get; private set; } // Инструмент "указатель"
        public ToolStripButton ToolBtnToggleGrid { get; private set; } // Инструмент "сетка"
        public ToolStripButton ToolBtnGridSettings { get; private set; } // Инструмент "настройки сетки"

        // --- Кнопки вкладки "Элементы" ---
        public ToolStripButton ToolBtnAddBeam { get; private set; } // Инструмент "балка"
        public ToolStripButton ToolBtnAddPinnedSupport { get; private set; } // Инструмент "неподвижн. опора"
        public ToolStripButton ToolBtnAddRollerSupport { get; private set; } // Инструмент "подвижн. опора"
        public ToolStripButton ToolBtnAddForce { get; private set; } // Инструмент "сила"

        // --- Кнопки вкладки "Анализ" ---
        public ToolStripButton ToolBtnSolve { get; private set; } // Инструмент "Выполнить расчет"

        public TabMenuPanel(MainForm ownerForm)
        {
            mainForm = ownerForm;
            InitializeRibbon();
        }

        private void InitializeRibbon()
        {
            // --- Создание меню с множественными вкладками ---
            TabControlRibbon = new TabControl
            {
                Name = "tabControlRibbon",
                Dock = DockStyle.Top,
                Appearance = TabAppearance.Normal,
                Height = 65
            };

            CreateHomePage();
            CreateElementsPage();
            CreateAnalysisPage();
        }

        // --- Создание вкладки "Главная" ---
        private void CreateHomePage()
        {
            TabPage tabPageHome = new TabPage("Главная")
            {
                Name = "tabPageHome"
            };

            ToolStripHome = new ToolStrip
            {
                Name = "toolStripHome",
                Dock = DockStyle.Fill,
                GripStyle = ToolStripGripStyle.Hidden,
                ImageScalingSize = new Size(24, 24)
            };

            // Инструмент "Указатель" (Выбор)
            ToolBtnSelectTool = new ToolStripButton("Указатель")
            {
                Name = "toolBtnSelect",
                Checked = true,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.select_tool_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Выбрать и переместить элементы"
            };

            // Инструмент "Показать/Скрыть сетку"
            ToolBtnToggleGrid = new ToolStripButton("Сетка")
            {
                Name = "toolBtnToggleGrid",
                Checked = true,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.mesh_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Показать/Скрыть сетку"
            };

            // Инструмент "Настройки сетки"
            ToolBtnGridSettings = new ToolStripButton("Настр. сетки")
            {
                Name = "toolBtnGridSettings",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.mesh_settings_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Настроить параметры сетки"
            };

            ToolStripHome.Items.AddRange(new ToolStripItem[] {
                ToolBtnSelectTool,

                new ToolStripSeparator() // Разделитель
                {
                    Margin = new Padding(4, 2, 4, 2)
                },

                ToolBtnToggleGrid,
                ToolBtnGridSettings
            });

            tabPageHome.Controls.Add(ToolStripHome);
            TabControlRibbon.TabPages.Add(tabPageHome);
        }

        // --- Создание вкладки "Элементы" ---
        private void CreateElementsPage()
        {
            TabPage tabPageElements = new TabPage("Элементы")
            {
                Name = "tabPageElements"
            };

            ToolStripElements = new ToolStrip
            {
                Name = "toolStripElements",
                Dock = DockStyle.Fill,
                GripStyle = ToolStripGripStyle.Hidden,
                ImageScalingSize = new Size(24, 24)
            };

            // Инструмент "Балка"
            ToolBtnAddBeam = new ToolStripButton("Балка")
            {
                Name = "toolBtnBeam",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.beam_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Добавить элемент \"Балка\""
            };

            // Инструмент "Шарнирно-неподвижная опора"
            ToolBtnAddPinnedSupport = new ToolStripButton("Шарнирно-неподвижная опора")
            {
                Name = "toolBtnAddPinnedSupport",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.pinned_support_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Добавить элемент \"Шарнирно-неподвижная опора\""
            };

            // Инструмент "Шарнирно-подвижная опора"
            ToolBtnAddRollerSupport = new ToolStripButton("Шарнирно-подвижная опора")
            {
                Name = "toolBtnAddRollerSupport",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.roller_support_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Добавить элемент \"Шарнирно-подвижная опора\""
            };

            // Инструмент "Сосредоточенная сила"
            ToolBtnAddForce = new ToolStripButton("Сосредоточенная сила")
            {
                Name = "toolBtnAddForce",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.force_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Добавить элемент \"Сосредоточенная сила\""
            };

            ToolStripElements.Items.AddRange(new ToolStripItem[] {
                ToolBtnAddBeam,

                new ToolStripSeparator() // Разделитель
                {
                    Margin = new Padding(4, 2, 4, 2)
                },

                ToolBtnAddPinnedSupport,
                ToolBtnAddRollerSupport,

                new ToolStripSeparator() // Разделитель
                {
                    Margin = new Padding(4, 2, 4, 2)
                },

                ToolBtnAddForce
            });

            tabPageElements.Controls.Add(ToolStripElements);
            TabControlRibbon.TabPages.Add(tabPageElements);
        }

        // --- Создание вкладки "Анализ" ---
        private void CreateAnalysisPage()
        {
            TabPage tabPageAnalysis = new TabPage("Анализ")
            {
                Name = "tabPageAnalysis"
            };

            ToolStripAnalysis = new ToolStrip
            {
                Name = "toolStripAnalysis",
                Dock = DockStyle.Fill,
                GripStyle = ToolStripGripStyle.Hidden,
                ImageScalingSize = new Size(24, 24)
            };

            // Инструмент "Выполнить расчет"
            ToolBtnSolve = new ToolStripButton("Выполнить расчет")
            {
                Name = "toolBtnSolve",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = Resources.solve_icon,
                Margin = new Padding(4, 2, 4, 2),
                ToolTipText = "Выполнить статистический расчет схемы"
            };

            ToolStripAnalysis.Items.AddRange(new ToolStripItem[] {
                ToolBtnSolve
            });

            tabPageAnalysis.Controls.Add(ToolStripAnalysis);
            TabControlRibbon.TabPages.Add(tabPageAnalysis);
        }
    }
}