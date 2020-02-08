using System;
using System.Collections.Generic;
using System.Diagnostics;

// https://stackoverflow.com/questions/50664273/how-to-create-a-generic-pipeline-in-c

namespace UglyToad.PdfPig.DocumentLayoutAnalysis.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    public class DLAContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DLAContext()
        {
            UniqueToken = new Guid();
            Logs = new List<string>();
            Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 
        /// </summary>
        internal Stopwatch Stopwatch { get; }

        /// <summary>
        /// 
        /// </summary>
        internal Guid UniqueToken { get; }

        /// <summary>
        /// 
        /// </summary>
        public long ProcessTimeInMilliseconds
        {
            get
            {
                return Stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Logs { get; set; }
    }
}
