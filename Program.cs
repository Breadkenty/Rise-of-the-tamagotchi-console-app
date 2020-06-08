using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using ConsoleTables;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http.Headers;

namespace Rise_of_the_tamagotchi_console_app
{
    class Program
    {
        public static void PressAnyKeyPrompt(string prompt)
        {
            Console.WriteLine(prompt);
            Console.ReadKey();
        }

        public static int AskForInt(string prompt)
        {
            Console.WriteLine(prompt);

            var result = 0;
            var goodInput = int.TryParse(Console.ReadLine(), out result);

            while (!goodInput)
            {
                Console.WriteLine($"Invalid input! please enter a number!");
                goodInput = int.TryParse(Console.ReadLine(), out result);
            }

            return result;
        }

        private static Pet SelectAPetByName(List<Pet> pets, string prompt)
        {
            Console.WriteLine($"\nAll pets:");

            foreach (var pet in pets)
            {
                Console.WriteLine($"- {pet.Name}");
            }

            Console.WriteLine(prompt);

            var nameInput = Console.ReadLine();
            var petExists = pets.Any(pet => pet.Name == nameInput);
            var petSelected = pets.FirstOrDefault(pet => pet.Name == nameInput);

            while (!petExists)
            {
                Console.WriteLine($"Pet doesn't exist. Try again...");
                nameInput = Console.ReadLine();

                petExists = pets.Any(pet => pet.Name == nameInput);
                petSelected = pets.FirstOrDefault(pet => pet.Name == nameInput);
            }

            return petSelected;
        }

