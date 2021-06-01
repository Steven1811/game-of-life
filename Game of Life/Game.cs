using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Game_of_Life
{
    class Game
    {
        protected RenderWindow window;
        protected Thread renderThread;
        protected Font font;
        protected Grid grid;
        protected Text fpsText;
        protected Text gameCaptionText;
        protected Gui gui;
        protected Simulation simulation;

        protected float zoomFactor = 1.0f;
        protected float mouseDeltaX = 0.0f;
        protected float mouseDeltaY = 0.0f;

        // Main render thread
        protected void doRender()
        {
            this.window.SetActive(true);

            this.gameCaptionText = new Text("Game of Life", this.font, 48);
            this.gameCaptionText.Position = new Vector2f(0f, 8f);
            this.gameCaptionText.FillColor = Color.Yellow;

            this.fpsText = new Text("FPS: ", this.font, 48);
            this.AlignFpsText();

            this.fpsText.FillColor = Color.Yellow;
            int frameCount = 0;
            Clock clock = new Clock();

            // Main draw loop
            while (this.window.IsOpen)
            {
                // Clear the window with an blue background as grid lines
                this.window.Clear(new Color(0, 48, 63, 255));

                // Draw calls, the less the better (; We probably need some shape/sprite batch processing here
                this.window.Draw(this.grid);
                this.window.Draw(this.gameCaptionText);
                this.gui.Draw();
                this.window.Draw(this.fpsText);
                this.window.Display();

                // Simple FPS Counter
                frameCount++;
                if (clock.ElapsedTime.AsMilliseconds() > 1000)
                {
                    this.fpsText.DisplayedString = String.Format("FPS: {0}", frameCount);
                    this.AlignFpsText();
                    frameCount = 0;
                    clock.Restart();
                }
            }
        }

        protected void AlignFpsText()
        {
            this.fpsText.Origin = new Vector2f(fpsText.GetGlobalBounds().Width, 0);
            this.fpsText.Position = new Vector2f(this.window.Size.X, 8f);
        }

        // EventHandler for Window Closed Events
        protected void OnWindowClosed(object sender, EventArgs args)
        {
            if (sender is Window)
            {
                Window window = sender as Window;
                window.Close();
            }
        }
        
        // EventHandler for Window Resized Events. In/Decreases Viewport size.
        protected void OnWindowResized(object sender, SizeEventArgs args)
        {
            if (sender is RenderWindow)
            {
                RenderWindow window = sender as RenderWindow;
                FloatRect visibleArea = new FloatRect(0f, 0f, args.Width, args.Height);
                View newView = new View(visibleArea);
                newView.Zoom(this.zoomFactor);
                window.SetView(newView);
                this.AlignFpsText();
            }
        }

        public Game()
        {
            // Load game assets
            // Load Arcade game font
            this.font = new Font("assets/fonts/ARCADE.TTF");

            // Setup Window
            this.window = new RenderWindow(new VideoMode(800, 600), "Game of Life");

            // Setup GUI
            this.gui = new Gui(this.window);
            this.gui.ControlEnabled += GuiControlEnabled;
            this.gui.ControlDisabled += GuiControlDisabled;
            this.gui.ViewReset += OnViewReset;
            this.gui.GridClearClicked += OnGridClearClicked;
            this.gui.RunClicked += OnRunClicked;
            this.gui.StepClicked += OnStepClicked;
            this.gui.SpeedChanged += OnSpeedChanged;
            // Setup grid
            this.grid = new Grid(this.window, 32, 32);

            // Setup simulation
            this.simulation = new Simulation(this.grid);

            // Set Window Events
            this.window.Closed += OnWindowClosed;
            this.window.Resized += OnWindowResized;
            this.window.KeyPressed += OnKeyPressed;
            this.window.MouseWheelScrolled += OnMouseWheelScrolled;
            this.window.MouseMoved += OnMouseMoved;

            // Set Window Config
            // this.window.SetFramerateLimit(60);
            this.window.SetVerticalSyncEnabled(true);
            this.window.SetActive(false);
            
            // Setup rendering thread
            this.renderThread = new Thread(new ThreadStart(this.doRender));
        }

        private void OnSpeedChanged(object sender, TGUI.SignalArgsFloat e)
        {
            if (e.Value != 10.0f)
            {
                this.simulation.FullSpeed = false;
                this.simulation.SpeedDivider = e.Value;
            }
            else
            {
                this.simulation.FullSpeed = true;
            }
            
            
        }

        private void OnStepClicked(object sender, EventArgs e)
        {
            this.simulation.Step();
        }

        private void OnRunClicked(object sender, EventArgs e)
        {
            if (this.simulation.ThreadRunning)
            {
                this.simulation.Stop();
            }
            else
            {
                this.simulation.Start();
            }
        }

        private void OnGridClearClicked(object sender, EventArgs e)
        {
            this.grid.Clear();
        }

        private void OnViewReset(object sender, EventArgs e)
        {
            this.zoomFactor = 1.0f;
            FloatRect visibleArea = new FloatRect(0f, 0f, this.window.Size.X, this.window.Size.Y);
            View newView = new View(visibleArea);
            window.SetView(newView);
            this.AlignFpsText();
        }

        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            this.mouseDeltaX -= e.X;
            this.mouseDeltaY -= e.Y;

            if (Mouse.IsButtonPressed(Mouse.Button.Middle))
            {
                View oldView = this.window.GetView();
                oldView.Move(new Vector2f(this.mouseDeltaX * this.zoomFactor, this.mouseDeltaY * this.zoomFactor));
                this.window.SetView(oldView);
            }
            this.mouseDeltaX = e.X;
            this.mouseDeltaY = e.Y;
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            View oldView = this.window.GetView();
            if (e.Delta > 0)
            {
                oldView.Zoom(e.Delta * 0.8f);
                this.zoomFactor *= e.Delta * 0.8f;
            }
            else
            {
                oldView.Zoom(e.Delta * -1.25f);
                this.zoomFactor *= e.Delta * -1.25f;
            }
            this.window.SetView(oldView);
        }

        private void GuiControlDisabled(object sender, EventArgs e)
        {
            this.grid.DisableMouse();
        }

        private void GuiControlEnabled(object sender, EventArgs e)
        {
            this.grid.EnableMouse();
        }

        public void LoadGridFromFile(string filePath)
        {
            string serialized = File.ReadAllText(filePath);
            Grid newGrid = new Grid(this.window);
            JsonConvert.PopulateObject(serialized, newGrid);
            newGrid.Rebuild();
            this.grid = newGrid;
            this.grid.CenterInWindow();
        }

        public void SaveGridToFile(string filePath)
        {
            string serialized = JsonConvert.SerializeObject(this.grid, Formatting.Indented);
            File.WriteAllText(filePath, serialized);
        }

        private void OnKeyPressed(object sender, KeyEventArgs args)
        {

            switch(args.Code)
            {
                case Keyboard.Key.F1:
                    this.gui.ShowHelpMessageBox();
                    break;
                case Keyboard.Key.F3:
                    this.LoadGridFromFile("saves/save1.json");
                    break;
                case Keyboard.Key.F4:
                    this.SaveGridToFile("saves/save1.json");
                    break;
                case Keyboard.Key.F12:
                    Environment.Exit(0);
                    break;
            }
        }

        // Starts the game
        public int Start()
        {
            this.renderThread.Start();

            // Poll window events. This must happen in the main thread.
            while (this.window.IsOpen)
            {
                this.window.DispatchEvents();
            }
            return 0;
        }
    }
}
