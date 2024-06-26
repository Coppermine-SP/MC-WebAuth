package corp.cloudint.webauth;

import io.papermc.paper.event.player.AsyncChatEvent;
import net.kyori.adventure.text.Component;
import net.kyori.adventure.text.format.NamedTextColor;
import net.kyori.adventure.text.format.TextDecoration;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.util.EntityUtils;
import org.bukkit.Bukkit;
import org.bukkit.entity.Player;
import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.player.AsyncPlayerPreLoginEvent;
import org.bukkit.event.player.PlayerLoginEvent;
import org.bukkit.metadata.FixedMetadataValue;
import org.bukkit.plugin.java.JavaPlugin;
import org.json.JSONArray;
import org.json.JSONObject;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

public final class WebAuth extends JavaPlugin implements Listener {
    private ConcurrentHashMap<Integer, String> deptMap;
    private final ConcurrentHashMap<UUID, Integer> playerDeptMap = new ConcurrentHashMap<>();

    @Override
    public void onEnable() {
        // Plugin startup logic
        saveDefaultConfig();
        getDeptMap();
        getServer().getPluginManager().registerEvents(this, this);
    }

    private void getDeptMap(){
        try(CloseableHttpClient client = HttpClients.createDefault()){
            HttpPost post = new HttpPost( getConfig().getString("authApiUrl") + "/api/getdept");

            ArrayList<NameValuePair> data = new ArrayList<>();
            data.add(new BasicNameValuePair("secret", getConfig().getString("authApiSecret")));
            post.setEntity(new UrlEncodedFormEntity(data, StandardCharsets.UTF_8));

            HttpResponse response = client.execute(post);
            if(response.getStatusLine().getStatusCode() != 200) throw new Exception();
            String body = EntityUtils.toString(response.getEntity());

            JSONObject jObject = new JSONObject(body);
            JSONArray arr = jObject.getJSONArray("depts");

            getLogger().info("Get " + arr.length() + " depts from authentication server.");
            deptMap = new ConcurrentHashMap<>();
            for(Object x : arr){
                JSONObject tmp = (JSONObject)x;
                deptMap.put(tmp.getInt("deptId"), tmp.getString("name"));
            }
        }
        catch(Exception e){
            getLogger().warning("Exception occurred in get depts from authentication server :\n" + e);
            deptMap = null;
        }
    }

    @EventHandler
    public void onPlayerLogin(PlayerLoginEvent event){
        if(deptMap != null){
            UUID uid = event.getPlayer().getUniqueId();
            if(playerDeptMap.containsKey(uid)){
                int deptId = playerDeptMap.get(uid);
                if(deptMap.containsKey(deptId)) {
                    String deptName = deptMap.get(deptId);
                    event.getPlayer().setMetadata("deptTitle", new FixedMetadataValue(WebAuth.this, deptName));
                }
                playerDeptMap.remove(uid);
            }
        }
    }

    @EventHandler
    public void onPlayerChat(AsyncChatEvent event){
        Player player = event.getPlayer();

        if(player.hasMetadata("deptTitle")){
            String deptTitle = player.getMetadata("deptTitle").get(0).asString();
            Component deptComponent = Component.text("(" + deptTitle + ")", NamedTextColor.GRAY);
            Component spaceComponent = Component.text(" ", NamedTextColor.WHITE);
            Component playerComponent = player.displayName().colorIfAbsent(NamedTextColor.WHITE);
            Component arrowComponent = Component.text(": ", NamedTextColor.WHITE);
            Component messageComponent = event.message();
            Component formattedMessage = deptComponent
                    .append(spaceComponent)
                    .append(playerComponent)
                    .append(arrowComponent)
                    .append(messageComponent);

            event.setCancelled(true);
            Bukkit.getServer().sendMessage(formattedMessage);
        }
    }

    @EventHandler
    public void onPlayerPreLogin(AsyncPlayerPreLoginEvent event) {
        getLogger().info(String.format("%s => Auth request.", event.getName()));

        try (CloseableHttpClient client = HttpClients.createDefault()) {
            HttpPost post = new HttpPost(getConfig().getString("authApiUrl") + "/api/authorize");
            ArrayList<NameValuePair> data = new ArrayList<>();

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
                                                Component.text(getConfig().getString("authUrl") + " ", NamedTextColor.BLUE))));
            }
            else{
                getLogger().info(String.format("%s => Authorized.", event.getName()));
                playerDeptMap.put(event.getUniqueId(), json.getInt("deptId"));
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
