﻿using System;
using System.Collections.Generic;
using static Evacuation_Master_3000.ImportExportSettings;
namespace Evacuation_Master_3000
{
    public class Tile
    {
        public Tile(int x, int y, int z = 0, Types type = Types.Free)
        {
            Neighbours = new HashSet<Tile>();
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
        public enum Types
        {
            Free,
            Occupied,
            Furniture,
            Wall,
            Door,
            Exit,
            Person,
            Stair
        }
        public Types Type { get; set; }
        public Types OriginalType { get; set; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; } // Translates into the floor level
        public HashSet<Tile> Neighbours { get; }
        // Needs to remember original Types for after a person leaves. <<--- Needs implementation

        public double DiagonalDistanceTo(BuildingBlock point)
        {
            double xDistance = Math.Abs(X - point.X);
            double yDistance = Math.Abs(Y - point.Y);
            if (xDistance > yDistance)
                return 14 * yDistance + 10 * (xDistance - yDistance);
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        public double DistanceTo(Tile other)
        {
            //Diagonal distance
            // 15 represents the amount of steps on a staircase. 
            return Math.Abs(Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2)) + Math.Abs(Z-other.Z)*15);   
        }

        public override string ToString()
        {
            return Coordinate(this) + " : " + Type;
        }

        public override int GetHashCode()
        {
            return X * 7 + Y * 11 + Z * 13;
        }

        public override bool Equals(object obj)
        {
            Tile other = obj as Tile;
            return other != null && (Z == other.Z && Y == other.Y && X == other.X && Type == other.Type);
        }
    }
}