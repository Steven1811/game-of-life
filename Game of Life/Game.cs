using System;
using System.Threading;
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

        // Main render thread
        protected void doRender()
        {
            this.window.SetActive(true);

            this.gameCaptionText = new Text("Game of Life\nBy Steven & Lion", this.font, 48);
            this.gameCaptionText.FillColor = Color.Yellow;
            // text.Position = new Vector2f(200f, 200f);

            this.fpsText = new Text("FPS: ", this.font, 48);
            this.AlignFpsText();

            this.fpsText.FillColor = Color.Yellow;
            int frameCount = 0;
            Clock clock = new Clock();

            // Main draw loop
            while (this.window.IsOpen)
            {
                // Clear the window with an blue background as grid lines
                this.window.Clear(Color.Blue);

                // Draw calls, the less the better (; We probably need some shape/sprite batch processing here
                this.window.Draw(this.grid);
                this.window.Draw(this.gameCaptionText);
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
            this.fpsText.Position = new Vector2f(this.window.Size.X, 0);
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
                window.SetView(new View(visibleArea));

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

            // Setup grid
            this.grid = new Grid(this.window, 12, 16);

            // Set Window Events
            this.window.Closed += OnWindowClosed;
            this.window.Resized += OnWindowResized;
            this.window.KeyPressed += OnKeyPressed;

            // Set Window Config
            // this.window.SetFramerateLimit(60);
            this.window.SetActive(false);
            
            // Setup rendering thread
            this.renderThread = new Thread(new ThreadStart(this.doRender));
        }

        private void OnKeyPressed(object sender, KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.F1)
            {
                this.grid.ColorMap[0] = Color.Yellow;
                this.grid.ColorMap[1] = Color.Magenta;
                this.grid.Rebuild();
            }
            else if (args.Code == Keyboard.Key.F2)
            {
                Console.WriteLine(this.grid.GetCell(-2, -2));
                Console.WriteLine(this.grid.GetCell(17, 17));
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
