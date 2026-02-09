using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ALGeo_GH
{
    public class ALGeo_GHInfo : GH_AssemblyInfo
    {
        public override string Name => "ALGeo";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("15bde3bb-4d33-4864-b349-dc8ed2a22f22");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}