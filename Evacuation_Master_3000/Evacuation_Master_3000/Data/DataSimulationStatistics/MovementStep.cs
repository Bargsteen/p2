﻿using System;

namespace Evacuation_Master_3000
{
    public class MovementStep
    {
        public MovementStep(Person person, Tile sourceTile, Tile destinationTile)
        {
            Person = person;
            SourceTile = sourceTile;
            DestinationTile = destinationTile;
            Distance = sourceTile.DistanceTo(destinationTile);
            DistanceInMeters = Distance * GlobalVariables.BlockWidthInMeters;
            Person.PersonInteractionStats.DistanceTraveled += DistanceInMeters;
        }

        public int TicksAtArrival { get; set; }
        public Person Person { get; set; }
        public Tile SourceTile { get; set; }
        public Tile DestinationTile { get; set; }
        public double Distance { get; set; }

        private double _distanceMeters;
        public double DistanceInMeters
        {
            get { return Math.Round(_distanceMeters, 2); }
            set { _distanceMeters = value; }
        }
    }
}
