// Guids.cs
// MUST match guids.h
using System;

namespace BenClarkRobinson.AccurateReferences
{
    static class GuidList
    {
        public const string guidAccurateReferences2010PkgString = "9dd84151-86dd-413f-b40f-e70556aca66a";
        public const string guidAccurateReferences2010CmdSetString = "bd6bf101-deb7-4793-8376-e8b93aeeb8e8";
        public const string guidToolWindowPersistanceString = "104b5319-a714-4b17-9c73-48511995d430";

        public static readonly Guid guidAccurateReferences2010CmdSet = new Guid(guidAccurateReferences2010CmdSetString);
    };
}