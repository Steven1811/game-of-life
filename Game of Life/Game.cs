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

        // Main render thread
        protected void doRender()
        {
            this.window.SetActive(true);

            // Some example stuff to be drawn
            CircleShape shape = new CircleShape(50f);
            bool goRight = true;

            Text text = new Text("Game of Life", this.font, 96);
            text.FillColor = Color.Magenta;
            text.Position = new Vector2f(200f, 200f);

            Text fpsText = new Text("FPS: ", this.font, 48);
            fpsText.FillColor = Color.Yellow;
            int frameCount = 0;
            Clock clock = new Clock();

            // Main draw loop
            while (this.window.IsOpen)
            {
                // Clear the window with an eyepiercing blue... for now
                this.window.Clear(Color.Blue);

                // Draw calls, the less the better (; We probably need some shape/sprite batch processing here
                this.window.Draw(shape);
                this.window.Draw(text);
                this.window.Draw(fpsText);
                this.window.Display();

                // Simple FPS Counter
                frameCount++;
                if (clock.ElapsedTime.AsMilliseconds() > 1000)
                {
                    fpsText.DisplayedString = String.Format("FPS: {0}", frameCount);
                    frameCount = 0;
                    clock.Restart();
                }

                // Example: Move a circle left and right on window boundaries
                if (shape.Position.X > this.window.Size.X - (2f * shape.Radius))
                {
                    goRight = false;
                }
                else if (shape.Position.X < 0)
                {
                    goRight = true;
                }

                // Example: Move the circle
                if (goRight)
                {
                    shape.Position = new Vector2f(shape.Position.X + 1f, 0f);
                }
                else
                {
                    shape.Position = new Vector2f(shape.Position.X - 1f, 0f);
                }
            }
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
            }
        }

        public Game()
        {
            // Load game assets
            // Load Arcade game font
            this.font = new Font("assets/fonts/ARCADE.TTF");

            this.window = new RenderWindow(new VideoMode(800, 600), "Game of Life");

            // Set Window Events
            this.window.Closed += OnWindowClosed;
            this.window.Resized += OnWindowResized;

            // Set Window Config
            this.window.SetFramerateLimit(60);
            this.window.SetActive(false);
            
            // Setup rendering thread
            this.renderThread = new Thread(new ThreadStart(this.doRender));
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
