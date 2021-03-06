﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Evacuation_Master_3000
{
    /// <summary>
    /// Interaction logic for CP_SimulationStats.xaml
    /// </summary>
    public partial class CP_SimulationStats
    {
        public CP_SimulationStats(MainWindow parentWindow)
        {
            InitializeComponent();
            Person.OnPersonMoved += UpdateSimulationStats;
            Data.OnTick += UpdateTicksAndTime;
            UserInterface.OnReset += ResetPeopleAndSimulationInformation;
            SimulationSpeed.ValueChanged += ChangeSimSpeed;
            _parentWindow = parentWindow;
        }


        readonly List<Person> _evacuatedPeopleList = new List<Person>();
        private int _ticks;
        private double _fillWidthPerPerson;
        private readonly MainWindow _parentWindow;
        /// <summary>
        /// Resets everything when we click reset, resets the stats, and person pathlist. Clears each evacuated people pathlist,
        /// and copies the steps from the stats to path.
        /// </summary>
        private void ResetPeopleAndSimulationInformation()
        {
            foreach (Person person in _parentWindow.TheUserInterface.LocalPeopleDictionary.Values.Where(p => !p.NoPathAvailable))
            {
                if (!Equals(person.OriginalPosition, person.Position))
                {
                    person.Position.Type = person.Position.OriginalType;
                    person.Position = person.OriginalPosition;
                    person.Position.Type = person.Position.OriginalType;
                }
                person.AmountOfTicksSpent = 0;
                person.PersonInteractionStats.DistanceTraveled = 0;
                person.PersonInteractionStats.TicksWaited = 0;
                if (person.Evacuated)
                {
                    person.Evacuated = false;
                    person.PathList.Clear();
                    foreach (MovementStep movementStep in person.PersonInteractionStats.MovementSteps)
                    {
                        person.PathList.Add(movementStep.SourceTile as BuildingBlock);
                    }
                    person.PathList.Add(person.PersonInteractionStats.MovementSteps.Last().DestinationTile as BuildingBlock);
                }
                if (!Equals(person.PathList.First(), person.OriginalPosition))
                {
                    person.PathList.Clear();
                }
                person.StepsTaken = 0;
                person.PersonInteractionStats.MovementSteps.Clear();
            }
            _ticks = -1;
            _ticksBeforeChange = 0;
            _timeElapsedField = 0;
            _timeElapsedBeforeChange = 0;
            _totalTimeElapsedField = 0;
            _oldSimSpeed = 0;
            _evacuatedPeopleList.Clear();
            UpdateTicksAndTime();
            PersonsEvacuatedProgressBarFill.Width = 0;
            PersonsEvacuatedProgressBarText.Text = "0%";
            CurrentNumberOfEvacuatedPersons.Text = "0";
            PeopleWithNoPathAmount.Text = "0";
            UserInterface.IsSimulationReady = true;
        }

        public static bool UpdateTickCondition;
        private bool _hasSimSpeedBeenChanged;
        private void ChangeSimSpeed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (UserInterface.IsSimulationPaused)
            {
                foreach (Person person in _parentWindow.TheUserInterface.LocalPeopleDictionary.Values)
                {
                    person.UpdateTickCondition = true;
                }
                _hasSimSpeedBeenChanged = true;
            }
        }
        private int _ticksBeforeChange;
        private double _timeElapsedField;
        private double _totalTimeElapsedField;
        private double _timeElapsedBeforeChange;
        private double _oldSimSpeed;
        private int _totalTicksBeforeChange;
        /// <summary>
        /// Updates the ticks, somewhat updates the ticks correctly, if the simulation speed increases or decreases. Still some minor issues.
        /// </summary>
        private void UpdateTicksAndTime()
        {
            if (_hasSimSpeedBeenChanged)
            {
                _ticksBeforeChange = (_ticks - _ticksBeforeChange);
                _totalTicksBeforeChange += _ticksBeforeChange;
                if (_ticksBeforeChange != 0)
                {
                    _timeElapsedBeforeChange += Math.Round(_ticksBeforeChange / _oldSimSpeed, 2);
                }
                _hasSimSpeedBeenChanged = false;
            }
            _ticks++;
            _timeElapsedField = Math.Round((Math.Abs(_ticks - _totalTicksBeforeChange)) / SimulationSpeed.Value, 2);
            _totalTimeElapsedField = _timeElapsedBeforeChange + _timeElapsedField;
            TimeElapsedInDateTimeFormat.Text = _totalTimeElapsedField + " Seconds";
            TicksElapsed.Text = _ticks + " Ticks";
            _oldSimSpeed = SimulationSpeed.Value;
        }
        /// <summary>
        /// Updates the simulation stats, bar, evacuated people count etc.
        /// </summary>
        /// <param name="person"></param>
        private void UpdateSimulationStats(Person person)
        {
            int peopleCount = _parentWindow.TheUserInterface.LocalPeopleDictionary.Count(p => !p.Value.NoPathAvailable);
            PeopleWithNoPathAmount.Text = (_parentWindow.TheUserInterface.LocalPeopleDictionary.Count - peopleCount) + "";
            TotalPersonCount.Text = peopleCount + "";
            _fillWidthPerPerson = (PersonsEvacuatedProgressBarBackground.ActualWidth) / peopleCount;
            if (person.Evacuated)
            {
                if (!_evacuatedPeopleList.Contains(person))
                {
                    _evacuatedPeopleList.Add(person);
                    double multiplier = 255f / peopleCount;
                    double count = multiplier * _evacuatedPeopleList.Count;
                    PersonsEvacuatedProgressBarFill.Fill = BarColor(count);
                    PersonsEvacuatedProgressBarFill.Width = _fillWidthPerPerson * _evacuatedPeopleList.Count;
                    CurrentNumberOfEvacuatedPersons.Text = _evacuatedPeopleList.Count + "";
                    double percentageEvacuated = ((double)_evacuatedPeopleList.Count) / peopleCount * 100;
                    PersonsEvacuatedProgressBarText.Text = Math.Round(percentageEvacuated, 2) + "%";
                    person.PersonInteractionStats.TimeWhenEvacuated = _totalTimeElapsedField;
                }
                if (peopleCount == _evacuatedPeopleList.Count)
                {
                    //Simulation has ended, so now we writes to the text file, uses stringbuilder all the way through
                    UserInterface.HasSimulationEnded = true;
                    UserInterface.IsSimulationReady = false;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Statistics for simulation run at {DateTime.Now} by {Environment.UserName}{Environment.NewLine}");
                    double timeElapsed = _evacuatedPeopleList.Max(p => p.PersonInteractionStats.TimeWhenEvacuated);
                    sb.AppendLine($"Time elapsed in simulation: {timeElapsed} seconds");
                    sb.AppendLine($"Amount of people: {_evacuatedPeopleList.Count}{Environment.NewLine}");
                    double quarterOfTimeElapsed = timeElapsed * 0.25;
                    double halfOfTimeElapsed = timeElapsed * 0.50;
                    double threeQuarterOfTimeElapsed = timeElapsed * 0.75;
                    sb.AppendLine(
                        $"Amount of people evacuated at {quarterOfTimeElapsed} seconds: " +
                        $"{Math.Round((double)_evacuatedPeopleList.Count(p => p.PersonInteractionStats.TimeWhenEvacuated <= quarterOfTimeElapsed) / (double)_evacuatedPeopleList.Count * 100, 2)}%");
                    sb.AppendLine(
                        $"Amount of people evacuated at {halfOfTimeElapsed} seconds: " +
                        $"{Math.Round((double)_evacuatedPeopleList.Count(p => p.PersonInteractionStats.TimeWhenEvacuated <= halfOfTimeElapsed) / (double)_evacuatedPeopleList.Count * 100, 2)}%");
                    sb.AppendLine(
                        $"Amount of people evacuated at {threeQuarterOfTimeElapsed} seconds: " +
                        $"{Math.Round((double)_evacuatedPeopleList.Count(p => p.PersonInteractionStats.TimeWhenEvacuated <= threeQuarterOfTimeElapsed) / (double)_evacuatedPeopleList.Count * 100, 2)}%");
                    sb.AppendLine(
                        $"Amount of people evacuated at {timeElapsed} seconds: " +
                        $"{_evacuatedPeopleList.Count(p => p.PersonInteractionStats.TimeWhenEvacuated <= timeElapsed) / _evacuatedPeopleList.Count * 100}%");

                    double averageTime = _evacuatedPeopleList.Sum(p => p.PersonInteractionStats.TimeWhenEvacuated) /
                                         _evacuatedPeopleList.Count;
                    sb.AppendLine($"Average evacuation time:{Math.Round(averageTime, 2)}");

                    double[] timearray = _evacuatedPeopleList.Select(p => p.PersonInteractionStats.TimeWhenEvacuated).ToArray();
                    var sortedTimearray = timearray.OrderBy(t => t);
                    var median = sortedTimearray.ElementAt(sortedTimearray.Count() / 2);

                    sb.AppendLine($"{Environment.NewLine}Evacuated before {median} seconds:");
                    foreach (Person person1 in _evacuatedPeopleList.Where(p => p.PersonInteractionStats.TimeWhenEvacuated < median))
                    {
                        sb.AppendLine($"Person: {person1.ID}    Time:{person1.PersonInteractionStats.TimeWhenEvacuated} seconds");
                    }

                    sb.AppendLine($"{Environment.NewLine}Evacuated after or at {median} seconds:");
                    foreach (Person person1 in _evacuatedPeopleList.Where(p => p.PersonInteractionStats.TimeWhenEvacuated >= median))
                    {
                        sb.AppendLine($"Person: {person1.ID}    Time:{person1.PersonInteractionStats.TimeWhenEvacuated} seconds");
                    }

                    Person personWithLongestDistance = _evacuatedPeopleList.OrderByDescending(p => p.PersonInteractionStats.DistanceTraveled).First();
                    Person personWithLongestTimeBeforeEvacuated = _evacuatedPeopleList.OrderByDescending(p => p.PersonInteractionStats.TimeWhenEvacuated).First();
                    sb.AppendLine($"{Environment.NewLine}Longest distance:{Environment.NewLine}" +
                                  $"{personWithLongestDistance.ID} - {personWithLongestDistance.PersonInteractionStats.DistanceTraveled} m{Environment.NewLine}");
                    sb.AppendLine($"Most time before evacuated: {Environment.NewLine}" +
                                  $"{personWithLongestTimeBeforeEvacuated.ID} - {personWithLongestTimeBeforeEvacuated.PersonInteractionStats.TimeWhenEvacuated} seconds{Environment.NewLine}");

                    foreach (Person person1 in _evacuatedPeopleList.OrderBy(p => p.ID))
                    {
                        sb.AppendLine(
                            "_____________________________________________________________________________________________________________");
                        int movementStepCounter = 0;
                        sb.AppendLine(
                            $"Person: {person1.ID}, Position{Settings.Coordinate(person.OriginalPosition)}{Environment.NewLine}" +
                            $"Steps taken: {person1.StepsTaken}{Environment.NewLine}" +
                            $"Distance traveled in meters: {person1.PersonInteractionStats.DistanceTraveled} m{Environment.NewLine}" +
                            $"Movement speed: {Math.Round(person1.MovementSpeedInMetersPerSecond, 2)} m/s{Environment.NewLine}" +
                            $"Time at evacuation: {person1.PersonInteractionStats.TimeWhenEvacuated} seconds{Environment.NewLine}" +
                            $"Time waited because of blocked path: {Math.Round((double)person1.PersonInteractionStats.TicksWaited / SimulationSpeed.Value, 2)} seconds{Environment.NewLine}{Environment.NewLine}" +
                            $"List of steps:");
                        foreach (MovementStep movementStep in person1.PersonInteractionStats.MovementSteps)
                        {
                            movementStepCounter++;
                            sb.AppendLine(
                                $"Step:{movementStepCounter}   From: {Settings.Coordinate(movementStep.SourceTile)}\n" +
                                $" - To: {Settings.Coordinate(movementStep.DestinationTile)}\n" +
                                $"      Distance in meters: {movementStep.DistanceInMeters} m{Environment.NewLine}" +
                                $"          Time at arrival: {Math.Round((double)movementStep.TicksAtArrival / SimulationSpeed.Value, 2)} seconds{Environment.NewLine}");
                        }
                    }
                    string stringToWrite = sb.ToString();
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\Statistics");
                    string nameOfStatsFile = $"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}.txt";
                    string path = Environment.CurrentDirectory + @"\Statistics\" + nameOfStatsFile;
                    StatisticsTextbox.Text = $"Look at:{Environment.NewLine}{path}";
                    File.WriteAllText(path, stringToWrite);
                }
            }
        }
        /// <summary>
        /// Used to give a color to the simulation bar.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static SolidColorBrush BarColor(double count)
        {
            if (count > 255)
            {
                count = 255;
            }
            var red = 255 - count;
            var green = 0 + count;
            var blue = 0;
            return new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
        }
    }
}
