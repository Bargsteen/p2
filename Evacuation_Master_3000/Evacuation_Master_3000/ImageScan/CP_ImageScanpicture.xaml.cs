﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.IO;
using System.Drawing;
using static Evacuation_Master_3000.Settings;

namespace Evacuation_Master_3000.ImageScan
{
    /// <summary>
    /// Interaction logic for CP_ImageScanPicture.xaml
    /// </summary>
    public partial class CP_ImageScanPicture
    {
        private static Bitmap _theImage;
        private double[,] _pixelsRegular;
        private double[,] _pixelsSobel;
        private double[,] _pixelsCurrentlyActive;
        private const int _maxWidth = 200;
        private const int _maxHeight = 200;
        private bool _firstTimeDrawing = true;
        private bool _sobelFilterActivated;
        private ImageScanWindow ParentWindow { get; }
        public bool SobelFilterActivated
        {
            private get { return _sobelFilterActivated; }
            set
            {
                _sobelFilterActivated = value;
                _pixelsCurrentlyActive = value ? _pixelsSobel : _pixelsRegular;
            }
        }
        private int ImageWidth { get; set; }
        private int ImageHeight { get; set; }

        public CP_ImageScanPicture(ImageScanWindow parentWindow, string imageFilePath)
        {
            
            InitializeComponent();
            ParentWindow = parentWindow;

            CalculateAndSetupVisualRepresentation(imageFilePath);  
        }

        private void CalculateAndSetupVisualRepresentation(string imageFilePath)
        {
            if (!File.Exists(imageFilePath))
            {
                throw new FileNotFoundException();
            }
            _theImage = ImageScanningTools.ResizeIfNecessary(new Bitmap(imageFilePath), _maxWidth, _maxHeight);

            ImageWidth = _theImage.Width;
            ImageHeight = _theImage.Height;

            _pixelsRegular = ImageScanningTools.ConvertImageToGrayscale(_theImage);
            _pixelsSobel = ImageScanningTools.ApplySobelFilter(_pixelsRegular, ImageWidth, ImageHeight);
            _pixelsCurrentlyActive = _pixelsRegular;
            CreateOrUpdateVisualRepresentation();
        }


        public void CreateOrUpdateVisualRepresentation()
        {
            if (_firstTimeDrawing)
            {
            	// First time it loops through, creates and colors the rectangles.
                for (int y = 0; y < ImageHeight; y++)
                {
                    for (int x = 0; x < ImageWidth; x++)
                    {
                        int[] coordinates = {x, y};

                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle()
                        {
                            Height = 10,
                            Width = 10,
                            Tag = coordinates,
                            Fill = DecideColor(_pixelsCurrentlyActive[y, x])
                        };
                        BuildingBlockContainer.Children.Add(rect);
                    }
                }
                _firstTimeDrawing = false;
            }
            else
            {
                foreach (System.Windows.Shapes.Rectangle rect in BuildingBlockContainer.Children)
                {
                    if (rect is System.Windows.Shapes.Rectangle)
                    {
                        System.Windows.Shapes.Rectangle rectangle = rect as System.Windows.Shapes.Rectangle;
                        int[] coordinates = rectangle.Tag as int[];
                        if (coordinates != null)
                        {
                            rectangle.Fill = DecideColor(_pixelsCurrentlyActive[coordinates[1], coordinates[0]]);
                        }
                        else
                        {
                            throw new GeneralInternalException("The rectangles coordinates could not be found.");
                        }
                        
                    }
                }
            }
        }
        

        private SolidColorBrush DecideColor(double pixelValue)
        {
            if (SobelFilterActivated)
            {
                return pixelValue >= ParentWindow.CpImageScanControls.ContrastSlider.Value
                    ? new SolidColorBrush(Colors.Black)
                    : new SolidColorBrush(Colors.White);
            }
            // The Sobel filter gives a high value if there is a big contrast.
            return pixelValue >= ParentWindow.CpImageScanControls.ContrastSlider.Value
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Colors.Black);
        }


        public void SaveAsGridFile(string filePath)
        {
            Dictionary<string, Tile> theTiles = new Dictionary<string, Tile>();
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                	// Decides the Tile.Type based on the ConstrastSliders value.
                    Tile.Types theType;
                    if (SobelFilterActivated)
                        theType = _pixelsCurrentlyActive[y, x] >= ParentWindow.CpImageScanControls.ContrastSlider.Value ? Tile.Types.Wall: Tile.Types.Free;
                    else
                        theType = _pixelsCurrentlyActive[y, x] <= ParentWindow.CpImageScanControls.ContrastSlider.Value ? Tile.Types.Wall : Tile.Types.Free;

                    Tile currentTile = new Tile(x, y, 0, theType);

                    theTiles.Add(Coordinate(currentTile), currentTile);   
                }
            }
            // Creates a floorplan for use with the ExportBuilding function.
            // The filepath is read from the filepath field in the window.
            IFloorPlan theFloorPlan = new FloorPlan();
            theFloorPlan.CreateFloorPlan(ImageWidth, ImageHeight, theTiles);
            Export.ExportBuilding(filePath, theFloorPlan, new Dictionary<int, Person>());
        }

    }
}
