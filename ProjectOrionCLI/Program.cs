// See https://aka.ms/new-console-template for more information
using ShipSheets;
using Compendium;

const string PROMPT = ">  ";
Ship ship = null;
Compendium.Compendium compendium = Compendium.Compendium.GetCompendium();

string GetMountDescription(Mount mount)
{
    return String.Format("Mount S{0}C{1} ({2} {3}): {4}", mount.Size, mount.Count, mount.MainArc, mount.Type, (mount.Weapon != null) ? mount.Weapon.Name : "Empty");
}

string GetBayDescription(Bay bay)
{
    return String.Format("Bay S{0}C{1} ({2}): {3}", bay.Size, bay.Count, String.Join("", (from arc in bay.Arcs select arc.GetDisplayString())), (bay.Craft != null) ? bay.Craft.Name : "Empty");
}

string GetSystemDescription(ShipSystem system)
{
    return String.Format("{0} ({1} slots)", system.Name, system.Slots);
}

void ShowShip(Ship ship)
{
    Console.WriteLine();
    Console.WriteLine(String.Format("{0}: {1}", (ship.Identifier == "" || ship.Identifier == null) ? "<No Identifier>": ship.Identifier, ship.Name));
    Console.WriteLine("Mounts:");
    int i = 0;
    foreach (Mount mount in ship.Mounts)
    {
        Console.WriteLine(String.Format("{0} | {1}", ++i, GetMountDescription(mount)));
    }
    i = 0;
    Console.WriteLine("Bays:");
    foreach (Bay bay in ship.Bays)
    {
        Console.WriteLine(String.Format("{0} | {1}", ++i, GetBayDescription(bay)));
    }
    i = 0;
    Console.WriteLine(String.Format("Systems ({0}/{1} slots free):", ship.GetFreeSystemSlots(), ship.SystemSlots));
    foreach (ShipSystem system in ship.Systems)
    {
        if (system.Slots > 0) Console.WriteLine(String.Format("{0} | {1}", ++i, GetSystemDescription(system)));
    }
}

Ship SelectShip()
{
    Console.WriteLine("Enter the name of the ship template to use (ex: Emblem).");
    Ship shipTemplate = null;
    string input = "";
    while (shipTemplate == null)
    {
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Console.WriteLine("No input found. Exiting.");
            Environment.Exit(0);
        }
        if (input == "" && shipTemplate != null) return ship;
        shipTemplate = compendium.GetShip(input);
        if (shipTemplate == null)
        {
            Console.WriteLine(String.Format("{0} does not match any ship template. Enter another name.", input));
        }
    }
    SelectIdentifier(shipTemplate);
    compendium.EquipDefaultSystems(shipTemplate);
    compendium.AddDefaultTraits(shipTemplate);
    return shipTemplate;
}

void SelectIdentifier(Ship ship)
{
    string input = "";
    Console.WriteLine("Enter the identifier of the ship.");
    Console.Write(PROMPT);
    input = Console.ReadLine();
    if (input == null)
    {
        Console.WriteLine("No input found. Exiting.");
        Environment.Exit(0);
    }
    ship.Identifier = input;
}

void EquipMount(Ship ship, int number)
{
    if (number < 1 || number > ship.Mounts.Length)
    {
        Console.WriteLine(String.Format("The ship has no mount with number {0}.", number));
        return;
    }
    var mount = ship.Mounts[number-1];
    var success = false;
    string input;
    Console.WriteLine("\nEnter the name of the weapon to equip (ex: Light Spinal Rail), nothing to make no changes, or '-' to remove the weapon.");
    Console.WriteLine(String.Format("{0} | {1}", number, GetMountDescription(mount)));
    while (!success)
    {
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Environment.Exit(0);
        } else if (input == "")
        {
            return;
        } else if (input == "-")
        {
            mount.Weapon = null;
            Console.WriteLine("Unequipped weapon from mount.");
            success = true;
        } else
        {
            Weapon weapon = compendium.GetWeapon(input);
            if (weapon != null)
            {
                try
                {
                    mount.Weapon = weapon;
                    Console.WriteLine("Equipped weapon to mount!");
                    success = true;
                } catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                
            } else
            {
                Console.WriteLine(string.Format("No weapon named {0}. Make sure the weapon name is capitalized correctly.", input));
            }
        }
    }
}

