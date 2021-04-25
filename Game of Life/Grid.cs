using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using SFML.System;


namespace Game_of_Life
{
    class Grid : Transformable, Drawable
    {
        protected Mouse.Button currentMouseButton;
        protected VertexArray vertices;
        protected uint[,] map;
        protected Dictionary<Mouse.Button, uint> MouseMap { get; set; }

        public Dictionary<uint, Color> ColorMap { get; }
        public RenderWindow Window { get; }
        public uint GridWidth { get; }
        public uint GridHeight { get; }
        public float CellWidth { get; }
        public float CellHeight { get; }
        public float LineWidth { get; }

        
        public Grid(RenderWindow window, uint gridWidth, uint gridHeight, float cellWidth = 16f, float cellHeight = 16f, float lineWidth = 1f)
        {
            // Set properties
            this.Window = window;
            this.GridWidth = gridWidth;
            this.GridHeight = gridHeight;
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;
            this.LineWidth = lineWidth;

            // Initialize Map
            this.map = new uint[gridWidth, gridHeight];

            // Initialize Colormap
            this.ColorMap = new Dictionary<uint, Color>();
            this.ColorMap.Add(0, Color.Black);
            this.ColorMap.Add(1, Color.Green);

            // Initialize MouseMap
            this.MouseMap = new Dictionary<Mouse.Button, uint>();
            this.MouseMap.Add(Mouse.Button.Left, 1);
            this.MouseMap.Add(Mouse.Button.Right, 0);

            // Add window mouse Handlers
            this.Window.MouseButtonPressed += this.OnMouseButtonPressed;
            this.Window.MouseButtonReleased += this.OnMouseButtonReleased;

            // Create VertexArray
            uint quadVerticesCount = gridWidth * gridHeight * 4;
            this.vertices = new VertexArray(PrimitiveType.Quads, quadVerticesCount);

            // Fill the grid with dead cells first
            this.FillGrid(0);
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

        public void SetCellByWorldCoordinates(float x, float y, uint value)
        {
            uint selectedCellX = (uint)((x - this.Position.X) / (this.CellWidth * this.Scale.X));
            uint selectedCellY = (uint)((y- this.Position.Y) / (this.CellHeight * this.Scale.Y));
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
            for (uint y = 0; y < this.GridHeight; y++)
            {
                for (uint x = 0; x < this.GridWidth; x++)
                {
                    this.SetCell(x, y, value);
                }
            }
        }

        public void SetCell(uint x, uint y, uint value)
        {
            this.map[x, y] = value;

            // Update Vertex array
            this.vertices[((x + y * this.GridWidth) * 4) + 0] = new Vertex(new Vector2f((x * this.CellWidth) + this.LineWidth, (y * this.CellHeight) + this.LineWidth), this.ColorMap[value]);
            this.vertices[((x + y * this.GridWidth) * 4) + 1] = new Vertex(new Vector2f((x * this.CellWidth) + this.CellWidth, (y * this.CellHeight) + this.LineWidth), this.ColorMap[value]);
            this.vertices[((x + y * this.GridWidth) * 4) + 2] = new Vertex(new Vector2f((x * this.CellWidth) + this.CellWidth, (y * this.CellHeight) + this.CellHeight), this.ColorMap[value]);
            this.vertices[((x + y * this.GridWidth) * 4) + 3] = new Vertex(new Vector2f((x * this.CellWidth) + this.LineWidth, (y * this.CellHeight) + this.CellHeight), this.ColorMap[value]);
        }

        public void Rebuild()
        {
            for (uint x = 0; x < this.map.GetLength(0); x++)
            {
                for (uint y = 0; y < this.map.GetLength(1); y++)
                {
                    this.SetCell(x, y, this.map[x, y]);
                }
            }
        }

        public uint GetCell(uint x, uint y)
        {
            return this.map[x, y];
        }

        public static void LoadFromFile()
        {

        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform = this.Transform;
            target.Draw(this.vertices, states);
        }
    }
}
