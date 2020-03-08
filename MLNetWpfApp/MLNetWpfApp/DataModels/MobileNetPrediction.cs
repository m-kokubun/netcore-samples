using Microsoft.ML.Data;
using MLNetWpfApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLNetWpfApp.DataModels
{
    public class MobileNetPrediction
    {
        [ColumnName(MobileNetContext.MobileNetOutputColumnName)]
        public float[] Output { get; set; }
    }
}
