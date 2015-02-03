// This files contains the interfaces used to interop with the VS SQM COM components.
#pragma warning disable 3001

using System;
using System.Runtime.InteropServices;

namespace Salesforce.VisualStudio.Services.ConnectedService.Utilities
{
    [ComImport()]
    [ComVisible(false)]
    [Guid("C1F63D0C-4CAE-4907-BE74-EEB75D386ECB")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IVsSqm
    {
        void GetSessionStartTime(
            [Out] out System.Runtime.InteropServices.ComTypes.FILETIME time
            );
        void GetFlags(
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 flags
            );
        void SetFlags(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 flags
            );
        void ClearFlags(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 flags
            );
        void AddItemToStream(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        void SetDatapoint(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        // OBSOLETE IN SQMAPI.DLL. DO NOT CALL.
        void GetDatapoint(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 value
            );
        void EnterTaggedAssert(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dwTag,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dwPossibleBuild,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dwActualBuild
            );
        void RecordCmdData(
            [In] ref Guid pguidCmdGroup,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        void GetHashOfGuid(
            [In] ref Guid hashGuid,
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 resultantHash
            );
        void GetHashOfString(
            [In, MarshalAs(UnmanagedType.BStr)] string hashString,
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 resultantHash
            );
        void IncrementDatapoint(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );

        void SetDatapointBits(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );

        void SetDatapointIfMax(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        void SetDatapointIfMin(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        void AddToDatapointAverage(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 value
            );
        void StartDatapointTimer(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID
            );
        void RecordDatapointTimer(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID
            );
        void AccumulateDatapointTimer(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID
            );
        void AddTimerToDatapointAverage(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID
            );
        void AddArrayToStream(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 dataPointID,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4, SizeParamIndex = 2)] System.UInt32[] data,
            [In, MarshalAs(UnmanagedType.I4)] int count
        );
    }

    [ComImport()]
    [ComVisible(false)]
    [Guid("16be4288-950b-4265-b0dc-280b89ca9979")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IVsSqmOptinManager
    {
        void GetOptinStatus(
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 optinStatus,
            [Out, MarshalAs(UnmanagedType.U4)] out System.UInt32 preferences
            );

        void SetOptinStatus(
            [In, MarshalAs(UnmanagedType.U4)] System.UInt32 optinStatus
            );
    }

    [ComImport()]
    [ComVisible(false)]
    [Guid("2508FDF0-EF80-4366-878E-C9F024B8D981")]
    internal interface SVsLog
    {
    }
}
