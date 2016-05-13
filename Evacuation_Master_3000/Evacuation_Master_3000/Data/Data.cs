using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Diagnostics;

[assembly: InternalsVisibleTo("P2TestEnvironment")]
namespace Evacuation_Master_3000
{
    internal class Data : IData
    {
        public Data()
        {
            _unevacuatedPeople = new List<Person>();
            TheFloorPlan = new FloorPlan();
            _allPeople = new Dictionary<int, Person>();
            //Person.OnPersonMoved += Statistics?
        }


        public IFloorPlan TheFloorPlan { get; private set; }
        private Dictionary<int, Person> _allPeople;
        public Dictionary<int, Person> AllPeople { get { return _allPeople; } }
        public event PersonMoved OnSendPersonMoved;

        public IEnumerable<Person> GetPeople(Predicate<Person> predicate)
        {
            return AllPeople.Values.Where(predicate.Invoke);
        }


        public Dictionary<int, Person> PrepareSimulation(IFloorPlan floorPlan) {
            TheFloorPlan.Initiate();
            foreach (Tile tile in floorPlan.Tiles.Values) {
                tile.OriginalType = tile.Type;

                if(tile.Type == Tile.Types.Person) {
                    Person current = new Person(tile as BuildingBlock);
                    if (!AllPeople.Values.Any(p => p.OriginalPosition == tile)) {
                        AllPeople.Add(current.ID, current);
                    }
                }
            }
            return AllPeople;
        }

        public static bool SimulationStart = true;
        public Dictionary<int, Person> StartSimulation(bool heatmap, bool stepByStep, IPathfinding pathfindingAlgorithm, int tickLength)                                           //<---- kan formentligt v�re void?
        {
            if (AllPeople != null) {
                _unevacuatedPeople.AddRange(AllPeople.Values);
                foreach (Person person in AllPeople.Values.Where(p => p.PathList.Count == 0)) {
                    person.OnPersonEvacuated += RemoveEvacuatedPerson;
                    person.TickLength = tickLength;
                    //person.OnExtendedPathRequest += IPathfinding.
                    person.PathList.AddRange(pathfindingAlgorithm.CalculatePath(person).Cast<BuildingBlock>().ToList());
                    person.Evacuated = false;
                }
            }

            StartTicks();
            return AllPeople;
        }
        public void StartTicks()
        {
            while (AllPeople.Values.Any(p => !p.Evacuated) && !UserInterface.IsSimulationPaused)
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                Yield(1);
                OnTick?.Invoke();
                stopWatch.Stop();
                // unchecked throws an OverflowException if we've spent more than 600+ hours on one tick.
            }

        }
        private void Yield(long ticks)
        {
            long dtEnd = DateTime.Now.AddTicks(ticks).Ticks;
            while (DateTime.Now.Ticks < dtEnd)
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate (object unused) { return null; }, null);
            }
        }
        private void RemoveEvacuatedPerson(Person person)
        {
            OnTick -= person.ConditionalMove;
        }

        public static event Tick OnTick;

        public DataSimulationStatistics GetSimulationStatistics()
        {
            throw new NotImplementedException();
        }

        private readonly List<Person> _unevacuatedPeople;

        public IFloorPlan ImportFloorPlan(string fileName)
        {
            BuildingInformationCollection buildingInformation = Import.ImportBuilding(fileName);
            IFloorPlan temporaryFloorPlan = CreateFloorPlan(buildingInformation);
            Import.EffectuateFloorPlanSettings(buildingInformation, ref temporaryFloorPlan, ref _allPeople);
            return temporaryFloorPlan;
        }

        public IFloorPlan CreateFloorPlan(BuildingInformationCollection buildingInformation)
        {
            string[] headers = new string[buildingInformation.Floors];
            for (int currentFloor = 0; currentFloor < buildingInformation.Floors; currentFloor++)
            {
                if (buildingInformation.FloorCollection[currentFloor] == null)
                    continue;

                headers[currentFloor] = buildingInformation.FloorCollection[currentFloor].Header;
            }
            return CreateFloorPlan(buildingInformation.Width, buildingInformation.Height, buildingInformation.Floors, buildingInformation.Description, headers);
        }

        public IFloorPlan CreateFloorPlan(int width, int height, int floorAmount, string description)
        {
            return CreateFloorPlan(width, height, floorAmount, description, null);
        }
        private IFloorPlan CreateFloorPlan(int width, int height, int floorAmount, string description, string[] headers)
        {
            TheFloorPlan.CreateFloorPlan(width, height, floorAmount, description, headers);
            return TheFloorPlan;
        }

        public IFloorPlan ExportFloorPlan(string filePath, IFloorPlan floorPlan, Dictionary<int, Person> allPeople)
        {
            Export.ExportBuilding(filePath, floorPlan, allPeople);
            return TheFloorPlan;                                                                            ////<<<-------- OBS OBS OBS skal TheFloorPlan s�ttes til floorPlan (metode parameter)???

        }

        public void ResetData()
        {
            //Reset floorplan
            //Reset Persons
        }

    }

}