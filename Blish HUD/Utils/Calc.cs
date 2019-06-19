using System;

namespace Blish_HUD.Utils {
    public static class Calc {

        private static Random rand;

        public static int GetRandom(int leftBound, int rightBound) {
            if (rand == null) rand = new Random();

            return rand.Next(leftBound, rightBound);
        }

        public static int GetRandomWithSeed(int leftBound, int rightBound, int seed) {
            return new Random(seed).Next(leftBound, rightBound);
        }

    }
}
