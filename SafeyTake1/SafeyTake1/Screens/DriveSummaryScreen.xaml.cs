﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SafeyTake1.DataStructures;

namespace SafeyTake1.Screens
{
    public partial class DriveSummaryScreen : PhoneApplicationPage
    {
        public DriveSummaryScreen()
        {
            InitializeComponent();
        }

        private void populateFields(Drive drive)
        {
            txtDrivingTime = drive.EndTime.Subtract(drive.StartTime).ToString("HH:MM");
            txtDriverAVGScore = drive.Driver;
        }
    }
}