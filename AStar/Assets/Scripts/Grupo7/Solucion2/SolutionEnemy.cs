using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using Assets.Scripts.DataStructures;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.AStarSolution
{
    public class SolutionEnemy : AbstractPathMind
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
            // booleana que devuelve verdadero cuando no hay enemigos
            bool noEnemies = true;
            var searchResult = Search(board, currentPos, goals, noEnemies);


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

        private Nodo<Locomotion.MoveDirection> Search(BoardInfo board, CellInfo start, CellInfo[] goals, bool noEnemies) // CellInfo [] goals
        {
            // crea una lista vacía de nodos
            var open = new List<Nodo<Locomotion.MoveDirection>>();

            // node inicial
            Nodo<Locomotion.MoveDirection> n = new Nodo<Locomotion.MoveDirection>(start, null, goals[0]);

            // añade nodo inicial a la lista
            open.Add(n);

            //Variable para evitar que Unity crashee en caso de escenarios sin solución
            int notFound = 0;           
            
            // lista de celdas que serán destinos
            List<CellInfo> destinations = new List<CellInfo>();
            destinations.Add(goals[0]); // añadimos la meta
            FindEnemies(destinations, ref noEnemies); // encontramos a los enemigos           
            CellInfo closestGoal = FindClosestDestination(n, destinations, noEnemies); // el destino sobre la que buscar el camino

            if(!noEnemies){
                return HillClimb(open, board, closestGoal, goals[0]); // algoritmo Hill Climb
            } else{
                return AStar(open, board, closestGoal, notFound); // algoritmo A*
            }
        }


        private Nodo<Locomotion.MoveDirection> AStar(List<Nodo<Locomotion.MoveDirection>> open, BoardInfo board, CellInfo closestGoal, int notFound){
            // mientras la lista no esté vacia
            while (open.Any())
            {
                // mira el primer nodo de la lista
                Nodo<Locomotion.MoveDirection> current = open.First();
                open.RemoveAt(0); // lo sacamos de la lista

                // si el primer nodo es goal, returns current node
                if (current.Cell.CellId.Equals(closestGoal.CellId))
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

                for (int i = 0; i < 4; i++)
                {
                    if (wN[i] != null)
                    {
                        Nodo<Locomotion.MoveDirection> aux = new Nodo<Locomotion.MoveDirection>(wN[i], current, closestGoal);
                        if (i == 0) aux.ProducedBy = Locomotion.MoveDirection.Up;
                        if (i == 1) aux.ProducedBy = Locomotion.MoveDirection.Right;
                        if (i == 2) aux.ProducedBy = Locomotion.MoveDirection.Down;
                        if (i == 3) aux.ProducedBy = Locomotion.MoveDirection.Left;

                        // Añadimos el vecino a la lista comprobando que no sea el padre de current
                        if ((current.Parent != null))
                        {
                            if (aux.Cell.CellId != current.Parent.Cell.CellId)
                            {
                                aux.gValue = aux.Cell.WalkCost;
                                aux.SetFValue(closestGoal);
                                open.Add(aux);                               
                            }
                        }
                        else{
                            aux.gValue = aux.Cell.WalkCost;
                            aux.SetFValue(closestGoal);
                            open.Add(aux);
                        }
                        
                    }
                }
                open = open.OrderBy(nodo => nodo.fValue).ToList(); // ordenamos según el valor del nodo
                notFound++; // incrementamos la variable de salida por error
            }
            return null;
        }

        private Nodo<Locomotion.MoveDirection> HillClimb(List<Nodo<Locomotion.MoveDirection>> open, BoardInfo board, CellInfo closestGoal, CellInfo finalGoal){
                Nodo<Locomotion.MoveDirection> current = open.First();
                open.RemoveAt(0); // lo sacamos de la lista

                // si el primer nodo es goal, returns current node
                if (current.Cell.CellId.Equals(closestGoal.CellId))
                {
                    return current;
                }

                // expande vecinos (calcula coste de cada uno, etc)y los añade en la lista
                CellInfo[] wN = current.Cell.WalkableNeighbours(board); // vecinos caminables de current

                for (int i = 0; i < 4; i++)
                {
                    if (wN[i] != null)
                    {
                        Nodo<Locomotion.MoveDirection> aux = new Nodo<Locomotion.MoveDirection>(wN[i], current, closestGoal);
                        if (i == 0) aux.ProducedBy = Locomotion.MoveDirection.Up;
                        if (i == 1) aux.ProducedBy = Locomotion.MoveDirection.Right;
                        if (i == 2) aux.ProducedBy = Locomotion.MoveDirection.Down;
                        if (i == 3) aux.ProducedBy = Locomotion.MoveDirection.Left;

                        // Añadimos el vecino a la lista comprobando que no sea el padre de current
                        if ((current.Parent != null) && aux.Cell.Walkable && !aux.Cell.CellId.Equals(finalGoal.CellId))
                        {
                                aux.gValue = aux.Parent.gValue + aux.Cell.WalkCost;
                                aux.SetFValue(closestGoal);
                                open.Add(aux);                               
                        }
                        else{
                            if(aux.Cell.Walkable && !aux.Cell.CellId.Equals(finalGoal.CellId)){
                                aux.gValue = aux.Parent.gValue + aux.Cell.WalkCost;
                                aux.SetFValue(closestGoal);
                                open.Add(aux);
                            }                          
                        }                       
                    }
                }
                open = open.OrderBy(nodo => nodo.fValue).ToList(); // ordenamos según el valor del nodo
                return open.First(); // devolvemos la mejor acción
            }
        
        // devuelve la celda de la meta más cercana
        private CellInfo FindClosestDestination(Nodo<Locomotion.MoveDirection> nodo, List<CellInfo> goals, bool noEnemies)
        {
            return goals[nodo.FindClosestGoal(goals, noEnemies)];
        }

        // Con entrada de una lista de destinos y una booleana noEnemies,
        // insertamos los enemigos encontrados en la lista de destinos
        private void FindEnemies(List<CellInfo> destinations, ref bool noEmenies)
        {
            if (GameObject.FindGameObjectWithTag("Enemy")) // buscamos objetos enemy
            {
                noEmenies = false; // al haber enemigos, noEnemies es falsa
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");// array para almacenar enemigos
                
                for(int i = 0; i<enemies.Length; i++){
                    destinations.Add(enemies[i].GetComponent<Locomotion>().CurrentPosition());
                }
            }
            
        }       
    }
}