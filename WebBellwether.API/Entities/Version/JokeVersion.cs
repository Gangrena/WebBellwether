﻿using WebBellwether.API.Entities.Translations;

namespace WebBellwether.API.Entities.Version
{
    public class JokeVersion
    {
        public int Id { get; set; }
        public virtual Language Language { get; set; }
        public double Version { get; set; }
        public int NumberOfJokes { get; set; }
    }
}