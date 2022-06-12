﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Programming
{
    public class ProgrammingEventManager : IProgrammingEventManager
    {
        public event EventHandler<ProgrammingEventArgs> ProgrammingEventRaised;

        public void OnCurrentEcuChanged(IEcuProgrammingInfo ecu)
        {
            if (this.ProgrammingEventRaised != null)
            {
                this.ProgrammingEventRaised(this, new ProgrammingCurrentEcuChangedEventArgs(DateTime.Now, ecu));
            }
        }

        public void OnProgrammingActionStateChanged(IEcu ecu, ProgrammingActionType programmingActionType, ProgrammingActionState programmingActionState)
        {
            if (this.ProgrammingEventRaised != null)
            {
                this.ProgrammingEventRaised(this, new ProgrammingActionStateChangedEventArgs(DateTime.Now, ecu, programmingActionType, programmingActionState));
            }
        }

        public void OnProgressChanged(string taskName, double progress, double timeLeftSec, bool isTaskFinished)
        {
            if (this.ProgrammingEventRaised != null)
            {
                this.ProgrammingEventRaised(this, new ProgrammingTaskEventArgs(DateTime.Now, taskName, progress, timeLeftSec, isTaskFinished));
            }
        }
    }
}
