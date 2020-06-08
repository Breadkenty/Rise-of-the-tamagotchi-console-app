using System;
using System.Text.Json.Serialization;

namespace Rise_of_the_tamagotchi_console_app
{
    class Pet
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("birthday")]
        public DateTime Birthday { get; set; }

        [JsonPropertyName("hungerLevel")]
        public int HungerLevel { get; set; }

        [JsonPropertyName("happinessLevel")]
        public int HappinessLevel { get; set; }

        [JsonPropertyName("lastInteracted")]
        public DateTime LastInteracted { get; set; }

        [JsonPropertyName("isDead")]
        public bool IsDead { get; set; }

        public string IsDeadStatus()
        {
            if (IsDead == false)
            {
                return "Alive";
            }
            else
            {
                return "Dead";
            }
        }

    }
}
