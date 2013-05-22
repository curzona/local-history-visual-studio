// Guids.cs
// MUST match guids.h
using System;

namespace Intel.LocalHistory
{
    static class GuidList
    {
        public const string guidLocalHistoryPkgString = "f39222a4-c9d6-4f51-8a0e-10b8b9dc1e4e";
        public const string guidLocalHistoryCmdSetString = "9c0eb15a-b3d9-4d2c-b4eb-57703d1ee539";
        public const string guidToolWindowPersistanceString = "d3b3b452-f976-4158-9451-d0a5ba7ee6a0";

        public static readonly Guid guidLocalHistoryCmdSet = new Guid(guidLocalHistoryCmdSetString);
    };
}