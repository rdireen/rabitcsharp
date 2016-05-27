/***************************************************************************
=========================================================================
			DireenTech Inc. (www.DireenTech.com)

  File Name:      Utils.cs
  Created:        Aug. 30, 2012
  Author:		  Harry Direen PhD, DireenTech
  Description:    Various Utility Functions.
	
=========================================================================
Copyright (c) 2015 DireenTech Inc.
www.DireenTech.com
All or Portions of This Software are Copyrighted by DireenTech Inc.
                        Company Proprietary
=========================================================================
***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Rabit.Utils
{

    public class TimerObject
    {
        private Stopwatch _stopWatch;
        private double[] _measTimes = null;
        private int _measIdx;

        public int CurrTimerIdx
        {
            get { return _measIdx; }
        }

        public int NoTimeMeasurements
        {
            get { return _measIdx; }
        }

        public double[] MeasTimes
        {
            get { return _measTimes; }
        }

        public int MaxNoMeasTimes
        {
            get
            {
                if (_measTimes != null)
                    return _measTimes.Length;
                else
                    return 0;
            }
        }

        public TimerObject()
        {
            _stopWatch = new Stopwatch();
            _measTimes = new double[16];
            _measIdx = 0;
        }

        public TimerObject(int maxNoTimeCaptures)
        {
            _stopWatch = new Stopwatch();
            _measTimes = new double[maxNoTimeCaptures];
            _measIdx = 0;
        }

        public void setMaxNoTimeSamples(int maxNoTimeCaptures)
        {
            _measTimes = new double[maxNoTimeCaptures];
            _measIdx = 0;
        }

        public void clear()
        {
            _stopWatch.Stop();
            _measIdx = 0;
        }

        public void startMeasTime()
        {
            _stopWatch.Reset();
            _stopWatch.Start();
            _measIdx = 0;
        }

        public double captureTime()
        {
            if (_measIdx >= _measTimes.Length)
                _measIdx = _measTimes.Length - 1;

            double timeSec = 0.001 * (double)_stopWatch.ElapsedMilliseconds;
            _measTimes[_measIdx++] = timeSec;
            return timeSec;
        }

        public void stopMeasTime()
        {
            _stopWatch.Stop();
            captureTime();
        }

        public double getLastCaptureTime()
        {
            double time = 0;
            if (_measIdx > 0)
                time = _measTimes[_measIdx - 1];
            return time;
        }

        public double getTimeAtIdx(int idx)
        {
            idx = idx >= _measIdx ? _measIdx - 1 : idx;
            idx = idx < 0 ? 0 : idx;
            return _measTimes[idx];
        }
    }





}
