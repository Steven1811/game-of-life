using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using TGUI;

namespace Game_of_Life
{
    class Gui
    {
        // Render window
        protected RenderWindow window;

        // GUI Elements
        protected TGUI.Gui gui;
        protected Panel bottomPanel;
        protected Button runButton;
        protected Button stepButton;
        protected Slider speedSlider;
        protected Label speedLabel;
        protected MenuBar menuBar;
        protected MessageBox aboutMessageBox;
        protected MessageBox helpMessageBox;
        protected MessageBox newFileMessageBox;
        protected TextBox gridWidthTextBox;
        protected TextBox gridHeightTextBox;
        
        // Strings
        // Menu
        private const string MENU_FILE = "File";
        private const string MENU_NEW = "New... (F2)";
        private const string MENU_LOAD = "Load... (F3)";
        private const string MENU_SAVE = "Save... (F4)";
        private const string MENU_CLEAR_GRID = "Clear grid";
        private const string MENU_EXIT = "Exit (F12)";
        private const string MENU_VIEW = "View";
        private const string MENU_RESET_VIEW = "Reset view";
        private const string MENU_OPTIONS = "Options";
        private const string MENU_VSYNC_ENABLED = "Enable VSync ✓";
        private const string MENU_VSYNC_DISABLED = "Enable VSync";
        private const string MENU_FPS_ENABLED = "Show FPS ✓";
        private const string MENU_FPS_DISABLED = "Show FPS";
        private const string MENU_EXTRA = "Extra";
        private const string MENU_HELP = "Help (F1)";
        private const string MENU_ABOUT = "About";

        public Gui(RenderWindow window)
        {
            this.window = window;
            this.window.Resized += OnWindowResized;
            // TODO: BabyBlue Theme disabled, due to missing textures
            // Theme blueTheme = new Theme("assets/themes/BabyBlue.txt");
            // Theme.Default = blueTheme;
            this.gui = new TGUI.Gui(this.window);
            this.gui.TabKeyUsageEnabled = false;
            this.BuildGui();
        }

