using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLNetWpfApp.DataModels
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath { get; set; }
    }
}
