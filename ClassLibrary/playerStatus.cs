using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public class playerStatus
    {
        public bool isRunning;
        public bool isReloading;
        public bool isShooting;
        public bool isWalking;

        public playerStatus()
        {
            isRunning = false;
            isReloading = false;
            isShooting = false;
            isWalking = false;
        }
    }
}
