using System;
using System.Collections.Generic;

namespace LaunchBoxRomPatchManager.Model
{
    public class PatcherPlatform
    {
        public string PlatformName { get; set; }
    }


    // Custom comparer for the Product class
    class PatcherPlatformComparer : IEqualityComparer<PatcherPlatform>
    {
        public bool Equals(PatcherPlatform x, PatcherPlatform y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.PlatformName == y.PlatformName;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(PatcherPlatform patcherPlatform)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(patcherPlatform, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = patcherPlatform.PlatformName == null ? 0 : patcherPlatform.PlatformName.GetHashCode();

            //Get hash code for the Code field.
            int hashProductCode = patcherPlatform.PlatformName.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName ^ hashProductCode;
        }
    }
}
