﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeyTake1.Alerters
{
    class FrontCameraAlerterAbstract : IAlerter
{
        private void GetFrontCameraPhoto()
        {

        }

        public bool GetData()
        {
            return false;
        }

        virtual public float ProcessData() {
            return -1;
        }
        
    }
    
}
