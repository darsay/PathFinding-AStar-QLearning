using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using Assets.Scripts.DataStructures;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.Solucion1
{
    public class AStarMind : AbstractPathMind
    {
        // declarar Stack de Locomotion.MoveDirection de los movimientos hasta llegar al objetivo
        private Stack<Locomotion.MoveDirection> currentPlan = new Stack<Locomotion.MoveDirection>();
        public override void Repath()
        {
            currentPlan.Clear();// limpiar Stack 
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo board, CellInfo currentPos, CellInfo[] goals)
        {
            // si la Stack no está vacía, hacer siguiente movimient
            if (currentPlan.Count != 0)
            {
                currentPlan.Pop();// devuelve siguiente movimient (pop the Stack)

            }

            // calcular camino, devuelve resultado de A*
            var searchResult = Search(board, currentPos, goals);

            // recorre searchResult and copia el camino a currentPlan
            while (searchResult.Parent != null)
            {
                currentPlan.Push(searchResult.ProducedBy);
                searchResult = searchResult.Parent;
            }

            // returns next move (pop Stack)
            if (currentPlan.Any())
                return currentPlan.Pop();

            return Locomotion.MoveDirection.None;

        }

        private Nodo<Locomotion.MoveDirection> Search(BoardInfo board, CellInfo start, CellInfo [] goals) // CellInfo [] goals
        {
            // crea una lista vacía de nodos
            var open = new List<Nodo<Locomotion.MoveDirection>>();

            // node inicial
            Nodo<Locomotion.MoveDirection> n = new Nodo<Locomotion.MoveDirection>(start, null, goals[0]);

            // añade nodo inicial a la lista
            open.Add(n);

            //Variable para evitar que Unity crashee en caso de escenarios sin solución
            int notFound = 0; 

            // mientras la lista no esté vacia
            while (open.Any())
            {
                // mira el primer nodo de la lista
                Nodo<Locomotion.MoveDirection> current = open.First();
                open.RemoveAt(0); // Lo sacamos de la lista

                // si el primer nodo es goal, returns current node
                if(current.Cell.CellId.Equals(goals[0].CellId))
                {
                    return current;
                }

                // Número arbitrario de bucles antes de encontrar una 
                // solución para evitar un while infinito
                if (notFound == 1000)
                {
                    Debug.Log("No es posible llegar a goal.");
                    return current;
                }

                // expande vecinos (calcula coste de cada uno, etc)y los añade en la lista
                CellInfo[] wN = current.Cell.WalkableNeighbours(board); // vecinos caminables de current
                
                for (int i = 0; i<4; i++)
                {
                    if(wN[i]!=null){
                        Nodo<Locomotion.MoveDirection> aux = new Nodo<Locomotion.MoveDirection>(wN[i], current, goals[0]);
                        if (i == 0) aux.ProducedBy = Locomotion.MoveDirection.Up;
                        if (i == 1) aux.ProducedBy = Locomotion.MoveDirection.Right;
                        if (i == 2) aux.ProducedBy = Locomotion.MoveDirection.Down;
                        if (i == 3) aux.ProducedBy = Locomotion.MoveDirection.Left;

                        // Añadimos el vecino a la lista comprobando que no sea el padre de current
                        if((current.Parent != null)){
                            if(aux.Cell.CellId != current.Parent.Cell.CellId && !open.Contains(aux))
                            {                
                                aux.gValue = aux.Parent.gValue + aux.Cell.WalkCost;
                                aux.SetFValue(goals[0]);
                                open.Add(aux);
                            }  
                        } else{
                            aux.gValue = aux.Parent.gValue + aux.Cell.WalkCost;
                            aux.SetFValue(goals[0]);
                            open.Add(aux);
                        }                                         
                    }
                }
                open = open.OrderBy(nodo => nodo.fValue).ToList(); // ordenamos según el valor del nodo
                notFound++; // incrementamos la variable de salida por error
            }
            return null;
        }
    }
}