        public static async Task<Pet> SelectPetById(string prompt)
        {
            try
            {
                var client = new HttpClient();

                var petId = AskForInt(prompt);
                var url = $"https://kento-tamagotchi.herokuapp.com/Pets/{petId}";

                var responseAsStream = await client.GetStreamAsync(url);

                var selectedPet = await JsonSerializer.DeserializeAsync<Pet>(responseAsStream);

                return selectedPet;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        static async Task AddNewPet(Pet newPet)
        {
            var client = new HttpClient();
            var url = $"http://kento-tamagotchi.herokuapp.com/Create_new_pet";

            var newPetToJSON = JsonSerializer.Serialize(newPet);

            var newPetJSONAsString = new StringContent(newPetToJSON);
            newPetJSONAsString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var postNewPet = await client.PostAsync(url, newPetJSONAsString);
        }

        static async Task DeletePet(int petId)
        {
            var client = new HttpClient();
            var url = $"http://kento-tamagotchi.herokuapp.com/Pets/{petId}";

            await client.DeleteAsync(url);
        }

        private static async Task InteractWithPet(int id, string interaction)
        {
            var client = new HttpClient();
            var url = $"http://kento-tamagotchi.herokuapp.com/Pets/{id}/{interaction}";

            var newPet = new Pet
            {
                Id = id
            };

            var newPetToJSON = JsonSerializer.Serialize(newPet);

            var newPetToJSONAsString = new StringContent(newPetToJSON);
            newPetToJSONAsString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var postNewPet = await client.PostAsync(url, newPetToJSONAsString);
        }

        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            bool applicationOpen = true;

            while (applicationOpen)
            {
                Console.Clear();
                Console.WriteLine($"------------------------");
                Console.WriteLine($"   Tamagotchi ver.K");
                Console.WriteLine($"------------------------");

                Console.WriteLine($"V: View pets | M: Manage pets | I: Interact with a pet | X: Quit");
                var menuInput = Console.ReadKey().KeyChar;

                if (menuInput == 'v')
                {
                    bool viewingPets = true;

                    while (viewingPets)
                    {
                        var responseAsStream = await client.GetStreamAsync("https://kento-tamagotchi.herokuapp.com/Pets");
                        var pets = await JsonSerializer.DeserializeAsync<List<Pet>>(responseAsStream);

                        Console.Clear();
                        Console.WriteLine($"A: View all pets | L: View all pets that are alive | I: View pet by ID | M: Main Menu");
                        var viewingInput = Console.ReadKey().KeyChar;

                        var table = new ConsoleTable("Id", "Name", "Birthday", "HungerLevel", "HappinessLevel", "LastInteracted", "Status");

                        if (viewingInput == 'a')
                        {
                            foreach (var pet in pets)
                            {
                                table.AddRow(pet.Id, pet.Name, pet.Birthday, pet.HungerLevel, pet.HappinessLevel, pet.LastInteracted, pet.IsDeadStatus());
                            }

                            table.Write();

                            PressAnyKeyPrompt("Press any key to continue...");
                        }

                        if (viewingInput == 'l')
                        {
                            responseAsStream = await client.GetStreamAsync("https://kento-tamagotchi.herokuapp.com/Pets_alive");
                            pets = await JsonSerializer.DeserializeAsync<List<Pet>>(responseAsStream);

                            foreach (var pet in pets)
                            {
                                table.AddRow(pet.Id, pet.Name, pet.Birthday, pet.HungerLevel, pet.HappinessLevel, pet.LastInteracted, pet.IsDeadStatus());
                            }

                            table.Write();

                            PressAnyKeyPrompt("Press any key to continue...");
                        }

                        if (viewingInput == 'i')
                        {
                            bool showingPet = true;

                            while (showingPet)
                            {
                                var selectedPet = await SelectPetById("Enter the pet ID");

                                if (selectedPet != null)
                                {
                                    Console.WriteLine($"Name: {selectedPet.Name} | Birthday: {selectedPet.Birthday} | Hunger: {selectedPet.HungerLevel} | Happiness: {selectedPet.HappinessLevel} | Last Interacted: {selectedPet.LastInteracted} | Status: {selectedPet.IsDeadStatus()}");

                                    PressAnyKeyPrompt("Press any key to continue...");
                                    showingPet = false;
                                }
                                else
                                {
                                    PressAnyKeyPrompt($"That pet doesn't exist. Press any key to try again...");
                                }
                            }
                        }

                        if (viewingInput == 'm')
                        {
                            viewingPets = false;
                        }
                    }
                }

                if (menuInput == 'm')
                {
                    bool managingPets = true;

                    while (managingPets)
                    {
                        var responseAsStream = await client.GetStreamAsync("https://kento-tamagotchi.herokuapp.com/Pets");
                        var pets = await JsonSerializer.DeserializeAsync<List<Pet>>(responseAsStream);

                        Console.Clear();
                        Console.WriteLine($"A: Add a pet | D: Delete a pet | M: Main Menu");
                        var managingInput = Console.ReadKey().KeyChar;

                        if (managingInput == 'a')
                        {
                            Console.WriteLine("What's the name of the pet?");
                            var nameInput = Console.ReadLine();

                            var newPet = new Pet
                            {
                                Name = nameInput
                            };

                            await AddNewPet(newPet);

                            PressAnyKeyPrompt($"{nameInput} has been added! Press any key to continue...");
                        }

                        if (managingInput == 'd')
                        {
                            var petSelected = SelectAPetByName(pets, "\nWhich pet did you want to delete?");

                            await DeletePet(petSelected.Id);

                            PressAnyKeyPrompt($"{petSelected.Name} has been deleted! Press any key to continue...");
                        }

                        if (managingInput == 'm')
                        {
                            managingPets = false;
                        }
                    }
                }

                if (menuInput == 'i')
                {
                    bool interactingWithPets = true;

                    while (interactingWithPets)
                    {
                        var responseAsStream = await client.GetStreamAsync("https://kento-tamagotchi.herokuapp.com/Pets");
                        var pets = await JsonSerializer.DeserializeAsync<List<Pet>>(responseAsStream);

                        Console.Clear();
                        Console.WriteLine($"P: Play with a pet | F: Feed a pet | S: Scold a pet | M: Main Menu");
                        var interactingInput = Console.ReadKey().KeyChar;

                        if (interactingInput == 'p')
                        {
                            var selectedPet = SelectAPetByName(pets, "\nWhich pet did you want to play with?");
                            var id = selectedPet.Id;

                            await InteractWithPet(id, "Play");

                            PressAnyKeyPrompt($"{selectedPet.Name}'s happiness has gone up by 5 points! \n{selectedPet.Name}'s hunger has gone up by 3 points! \nPress any key to continue...");
                        }

                        if (interactingInput == 'f')
                        {
                            var selectedPet = SelectAPetByName(pets, "\nWhich pet did you want to feed?");
                            var id = selectedPet.Id;

                            await InteractWithPet(id, "Feed");

                            PressAnyKeyPrompt($"{selectedPet.Name}'s happiness has gone up by 3 points! \n{selectedPet.Name}'s hunger has gone down by 5 points! \nPress any key to continue...");
                        }

                        if (interactingInput == 's')
                        {
                            var selectedPet = SelectAPetByName(pets, "\nWhich pet did you want to scold?");
                            var id = selectedPet.Id;

                            await InteractWithPet(id, "Scold");

                            PressAnyKeyPrompt($"{selectedPet.Name}'s happiness has gone down 5 points! \nPress any key to continue...");
                        }

                        if (interactingInput == 'm')
                        {
                            interactingWithPets = false;
                        }
                    }
                }

                if (menuInput == 'x')
                {
                    PressAnyKeyPrompt($"See you again real soon! Press any key to exit...");
                    applicationOpen = false;
                }
            }
        }
    }
}