        private void BuildGui()
        {
            // Bottom panel
            this.bottomPanel = new Panel(this.window.Size.X, 32f);
            this.bottomPanel.PositionLayout = new Layout2d("0f", "100% - height");
            this.bottomPanel.SizeLayout = new Layout2d("100%", "32f");
            this.gui.Add(bottomPanel, "BottomPanel");

            // Run Button
            this.runButton = new Button("Run");
            this.runButton.Clicked += OnRunButtonClicked;
            this.runButton.PositionLayout = new Layout2d("20f", "50% - (height / 2)");
            this.bottomPanel.Add(this.runButton, "RunButton");

            // Step Button
            this.stepButton = new Button("Step");
            this.stepButton.Clicked += OnStepButtonClicked;
            this.stepButton.PositionLayout = new Layout2d("RunButton.right + 10f", "50% - (height / 2)");
            this.bottomPanel.Add(this.stepButton, "StepButton");

            // Speed Slider
            this.speedSlider = new Slider(1f, 10f);
            this.speedSlider.Value = 1f;
            this.speedSlider.Step = 1f;
            this.speedSlider.PositionLayout = new Layout2d("100% - width - 20f", "50% - (height / 2)");
            this.speedSlider.ValueChanged += OnSpeedSliderValueChanged;
            this.bottomPanel.Add(this.speedSlider, "SpeedSlider");

            // Speed label
            this.speedLabel = new Label(String.Format("Speed: {0}", this.speedSlider.Value));
            this.speedLabel.PositionLayout = new Layout2d("SpeedSlider.left - width - 10f", "50% - (height / 2)");
            this.bottomPanel.Add(this.speedLabel);

            // Menu Bar
            this.menuBar = new MenuBar();
            this.menuBar.AddMenu(MENU_FILE);
            this.menuBar.AddMenuItem(MENU_NEW);
            this.menuBar.AddMenuItem(MENU_LOAD);
            this.menuBar.AddMenuItem(MENU_SAVE);
            this.menuBar.AddMenuItem(MENU_CLEAR_GRID);
            this.menuBar.AddMenuItem(MENU_EXIT);
            this.menuBar.AddMenu(MENU_VIEW);
            this.menuBar.AddMenuItem(MENU_RESET_VIEW);
            this.menuBar.AddMenu(MENU_OPTIONS);
            this.menuBar.AddMenuItem(MENU_VSYNC_ENABLED);
            this.menuBar.AddMenuItem(MENU_FPS_ENABLED);
            this.menuBar.AddMenu(MENU_EXTRA);
            this.menuBar.AddMenuItem(MENU_HELP);
            this.menuBar.AddMenuItem(MENU_ABOUT);
            this.menuBar.MenuItemClicked += OnMenuItemClicked;
            this.gui.Add(this.menuBar, "MenuBar");

            // About Message Box
            this.aboutMessageBox = new MessageBox("About", "Made by:\n▪ Steven - GUI, Graphic Programming\n▪ Lion - Cell Simulation Algorithm Programming\n\nUsed Libraries:\n▪ SFML.Net v2.4\n▪ TGUI.Net v0.8 \n▪ Newtonsoft Json.NET\n\nProject for AS3", new string[] { "Close" });
            this.aboutMessageBox.PositionLayout = new Layout2d("50% - (width / 2)", "50% - (height / 2)");
            this.aboutMessageBox.Visible = false;
            this.aboutMessageBox.Enabled = false;
            this.aboutMessageBox.PositionLocked = true;
            this.aboutMessageBox.ButtonPressed += OnAboutMessageBoxButtonPressed;
            this.gui.Add(this.aboutMessageBox, "AboutUsMessageBox");

            // Help Message Box
            this.helpMessageBox = new MessageBox("Help", "▪ Hold the left mouse button to draw alive cells.\n▪ Hold the right mouse button to erase alive cells (Mark them as dead).\n▪ Hold the center mouse button and move the mouse to move the view.\n▪ Scroll the mouse wheel to zoom in/out.\n▪ WARNING: When setting the speed slider to 10 the simulation thread will not sleep and run at full speed.\n▪ Press F3 to create a quick save.\n▪ Press F4 to quick load.", new string[] { "Close" });
            this.helpMessageBox.PositionLayout = new Layout2d("50% - (width / 2)", "50% - (height / 2)");
            this.helpMessageBox.Visible = false;
            this.helpMessageBox.Enabled = false;
            this.helpMessageBox.PositionLocked = true;
            this.helpMessageBox.ButtonPressed += OnHelpMessageBoxButtonPressed;
            this.gui.Add(this.helpMessageBox, "HelpMessageBox");

            // File -> New... Window
            this.newFileMessageBox = new MessageBox("New Project", "", new string[] { "Ok", "Cancel" });
            this.newFileMessageBox.Visible = false;
            this.newFileMessageBox.Enabled = false;
            this.newFileMessageBox.PositionLocked = true;
            this.gui.Add(this.newFileMessageBox, "NewFileChildWindow");

            // He
            this.gridWidthTextBox = new TextBox();
            this.gridWidthTextBox.MaximumCharacters = 4;
            this.gridWidthTextBox.SizeLayout = new Layout2d("40f", "20f");
            this.newFileMessageBox.Add(this.gridWidthTextBox, "GridWidthTextBox");

            this.gridHeightTextBox = new TextBox();
            this.gridHeightTextBox.MaximumCharacters = 4;
            this.gridHeightTextBox.SizeLayout = new Layout2d("40f", "20f");
            this.newFileMessageBox.Add(this.gridHeightTextBox, "GridHeightTextBox");
        }

        private void OnRunButtonClicked(object sender, SignalArgsVector2f e)
        {
            if (this.RunClicked != null)
            {
                this.RunClicked(this, new EventArgs());
            }
        }

