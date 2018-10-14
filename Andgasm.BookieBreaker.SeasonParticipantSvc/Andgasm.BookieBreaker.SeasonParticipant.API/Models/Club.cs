﻿using System.ComponentModel.DataAnnotations;

namespace Andgasm.BookieBreaker.SeasonParticipant.API.Models
{
    public class Club
    {
        [Key]
        public string Key
        {
            get
            {
                return $"{CountryKey}-{Name}";
            }
            set { }
        }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string StadiumName { get; set; }
        public string CountryKey { get; set; }
    }
}
