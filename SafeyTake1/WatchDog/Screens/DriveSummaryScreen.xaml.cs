﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WatchDOG.DataStructures;
using WatchDOG.Helpers;

namespace WatchDOG.Screens
{
    public partial class DriveSummaryScreen : PhoneApplicationPage
    {
        #region Constructor
        public DriveSummaryScreen()
        {
            InitializeComponent();
        }
        #endregion

        #region UI Methods

        /// <summary>
        /// Populate the UI Fields via the Drive variable passed.
        /// </summary>
        /// <param name="drive">Last Drive to be used</param>
        private void populateFields(Drive drive)
        {
            txtDrivingTime.Text = drive.EndTime.Subtract(drive.StartTime).ToString("hh:mm");
            txtDriverAVGScore.Text = String.Format("{0:0.##}", drive.Driver.AverageScore);
            AlertEvent commonAlert = drive.Events.GroupBy(evnt => evnt.AlertType)
                .OrderByDescending(type => type.Count()).SelectMany(g => g).FirstOrDefault();

            txtCommonHazzard.Text = commonAlert != null ? 
                WatchDogHelper.GetEnumDescription(commonAlert.AlertType) : 
                "--";
            txtSafetyScore.Text = drive.Events.Count() > 0 ? 
                String.Format("{0:0.##}",drive.Events.Average(evnt => evnt.AlertLevel)) 
                : "0";
        }
        #endregion

        #region Button Behavior
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Drive lastDrive = (Drive)PhoneApplicationService.Current.State["CurrentDrive"];
            populateFields(lastDrive);

        }

        /// <summary>
        /// Try to return to the Main Screen or terminate if you can't
        /// </summary>
        private void Close()
        {   

            if (NavigationService.CanGoBack)
            {
                // Go back to Main Screen
                NavigationService.RemoveBackEntry();
                NavigationService.GoBack();
            }
            else
                // Exit if can't
                Application.Current.Terminate();
        }

        #endregion

    }
}