﻿#region

using System.Collections.Generic;
using System.Linq;
using static Evacuation_Master_3000.ImportExportSettings;

#endregion

namespace Evacuation_Master_3000
{
    class Graph
    {
        private static bool _firstRun = true;
        public List<BuildingBlock> Vertices = new List<BuildingBlock>();

        public Graph(List<BuildingBlock> vertices)
        {
            Vertices = vertices;
        }

        public List<BuildingBlock> AStar(BuildingBlock source, BuildingBlock destination)
        {
            SortedSet<BuildingBlock> priorityQueue = new SortedSet<BuildingBlock>(Comparer<BuildingBlock>.Default);
            Dictionary<string, BuildingBlock> closedSet = new Dictionary<string, BuildingBlock>();
            List<BuildingBlock> unvisitedVertices = Vertices.ToList();
            BuildingBlock current;

            foreach (BuildingBlock point in unvisitedVertices)
            {
                point.LengthToDestination = point.DistanceToPoint(destination);
                point.Parent = null;
                if (_firstRun)
                    _firstRun = false;
                else
                    point.LengthFromSource = 100000;
                point.IsChecked = false;
            }

            unvisitedVertices.Remove(source);
            source.LengthFromSource = 0;
            unvisitedVertices.Insert(0, source);
            current = unvisitedVertices[0];

            while (current != destination)
            {
                foreach (BuildingBlock point in current.Neighbours)
                {
                    if (point.IsChecked == false)
                    {
                        if (!closedSet.ContainsKey(Coordinate(point.X, point.Y)))
                        {
                            priorityQueue.Add(point);
                        }
                    }
                }
                CheckNeighbors(current, priorityQueue);
                if (priorityQueue.Count == 0)
                {
                    if (closedSet.ContainsKey(Coordinate(current.X, current.Y)) == false)
                        closedSet.Add(Coordinate(current.X, current.Y), current);

                    current = source;
                    foreach (BuildingBlock point in unvisitedVertices)
                    {
                        point.IsChecked = false;
                    }
                    continue;
                }
                current.IsChecked = true;
                current = priorityQueue.First();
                priorityQueue.Clear();
            }

            List<BuildingBlock> path = new List<BuildingBlock>();
            BuildingBlock parent = destination;
            do
            {
                path.Add(parent);
                parent = parent.Parent;
            } while (parent != null);
            path.Reverse();
            return path;
        }

        public void CheckNeighbors(BuildingBlock currentPoint, SortedSet<BuildingBlock> priorityQueue)
        {
            foreach (BuildingBlock neighbour in priorityQueue)
            {
                if (currentPoint.DistanceToPoint(neighbour) + currentPoint.LengthFromSource < neighbour.LengthFromSource)
                {
                    neighbour.LengthFromSource = currentPoint.DistanceToPoint(neighbour) + currentPoint.LengthFromSource;
                    neighbour.Parent = currentPoint;
                }
            }
        }
    }
}