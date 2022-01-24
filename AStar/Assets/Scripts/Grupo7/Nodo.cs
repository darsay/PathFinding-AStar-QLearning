using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using Assets.Scripts.DataStructures;
using UnityEngine;
using  UnityEditor.Experimental.GraphView;

public class Nodo<T>
{
        public CellInfo Cell; // celda propia del nodo
        public Nodo<T> Parent; // nodo padre del nodo
        public float fValue; // f*(n)       
        public float gValue; // g(n)
        public T ProducedBy; // movimiento producido por el nodo
        
        public Nodo(CellInfo c, Nodo<T> par, CellInfo goal)
        {
            Cell = c;
            Parent = par;
                        
            gValue=0; // el valor de g se actualizará desde fuera de la clase
        
            // valor = distancia de Manhattan(h*) + g
            fValue = (Mathf.Abs(goal.ColumnId-c.ColumnId) + Mathf.Abs(goal.RowId-c.RowId)) + gValue;
        
        }

        // De una lista de destinos, determina el más cercano y devuelve su índice
        public int FindClosestGoal(List<CellInfo> goals, bool noEnemies)
        {
            int distance = 0; // distancia mínima
            int closest = 0; // índice del más cercano
            for(int i = 0; i<goals.Count; i++)
            {
                // cálculo de la distancia al nodo
                int value = (Mathf.Abs(goals[i].ColumnId - Cell.ColumnId) + Mathf.Abs(goals[i].RowId - Cell.RowId));

                // actualizar índice en caso de ser la distancia menor
                if ((distance == 0 || value < distance))
                {
                    if((i == 0 && noEnemies || i!=0)){
                        distance = value;
                        closest = i;
                    }                   
                }
            }

            return closest;
        }

        public void SetFValue(CellInfo goal){
            fValue = (Mathf.Abs(goal.ColumnId-Cell.ColumnId) + Mathf.Abs(goal.RowId-Cell.RowId)) + gValue;
        }
}
