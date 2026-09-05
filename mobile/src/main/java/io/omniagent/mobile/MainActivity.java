package io.omniagent.mobile;

import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.AlarmClock;
import android.provider.MediaStore;
import android.provider.Settings;
import android.speech.RecognizerIntent;
import android.telecom.TelecomManager;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.SwitchCompat;

import java.util.ArrayList;
import java.util.Locale;

/**
 * OmniAgent Mobile Companion — Phone Assistant Android Activity
 *
 * Provides a ChatGPT-inspired cobalt dark interface for phone automation:
 *  - On-Device Smart Engine (0 MB model download required)
 *  - Self-Hosted Remote Server option
 *  - Optional Accessibility Automation Service toggle
 *  - "Hey Omni" Wake Word and Speech Recognition
 *  - Real Android Intent dispatch (Spotify, Alarm, Calls, SMS, WhatsApp, Gmail, Apps)
 *  - Zero emojis, clean vector icons and typography
 */
public class MainActivity extends AppCompatActivity {

    private static final int SPEECH_REQUEST_CODE = 1001;

    private MobileAgentService assistantService;
    private AssistantConfig config;

    // UI Bindings
    private SwitchCompat switchAssistantMode;
    private TextView tvAccessibilityStatus;
    private RadioGroup rgEngineMode;
    private RadioButton rbModeLocal;
    private RadioButton rbModeServer;
    private LinearLayout layoutServerInput;
    private EditText etServerUrl;
    private Button btnSaveServer;
    private ImageButton btnVoiceListen;
    private TextView tvVoiceStatus;
    private EditText etCommandInput;
    private Button btnRunCommand;
    private TextView tvMonitorStatusTag;
    private TextView tvExecutionLog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        this.config = new AssistantConfig();
        this.assistantService = new MobileAgentService(config);

