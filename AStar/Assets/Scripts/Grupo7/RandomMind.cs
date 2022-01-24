using Assets.Scripts.DataStructures;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class RandomMind : AbstractPathMind {
        public override void Repath()
        {
            
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            var val = Random.Range(0, 4); 
            if (val == 0 && currentPos.RowId < 6) // comprobamos si está en la altura máxima
            {
                if(boardInfo.CellInfos[currentPos.ColumnId, currentPos.RowId + 1].Walkable)
                {
                    return Locomotion.MoveDirection.Up;
                }
                
            }
            if (val == 1 && currentPos.RowId > 0) // comprobamos si está en la altura mínima
            {
                if (boardInfo.CellInfos[currentPos.ColumnId, currentPos.RowId - 1].Walkable)
                {
                    return Locomotion.MoveDirection.Down;
                }
            }
            if (val == 2 && currentPos.ColumnId > 0) // comprobamos si está en el ancho mínimo
            {
                if (boardInfo.CellInfos[currentPos.ColumnId-1, currentPos.RowId].Walkable)
                {
                    return Locomotion.MoveDirection.Left;
                }
            }
            if (val == 3 && currentPos.ColumnId < 13) // comprobamos si está en el ancho máximo
            {
                if (boardInfo.CellInfos[currentPos.ColumnId + 1, currentPos.RowId].Walkable)
                {
                    return Locomotion.MoveDirection.Right;
                }
            }
            
            // si no se ha podido determinar un movimiento, volvemos a calcularlo
            return GetNextMove(boardInfo, currentPos, goals); 
        }
    }
}
