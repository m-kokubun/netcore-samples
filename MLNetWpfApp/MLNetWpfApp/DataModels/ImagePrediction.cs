using System;
using System.Collections.Generic;
using System.Text;

namespace MLNetWpfApp.DataModels
{
    public class ImagePrediction
    {
        public int Index { get; set; }

        public string Label { get; set; }

        public float Estimate { get; set; }
    }
}
