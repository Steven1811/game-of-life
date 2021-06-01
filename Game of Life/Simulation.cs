using System;
using System.Threading;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace Game_of_Life
{
    class Simulation
    {

        protected Thread simulationThread;
        protected Grid Grid { get; set; }

        public float SpeedDivider { get; set; }

        public int DelayMs { get; set; }

        public bool FullSpeed { get; set; }

        protected int OverpopulationMark{ get; set; }
        protected int StarvingMark{ get; set; }

        protected int KeepAliveMark {
            get => this.KeepAliveMark;
            set => this.KeepAliveMark = OverpopulationMark - StarvingMark > 0 ? OverpopulationMark - StarvingMark : 0;
        }
        public bool ThreadRunning { get; set; }

        public Simulation(Grid grid)
        {
            this.Grid = grid;
            this.simulationThread = new Thread(new ThreadStart(this.DoSimulation));
            SpeedDivider = 1;
            OverpopulationMark = 3;
            StarvingMark = 2;
            DelayMs = 1000;
            FullSpeed = false;
            this.simulationThread.Start();

        }

        public void Start()
        {
            this.ThreadRunning = true;
        }

        public void Stop()
        {
            this.ThreadRunning = false;
        }

        public void DoSimulation()
        {

            while (true)
            {
                if (this.ThreadRunning)
                {
                    this.Step();
                    if (!FullSpeed)
                    {
                        Thread.Sleep((int) (DelayMs / SpeedDivider));
                    }
                    
                }
            }

        }

        public void Step()
        {
            uint[,] newMap = (uint[,]) this.Grid.Map.Clone();

            // Loop through every cell 
            for (int row = 0; row < Grid.GridHeight - 1; row++)
            {
                for (int column = 0; column < Grid.GridWidth - 1; column++)
                {
                    uint currentCell = Grid.GetCell(column, row);

                    // find your alive neighbors
                    uint aliveNeighbors = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            aliveNeighbors += Grid.GetCell(column + j, row + i);

                        }
                    }

                    aliveNeighbors -= currentCell;

                    // Implementing the Rules of Life 

                    // Cell is lonely and dies 
                    if (currentCell == 1 && aliveNeighbors < StarvingMark)
                    {
                        newMap[column, row] = 0;
                    }

                    // Cell dies due to over population 
                    else if (currentCell == 1 && aliveNeighbors > OverpopulationMark)
                    {
                        newMap[column, row] = 0;
                    }

                    // A new cell is born 
                    else if (currentCell == 0 && aliveNeighbors == 3)
                    {
                        newMap[column, row] = 1;
                    }

                    //Continue if 2 or 3


                }
            }
            this.Grid.Map = newMap;
            this.Grid.Rebuild();
        }
    }
}
