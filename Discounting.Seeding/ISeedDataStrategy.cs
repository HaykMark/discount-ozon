﻿namespace Discounting.Seeding
{
    public interface ISeedDataStrategy<out T> where T : class
    {
        T[] GetSeedData();
    };

}