        initViews();
        setupListeners();
        updateAccessibilityStatus();
    }

    @Override
    protected void onResume() {
        super.onResume();
        updateAccessibilityStatus();
    }

    private void initViews() {
        switchAssistantMode = findViewById(R.id.switch_assistant_mode);
        tvAccessibilityStatus = findViewById(R.id.tv_accessibility_service_status);
        rgEngineMode = findViewById(R.id.rg_engine_mode);
        rbModeLocal = findViewById(R.id.rb_mode_local);
        rbModeServer = findViewById(R.id.rb_mode_server);
        layoutServerInput = findViewById(R.id.layout_server_input);
        etServerUrl = findViewById(R.id.et_server_url);
        btnSaveServer = findViewById(R.id.btn_save_server);
        btnVoiceListen = findViewById(R.id.btn_voice_listen);
        tvVoiceStatus = findViewById(R.id.tv_voice_status);
        etCommandInput = findViewById(R.id.et_command_input);
        btnRunCommand = findViewById(R.id.btn_run_command);
        tvMonitorStatusTag = findViewById(R.id.tv_monitor_status_tag);
        tvExecutionLog = findViewById(R.id.tv_execution_log);

        etServerUrl.setText(config.getServerUrl());
    }

    private void setupListeners() {
        // Optional Accessibility Automation Switch
        switchAssistantMode.setOnCheckedChangeListener((buttonView, isChecked) -> {
            if (isChecked) {
                if (!OmniAccessibilityService.isServiceActive()) {
                    promptEnableAccessibilityService();
                } else {
                    tvAccessibilityStatus.setText(R.string.accessibility_status_active);
                }
            } else {
                tvAccessibilityStatus.setText(R.string.accessibility_status_inactive);
            }
        });

        // Backend Engine Mode Selector (0 MB On-Device vs Remote Server)
        rgEngineMode.setOnCheckedChangeListener((group, checkedId) -> {
            if (checkedId == R.id.rb_mode_server) {
                layoutServerInput.setVisibility(View.VISIBLE);
                config.setMode(AssistantConfig.EngineMode.REMOTE_SERVER);
                assistantService.setConfig(config);
                logAction("Engine Backend", "Switched to Remote Server mode (" + config.getServerUrl() + ")");
            } else {
                layoutServerInput.setVisibility(View.GONE);
                config.setMode(AssistantConfig.EngineMode.ON_DEVICE_SLM);
                assistantService.setConfig(config);
                logAction("Engine Backend", "Active: On-Device Smart Engine (0 MB Download Required)");
            }
        });

        btnSaveServer.setOnClickListener(v -> {
            String url = etServerUrl.getText().toString().trim();
            if (url.isEmpty()) url = "http://127.0.0.1:8765";
            config.setServerUrl(url);
            assistantService.setConfig(config);
            Toast.makeText(this, "Remote server set to: " + url, Toast.LENGTH_SHORT).show();
            logAction("Config", "Updated server endpoint: " + url);
        });

        // Voice Listen Button
        btnVoiceListen.setOnClickListener(v -> startVoiceListening());

        // Command Text Run Button
        btnRunCommand.setOnClickListener(v -> {
            String command = etCommandInput.getText().toString().trim();
            if (!command.isEmpty()) {
                handleSpokenCommand(command);
                etCommandInput.setText("");
            }
        });

        // Quick Automation Chips
        findViewById(R.id.chip_action_music).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, play the box by roddy rich"));

        findViewById(R.id.chip_action_alarm).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, set an alarm for 7:00 AM"));

        findViewById(R.id.chip_action_call).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, call mum"));

        findViewById(R.id.chip_action_whatsapp).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, open whatsapp"));

        findViewById(R.id.chip_action_tiktok).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, open tiktok"));

        findViewById(R.id.chip_action_gmail).setOnClickListener(v ->
                handleSpokenCommand("Hey Omni, draft a gmail to boss saying running late"));
    }

    private void updateAccessibilityStatus() {
        boolean active = OmniAccessibilityService.isServiceActive();
        switchAssistantMode.setChecked(active);
        tvAccessibilityStatus.setText(active ?
                R.string.accessibility_status_active : R.string.accessibility_status_inactive);
    }

    private void promptEnableAccessibilityService() {
        new AlertDialog.Builder(this)
                .setTitle("Enable Assistant Automation")
                .setMessage("OmniAgent uses an optional Accessibility Service for hands-free automation (opening apps, navigating, and executing voice tasks). This runs 100% locally and does not alter your existing companion settings.\n\nEnable OmniAgent in Accessibility settings?")
                .setPositiveButton("Open Settings", (dialog, which) -> {
                    Intent intent = new Intent(Settings.ACTION_ACCESSIBILITY_SETTINGS);
                    startActivity(intent);
                })
                .setNegativeButton("Cancel", (dialog, which) -> switchAssistantMode.setChecked(false))
                .show();
    }

    public void startVoiceListening() {
        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, Locale.getDefault());
        intent.putExtra(RecognizerIntent.EXTRA_PROMPT, "Say 'Hey Omni' followed by your command...");
        try {
            tvVoiceStatus.setText("Listening for 'Hey Omni'...");
            startActivityForResult(intent, SPEECH_REQUEST_CODE);
        } catch (Exception e) {
            tvVoiceStatus.setText("Speech recognition unavailable");
            Toast.makeText(this, "Speech recognition not available on this device", Toast.LENGTH_SHORT).show();
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        tvVoiceStatus.setText("Tap mic or say 'Hey Omni' to automate");

        if (requestCode == SPEECH_REQUEST_CODE && resultCode == RESULT_OK && data != null) {
            ArrayList<String> results = data.getStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS);
            if (results != null && !results.isEmpty()) {
                String spokenText = results.get(0);
                handleSpokenCommand(spokenText);
            }
        }
    }

    public void handleSpokenCommand(String spokenText) {
        tvMonitorStatusTag.setText("PARSING");
        MobileAgentService.VoiceCommandResult result = assistantService.processVoiceCommand(spokenText);

        StringBuilder log = new StringBuilder();
        log.append("Input: \"").append(spokenText).append("\"\n");
        if (result.wakeWordDetected) {
            log.append("Wake Word: \"").append(result.wakeWordUsed).append("\" (Detected)\n");
        }
        log.append("Action: ").append(result.action.type.name()).append(" — ").append(result.action.title).append("\n");
        log.append("Response: \"").append(result.action.voiceResponse).append("\"\n");

        if (result.action.targetPackage != null) {
            log.append("Target: ").append(result.action.targetPackage).append("\n");
        }
        if (result.action.androidIntentAction != null) {
            log.append("Intent: ").append(result.action.androidIntentAction).append("\n");
        }

        tvExecutionLog.setText(log.toString().trim());
        tvMonitorStatusTag.setText("EXECUTED");
        Toast.makeText(this, result.action.voiceResponse, Toast.LENGTH_SHORT).show();

        dispatchAndroidAction(result.action);
    }

    private void logAction(String tag, String message) {
        tvExecutionLog.setText("[" + tag + "] " + message);
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
