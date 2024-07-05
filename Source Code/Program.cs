using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        MTGOParser parser = new();
        //display program enter text
        parser.Greet();
        while (parser.exit != true)
        {
            //Get file (or files)
            parser.GetInput();
            //Regex for getting players playing cards: @P(\w*? )*?(plays|casts) @\[.*?@:\d*?,\d*?:@]
            parser.GetPlayedCards();
            //Regex for getting players getting triggered abilities of cards (in case something hasn't been cast): @P(\w*? )*?puts triggered ability from @\[.*?@:\d*?,\d*?:@]
            parser.GetTriggeredAbilities();
            //Append the results to a file.
            parser.Export();
            //Do everything again (if the user wants)
            parser.LoopCheck();
        }
        //display program exit text
        parser.Exit();
    }
}

public partial class MTGOParser
{
    [GeneratedRegex("@P(\\w*? )*?(plays|casts) @\\[.*?@:\\d*?,\\d*?:@\\]")]
    private static partial Regex PlayedCardsPattern();

    [GeneratedRegex("@P(\\w*? )*?puts triggered ability from @\\[.*?@:\\d*?,\\d*?:@]")]
    private static partial Regex AbilitiesPattern();

    [GeneratedRegex("@P(\\w*? )*?(?=plays|casts|puts)")]
    private static partial Regex PlayerPattern();

    [GeneratedRegex("@P")]
    private static partial Regex PlayerMarkerPattern();

    [GeneratedRegex("@\\[.*?@:\\d*?,\\d*?:@\\]")]
    private static partial Regex CardPattern();

    [GeneratedRegex("@\\[")]
    private static partial Regex CardFrontMarkerPattern();

    [GeneratedRegex("@:\\d*?,\\d*?:@\\]")]
    private static partial Regex CardBackMarkerPattern();


    public bool exit = false;
    string fileText = "";
    List<string> players = [];
    List<string> playerAssociatedCards = [];
    const string VERSION = "0.0.1";

    public void Greet()
    {
        Console.WriteLine("Fungustober\'s MTGO replay parser (v" + VERSION + ")");
    }

    public void GetInput()
    {
        string path = "";
        while (path == "" || path == null)
        {
            Console.Write("Please enter the replay's file path: ");
            try
            {
                path = Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("There is an error with the file path.");
            }
        }
        //add processing here to make sure that it's a MTGO DAT file
        //we're processing
        //and that the file at the path exists
        fileText = File.ReadAllText(path);
    }

    public void GetPlayedCards()
    {
        Console.WriteLine("Parsing played cards...");
        Match match = PlayedCardsPattern().Match(fileText);
        while (match.Success)
        {
            string matchInstance = match.Value;
            string player = PlayerPattern().Match(matchInstance).Value;
            player = PlayerMarkerPattern().Replace(player, "");
            player = player.Substring(0, player.Length - 1);
            string card = CardPattern().Match(matchInstance).Value;
            card = CardFrontMarkerPattern().Replace(card, "");
            card = CardBackMarkerPattern().Replace(card, "");
            if (players.Contains(player) != true)
            {
                players.Add(player);
            }
            playerAssociatedCards.Add(player + ": " + card + " (played/cast).");
            match = match.NextMatch();
        }
    }

    public void GetTriggeredAbilities()
    {
        Console.WriteLine("Parsing triggered abilities from cards...");
        Match match = AbilitiesPattern().Match(fileText);
        while (match.Success)
        {
            string matchInstance = match.Value;
            string player = PlayerPattern().Match(matchInstance).Value;
            player = PlayerMarkerPattern().Replace(player, "");
            player = player.Substring(0, player.Length - 1);
            string card = CardPattern().Match(matchInstance).Value;
            card = CardFrontMarkerPattern().Replace(card, "");
            card = CardBackMarkerPattern().Replace(card, "");
            if (players.Contains(player) != true)
            {
                players.Add(player);
            }
            playerAssociatedCards.Add(player + ": " + card + " (triggered).");
            match = match.NextMatch();
        }
        playerAssociatedCards.Sort();
    }

    public void Export()
    {
        Console.WriteLine("Exporting...");
        players.Sort();
        string gameName = "Game: ";
        foreach (string p in players)
        {
            gameName += p;
            if (players.IndexOf(p) != players.Count -1)
            {
                gameName += " vs. ";
            }
        }
        playerAssociatedCards.Insert(0, gameName);
        File.AppendAllLines("output.txt", playerAssociatedCards);
        Console.WriteLine("Data exported to output.txt");
    }

    public void LoopCheck()
    {
        string checker = "";
        while (checker == "" || checker == null)
        {
            Console.Write("Do you wish to parse another file? (Y/N) ");
            try
            {
                checker = Console.ReadLine();
                if (checker != "y" && checker != "Y" && checker != "n" && checker != "N")
                {
                    throw new Exception();
                }
            }
            catch
            {
                Console.WriteLine("An error occured with input.");
            }
        }
        if (checker == "n" || checker == "N")
        {
            exit = true;
        }
    }

    public void Exit()
    {
        Console.WriteLine("Press any key to exit program.");
        Console.Read();
    }

    public MTGOParser() { }
}