namespace SpriteSplitter
{
    public static class ExampleMasks
    {
        public static bool[,] ExampleMask1()
        {
            #region disable_format
            bool[,] map = {
                { true, true , false, false, false },
                { true, true , false, false, true  },
                { true, false, false, true , true  },
                { true, false, false, true , false },
                { true, false, true , true , false }
            };
            #endregion

            return map;
        }

        public static bool[,] ExampleMask2()
        {
            #region disable_format
            bool[,] map = {
                { true , true , false, false, false },
                { false, true , false, false, true  },
                { true , false, false, true , true  },
                { false, false, false, false, false },
                { true , false, true , false, false }
            };
            #endregion
            
            return map;
        }
    }
}
