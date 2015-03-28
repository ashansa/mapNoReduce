using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public class Common
    {
        #region Initialization

        private static ILogger logger;

        #endregion

        #region Public Members

        /// <summary>
        /// Log instance initiation
        /// </summary>
        /// <returns></returns>
        public static ILogger Logger()
        {
            if (logger == null)
                logger = Log4NetLogger.GetInstance();
            return logger;
        }
        #endregion
    }
}