void EquipMounts(Ship ship)
{
    for (int i = 1; i <= ship.Mounts.Length; i++)
    {
        EquipMount(ship, i);
    }
}

void EquipBay(Ship ship, int number)
{
    if (number < 1 || number > ship.Bays.Length)
    {
        Console.WriteLine(String.Format("The ship has no bay with number {0}.", number));
        return;
    }
    var bay = ship.Bays[number - 1];
    var success = false;
    string input;
    Console.WriteLine("\nEnter the name of the craft to equip (ex: Light Missile), nothing to make no changes, or '-' to remove the craft.");
    Console.WriteLine(String.Format("{0} | {1}", number, GetBayDescription(bay)));
    while (!success)
    {
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Environment.Exit(0);
        }
        else if (input == "")
        {
            return;
        }
        else if (input == "-")
        {
            bay.Craft = null;
            Console.WriteLine("Unequipped craft from bay.");
            success = true;
        }
        else
        {
            Craft craft = compendium.GetCraft(input);
            if (craft != null)
            {
                try
                {
                    bay.Craft = craft;
                    Console.WriteLine("Equipped craft to bay!");
                    success = true;
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                Console.WriteLine(string.Format("No craft named {0}. Make sure the craft name is capitalized correctly.", input));
            }
        }
    }
}

void EquipBays(Ship ship)
{
    for (int i = 1; i <= ship.Bays.Length; i++)
    {
        EquipBay(ship, i);
    }
}

void AddSystem(Ship ship)
{
    var success = false;
    string input;
    Console.WriteLine(string.Format("\nEnter the name of the system to equip (ex: Bulk Magazine), or nothing to go back. Free slots: {0}/{1}.", ship.GetFreeSystemSlots(), ship.SystemSlots));
    while (!success)
    {
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Environment.Exit(0);
        }
        else if (input == "")
        {
            return;
        }
        else
        {
            ShipSystem system = compendium.GetSystem(input);
            if (system != null)
            {
                try
                {
                    ship.Equip(system);
                    Console.WriteLine("Equipped system!");
                    success = true;
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                Console.WriteLine(string.Format("No system named {0}. Make sure the system name is capitalized correctly.", input));
            }
        }
    }
}

void RemoveSystem(Ship ship)
{
    var success = false;
    string input;
    Console.WriteLine("\nEnter the name of the system to remove (ex: Bulk Magazine), or nothing to go back.");
    int i = 0;
    foreach (ShipSystem system in ship.Systems)
    {
        if (system.Slots > 0) Console.WriteLine(String.Format("{0} | {1}", ++i, GetSystemDescription(system)));
    }
    while (!success)
    {
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Environment.Exit(0);
        }
        else if (input == "")
        {
            return;
        }
        else
        {
            ShipSystem system = compendium.GetSystem(input);
            if (system != null)
            {
                if (system.Slots > 0)
                {
                    ship.Systems.Remove(system);
                    success = true;
                    Console.WriteLine("System removed.");
                } else
                {
                    Console.WriteLine(string.Format("Cannot remove default system {0}. Only systems that cost slots can be removed.", input));
                }
                
            }
            else
            {
                Console.WriteLine(string.Format("No system named {0}. Make sure the system name is capitalized correctly.", input));
            }
        }
    }
}

