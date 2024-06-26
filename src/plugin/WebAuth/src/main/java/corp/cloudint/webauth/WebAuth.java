package corp.cloudint.webauth;
import org.bukkit.plugin.java.JavaPlugin;
import java.util.HashMap;

public final class WebAuth extends JavaPlugin {
    private var deptMap = new HashMap<String, String>();
    @Override
    public void onEnable() {
        // Plugin startup logic
        saveDefaultConfig();

    }

    @Override
    public void onDisable() {
        // Plugin shutdown logic
    }
}
