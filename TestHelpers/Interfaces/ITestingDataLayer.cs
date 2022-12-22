﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataHarvester
{
    public interface ITestingDataLayer
    {
        Credentials Credentials { get; }

        int EstablishExpectedData();
        void TransferTestSDData(ISource source);
        IEnumerable<int> ObtainTestSourceIDs();
    }
}
