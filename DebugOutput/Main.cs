using MelonLoader;
using Debug = UnityEngine.Debug;

namespace DebugOutput;

public class Main : MelonMod
{
    public override void OnInitializeMelon()
    {
        Debug.Log($"[{Info.Name}] Version {Info.Version} loaded!");

        CheckForReferences();
    }

    private void CheckForReferences()
    {
        var assembliesList = AppDomain.CurrentDomain.GetAssemblies();
        var currentAssemblyName = MelonAssembly.Assembly.GetName().Name;
        var referencers = assembliesList
            .Where(a => a.GetCustomAttributes(typeof(MelonInfoAttribute), false).Any())
            .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == currentAssemblyName))
            .Select(a => a.GetName().Name)
            .ToArray();
        if (referencers.Length != 0)
        {
            var referencersList = string.Join(", ", referencers);
            MelonLogger.Error(
                "If you are not a mod developer, ignore this message. " +
                $"Detected direct reference to this mod from {referencersList}. " +
                "It is strongly recommended to use DebugManagerProxy instead. " +
                "Navigate to https://github.com/Danileros/DebugOutput for more information.");
        }
    }
}