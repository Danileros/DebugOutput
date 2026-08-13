using System.Data;
using System.Text;
using Il2Cpp;

namespace DebugOutput;

/// <summary>
/// Debugging utility main class.
/// </summary>
public static class DebugManager
{
    private static Dictionary<string, DebugAction> _debugCommands = new Dictionary<string, DebugAction>();

    internal class DebugAction
    {
        public bool Enabled { get; set; }
    
        public Func<string> DebugFunction {get; set;}
    }

    /// <summary>
    /// Registers new debug function.
    /// Usage:
    /// <see cref="DebugManager.RegisterDebugCommand"/>("mymod_debug_feature1", new Func&lt;string&gt;(Feature1Manager.GetDebugOutput));
    /// type mymod_debug_feature1 command into console to activate debugging, type again to deactivate.
    /// </summary>
    /// <param name="commandName">Command name you want to have in console. Only a-z, 0-9, _ symbols are preferred.</param>
    /// <param name="debugOutputGetter">Function that obtains text to draw. Executes every frame, optimize it!</param>
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    /// <exception cref="DuplicateNameException">Throws if this command is already exists.</exception>
    public static void RegisterDebugCommand(string commandName, Func<string> debugOutputGetter)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            throw new ArgumentException("Name should not be null or empty");
        }

        if (debugOutputGetter == null)
        {
            throw new ArgumentException("Func should not be null");
        }
        
        commandName = commandName.ToLower();
        
        if (_debugCommands.ContainsKey(commandName) || uConsole.CommandAlreadyRegistered(commandName))
        {
            throw new DuplicateNameException("Duplicate debug name: " + commandName);
        }

        var action = new DebugAction
        {
            Enabled = false,
            DebugFunction = debugOutputGetter,
        };
        
        _debugCommands.Add(commandName, action);
        uConsole.RegisterCommand(commandName, new Action(ToggleDebug));
        MelonLoader.Melon<Main>.Logger.Msg("Registered debug command: " + commandName);
    }

    /// <summary>
    /// Unregisters new debug function.
    /// Usage:
    /// DebugManager.UnregisterDebugCommand("mymod_debug_feature1");
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManager.RegisterDebugCommand"/>.</param>
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    public static void UnregisterDebugCommand(string commandName)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            throw new ArgumentException("Name should not be null or empty");
        }
        
        commandName = commandName.ToLower();
        _debugCommands.Remove(commandName);
        uConsole.UnRegisterCommand(commandName);
        MelonLoader.Melon<Main>.Logger.Msg("Unregistered debug command: " + commandName);
    }

    /// <summary>
    /// Checks the current status of debug command (enabled or not).
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManager.RegisterDebugCommand"/>.</param>.
    /// <returns>true if active.</returns>
    public static bool GetDebugEnabled(string commandName)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            throw new ArgumentException("name should not be null or empty");
        }
        
        commandName = commandName.ToLower();
        if (_debugCommands.TryGetValue(commandName, out var action))
        {
            return action.Enabled;
        }
        
        return false;
    }

    /// <summary>
    /// Enables or disables debug command (enabled or not).
    /// </summary>
    /// <param name="commandName">Command name that was registered via <see cref="DebugManager.RegisterDebugCommand"/>.</param>.
    /// <param name="newStatus">True if active.</param>.
    /// <exception cref="ArgumentException">Throws on bad arguments.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Throws if such a command does not exists.</exception>
    public static void SetDebugEnabled(string commandName, bool newStatus)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            throw new ArgumentException("Name should not be null or empty");
        }
        
        commandName = commandName.ToLower();
        if (_debugCommands.TryGetValue(commandName, out var action))
        {
            action.Enabled = newStatus;
            MelonLoader.Melon<Main>.Logger.Msg($"Debug command {commandName} " + (newStatus ? "enabled" : "disabled"));
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(commandName), $"Command {commandName} does not exist");
        }
    }

    internal static void DebugOutput(UILabel debugLabel)
    {
        var activeOutputs = new Dictionary<string, string>(_debugCommands.Count + 1);
        if (debugLabel.gameObject.active)
        {
            activeOutputs.Add("vanilla", debugLabel.text);
        }

        foreach (var kvp in _debugCommands)
        {
            if (kvp.Value.Enabled)
            {
                try
                {
                    activeOutputs.Add(kvp.Key, kvp.Value.DebugFunction());
                }
                catch (Exception e)
                {
                }
            }
        }
        
        if (activeOutputs.Count == 0)
        {
            return;
        }

        if (activeOutputs.Count == 1)
        {
            debugLabel.gameObject.SetActive(true);
            debugLabel.text = activeOutputs.First().Value;
            return;
        }
        
        var sb = new StringBuilder();
        foreach (var kvp in activeOutputs)
        {
            sb.AppendLine($"==== {kvp.Key} ====");
            sb.AppendLine(kvp.Value);
        }
        
        debugLabel.gameObject.SetActive(true);
        debugLabel.text = sb.ToString();
    }

    private static void ToggleDebug()
    {
        var commandName = uConsole.m_Argv[0].ToLower();
        commandName = commandName.ToLower();
        var action = _debugCommands[commandName];
        if (action != null)
        {
            action.Enabled = !action.Enabled;
            MelonLoader.Melon<Main>.Logger.Msg($"Debug command {commandName} manually " + (action.Enabled ? "enabled" : "disabled"));
        }
    }
}