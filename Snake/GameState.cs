using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class GameState // Daje trenutno stanje igre
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; } // 2D niz platforme za igru

        public Direction Dir { get; private set; } // Kretanje zmije
        public int Score { get; private set; }

        public bool GameOver { get; private set; }

        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        
        private readonly Random random = new Random(); // Za spawn hrane

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();

        public static int hiScore;

        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols]; // Incijaliziranje velicine platforme
            Dir = Direction.Right;

            AddSnake();
            AddFood();

        }

        private void AddSnake() // zelimo da krene prema desno u sredini na 123 poziciji
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++) // Loop kroz grid
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions() // Prazne pozicije na platformi
        {
            for (int r=0; r < Rows; r++) // prolazak kroz kolone i redove
            {
                for (int c=0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            // Kreiramo listu praznih pozicija
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            } // Da se igra ne sruši kad se riješi 

            Position pos = empty[random.Next(empty.Count)]; // Random pozicija
            Grid[pos.Row, pos.Col] = GridValue.Food;

        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos) //Kretanje zmije naprijed
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail() // Kad se krece zmija pa nema repa
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row<0 || pos.Row >= Rows || pos.Col<0 || pos.Col >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }


            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if(dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit= WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;

            } else if ( hit== GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            } else if (hit == GridValue.Food) 
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
                if (Score > hiScore)
                {
                    hiScore = Score;
                }
            }
        }
        public int GetHighScore()
        {
            return hiScore;
        }
    }
}
