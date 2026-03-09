package ru.rustore.unitysdk;

import android.os.Bundle;
import android.content.Intent;
import ru.rustore.unitysdk.payclient.RuStoreUnityPayClient;
import com.unity3d.player.UnityPlayerActivity;

public class RuStoreUnityPayActivity extends UnityPlayerActivity {

    @Override protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        if (savedInstanceState == null) {
            RuStoreUnityPayClient.INSTANCE.proceedIntent(getIntent());
        }
    }

    @Override protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        RuStoreUnityPayClient.INSTANCE.proceedIntent(intent);
    }
}
