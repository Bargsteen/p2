﻿using System;
using System.Windows;

namespace Evacuation_Master_3000 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void StartUp(object sender, StartupEventArgs e) {
            IData data = new Data();
            IUserInterface ui = new UserInterface();
            Controller controller = new Controller(data, ui);
            controller.Start();
        }
    }
}
