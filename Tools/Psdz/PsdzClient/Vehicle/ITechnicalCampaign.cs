﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum technicalCampaignTypeState
    {
        open,
        active,
        closed
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum typeTechnicalCampaignRecallType
    {
        NONE,
        SAFETY,
        EMISSION,
        NONCOMPLIANT
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITechnicalCampaign : INotifyPropertyChanged
    {
        string description { get; }

        string labelCode { get; }

        string localCampaignNumber { get; }

        uint maxFlatRate { get; }

        bool maxFlatRateSpecified { get; }

        uint minFlatRate { get; }

        bool minFlatRateSpecified { get; }

        string serviceInformationNumber { get; }

        string specialDefectCode { get; }

        technicalCampaignTypeState state { get; }

        IEnumerable<string> SoftwareVersions { get; }

        typeTechnicalCampaignRecallType RecallType { get; }

        bool IsSalesStop { get; }

        bool IsSoftwareCampaign { get; }
    }
}
