using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.tracker
{
    public class TrackerTask
    {
        TrackerDetails trackerDetails = new TrackerDetails();

        public TrackerDetails TrackerDetails
        {
            get { return trackerDetails; }
            set { trackerDetails = value; }
        }


    }
}
