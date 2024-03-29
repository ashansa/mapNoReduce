﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    
   public class Constants
    {
       public const double maxThreshold = 80;
       public const double jobReplaceBoundaryPercentage = 50;
       public const string APPSETT_CLIENT_URL= "CLIENT_URL";
       public const string APPSET_DLL_PATH = "DLL_PATH";
       public const string APPSETT_PUPPETS_URL = "PUPPET_URLS";
       public const string JOB_TRACKER_HEARTBEAT_INTERVAL ="JOB_TRACKER_HEARTBEAT_INTERVAL";
       public const string TASK_TRACKER_HEARTBEAT_INTERVAL = "TASK_TRACKER_HEARTBEAT_INTERVAL";
       public const string TASK_TRACKER_TIMEOUT_SECONDS = "TASK_TRACKER_TIMEOUT_SECONDS";
       public const char SEP_PIPE = '/';
       public const char COLON_STR = ':';
       public const char SPACE_CHAR = ' ';
       public const int RETRY_ROUNDS = 2;
       public const int criticalRetryRounds = 5;
    }
}
