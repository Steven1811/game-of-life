using System.Collections.Generic;
using Newtonsoft.Json;
using SFML.Window;
using SFML.Graphics;
using SFML.System;


namespace Game_of_Life
{
    [JsonObject(MemberSerialization.OptIn)]
    class Grid : Transformable, Drawable
    {
        protected Mouse.Button currentMouseButton;
        protected VertexArray vertices;
        
        [JsonProperty]
        public uint[,] map { get; set; }
        protected Dictionary<Mouse.Button, uint> MouseMap { get; set; }

        [JsonProperty]
        public Dictionary<uint, Color> ColorMap { get; set; }
        public RenderWindow Window { get; }
        [JsonProperty]
        public uint GridWidth { get; set; }
        [JsonProperty]
        public uint GridHeight { get; set; }
        [JsonProperty]
        public float CellWidth { get; set; }
        [JsonProperty]
        public float CellHeight { get; set; }
        [JsonProperty]
        public float LineWidth { get; set; }

        public Grid(RenderWindow window)
        {
            // Set properties
            this.Window = window;

            // Initialize MouseMap
            this.MouseMap = new Dictionary<Mouse.Button, uint>();
            this.MouseMap.Add(Mouse.Button.Left, 1);
            this.MouseMap.Add(Mouse.Button.Right, 0);

            // Add window Handlers
            this.Window.MouseButtonPressed += this.OnMouseButtonPressed;
            this.Window.MouseButtonReleased += this.OnMouseButtonReleased;
            this.Window.Resized += this.OnWindowResized;
        }
        public Grid(RenderWindow window, uint gridWidth, uint gridHeight, float cellWidth = 16f, float cellHeight = 16f, float lineWidth = 2f)
        {
            // Set properties
            this.Window = window;
            this.GridWidth = gridWidth;
            this.GridHeight = gridHeight;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;
            this.LineWidth = lineWidth;

            // Initialize Map
            this.map = new uint[this.GridWidth, this.GridHeight];

            // Initialize Colormap
            this.ColorMap = new Dictionary<uint, Color>();
            this.ColorMap.Add(0, Color.Black);
            this.ColorMap.Add(1, Color.Green);

            // Initialize MouseMap
            this.MouseMap = new Dictionary<Mouse.Button, uint>();
            this.MouseMap.Add(Mouse.Button.Left, 1);
            this.MouseMap.Add(Mouse.Button.Right, 0);

            // Add window Handlers
            this.Window.MouseButtonPressed += this.OnMouseButtonPressed;
            this.Window.MouseButtonReleased += this.OnMouseButtonReleased;
            this.Window.Resized += this.OnWindowResized;

            // Rebuild Grid
            this.Rebuild();

            // Center in Window
            this.CenterInWindow();
        }

        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs args)
        {
            Vector2f coords = this.Window.MapPixelToCoords(new Vector2i(args.X, args.Y));
            
            
            this.currentMouseButton = args.Button;

            if (this.GetGlobalBounds().Contains(coords.X, coords.Y) && this.MouseMap.ContainsKey(this.currentMouseButton))
            {
                this.SetCellByWorldCoordinates(coords.X, coords.Y, this.MouseMap[this.currentMouseButton]);
                this.Window.MouseMoved += this.OnMouseMoved;
            }
            
        }

        protected void OnWindowResized(object sender, SizeEventArgs args)
        {
            if (sender is RenderWindow)
            {
                this.CenterInWindow();
            }
        }

        public void SetCellByWorldCoordinates(float x, float y, uint value)
        {
            int selectedCellX = (int)((x - this.Position.X + this.Origin.X) / (this.CellWidth * this.Scale.X));
            int selectedCellY = (int)((y - this.Position.Y + this.Origin.Y) / (this.CellHeight * this.Scale.Y));
            this.SetCell(selectedCellX, selectedCellY, value);
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            Vector2f coords = this.Window.MapPixelToCoords(new Vector2i(args.X, args.Y));

            if (this.GetGlobalBounds().Contains(coords.X, coords.Y))
            {
                this.SetCellByWorldCoordinates(coords.X, coords.Y, this.MouseMap[this.currentMouseButton]);
            }
        }

        public void OnMouseButtonReleased(object sender, MouseButtonEventArgs args)
        {
            if (this.MouseMap.ContainsKey(this.currentMouseButton))
            {
                this.Window.MouseMoved -= this.OnMouseMoved;
            }
        }

        public FloatRect GetLocalBounds()
        {
            return this.vertices.Bounds;
        }