        private void OnStepButtonClicked(object sender, SignalArgsVector2f e)
        {
            if (this.StepClicked != null)
            {
                this.StepClicked(this, new EventArgs());
            }
        }

        protected void OnWindowResized(object sender, SizeEventArgs args)
        {
            if (sender is RenderWindow)
            {
                FloatRect visibleArea = new FloatRect(0f, 0f, args.Width, args.Height);
                this.gui.View = new View(visibleArea); ;
            }
        }

        private void OnHelpMessageBoxButtonPressed(object sender, SignalArgsString args)
        {
            if (args.Value == "Close")
            {
                this.HideHelpMessageBox();
            }
        }

        private void OnSpeedSliderValueChanged(object sender, SignalArgsFloat args)
        {
            this.speedLabel.Text = String.Format("Speed: {0}", args.Value);

            if (this.SpeedChanged != null)
            {
                this.SpeedChanged(this, args);
            }
        }

        private void OnAboutMessageBoxButtonPressed(object sender, SignalArgsString args)
        {
            if (args.Value == "Close")
            {
                this.HideAboutMessageBox();
            }
        }

        private void OnMenuItemClicked(object sender, SignalArgsString args)
        {
            switch (args.Value)
            {
                case MENU_ABOUT:
                    this.ShowAboutMessageBox();
                    break;
                case MENU_HELP:
                    this.ShowHelpMessageBox();
                    break;
                case MENU_CLEAR_GRID:
                    if (this.GridClearClicked != null)
                    {
                        this.GridClearClicked(this, new EventArgs());
                    }
                    break;
                case MENU_RESET_VIEW:
                    if (this.ViewReset != null)
                    {
                        this.ViewReset(this, new EventArgs());
                    }
                    break;
                case MENU_EXIT:
                    Environment.Exit(0);
                    break;
            }
        }

        public void ShowAboutMessageBox()
        {
            this.aboutMessageBox.Enabled = true;
            this.aboutMessageBox.ShowWithEffect(ShowAnimationType.Fade, Time.FromMilliseconds(500));
            this.DisableControl();
        }
        public void HideAboutMessageBox()
        {
            this.aboutMessageBox.Enabled = false;
            this.aboutMessageBox.HideWithEffect(ShowAnimationType.Fade, Time.FromMilliseconds(500));
            this.EnableControl();
        }

        public void ShowHelpMessageBox()
        {
            this.helpMessageBox.Enabled = true;
            this.helpMessageBox.ShowWithEffect(ShowAnimationType.Fade, Time.FromMilliseconds(500));
            this.DisableControl();
        }

        public void HideHelpMessageBox()
        {
            this.helpMessageBox.Enabled = false;
            this.helpMessageBox.HideWithEffect(ShowAnimationType.Fade, Time.FromMilliseconds(500));
            this.EnableControl();
        }

        protected void DisableControl()
        {
            this.menuBar.Enabled = false;
            this.bottomPanel.Enabled = false;
            if (this.ControlDisabled != null)
            {
                this.ControlDisabled(this, new EventArgs());
            }
        }

        protected void EnableControl()
        {
            this.menuBar.Enabled = true;
            this.bottomPanel.Enabled = true;
            if (this.ControlEnabled != null)
            {
                this.ControlEnabled(this, new EventArgs());
            }
        }

        public void Draw()
        {
            this.gui.Draw();
        }

        public event EventHandler<EventArgs> RunClicked;
        public event EventHandler<EventArgs> StepClicked;
        public event EventHandler<SignalArgsFloat> SpeedChanged;
        public event EventHandler<EventArgs> NewProjectStarted;
        public event EventHandler<EventArgs> ProjectLoaded;
        public event EventHandler<EventArgs> ProjectSaved;
        public event EventHandler<EventArgs> ControlDisabled;
        public event EventHandler<EventArgs> ControlEnabled;
        public event EventHandler<EventArgs> GridClearClicked;
        public event EventHandler<EventArgs> ViewReset;
    }
}
