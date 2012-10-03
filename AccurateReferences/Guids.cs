// Guids.cs
// MUST match guids.h
using System;

namespace BenClarkRobinson.AccurateReferences
{
    static class GuidList
    {
        public const string guidAccurateReferencesPkgString = "7484edf4-0ab6-46a6-9a77-d1029ac12ff9";
        public const string guidAccurateReferencesCmdSetString = "af086e42-2710-4f37-b8f8-e1c37c813989";
        public const string guidToolWindowPersistanceString = "655a8b1e-20ae-4f43-bfa2-ac76cca45d2c";

        public static readonly Guid guidAccurateReferencesCmdSet = new Guid(guidAccurateReferencesCmdSetString);
    };
}