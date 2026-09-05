package io.omniagent.mobile;

import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.AlarmClock;
import android.provider.MediaStore;
import android.speech.RecognizerIntent;
import android.telecom.TelecomManager;
import android.widget.Toast;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import java.util.ArrayList;
import java.util.Locale;

/**
 * OmniAgent Mobile Companion — Phone Assistant Android Activity
 *
 * Provides the interactive phone assistant interface:
 *  - Setup dialog on entry: Point to custom server or use on-device SLM
 *  - Speech recognition input with "Hey Omni" wake word detection
 *  - Real Android Intent dispatching (Spotify, AlarmClock, Calls, SMS, WhatsApp, Gmail, Apps)
 */
public class MainActivity extends AppCompatActivity {

    private static final int SPEECH_REQUEST_CODE = 1001;

    private MobileAgentService assistantService;
    private AssistantConfig config;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        this.config = new AssistantConfig();
        this.assistantService = new MobileAgentService(config);

        showOnboardingDialog();
    }

    /**
     * Prompts user on entry to choose between On-Device SLM and Remote Server.
     */
    private void showOnboardingDialog() {
        String[] options = {
            "On-Device SLM (100% Offline & Free)",
            "Point to Custom Remote Server (http://...)"
        };

        new AlertDialog.Builder(this)
            .setTitle("OmniAgent Phone Assistant Setup")
            .setItems(options, (dialog, which) -> {
                if (which == 1) {
                    promptForServerUrl();
                } else {
                    config.setMode(AssistantConfig.EngineMode.ON_DEVICE_SLM);
                    assistantService.setConfig(config);
                    Toast.makeText(this, "Active: On-Device SLM Core", Toast.LENGTH_SHORT).show();
                }
            })
            .setCancelable(false)
            .show();
    }

    private void promptForServerUrl() {
        final android.widget.EditText input = new android.widget.EditText(this);
        input.setHint("http://192.168.1.50:8765");

        new AlertDialog.Builder(this)
            .setTitle("Connect to OmniAgent Server")
            .setView(input)
            .setPositiveButton("Connect", (dialog, which) -> {
                String url = input.getText().toString().trim();
                if (url.isEmpty()) url = "http://127.0.0.1:8765";
                config.setMode(AssistantConfig.EngineMode.REMOTE_SERVER);
                config.setServerUrl(url);
                assistantService.setConfig(config);
                Toast.makeText(this, "Connected to: " + url, Toast.LENGTH_LONG).show();
            })
            .setNegativeButton("Use Local Model", (dialog, which) -> {
                config.setMode(AssistantConfig.EngineMode.ON_DEVICE_SLM);
                assistantService.setConfig(config);
            })
            .show();
    }

    /**
     * Triggers Android speech recognition intent.
     */
    public void startVoiceListening() {
        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, Locale.getDefault());
        intent.putExtra(RecognizerIntent.EXTRA_PROMPT, "Say 'Hey Omni' followed by your command...");
        try {
            startActivityForResult(intent, SPEECH_REQUEST_CODE);
        } catch (Exception e) {
            Toast.makeText(this, "Speech recognition not available on this device", Toast.LENGTH_SHORT).show();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == SPEECH_REQUEST_CODE && resultCode == RESULT_OK && data != null) {
            ArrayList<String> results = data.getStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS);
            if (results != null && !results.isEmpty()) {
                String spokenText = results.get(0);
                handleSpokenCommand(spokenText);
            }
        }
    }

    public void handleSpokenCommand(String spokenText) {
        MobileAgentService.VoiceCommandResult result = assistantService.processVoiceCommand(spokenText);
        Toast.makeText(this, result.action.voiceResponse, Toast.LENGTH_LONG).show();
        dispatchAndroidAction(result.action);
    }

    /**
     * Dispatches real Android System Intents to automate the phone without 3rd party APIs.
     */
    public void dispatchAndroidAction(DeviceAction action) {
        if (action == null) return;

        try {
            switch (action.type) {
                case PLAY_MUSIC: {
                    String query = action.getParam("query", "");
                    Intent intent = new Intent(MediaStore.INTENT_ACTION_MEDIA_PLAY_FROM_SEARCH);
                    intent.putExtra(MediaStore.EXTRA_MEDIA_FOCUS, "vnd.android.cursor.item/*");
                    intent.putExtra(android.app.SearchManager.QUERY, query);
                    if (isAppInstalled(action.targetPackage)) {
                        intent.setPackage(action.targetPackage);
                    }
                    startActivity(intent);
                    break;
                }

                case SET_ALARM: {
                    Intent intent = new Intent(AlarmClock.ACTION_SET_ALARM);
                    intent.putExtra(AlarmClock.EXTRA_MESSAGE, "OmniAgent Alarm");
                    intent.putExtra(AlarmClock.EXTRA_SKIP_UI, false);
                    startActivity(intent);
                    break;
                }

                case SET_TIMER: {
                    Intent intent = new Intent(AlarmClock.ACTION_SET_TIMER);
                    intent.putExtra(AlarmClock.EXTRA_SKIP_UI, false);
                    startActivity(intent);
                    break;
                }

                case CALL_CONTACT: {
                    Intent intent = new Intent(Intent.ACTION_DIAL);
                    if (action.androidDataUri != null) {
                        intent.setData(Uri.parse(action.androidDataUri));
                    }
                    startActivity(intent);
                    break;
                }

                case ANSWER_CALL: {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                        TelecomManager tm = (TelecomManager) getSystemService(Context.TELECOM_SERVICE);
                        if (tm != null && checkSelfPermission(android.Manifest.permission.ANSWER_PHONE_CALLS) == PackageManager.PERMISSION_GRANTED) {
                            tm.acceptRingingCall();
                        }
                    }
                    break;
                }

                case END_CALL: {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                        TelecomManager tm = (TelecomManager) getSystemService(Context.TELECOM_SERVICE);
                        if (tm != null && checkSelfPermission(android.Manifest.permission.ANSWER_PHONE_CALLS) == PackageManager.PERMISSION_GRANTED) {
                            tm.endCall();
                        }
                    }
                    break;
                }

                case SEND_SMS: {
                    Intent intent = new Intent(Intent.ACTION_SENDTO);
                    if (action.androidDataUri != null) {
                        intent.setData(Uri.parse(action.androidDataUri));
                    }
                    intent.putExtra("sms_body", action.getParam("message", ""));
                    startActivity(intent);
                    break;
                }

                case SEND_WHATSAPP: {
                    Intent intent = new Intent(Intent.ACTION_VIEW);
                    if (action.androidDataUri != null) {
                        intent.setData(Uri.parse(action.androidDataUri));
                    }
                    intent.setPackage("com.whatsapp");
                    startActivity(intent);
                    break;
                }

                case DRAFT_GMAIL: {
                    Intent intent = new Intent(Intent.ACTION_SENDTO);
                    if (action.androidDataUri != null) {
                        intent.setData(Uri.parse(action.androidDataUri));
                    }
                    intent.putExtra(Intent.EXTRA_SUBJECT, "OmniAgent Voice Note");
                    startActivity(intent);
                    break;
                }

                case OPEN_APP: {
                    if (action.targetPackage != null) {
                        Intent launchIntent = getPackageManager().getLaunchIntentForPackage(action.targetPackage);
                        if (launchIntent != null) {
                            startActivity(launchIntent);
                        } else {
                            Toast.makeText(this, action.getParam("app", "App") + " is not installed on this device.", Toast.LENGTH_SHORT).show();
                        }
                    }
                    break;
                }

                case SUMMARIZE_NOTIFICATIONS:
                case GENERAL_QUERY:
                default:
                    // Display text result
                    break;
            }
        } catch (Exception e) {
            Toast.makeText(this, "Unable to execute action: " + e.getMessage(), Toast.LENGTH_SHORT).show();
        }
    }

    private boolean isAppInstalled(String packageName) {
        if (packageName == null) return false;
        try {
            getPackageManager().getPackageInfo(packageName, 0);
            return true;
        } catch (PackageManager.NameNotFoundException e) {
            return false;
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (assistantService != null) {
            assistantService.destroy();
        }
    }
}
