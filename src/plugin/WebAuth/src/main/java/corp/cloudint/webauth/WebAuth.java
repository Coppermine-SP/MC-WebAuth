package corp.cloudint.webauth;

import net.kyori.adventure.text.Component;
import net.kyori.adventure.text.format.NamedTextColor;
import net.kyori.adventure.text.format.TextColor;
import net.kyori.adventure.text.format.TextDecoration;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.AuthCache;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.util.EntityUtils;
import org.bukkit.ChatColor;
import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.player.AsyncPlayerPreLoginEvent;
import org.bukkit.plugin.java.JavaPlugin;
import org.json.JSONObject;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.HashMap;

public final class WebAuth extends JavaPlugin implements Listener {
    private final HashMap<String, String> deptMap = new HashMap<String, String>();

    @Override
    public void onEnable() {
        // Plugin startup logic
        saveDefaultConfig();
        getServer().getPluginManager().registerEvents(this, this);
    }

    @EventHandler
    public void onPlayerPreLogin(AsyncPlayerPreLoginEvent event) {
        getLogger().info(String.format("%s => Auth request.", event.getName()));

        try (CloseableHttpClient client = HttpClients.createDefault()) {
            HttpPost post = new HttpPost(getConfig().getString("authApiUrl") + "/api/authorize");
            ArrayList<NameValuePair> data = new ArrayList<NameValuePair>();

            data.add(new BasicNameValuePair("uuid", event.getUniqueId().toString()));
            data.add(new BasicNameValuePair("name", event.getName()));
            data.add(new BasicNameValuePair("secret", getConfig().getString("authApiSecret")));
            post.setEntity(new UrlEncodedFormEntity(data, StandardCharsets.UTF_8));

            HttpResponse response = client.execute(post);
            if (response.getStatusLine().getStatusCode() != 200)
                throw new Exception("Authentication server returns status code " + response.getStatusLine().getStatusCode());

            String body = EntityUtils.toString(response.getEntity());
            JSONObject json = new JSONObject(body);

            if (!json.getBoolean("isAuthorized")) {
                String authCode = json.getString("authCode");
                getLogger().warning(String.format("%s => Not authorized. user verification need.", event.getName()));
                event.disallow(AsyncPlayerPreLoginEvent.Result.KICK_WHITELIST,
                        Component.text("재학생 인증을 수행하지 않았습니다.\n\n\n").append(
                                Component.text(authCode + "\n\n\n").decorate(TextDecoration.BOLD)).append(
                                        Component.text("인증 코드는 10분 간 유효합니다.\n\n").decorate(TextDecoration.BOLD)).append(
                                                Component.text("아래 페이지에서 재학생 인증을 수행하세요:\n").append(
                                                Component.text(getConfig().getString("authUrl"), NamedTextColor.BLUE))));
            }
            else{
                getLogger().info(String.format("%s => Authorized.", event.getName()));

            }

        }
        catch (Exception e) {
            getLogger().warning(String.format("%s => Auth failed due to exception:\n" + e, event.getName()));
            event.disallow(AsyncPlayerPreLoginEvent.Result.KICK_WHITELIST,
                    Component.text("죄송합니다. 지금은 인증 서버를 사용할 수 없습니다.\n\n").append(
                            Component.text("잠시 후에 다시 시도하세요.").decorate(TextDecoration.BOLD)));
        }
    }

    @Override
    public void onDisable() {
    // Plugin shutdown logic
    }
}
