﻿using System;
using System.Collections.Generic;

namespace Evacuation_Master_3000
{
    public class UserInterface : IUserInterface
    {
        public UserInterface() {
            TheMainWindow = new TheRealMainWindow(this);
        }

        public static bool IsSimulationPaused = false;
        public static bool HasSimulationEnded
        {
            set
            {
                if (value)
                {
                    OnSimulationEnd?.Invoke();
                } 
            }
        }

        public static bool ResetButtonClicked
        {
            set
            {
                if (value)
                {
                    OnReset?.Invoke();
                }
            }
        }

        public static event ResetClicked OnReset;
        public static event SimulationEnd OnSimulationEnd;
        private TheRealMainWindow TheMainWindow { get; }
        public event PrepareSimulation OnPrepareSimulation;
        public event UISimulationStart OnUISimulationStart;
        public event ImportFloorPlan OnImportFloorPlan;
        public event ExportFloorPlan OnExportFloorPlan;
        public event NewFloorPlan OnNewFloorPlan;
        public IFloorPlan LocalFloorPlan { get; private set; }
        private Dictionary<int, Person> People { get; set; } = new Dictionary<int, Person>();
        public IReadOnlyDictionary<int, Person> LocalPeopleDictionary => People;
        private bool _floorplanHasBeenCreated;

        public bool HeatMapActivated { get; private set; }

        public void Display() {
            TheMainWindow.ShowWindow();
        }

        public void DisplayGeneralErrorMessage(string errorMessage) {
            throw new NotImplementedException();
        }

        public void DisplayStatistics(DataSimulationStatistics dataSimulationStatistics) {
            throw new NotImplementedException();
        }

        

        public void CreateFloorplan(int width, int height, int floorAmount, string description)
        {
            if (!_floorplanHasBeenCreated)
            {
                LocalFloorPlan = OnNewFloorPlan?.Invoke(width, height, floorAmount, description);
                TheMainWindow.floorPlanVisualiserControl.ImplementFloorPlan(LocalFloorPlan, People);
                _floorplanHasBeenCreated = true;
            }
            else
            {

                System.Windows.MessageBox.Show("A building has already been made.");
            }
        }

        public void ImportFloorPlan(string filePath) {
            LocalFloorPlan = OnImportFloorPlan?.Invoke(filePath);
            People = OnPrepareSimulation?.Invoke(LocalFloorPlan);
            TheMainWindow.floorPlanVisualiserControl.ImplementFloorPlan(LocalFloorPlan, People);
        }

        public void ExportFloorPlan(string filePath) {

            People = OnPrepareSimulation?.Invoke(LocalFloorPlan);
            LocalFloorPlan = OnExportFloorPlan?.Invoke(filePath, LocalFloorPlan, People);
        }

        public void SimulationStart(bool showHeatMap, bool stepByStep, IPathfinding pathfinding, int milliseconds)
        {
            People = OnPrepareSimulation?.Invoke(LocalFloorPlan);
            /*People = */OnUISimulationStart?.Invoke(showHeatMap, stepByStep, pathfinding, milliseconds);
            HeatMapActivated = showHeatMap;
        }

        private void VisualizeFloorPlan() {
            //TheMainWindow.floorPlanVisualiserControl.
        }
    }
}