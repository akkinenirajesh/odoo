csharp
public partial class IrModule
{
    public override string ToString()
    {
        return Name;
    }

    public bool ImportModule(string module, string path, bool force = false, bool withDemo = false)
    {
        // Implementation of _import_module logic
        // Note: This is a simplified version and would need to be adapted for C#
        var knownMods = Env.Search<IrModule>(new object[] {});
        var knownModNames = knownMods.ToDictionary(m => m.Name, m => m);
        var installedMods = knownMods.Where(m => m.State == ModuleState.Installed).Select(m => m.Name).ToList();

        // ... (rest of the logic)

        return true;
    }

    public List<string> ImportZipfile(byte[] moduleFile, bool force = false, bool withDemo = false)
    {
        // Implementation of _import_zipfile logic
        // Note: This is a simplified version and would need to be adapted for C#
        if (!Env.IsAdmin())
        {
            throw new AccessDeniedException("Only administrators can install data modules.");
        }

        if (moduleFile == null || moduleFile.Length == 0)
        {
            throw new Exception("No file sent.");
        }

        // ... (rest of the logic)

        return moduleNames;
    }

    public void ModuleUninstall()
    {
        // Implementation of module_uninstall logic
        // Note: This is a simplified version and would need to be adapted for C#
        var modulesToDelete = this.Filtered(m => m.Imported);
        
        // Call base uninstall method
        base.ModuleUninstall();

        if (modulesToDelete.Any())
        {
            var deletedModulesNames = modulesToDelete.Select(m => m.Name).ToList();
            var assetsData = Env.Search<IrModelData>(new object[] 
            {
                new object[] { "model", "=", "ir.asset" },
                new object[] { "module", "in", deletedModulesNames }
            });

            var assets = Env.Search<IrAsset>(new object[] { new object[] { "id", "in", assetsData.Select(a => a.ResId).ToList() } });
            assets.Unlink();

            // Log deletion
            _logger.Info($"deleting imported modules upon uninstallation: {string.Join(", ", deletedModulesNames)}");
            modulesToDelete.Unlink();
        }
    }

    public Dictionary<string, object> MoreInfo()
    {
        return new Dictionary<string, object>
        {
            { "name", "Apps" },
            { "type", "ir.actions.act_window" },
            { "res_model", "ir.module.module" },
            { "view_mode", "form" },
            { "res_id", this.Id },
            { "context", Env.Context }
        };
    }

    // ... (other methods would be implemented similarly)
}
