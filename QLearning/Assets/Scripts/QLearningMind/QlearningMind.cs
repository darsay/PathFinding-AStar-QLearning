using Assets.Scripts.DataStructures;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.QLearningMind
{
    public class QlearningMind : AbstractPathMind
    {
        public int N_EPI_MAX = 50;
        public int N_ITER_MAX = 150;
        public float alpha = 0.3f;
        public float gamma = 0.95f;
        public bool train = true;


        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            Loader loader = GameObject.Find("Loader").GetComponent<Loader>();
            BoardInfo board = GameObject.Find("Character").GetComponent<CharacterBehaviour>().BoardManager.boardInfo;
            int seed = loader.seed;
            float[][] tableArray = new float[4][];
            initArray(tableArray, board.NumColumns, board.NumRows);
            string path = Application.dataPath + "/QTables/" + seed.ToString() + ".txt";
            CellInfo goal = GetGoal(board);

            if (File.Exists(path))
            {
                tableArray = ReadText(path, tableArray, board.NumColumns, board.NumRows);
            }

            if (train)
            {
                for (int i = 0; i < N_EPI_MAX; i++)
                {
                    int iter = 0;
                    CellInfo nextCell = GetRandomCell(board);
                    bool stopCondition = false;

                    while (!stopCondition)
                    {
                        CellInfo currentCell = nextCell;
                        Locomotion.MoveDirection currentAction = GetRandomAction();
                        nextCell = RunFSM(currentCell, currentAction, board);

                        float currentQ = GetQ(tableArray, currentCell, currentAction, board);
                        int reward = GetReward(nextCell, currentCell, goal);
                        float nextQMax = GetMaxQ(tableArray, nextCell, board);

                        float newQ = UpdateRule(currentQ, reward, nextQMax, alpha, gamma);
                        tableArray = UpdateTable(tableArray, currentCell, currentAction, newQ, board);

                        iter++;
                        stopCondition = EvaluateStop(iter, N_ITER_MAX, nextCell);

                    }
                }

                CreateTable(tableArray, board.NumColumns, board.NumRows, path);
                train = false;

            }
            
                int column = currentPos.RowId * board.NumColumns + currentPos.ColumnId;
                float max = -Mathf.Infinity;
                int k = 0;


                for (int i = 0; i < 4; i++)
                {
                    if (tableArray[i][column] > max)
                    {
                        max = tableArray[i][column];
                        k = i;
                    }
                }

                switch (k)
                {
                    case 0:
                        return Locomotion.MoveDirection.Up;

                    case 1:
                        return Locomotion.MoveDirection.Right;

                    case 2:
                        return Locomotion.MoveDirection.Down;

                    case 3:
                        return Locomotion.MoveDirection.Left;

                }          

            return Locomotion.MoveDirection.None;
        }


        private CellInfo GetGoal(BoardInfo board)
        {
            for(int i = 0; i<board.NumRows; i++)
            {
                for(int j = 0; j<board.NumColumns; j++)
                {
                    if(board.CellInfos[j,i].ItemInCell != null)
                    {
                        if (board.CellInfos[j, i].ItemInCell.Type == PlaceableItem.ItemType.Goal)
                        {
                            return board.CellInfos[j, i];
                        }
                    }
                    
                }
            }          
            return null;
        }

        private void initArray(float[][] tableArray, int cols, int rows)
        {
            for (int i = 0; i < tableArray.Length; i++)
            {
                tableArray[i] = new float[rows * cols];
                for (int j = 0; j < tableArray[i].Length; j++)
                {
                    tableArray[i][j] = 0;
                }
            }

        }

        private float[][] ReadText(string path, float[][] tableArray, int cols, int rows)
        {
            var sr = new StreamReader(path);
            var fileContents = sr.ReadToEnd();
            sr.Close();

            var lines = fileContents.Split("\n"[0]);

            for (int i = 0; i < 4; i++)
            {
                var nums = lines[i].Split("\t"[0]);
                for (int j = 0; j < cols * rows; j++)
                {
                    tableArray[i][j] = float.Parse(nums[j]);
                }

            }
            return tableArray;
        }

        private void CreateTable(float[][] tableArray, int columns, int rows, string path)
        {
            string content = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < columns * rows; j++)
                {
                    content += tableArray[i][j].ToString() + "\t";
                }
                if (i != 3)
                {
                    content += "\n";
                }

            }

            File.WriteAllText(path, content);
        }

        private CellInfo GetRandomCell(BoardInfo board)
        {
            int col = Random.Range(0, board.NumColumns);
            int row = Random.Range(0, board.NumRows);
            if (board.CellInfos[col, row].Walkable)
            {
                return board.CellInfos[col, row];
            }
            else
            {
                return GetRandomCell(board);
            }

        }

        private Locomotion.MoveDirection GetRandomAction()
        {
            int choice = Random.Range(0, 4);
            switch (choice)
            {
                case 0:
                    return Locomotion.MoveDirection.Up;

                case 1:
                    return Locomotion.MoveDirection.Right;

                case 2:
                    return Locomotion.MoveDirection.Down;

                case 3:
                    return Locomotion.MoveDirection.Left;

            }

            return Locomotion.MoveDirection.None;
        }

        private CellInfo RunFSM(CellInfo currentCell, Locomotion.MoveDirection currentAction, BoardInfo board)
        {
            switch (currentAction)
            {
                case Locomotion.MoveDirection.Up:
                    if (currentCell.RowId == board.NumRows - 1)
                    {
                        return currentCell;
                    }
                    else if (!board.CellInfos[currentCell.ColumnId, currentCell.RowId + 1].Walkable)
                    {
                        return currentCell;
                    }
                    else
                    {
                        return board.CellInfos[currentCell.ColumnId, currentCell.RowId + 1];
                    }

                case Locomotion.MoveDirection.Right:
                    if (currentCell.ColumnId == board.NumColumns - 1)
                    {
                        return currentCell;
                    }
                    else if (!board.CellInfos[currentCell.ColumnId + 1, currentCell.RowId].Walkable)
                    {
                        return currentCell;
                    }
                    else
                    {
                        return board.CellInfos[currentCell.ColumnId + 1, currentCell.RowId];
                    }

                case Locomotion.MoveDirection.Down:
                    if (currentCell.RowId == 0)
                    {
                        return currentCell;
                    }
                    else if (!board.CellInfos[currentCell.ColumnId, currentCell.RowId - 1].Walkable)
                    {
                        return currentCell;
                    }
                    else
                    {
                        return board.CellInfos[currentCell.ColumnId, currentCell.RowId - 1];
                    }

                case Locomotion.MoveDirection.Left:
                    if (currentCell.ColumnId == 0)
                    {
                        return currentCell;
                    }
                    else if (!board.CellInfos[currentCell.ColumnId - 1, currentCell.RowId].Walkable)
                    {
                        return currentCell;
                    }
                    else
                    {
                        return board.CellInfos[currentCell.ColumnId - 1, currentCell.RowId];
                    }
            }

            return null;
        }

        private float GetQ(float[][] tableArray, CellInfo currentCell, Locomotion.MoveDirection currentAction, BoardInfo board)
        {
            int column = currentCell.RowId * board.NumColumns + currentCell.ColumnId;

            switch (currentAction)
            {
                case Locomotion.MoveDirection.Up:
                    return tableArray[0][column];

                case Locomotion.MoveDirection.Right:
                    return tableArray[1][column];

                case Locomotion.MoveDirection.Down:
                    return tableArray[2][column];

                case Locomotion.MoveDirection.Left:
                    return tableArray[3][column];
            }
            return 0;
        }

        private int GetReward(CellInfo nextCell, CellInfo currentCell, CellInfo goal)
        {
            if (nextCell == currentCell)
            {
                return -1;
            }

            if (nextCell.ItemInCell != null)
            {
                if (nextCell.ItemInCell.Type == PlaceableItem.ItemType.Goal)
                {
                    return 100;
                }

            }

            return 0;
        }

        private float GetMaxQ(float[][] tableArray, CellInfo nextCell, BoardInfo board)
        {
            int column = nextCell.RowId * board.NumColumns + nextCell.ColumnId;
            float max = 0;

            for (int i = 0; i < 4; i++)
            {
                if (tableArray[i][column] > max)
                {
                    max = tableArray[i][column];
                }
            }
            return max;
        }

        private float UpdateRule(float currentQ, int reward, float nextQMax, float alpha, float gamma)
        {
            return (1 - alpha) * currentQ + alpha * (reward + gamma * nextQMax);
        }

        private float[][] UpdateTable(float[][] tableArray, CellInfo currentCell, Locomotion.MoveDirection currentAction, float newQ, BoardInfo board)
        {
            int column = currentCell.RowId * board.NumColumns + currentCell.ColumnId;

            switch (currentAction)
            {
                case Locomotion.MoveDirection.Up:
                    tableArray[0][column] = newQ;
                    break;

                case Locomotion.MoveDirection.Right:
                    tableArray[1][column] = newQ;
                    break;

                case Locomotion.MoveDirection.Down:
                    tableArray[2][column] = newQ;
                    break;

                case Locomotion.MoveDirection.Left:
                    tableArray[3][column] = newQ;
                    break;
            }
            return tableArray;
        }

        private bool EvaluateStop(int iter, int n_ITER_MAX, CellInfo nextCell)
        {
            if ((nextCell.ItemInCell != null))
            {
                return ((iter >= n_ITER_MAX - 1) || (nextCell.ItemInCell.Type == PlaceableItem.ItemType.Goal));
            }
            else
            {
                return (iter >= n_ITER_MAX - 1);
            }

        }

        public void QuitGame()
        {
            // save any game data here
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }
    }
    }
