﻿using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Microsoft.Devices;
using WatchDOG.Alerters;
using WatchDOG.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using WatchDOG.Helpers;
using WatchDOG.Screens;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using WatchDog;
using Microsoft.WindowsAzure.MobileServices;


namespace WatchDOG.Logic
{
    class DriveLogic
    {

        
        #region Constants
        /// <summary>
        /// Threshold from whatlevel up to alarm.
        /// </summary>
        public const double ALERT_THRESHOLD = 90;

        public const int DURATION_INTERVAL = 1000;

        #endregion


        #region Private Properties

        private IMobileServiceTable<AlertEvent> alertEventsTable = App.MobileService.GetTable<AlertEvent>();
        private List<FrontCameraAlerterAbstract> frontAlerters;
        internal Drive _currentDrive;
        private Driver _currentDriver;
        private string _alertMessage;
        private Boolean GPSEnabled;
        private Geoposition myLocation;


        
        #endregion 

        #region Constructor
        public DriveLogic(Driver driver)
        {
            _currentDriver = driver;
            frontAlerters = new List<FrontCameraAlerterAbstract>();
            frontAlerters.Add(new EyeDetectorAlerter());

            _currentDrive = new Drive(_currentDriver, DateTime.Now);


        }

        #endregion

        private Boolean isGPSEnabled()
        {
            if (Settings.IsGpsEnabledSettings == true)
            {
                Geolocator geolocator = new Geolocator();
                return !(geolocator.LocationStatus == PositionStatus.Disabled);
            }
            else return false;
        }

        
        

        //Option #1
        private Task<Geoposition> getLocationTask()
        {
            Geolocator geolocator = new Geolocator();
            return geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(2)
                ).AsTask<Geoposition>();

        }

        /*
        //Option #2
        private void GetCoordinate()
        {
            GeoCoordinateWatcher watcher;
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
            {
                MovementThreshold = 1
            };
            watcher.Start();
        }
        */


        /*
        //Option 3
        private async void OneShotLocation_Click(object sender, RoutedEventArgs e)
        {

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            
            Geoposition geoposition = await geolocator.GetGeopositionAsync(
                maximumAge: TimeSpan.FromMinutes(5),
                timeout: TimeSpan.FromSeconds(10)
                );

            LatitudeTextBlock.Text = geoposition.Coordinate.Latitude.ToString("0.00");
            LongitudeTextBlock.Text = geoposition.Coordinate.Longitude.ToString("0.00");
            
            
        }
        */

        #region Public Properties

        public float Score { get; set; }

        public int[] ContinousSafeTime { get; set; }


        internal List<AlertEvent> EventsList { get; set; }


        public Speed Speed { get; set; }

        public string AlertMessage
        {
            get { return _alertMessage; }
        }

        #endregion






        public double AnalyzeFrontPicture(WriteableBitmap bitmap)
        {
        // Foreach Alerter, try to analize the value.
            List<AlertEvent> alertEvents = new List<AlertEvent>();
            foreach (IAlerter alerter in frontAlerters)
            {
                alerter.GetData();

                AlertEvent alertEvent = new AlertEvent()
                {
                    AlertLevel = alerter.ProcessData(bitmap),
                    AlertTime = DateTime.Now,
                    AlertType = alerter.GetAlerterType(),
                    Driver = _currentDriver

                };

                //uploadEventToDatabase(alertEvent);

                if (alertEvent.AlertLevel >= ALERT_THRESHOLD)
                {
                    if (isGPSEnabled())
                    {
                        //Get GPS Location
                        var task = Task.Run(async () => { myLocation = await getLocationTask(); });
                        task.Wait() ;
                        
                        alertEvent.AlertLocation = myLocation.Coordinate;
                        myLocation = null;
                        
                    }

                    _currentDrive.Events.Add(alertEvent);
                    alertEvents.Add(alertEvent);
                }

                
            }

            // Calculate the safety score (for all valid values).
            return calculateSafetyScore(alertEvents.Where(_event => _event.AlertLevel >= 0));

            
        }

        private async void uploadEventToDatabase(AlertEvent alertEvent)
        {
            await alertEventsTable.InsertAsync(alertEvent);
        }


        
        
        #region Safety Calculation Functions
        private double calculateSafetyScore(IEnumerable<AlertEvent> alertEvents)
        {
            // Alert level is the Average of all alerters score
            double ret = alertEvents.Count() > 0 ? alertEvents.Average(alert => alert.AlertLevel) : 0; 
            
            if (ret > ALERT_THRESHOLD)
            {
                EAlertType highestAlertType = alertEvents.OrderByDescending(_event => _event.AlertLevel).FirstOrDefault().AlertType;
                updateTheMessageByType(highestAlertType);
            }
            else
            {
                _alertMessage = "Drive Safely";
            }
            return ret;
        }

        private void updateTheMessageByType(EAlertType highestAlertType)
        {
            switch (highestAlertType)
            {
                case EAlertType.EyeDetectionAlert:
                    _alertMessage = "Open Your Eyes!";
                    break;
                case EAlertType.DistanceAlert:
                    _alertMessage = "Keep Your Distance";
                    break;
                case EAlertType.LaneCrossingAlert:
                    _alertMessage = "Keep in Your Lane";
                    break;
                default:
                    _alertMessage = "Drive Safely";
                    break;
            }
        }
        #endregion

    }

    internal delegate void AlertEventHandler(object sender, AlertEventHandlerArgs args);

    public class AlertEventHandlerArgs
    {
        public AlertEventHandlerArgs(double level, string msg)
        {
            Level = level;
            Message = msg;
        }

        public double Level { get; set; }
        public string Message { get; set; }
    }
}

