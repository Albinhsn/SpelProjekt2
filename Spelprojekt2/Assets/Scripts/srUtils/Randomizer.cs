namespace srUtils
{
    public class Randomizer // Static System.Random for efficiency (on average 7 times faster than creating a new System.Random for each call)
    {
        private System.Random _random;
        private static Randomizer _instance;
        public static Randomizer instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new Randomizer();
                }
                return _instance;
            }
        }

        private Randomizer()
        {
            _random = new System.Random();
        }

        public static int Next() => instance._random.Next();
        public static int Next(int max) => instance._random.Next(max);
        public static int Next(int min, int max) => instance._random.Next(min, max);
        public static double NextDouble() => instance._random.NextDouble();
        public static double NextDouble(double max) => instance._random.NextDouble() * max;
        public static double NextDouble(double min, double max) => min + instance._random.NextDouble() * (max - min);
    }
}