void ExportShip(Ship ship)
{
    var sheetCreator = new ShipSheetCreator(ship);
    bool success = false;
    string input;
    while (!success)
    {
        Console.WriteLine("Enter the path where the sheet should be saved (ex: test.pdf), or nothing to go back.");
        Console.Write(PROMPT);
        input = Console.ReadLine();
        if (input == null)
        {
            Environment.Exit(0);
        }
        if (input.ToLower() == "") return;

        try
        {
            sheetCreator.CreateSheet(input, true);
            success = true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong. Possibly could not find or could not modify the file " + input);
            Console.WriteLine(e.StackTrace);
        }
    }
    Console.WriteLine("Sheet saved!");
}

void ShowCommands()
{
    Console.WriteLine();
    Console.WriteLine("Command list:");
    Console.WriteLine("When typing commands, do not enter the quotes ' or triangle brackets <>.");
    Console.WriteLine("\nBasic commands:\n");

    Console.WriteLine("'help': show this command list");
    Console.WriteLine("'ship': select a new ship template to use, discarding the current one");
    Console.WriteLine("'show': show the state of the current ship");
    Console.WriteLine("'sheet': output the character sheet for the current ship");
    Console.WriteLine("'exit': exit the application");

    Console.WriteLine("\nShip editing commands:\n");

    Console.WriteLine("'id': change the ship's identifier");
    Console.WriteLine("'mounts': equip weapons to all mounts, 1 by 1");
    Console.WriteLine("'mount <number>': equip a weapon to the mount with the corresponding number");
    Console.WriteLine("'bays': equip crafts to all bays, 1 by 1");
    Console.WriteLine("'bay <number>': equip a craft to the bay with the corresponding number");
    Console.WriteLine("'system add': add a non-default system");
    Console.WriteLine("'system remove': remove a non-default system");
}

void CommandLoop()
{
    var exiting = false;
    ShowCommands();
    while (!exiting)
    {
        
        Console.WriteLine("Enter a command ('help' for command list):");
        Console.Write(PROMPT);
        var input = Console.ReadLine().ToLower();
        var command = input.Trim().Split(" ");
        bool parseSuccess;
        int numberArgument;
        string suboptionArgument;
        switch (command[0])
        {
            case "help":
                ShowCommands();
                break;
            case "ship":
                ship = SelectShip();
                break;
            case "sheet":
                ExportShip(ship);
                break;
            case "show":
                ShowShip(ship);
                break;
            case "exit":
                exiting = true;
                break;
            case "id":
                SelectIdentifier(ship);
                break;
            case "mounts":
                EquipMounts(ship);
                break;
            case "mount":
                if (command.Length == 1)
                {
                    Console.WriteLine("Must provide a number for the mount.");
                    break;
                }
                parseSuccess = int.TryParse(command[1], out numberArgument);
                if (parseSuccess)
                {
                    EquipMount(ship, numberArgument);
                } else
                {
                    Console.WriteLine(command[1] + " is not an integer.");
                }
                break;
            case "bays":
                EquipBays(ship);
                break;
            case "bay":
                if (command.Length == 1)
                {
                    Console.WriteLine("Must provide a number for the bay.");
                    break;
                }
                parseSuccess = int.TryParse(command[1], out numberArgument);
                if (parseSuccess)
                {
                    EquipBay(ship, numberArgument);
                }
                else
                {
                    Console.WriteLine(command[1] + " is not an integer.");
                }
                break;
            case "system":
                if (command.Length == 1 || !(command[1] == "add" || command[1] == "remove"))
                {
                    Console.WriteLine("Must provide 'add' or 'remove' in the command.");
                    break;
                }
                if (command[1] == "add")
                {
                    AddSystem(ship);
                } else if (command[1] == "remove") {
                    RemoveSystem(ship);
                }
                break;
            default:
                Console.WriteLine("Invalid command.");
                break;
        }
    }
    Console.WriteLine("Exiting.");
}

void Main()
{
    ship = SelectShip();
    ShowShip(ship);
    CommandLoop();
}

Main();