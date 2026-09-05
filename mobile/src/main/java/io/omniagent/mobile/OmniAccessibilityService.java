package io.omniagent.mobile;

import android.accessibilityservice.AccessibilityService;
import android.accessibilityservice.AccessibilityServiceInfo;
import android.content.Intent;
import android.os.Build;
import android.view.accessibility.AccessibilityEvent;
import android.view.accessibility.AccessibilityNodeInfo;

/**
 * OmniAgent Mobile Companion — Accessibility Automation Service
 *
 * Optional accessibility feature that users can enable for hands-free
 * device automation, system navigation (Home, Back, Notifications),
 * and voice-directed interaction.
 *
 * Designed as a non-intrusive add-on that does not interfere with
 * existing companion features (task routing, native engine, or notifications).
 */
public class OmniAccessibilityService extends AccessibilityService {

    private static OmniAccessibilityService sInstance = null;

    public static boolean isServiceActive() {
        return sInstance != null;
    }

    public static OmniAccessibilityService getInstance() {
        return sInstance;
    }

    @Override
    public void onServiceConnected() {
        super.onServiceConnected();
        sInstance = this;

        AccessibilityServiceInfo info = new AccessibilityServiceInfo();
        info.eventTypes = AccessibilityEvent.TYPE_WINDOW_STATE_CHANGED
                | AccessibilityEvent.TYPE_WINDOW_CONTENT_CHANGED
                | AccessibilityEvent.TYPE_VIEW_CLICKED;
        info.feedbackType = AccessibilityServiceInfo.FEEDBACK_GENERIC;
        info.flags = AccessibilityServiceInfo.FLAG_INCLUDE_NOT_IMPORTANT_VIEWS
                | AccessibilityServiceInfo.FLAG_REPORT_VIEW_IDS
                | AccessibilityServiceInfo.FLAG_REQUEST_FILTER_KEY_EVENTS;
        info.notificationTimeout = 100;
        setServiceInfo(info);
    }

    @Override
    public void onAccessibilityEvent(AccessibilityEvent event) {
        // Observes window state changes for voice context if enabled
        if (event == null) return;
        // Non-intrusive monitoring only
    }

    @Override
    public void onInterrupt() {
        // Handle interruption cleanly
    }

    @Override
    public boolean onUnbind(Intent intent) {
        sInstance = null;
        return super.onUnbind(intent);
    }

    @Override
    public void onDestroy() {
        sInstance = null;
        super.onDestroy();
    }

    /**
     * Executes global navigation actions safely.
     */
    public boolean triggerGlobalAction(int action) {
        return performGlobalAction(action);
    }

    /**
     * Helper to navigate Home.
     */
    public boolean goHome() {
        return performGlobalAction(GLOBAL_ACTION_HOME);
    }

    /**
     * Helper to press Back.
     */
    public boolean goBack() {
        return performGlobalAction(GLOBAL_ACTION_BACK);
    }

    /**
     * Helper to open notification drawer.
     */
    public boolean openNotifications() {
        return performGlobalAction(GLOBAL_ACTION_NOTIFICATIONS);
    }

    /**
     * Helper to open recent apps.
     */
    public boolean openRecents() {
        return performGlobalAction(GLOBAL_ACTION_RECENTS);
    }
}