        public FloatRect GetGlobalBounds()
        {
            return this.Transform.TransformRect(this.GetLocalBounds());
        }

        public void FillGrid(uint value)
        {
            for (int y = 0; y < this.GridHeight; y++)
            {
                for (int x = 0; x < this.GridWidth; x++)
                {
                    this.SetCell(x, y, value);
                }
            }
        }

        public Vector2u MapToRealCell(int x, int y)
        {
            x = (int)(x % this.GridWidth);
            y = (int)(y % this.GridHeight);

            Vector2u result = new Vector2u((uint)x, (uint) y);

            if (x < 0)
            {
                result.X = (uint)(this.GridWidth + x);
            }

            if (y < 0)
            {
                result.Y = (uint)(this.GridHeight + y);
            }

            return result;
        }

        public void SetCell(int x, int y, uint value)
        {
            Vector2u realCoords = new Vector2u((uint)x, (uint)y);

            // Do mapping if required
            if (x > this.GridWidth - 1 || y > this.GridHeight - 1 || x < 0 || y < 0)
            {
                realCoords = this.MapToRealCell(x, y);
            }

            this.map[realCoords.X, realCoords.Y] = value;

            // Update Vertex array
            this.vertices[((realCoords.X + realCoords.Y * this.GridWidth) * 4) + 0] = new Vertex(new Vector2f((realCoords.X * this.CellWidth) + this.LineWidth, (realCoords.Y * this.CellHeight) + this.LineWidth), this.ColorMap[value]);
            this.vertices[((realCoords.X + realCoords.Y * this.GridWidth) * 4) + 1] = new Vertex(new Vector2f((realCoords.X * this.CellWidth) + this.CellWidth, (realCoords.Y * this.CellHeight) + this.LineWidth), this.ColorMap[value]);
            this.vertices[((realCoords.X + realCoords.Y * this.GridWidth) * 4) + 2] = new Vertex(new Vector2f((realCoords.X * this.CellWidth) + this.CellWidth, (realCoords.Y * this.CellHeight) + this.CellHeight), this.ColorMap[value]);
            this.vertices[((realCoords.X + realCoords.Y * this.GridWidth) * 4) + 3] = new Vertex(new Vector2f((realCoords.X * this.CellWidth) + this.LineWidth, (realCoords.Y * this.CellHeight) + this.CellHeight), this.ColorMap[value]);
        }

        public void Rebuild()
        {
            // Create VertexArray
            uint quadVerticesCount = this.GridWidth * this.GridHeight * 4;
            this.vertices = new VertexArray(PrimitiveType.Quads, quadVerticesCount);

            for (int x = 0; x < this.map.GetLength(0); x++)
            {
                for (int y = 0; y < this.map.GetLength(1); y++)
                {
                    this.SetCell(x, y, this.map[x, y]);
                }
            }
        }

        public void Clear()
        {
            // Create VertexArray
            uint quadVerticesCount = this.GridWidth * this.GridHeight * 4;
            this.vertices = new VertexArray(PrimitiveType.Quads, quadVerticesCount);

            for (int x = 0; x < this.map.GetLength(0); x++)
            {
                for (int y = 0; y < this.map.GetLength(1); y++)
                {
                    this.map[x, y] = 0;
                    this.SetCell(x, y, this.map[x, y]);
                }
            }
        }

        public uint GetCell(int x, int y)
        {
            Vector2u realCoords = new Vector2u((uint)x, (uint)y);

            // Do mapping if required
            if (x > this.GridWidth - 1 || y > this.GridHeight - 1 || x < 0 || y < 0)
            {
                realCoords = this.MapToRealCell(x, y);
            }

            return this.map[realCoords.X, realCoords.Y];
        }

        public void CenterInWindow()
        {
            this.Origin = new Vector2f(this.GetGlobalBounds().Width / 2, this.GetGlobalBounds().Height / 2);
            this.Position = new Vector2f((this.Window.Size.X / 2), this.Window.Size.Y / 2);
        }

        public void DisableMouse()
        {
            this.Window.MouseButtonPressed -= this.OnMouseButtonPressed;
            this.Window.MouseButtonReleased -= this.OnMouseButtonReleased;
            this.Window.MouseMoved -= this.OnMouseMoved;
        }

        public void EnableMouse()
        {
            this.Window.MouseButtonPressed += this.OnMouseButtonPressed;
            this.Window.MouseButtonReleased += this.OnMouseButtonReleased;
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform = this.Transform;
            target.Draw(this.vertices, states);
        }
    }
}
