// Guids.cs
// MUST match guids.h
using System;

namespace Vitevic.AssemblyEmbedder
{
    internal static class GuidList
    {
        public const string PkgGuidString = "bcb73f7d-0dd7-4683-9f54-5ac49c328125";
        public const string CmdSetGuidString = "3dc84af1-20ff-4b7f-88b7-56a808278b13";

        public static readonly Guid CmdSetGuid = new Guid(CmdSetGuidString);
    };
}
