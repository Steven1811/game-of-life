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
        protected Grid grid;

        protected int Speed { get; set; }
        protected int OverpopulationMark{ get; set; }
        protected int StarvingMark{ get; set; }

        protected int KeepAliveMark {
            get => this.KeepAliveMark;
            set => this.KeepAliveMark = OverpopulationMark - StarvingMark > 0 ? OverpopulationMark - StarvingMark : 0;
        }


        public Simulation(Grid grid)
        {
            this.grid = grid;
            this.simulationThread = new Thread(new ThreadStart(this.doSimultaion));
            Speed = 1;
            OverpopulationMark = 3;
            StarvingMark = 2;
        }


        public void doSimultaion()
        {
            while (true)
            {
                
            }
        }

        private Grid Step(Grid currentGrid)
        {
            // Loop through every cell 
            for (int row = 0; row < currentGrid.GridHeight - 1; row++)
            {
                for (int column = 0; column < currentGrid.GridWidth - 1; column++)
                {
                    // find your alive neighbors
                    int aliveNeighbors = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            aliveNeighbors += currentGrid.GetCell(row + i, column + j) == 1 ? 1 : 0;
                        }
                    }

                    uint currentCell = currentGrid.GetCell(row, column);

                    // The cell needs to be subtracted 
                    // from its neighbors as it was  
                    // counted before 
                    aliveNeighbors -= currentCell == 1 ? 1 : 0;

                    // Implementing the Rules of Life 

                    // Cell is lonely and dies 
                    if (currentCell == 1 && aliveNeighbors < StarvingMark)
                    {
                        currentGrid.SetCell(row, column, 0);
                    }

                    // Cell dies due to over population 
                    else if (currentCell == 1 && aliveNeighbors > OverpopulationMark)
                    {
                        currentGrid.SetCell(row, column, 0);
                    }

                    // A new cell is born 
                    else if (currentCell == 0 && aliveNeighbors == 3)
                    {
                        currentGrid.SetCell(row, column, 1);
                    }

                    //Continue if 2 or 3
                }
            }

            return currentGrid;               
            //return nextGeneration;
        }


    }
}
