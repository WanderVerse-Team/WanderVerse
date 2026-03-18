using UnityEngine;

namespace WanderVerse.Framework.Data
{
    public static class CourseCatalog
    {
        // Default unassigned state
        public const string NONE = "None";

        // Lesson 1: Counting - Hungry Golem
        public const string L01_GOL_1 = "L01_GOL_1";
        public const string L01_GOL_2 = "L01_GOL_2";
        public const string L01_GOL_3 = "L01_GOL_3";

        // Lesson 2: Numbers 1 - Talking Door
        public const string L02_DOR_1 = "L02_DOR_1";
        public const string L02_DOR_2 = "L02_DOR_2";

        // Lesson 2: Numbers 1 - Treasure Packer
        public const string L02_PAC_1 = "L02_PAC_1";
        public const string L02_PAC_2 = "L02_PAC_2";

        // Lesson 3: Addition 1 - Power Station
        public const string L03_POW_1 = "L03_POW_1";
        public const string L03_POW_2 = "L03_POW_2";
        public const string L03_POW_3 = "L03_POW_3";

        // Lesson 6: Time - Space Time Traveler
        public const string L06_SPA_1 = "L06_SPA_1";

        // Lesson 10: Fractions - Crumble's Bakery
        public const string L10_BAK_1 = "L10_BAK_1";
        public const string L10_BAK_2 = "L10_BAK_2";
        public const string L10_BAK_3 = "L10_BAK_3";

        // This array is what the Unity Inspector will use to create the dropdown
        public static readonly string[] AllLevels = new string[]
        {
            NONE,
            L01_GOL_1, L01_GOL_2, L01_GOL_3,
            L02_DOR_1, L02_DOR_2, L02_PAC_1, L02_PAC_2,
            L03_POW_1, L03_POW_2, L03_POW_3,
            L06_SPA_1,
            L10_BAK_1, L10_BAK_2, L10_BAK_3
        };
    }
}