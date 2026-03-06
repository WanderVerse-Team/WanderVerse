using UnityEngine;

namespace WanderVerse.Framework.Data
{
    public static class CourseCatalog
    {
        // The Game Levels for Grade 3 Mathematics
        public const string L01_1 = "L01_Counting";
        public const string L02 = "L02_Numbers1";
        public const string L03 = "L03_Addition1";
        public const string L04 = "L04_Length1";
        public const string L05 = "L05_Subtraction1";
        public const string L06 = "L06_Time";
        public const string L07 = "L07_Multiplication1";
        public const string L08 = "L08_Shapes";
        public const string L09 = "L09_Division1";
        public const string L10 = "L10_Fractions";
        public const string L11 = "L11_Directions";

        // This array is what the Unity Inspector will use to create the dropdown
        public static readonly string[] AllLevels = new string[]
        {
            L01_1, L02, L03, L04, L05, L06, L07, L08, L09, L10, L11
        };
    }
}