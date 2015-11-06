using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace society_tag
{
    public class InvalidCountException : Exception { }

    class Player
    {
        public string name;
        public string email;
        public int id;
        public int target;
        public int hunter;
        public bool untagged;
        public int tags;
        public bool disqualified;

        public string status() { throw new NotImplementedException(); }

        // for killing players who are tagged
        public void tag() { untagged = false; }

        // for setting new targets after a successful tag
        public void setNewTarget(int newTarget) { target = newTarget; }
        
        // for setting new hunter
        public void setNewHunter(int newHunter) { hunter = newHunter; }

        public void setNewID(int newID) { id = newID; }

        // for incrementing tags
        public void incTags() { ++tags; }

        // for disqualifying players
        public void disqualify() { disqualified = true; }

        public Player() { }

        public Player(string newName)
        {
            name = newName;
            email = "Don't have emails yet";
            // id = random number
            target = -1;
            hunter = -1;
            untagged = true;
            tags = 0;
            disqualified = false;
        }

        public Player(string newName, string newEmail)
        {
            name = newName;
            email = newEmail;
            id = -1;
            target = -1;
            hunter = -1;
            untagged = true;
            tags = 0;
            disqualified = false;
        }

        public Player(string[] args)
        {
            if (args.Length != 2)
                throw new InvalidCountException();
            name = args[0];
            email = args[1];
            id = -1;
            target = -1;
            hunter = -1;
            untagged = true;
            tags = 0;
            disqualified = false;
        }
    }//name email id target hunter untagged score disqualified

    class Game
    {
        const string gamenotinit = "Game not initialized. Please use init or open first.\n";

        string filename;

        string response;

        int taggedPenalty;
        int tagsBonus;
        int numAlive;

        Dictionary<int, Player> players;

        bool initiated;

        static string[] empty = new string[] { };

        Player player = null;

        public Game()
        {
            initiated = false;
        }

        public void Exit()
        {
            if (!initiated)
                return;
            this.Flush(new string[] { "",filename });

        }

        //FINSIHED
        //flush [<fname>] - opens the game file and writes the current state of the game to it. If <fname> is provided, writes current state to fname as backup
        public void Flush(string[] args)
        {
            if (!initiated)
            {
                response = gamenotinit;
                return;
            }

            string fname = filename;
            if (args != empty && args.Length > 1)
                fname = args[1];
            using (StreamWriter writer = new StreamWriter(fname))
            {
                string writeline;
                writeline = taggedPenalty + "," + tagsBonus + "," + numAlive + "\n";
                foreach(Player p in players.Values)
                {//name email id target hunter untagged score disqualified
                    writeline += p.name + "," + p.email + "," + p.id + "," + p.target + "," + p.hunter + "," + p.untagged + "," + p.tags + "," + p.disqualified + "," + players[p.target].name + "," + players[p.hunter].name + "\n";
                }
                writeline = writeline.Substring(0, writeline.Length - 1);
                writer.Write(writeline);
            }
        }

        //FINISHED
        //open <gfile> - loads the game located at gfile, defaults to 'society-tag.gfile' in the local directory
        public void Open(string[] args)
        {
            filename = (args.Length <= 1) ? "society-tag.gfile" : args[1];
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    players = new Dictionary<int, Player>();
                    string[] sline;
                    string line = reader.ReadLine();
                    char[] commarray = new char[] { ',' };
                    if (line == null)  
                    {
                        response = "Error: " + filename + " is empty. Open failed.\n";
                        reader.Close();
                        return;
                    }
                    sline = line.Split(commarray);

                    taggedPenalty = Convert.ToInt32(sline[0]);
                    tagsBonus = Convert.ToInt32(sline[1]);
                    numAlive = Convert.ToInt32(sline[2]);

                    Player p;
                    while ((line = reader.ReadLine()) != null && line != "")
                    {
                        sline = line.Split(commarray);
                        //name email id target hunter untagged score disqualified
                        p = new Player();
                        p.name = sline[0];
                        p.email = sline[1];
                        p.id = Convert.ToInt32(sline[2]);
                        p.target = Convert.ToInt32(sline[3]);
                        p.hunter = Convert.ToInt32(sline[4]);
                        p.untagged = Convert.ToBoolean(sline[5]);
                        p.tags = Convert.ToInt32(sline[6]);
                        p.disqualified = Convert.ToBoolean(sline[7]);

                        players.Add(p.id, p);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                response = "Error: " + filename + " not found.  Open failed.\n";
                initiated = false;
                return;
            }
            initiated = true;
        }

        //JOEL
        //init <pfile> <gfile> - creates a new game using <pfile> as the participants list and outputting the game state to <gfile>
        public void Init(string[] args)
        {
            string pfile, gfile;
            pfile = (args.Length < 2)? "participants-list.txt" : args[2];
            gfile = (args.Length < 3) ? "society-tag.gfile" : args[3];
            // pull in participants list (name, email)
            List<Player> people = new List<Player>();
            try
            {
                using (StreamReader r = new StreamReader(pfile))
                {
                    string line;
                    char[] commarray = new char[] {','};
                    while ((line = r.ReadLine()) != null && line != "")
                    {
                        Player newPlayer = new Player(line.Split(commarray));
                        people.Add(newPlayer);
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                response = "Error, " + pfile + " was not found.\n";
                initiated = false;
                return;
            }

            List<Player> processedPpl = new List<Player>();
            Dictionary<int, Player> mapping = new Dictionary<int, Player>();

            // add first person to processed list
            Random randint = new Random();

            int id = randint.Next(111111, 999999);
            int pos = randint.Next(0, people.Count-1);

            processedPpl.Add(people[pos]);          // add to new list
            mapping.Add(id,people[pos]);
            processedPpl[processedPpl.Count - 1].setNewID(id);         // assign ID
            people.RemoveAt(pos);                   // remove from original list to avoid being picked twice
            // assign players targets and random IDs
            while (people.Count > 0)
            {
                pos = randint.Next(0, people.Count-1);
                while(mapping.ContainsKey(id = randint.Next(111111, 999999))) { ; }

                processedPpl.Add(people[pos]);
                mapping.Add(id,people[pos]);
                people.RemoveAt(pos);
                processedPpl[processedPpl.Count - 1].setNewID(id);

                // current player's target is the one in the list before him
                processedPpl[processedPpl.Count - 1].setNewTarget(processedPpl[processedPpl.Count - 2].id);
                // guy in front of current player now has him as a hunter
                processedPpl[processedPpl.Count - 2].setNewHunter(processedPpl[processedPpl.Count - 1].id);
            }

            int last = processedPpl.Count - 1;
            // set first player's target to last player
            processedPpl[0].setNewTarget(processedPpl[last].id);
            // set last player's hunter to first player
            processedPpl[last].setNewHunter(processedPpl[0].id);

            // initialize the dictionary of players
            players = mapping;

            initiated = true;

            tagsBonus = 5;
            taggedPenalty = 2;
            numAlive = players.Count;
            // write current game state to file
            Flush (new string[] {"", gfile});

            filename = gfile;
            using (StreamWriter writer = new StreamWriter(filename + "-init.csv"))
            {
                string writeline;
                writeline = "Name,Email,PlayerID,TargetName\n";
                foreach (Player p in players.Values)
                {//name email id target hunter untagged score disqualified
                    writeline += p.name + "," + p.email + "," + p.id + "," + players[p.target].name + "\n";
                }
                writeline = writeline.Substring(0, writeline.Length - 1);
                writer.Write(writeline);
            }
        }

        //Finished
        //tag <chaserID> <targetID> - attempts to register a tag of user chaserID on user targetID (IDs are numbers)
        public void Tag(string[] args)
        {
            int chaserID = -1;
            int targetID = -1;

            if (!initiated)
            {
                response = gamenotinit;
                return;
            }

            string chaserTry, targetTry;
            // Get chaser and target IDs
            if (args != empty && args.Length > 2)
            {
                //chaserID = Convert.ToInt32(args[1]);
                //targetID = Convert.ToInt32(args[2]);
                chaserTry = args[1];
                targetTry = args[2];
            }
            else
            {
                string line;
                Console.Write("Please enter a chaser ID: ");
                chaserTry = Console.ReadLine();
                Console.WriteLine("Please enter a target ID: ");
                targetTry = Console.ReadLine();

                if (chaserTry == "" || targetTry == "")
                    return;
            }
            bool error = false;
            try
            {
                chaserID = Convert.ToInt32(chaserTry);
            }
            catch (FormatException e)
            {
                response += "Error, invalid chaser id - " + chaserTry + ". Please enter valid number.\n";
                error = true;
            }
            try
            {
                targetID = Convert.ToInt32(targetTry);
            }
            catch (FormatException e)
            {
                response += "Error, invalid target id - " + targetTry + ". Please enter a valid number.\n";
                error = true;
            }
            if (error)
                return;


            if (!players.ContainsKey(targetID) || !players.ContainsKey(chaserID))
            {
                response = "Error, either the target id or the chaser id is not an actual id.  Please use valid ids. \n";
                return;
            }

            Player target = players[targetID];
            Player chaser = players[chaserID];

            if (chaser.target != targetID)
            {
                response = "Error, " + chaser.name + " is not " + chaser.name + "'s target.\n";
                return;
            }

            if (!chaser.untagged || chaser.disqualified)
            {
                response = "Error, " + chaser.name + " is not an active player anymore. \n";
                return;
            }
            // target is killed
            target.tag();
            // chaser gets target's target
            chaser.setNewTarget(target.target);
            // target's target gets new chaser
            players[target.target].hunter = chaser.id;
            response = chaser.name + " has tagged " + target.name + ". The chaser's new target is " + players[chaser.target].name + ".\nThere are " + (numAlive - 1) + " remaining players in this game.\n";
            // chaser gets tag
            chaser.incTags();
            --numAlive;
        }

        //Finished
        //status <playerID> - prints out the state of user playerID (Name, Email, Target, Alive/Dead, points)
        public void Status(string[] args)
        {
            int playerID = -1;
            string idTry;

            if (!initiated)
            {
                response = gamenotinit;
                return;
            }

            // Get player ID
            if (args != empty && args.Length > 1)
            {
                idTry = args[1];
            }
            else
            {
                Console.Write("Please enter the id of the player to be displayed: ");
                idTry = Console.ReadLine();
                if (idTry == "")
                    return;
            }

            try
            {
                playerID = Convert.ToInt32(idTry);
            }
            catch (FormatException e)
            {
                response = "Error, " + idTry + " is not a valid number.  Please enter a valid number.\n";
                return;
            }

            if (!players.ContainsKey(playerID))
            {
                response = "Error, " + playerID + " is not a valid ID.  Please enter a valid ID.\n";
                return;
            }

            // set player based on ID given
            player = players[playerID];

            // Write player info
            Console.WriteLine("Name: " + player.name);
            Console.WriteLine("Email: " + player.email);
            Console.WriteLine("ID: " + player.id);
            Console.WriteLine("Target: " + players[player.target].name + " " + player.target);
            Console.WriteLine("Hunter: " + players[player.hunter].name + " " + player.hunter);
            Console.WriteLine("Alive: " + player.untagged);
            Console.WriteLine("Tags: " + player.tags);
            Console.WriteLine("Disqualified: " + player.disqualified);
        }

        //Finished
        //dump - prints out every user's status
        public void Dump()
        {
            if (!initiated)
            {
                response = gamenotinit;
                return;
            }

            response = "Number of players left active: " + numAlive + "\n";
            // loop through all players
            foreach (int id in players.Keys)
            {
                player = players[id];
                // Write player info
                /*Console.WriteLine("Name: " + player.name);
                Console.WriteLine("Email: " + player.email);
                Console.WriteLine("ID: " + player.id);
                Console.WriteLine("Target: " + player.target);
                Console.WriteLine("Hunter: " + player.hunter);
                Console.WriteLine("Alive: " + player.untagged);
                Console.WriteLine("Tags: " + player.tags);
                Console.WriteLine("Disqualified: " + player.disqualified);
                Console.Write(*/
                response += "Name: " + player.name + "\n"
                    + "Email: " + player.email + "\n"
                    + "ID:" + player.id + "\n"
                    + "Target: " + players[player.target].name + " " + player.target + "\n"
                    + "Hunter: " + players[player.hunter].name + " " + player.hunter + "\n"
                    + "Alive: " + player.untagged + "\n"
                    + "Tags: " + player.tags + "\n"
                    + "Disqualified: " + player.disqualified + "\n\n";
            }
        }

        //FINISHED
        //top <number> - prints out the top <number> scorers
        public void Top(string[] args)
        {
            if (!initiated)
            {
                response = gamenotinit;
                return;
            }

            string strTry;
            if (args != empty && args.Length > 1)
            {
                strTry = args[1];
            }
            else
            {
                Console.Write("Please enter the number of top scoring players to display: ");
                strTry = Console.ReadLine();
            }

            if (strTry == "")
                return;

            int numtoshow;

            try
            {
                numtoshow = Convert.ToInt32(strTry);
            }
            catch (FormatException e)
            {
                response = "Invalid number.  Please enter a valid number. \n";
                return;
            }

            if (numtoshow > players.Count)
            {
                response = "Number greater than number of players.  Please enter a valid number.  \n";
                return;
            }

            List<Player> sortableplayers = new List<Player>(players.Values);
            sortableplayers.Sort(
                delegate(Player p1, Player p2)
                {
                    int p1score = -1, p2score = -1;
                    p1score = ((!p1.disqualified) ? ((p1.tags - ((!p1.untagged) ? taggedPenalty : 0))) * tagsBonus : 0);
                    p2score = ((!p2.disqualified) ? ((p2.tags - ((!p2.untagged) ? taggedPenalty : 0))) * tagsBonus : 0);
                    return p1score.CompareTo(p2score);
                }
            );
            response += String.Format("{0,-9}{1,-23}{2,-6}{3,-11}{4,-7}\n","ID","Name","Tags","Untagged","Score");
            response += "--------------------------------------------------------\n";
            for (int i = sortableplayers.Count - 1; i > sortableplayers.Count - 1 - numtoshow; --i)
            {
                response += String.Format("{0,-9}", Convert.ToString(sortableplayers[i].id))
                    + String.Format("{0,-23}",sortableplayers[i].name)
                    + String.Format("{0,-6}", Convert.ToString(sortableplayers[i].tags))
                    + String.Format("{0,-11}", Convert.ToString(sortableplayers[i].untagged))
                    + String.Format("{0,-7}", Convert.ToString(((!sortableplayers[i].disqualified) ? ((sortableplayers[i].tags - ((!sortableplayers[i].untagged) ? taggedPenalty : 0))) * tagsBonus : 0))) + "\n";
            }
        }

        //Finished
        //disqualify <id>- disqualifies user <id> from game
        public void Disqualify(string[] args)
        {
            int playerID = -1;
            string idTry;
            if (!initiated)
            {
                response = gamenotinit;
                return;
            }
            //If args is appropriate, set the disqualified boolean on the player in args to true, give their hunter their target
            //could also pull in id from stdin

            // Get player ID
            if (args != empty && args.Length > 1)
            {
                idTry = args[1];
            }
            else
            {
                Console.Write("Please enter a player id to disqualify: ");
                idTry = Console.ReadLine();
                if (idTry == "")
                    return;
            }

            try
            {
                playerID = Convert.ToInt32(idTry);
            }
            catch (FormatException e)
            {
                response = "Error, " + idTry + " is not a valid number.  Please enter a valid id number.\n";
                return;
            }

            if (!players.ContainsKey(playerID))
            {
                response = "Error, " + playerID + " is not a valid id number.  Please enter a valid id number. \n";
                return;
            }

            Console.Write("You are about to disqualify " + players[playerID].name + " from the game.  Are you sure? [Y/N]");
            string line = Console.ReadLine();
            if (line[0] != 'y' && line[0] != 'Y')
                return;

            Player player = players[playerID];
            Player hunter = players[player.hunter];

            // disqualify player
            player.disqualify();

            // give disQ player's hunter their target
            hunter.setNewTarget(player.target);
            --numAlive;
        }

        //FINISHED
        //configure <bonus> <penalty> - changes the score modifier to <bonus> * tags (- <penalty> if dead)
        public void Configure(string[] args)
        {
            if (!initiated)
            {
                response = gamenotinit;
                return;
            }
            string bonTry, penTry;
            //pull in arguments from args, if args is empty / insufficient, prompt and receive arguments from stdin (Console.Readline)
            //this method sets taggedPenalty and tagsBonus
            if (args != empty && args.Length > 2)
            {
                bonTry = args[1];
                penTry = args[2];
            }
            else
            {
                Console.Write("Please enter the points per tag: ");
                bonTry = Console.ReadLine();
                Console.Write("Please enter the penalty tags: ");
                penTry = Console.ReadLine();
            }
            if (bonTry == "" || penTry == "")
                return;

            bool error = false;
            int bonVal = 0, penVal = 0;
            try
            {
                bonVal = Convert.ToInt32(bonTry);
            }
            catch (FormatException e)
            {
                response += "Error, bonus points string is not a valid number: " + bonTry + "\n";
                error = true;
            }
            try
            {
                penVal = Convert.ToInt32(penTry);
            }
            catch (FormatException e)
            {
                response += "Error, penalty tags string is not a valid number: " + penTry + "\n";
                error = true;
            }

            if (!error)
            {
                response = "Bonus point value set to " + Convert.ToString(bonVal) + ".  Penalty tags value set to " + Convert.ToString(penVal) + ".\n";
                tagsBonus = bonVal;
                taggedPenalty = penVal;
            }
        }

        public string Response()
        {
            string ret = response;
            response = "";
            return ret;
        }

        public Boolean Initiated() { return initiated; }

        public void Count() { response = Convert.ToString(players.Count) + "\n"; }
    }

    class MailSystem
    {
        static Game game = null;

        const string USAGE = "MAIL COMMANDS: \n"
            + "setup - automated set up for sending mail\n"
            + "send - sends the prepared mail\n"
            + "cleanup - clears all stored data\n"
            + "status - prints out the status of the mail system\n"
            + "pause - exits the mail system, leaving settings intact\n"
            + "exit - exits the mail system, deleting settings\n"
            + "template <fname> - sets template to <fname>, defaults to template.txt, follow instructions in README.txt\n"
            + "server <server> - sets server to <server>, defaults to 'smtp.outlook.com'\n"
            + "ssl - toggles ssl usage, defaults to enabled\n"
            + "port <port> - sets the port number to <port>, defaults to 587\n"
            + "username <uname> - sets username to <uname>\n"
            + "alias <alias> - sets the name on the emails to <alias>\n"
            + "password - enters the password entry system\n";

        static SmtpClient client = null;

        static NetworkCredential creds = null;

        static string[] empty = new string[] { };

        static string template = null;

        static string alias = null;

        public static void SetGame(Game game)
        {
            MailSystem.game = game;
        }

        static string TestPasswordInput()
        {
            string pass = "";
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (pass.Length > 0)
                    {
                        pass = pass.Substring(0, pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            Console.WriteLine("The Password You entered is : " + pass);
            return pass;
        }

        public static void InputPassword()
        {
            if (creds == null)
                creds = new NetworkCredential();
            string pass = "";
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (pass.Length > 0)
                    {
                        pass = pass.Substring(0, pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            creds.Password = pass;
        }

        public static void Cleanup()
        {
            client = null;
            creds = null;
            template = null;
        }

        public static void InputUsername(string[] args)
        {
            if (creds == null)
                creds = new NetworkCredential();
            if (args != empty && args.Length > 1)
            {
                creds.UserName = args[1];
            }
            else
            {
                Console.Write("Please enter a username: ");
                string line = Console.ReadLine();
                if (line == "")
                    return;
                creds.UserName = line;
            }
        }

        public static void InputTemplate(string[] args)
        {
            string fname = "template.txt";
            if (args != empty && args.Length > 1)
            {
                fname = args[1];
            }
            else
            {
                string line;
                Console.Write("Please enter a template, press enter for default: ");
                if ((line = Console.ReadLine()) != "")
                {
                    fname = line;
                }
            }
            try
            {
                using (StreamReader reader = new StreamReader(fname))
                {
                    template = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("File not found.");
            }
        }

        //similar to input template, default to smtp.outlook.com
        public static void InputServer(string[] args) { throw new NotImplementedException(); }

        //similar to input template, default to 587
        public static void InputPort(string[] args) { throw new NotImplementedException(); }

        //performs the actual mail merge, I can code this
        public static void Send() { throw new NotImplementedException(); }

        //similar to input username
        public static void InputAlias(string[] args) { throw new NotImplementedException(); }

        //should run through each of these methods: InputUsername, InputPassword, InputServer, InputPort, SSL, and InputTemplate.
        //after each one of the method calls, make sure the method actually did what it was supposed to (as in, the value did get set)
        public static void Setup() { throw new NotImplementedException(); }

        //display the current username, alias, template, server, port, and ssl
        public static void Status() { throw new NotImplementedException(); }

        //prompt the user if want to use ssl, defaults to true
        public static void SSL() { throw new NotImplementedException(); }

        public static void Shell()
        {
            string line;
            string[] cmdargs;
            string output;

            char[] spacearray = new char[] { ' ' };

            creds = new NetworkCredential();

            while (true)
            {
                output = "";
                Console.Write("Please enter a mail command: ");
                line = Console.ReadLine();
                cmdargs = line.Split(spacearray);
                if (cmdargs.Length == 0)
                {
                    Console.WriteLine(USAGE);
                    continue;
                }
                switch (cmdargs[0])
                {
                    case ("psswrdtst"):
                        TestPasswordInput();
                        break;
                    case ("exit"):
                        Cleanup();
                        return;
                    case ("pause"):
                        return;
                    case ("password"):
                        InputPassword();
                        break;
                    case ("username"):
                        InputUsername(cmdargs);
                        break;
                    case ("template"):
                        InputTemplate(cmdargs);
                        break;
                    case ("server"):
                        InputServer(cmdargs);
                        break;
                    case ("port"):
                        InputPort(cmdargs);
                        break;
                    case ("setup"):
                        Setup();
                        break;
                    case ("send"):
                        Send();
                        break;
                    case ("alias"):
                        InputAlias(cmdargs);
                        break;
                    case ("ssl"):
                        SSL();
                        break;
                    case ("cleanup"):
                        Cleanup();
                        break;
                    case ("status"):
                        Status();
                        break;
                    default:
                        output = USAGE;
                        break;
                }
                Console.Write(output);
            }
        }
    }

    class Program
    {
        static void Main(string[] argv)
        {
            const string USAGE = "Commands:\n"
                + "exit - exits the game, writing all changes out to the game file\n"
                + "flush [<fname>] - opens the game file and writes the current state of the game to it. If <fname> is provided, writes current state to fname as backup\n"
                + "init <pfile> <gfile> - creates a new game using <pfile> as the participants list and outputting the game state to <gfile>\n"
                + "open <gfile> - loads the game located at gfile, defaults to 'society-tag.gfile' in the local directory\n"
                + "tag <chaserID> <targetID> - attempts to register a tag of user chaserID on user targetID (IDs are numbers)\n"
                + "status <playerID> - prints out the state of user playerID (Name, Email, Target, Alive/Dead, points)\n"
                + "dump - prints out every user's status\n"
                + "top <number> - prints out the top <number> scorers\n"
                + "count - prints out the number of players\n"
                + "disqualify <id>- disqualifies user <id> from game\n"
                + "configure <bonus> <penalty> - changes the score modifier to <bonus> * tags (- <penalty> if dead)\n"
                + "mail - transfer control over to the mail subsystem\n";

            Game game = new Game();
            MailSystem.SetGame(game);
            string line;
            string[] cmdargs;
            string output;
            char[] spacearray = new char[] { ' ' };
            bool cont = true;
            while (cont)
            {
                output = "";
                Console.Write("Please enter a command: ");
                line = Console.ReadLine();
                cmdargs = line.Split(spacearray);
                if (cmdargs.Length == 0)
                {
                    Console.WriteLine(USAGE);
                    continue;
                }
                switch (cmdargs[0])
                {
                    case ("exit"):
                        game.Exit();
                        MailSystem.Cleanup();
                        cont = false;
                        output = "Thank you for using Society-Tag";
                        break;
                    case ("flush"):
                        game.Flush(cmdargs);
                        break;
                    case ("init"):
                        game.Init(cmdargs);
                        break;
                    case ("open"):
                        game.Open(cmdargs);
                        break;
                    case ("tag"):
                        game.Tag(cmdargs);
                        break;
                    case ("status"):
                        game.Status(cmdargs);
                        break;
                    case ("dump"):
                        game.Dump();
                        break;
                    case ("top"):
                        game.Top(cmdargs);
                        break;
                    case ("disqualify"):
                        game.Disqualify(cmdargs);
                        break;
                    case ("configure"):
                        game.Configure(cmdargs);
                        break;
                    case ("count"):
                        game.Count();
                        break;
                    case ("mail"):
                        //MailSystem.Shell();
                        break;
                    default:
                        output = USAGE;
                        break;
                }
                if (output == "")
                    output = game.Response();
                Console.Write(output);
            }
            Console.ReadLine();
        }
    }
}


/*
 * TO DO:
 * Implement rest of basic admin features
 * Streamline mailmerge type functionality 
 * Write README
 * Add automatic target reassignment emails?
*/