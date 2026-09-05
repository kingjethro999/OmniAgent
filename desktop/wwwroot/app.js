/* ══════════════════════════════════════════════════════════════════════════
   OmniAgent Desktop Voice Assistant - Client Controller & IPC Bridge
   ══════════════════════════════════════════════════════════════════════════ */

(function () {
  // DOM Elements
  const voiceOrb = document.getElementById("voice-orb");
  const assistantStatus = document.getElementById("assistant-status");
  const responseCard = document.getElementById("response-card");
  const userQueryText = document.getElementById("user-query-text");
  const assistantResponseText = document.getElementById("assistant-response-text");
  const responseTimestamp = document.getElementById("response-timestamp");
  
  const spotifyCard = document.getElementById("spotify-card");
  const mediaTitle = document.getElementById("media-title");
  const mediaArtist = document.getElementById("media-artist");
  const btnMediaToggle = document.getElementById("btn-media-toggle");
  const svgPlay = document.getElementById("svg-play");
  const svgPause = document.getElementById("svg-pause");
  const btnMediaPrev = document.getElementById("btn-media-prev");
  const btnMediaNext = document.getElementById("btn-media-next");

  const timerCard = document.getElementById("timer-card");
  const timerLabel = document.getElementById("timer-label");
  const timerRemaining = document.getElementById("timer-remaining");
  const btnCancelTimer = document.getElementById("btn-cancel-timer");

  const cmdInput = document.getElementById("cmd-input");
  const btnSendCmd = document.getElementById("btn-send-cmd");
  const btnVoiceMic = document.getElementById("btn-voice-mic");

  const btnMinimizePill = document.getElementById("btn-minimize-pill");
  const btnClose = document.getElementById("btn-close");

  // Navigation
  const navTabs = document.querySelectorAll(".nav-tab");
  const tabPanels = document.querySelectorAll(".tab-panel");

  // Telemetry elements
  const btnRefreshTelemetry = document.getElementById("btn-refresh-telemetry");
  const valCpu = document.getElementById("val-cpu");
  const barCpu = document.getElementById("bar-cpu");
  const valRam = document.getElementById("val-ram");
  const barRam = document.getElementById("bar-ram");
  const valDisk = document.getElementById("val-disk");
  const barDisk = document.getElementById("bar-disk");
  const telemetryRawText = document.getElementById("telemetry-raw-text");

  // Calibration elements
  const calibStepNum = document.getElementById("calib-step-num");
  const calibPhraseText = document.getElementById("calib-phrase-text");
  const calibMeterBar = document.getElementById("calib-meter-bar");
  const btnCalibRecord = document.getElementById("btn-calib-record");
  const btnCalibReset = document.getElementById("btn-calib-reset");
  const calibStatusLabel = document.getElementById("calib-status-label");
  const calibProfileStats = document.getElementById("calib-profile-stats");

  // State
  let isListening = false;
  let isSpeaking = false;
  let currentTimerSeconds = 0;
  let timerInterval = null;
  let isSpotifyPlaying = false;

  const calibrationPhrases = [
    "Hey Omni",
    "OK Omni",
    "Hey Omni, play music",
    "Hey Omni, what's the weather today?"
  ];
  let currentCalibrationStep = 0;

  // ─── IPC Helper ───
  function sendToHost(type, payload = {}) {
    const msg = JSON.stringify({ type, ...payload });
    if (window.external && typeof window.external.sendMessage === "function") {
      window.external.sendMessage(msg);
    } else {
      console.log("[Photino IPC Out]:", msg);
    }
  }

  // ─── IPC Receiver ───
  if (window.external && typeof window.external.receiveMessage === "function") {
    window.external.receiveMessage(handleHostMessage);
  }

  function handleHostMessage(rawMessage) {
    try {
      const data = typeof rawMessage === "string" ? JSON.parse(rawMessage) : rawMessage;
      console.log("[Photino IPC In]:", data);

      switch (data.type) {
        case "wake_word_detected":
          setAssistantState("listening", `Wake word heard! Listening for command...`);
          break;

        case "speech_transcript":
          userQueryText.textContent = `"${data.text}"`;
          responseTimestamp.textContent = "Just now";
          responseCard.classList.remove("hidden");
          setAssistantState("executing", `Processing "${data.text}"...`);
          break;

        case "command_result":
          displayAssistantResponse(data.action, data.feedback, data.query);
          if (data.action === "PLAY_MUSIC") {
            showSpotifyPlayer(data.query || "Spotify Track");
          } else if (data.action === "SET_TIMER") {
            startVisualTimer(data.durationMinutes || 5);
          } else if (data.action === "SYSTEM_STATS") {
            updateTelemetryData(data.telemetry);
          }
          break;

        case "speech_state":
          if (data.speaking) {
            setAssistantState("speaking", "Speaking response...");
          } else {
            setAssistantState("idle", `Listening for <span class="wake-highlight">"Hey Omni"</span>...`);
          }
          break;

        case "telemetry_update":
          updateTelemetryData(data);
          break;

        case "calibration_status":
          updateCalibrationUI(data);
          break;
      }
    } catch (err) {
      console.error("Error parsing host message:", err, rawMessage);
    }
  }

  // ─── UI State Transitions ───
  function setAssistantState(state, captionHtml) {
    voiceOrb.classList.remove("listening", "speaking");
    btnVoiceMic.classList.remove("active");

    if (state === "listening") {
      isListening = true;
      voiceOrb.classList.add("listening");
      btnVoiceMic.classList.add("active");
    } else if (state === "speaking") {
      isSpeaking = true;
      voiceOrb.classList.add("speaking");
    } else {
      isListening = false;
      isSpeaking = false;
    }

    if (captionHtml) {
      assistantStatus.innerHTML = captionHtml;
    }
  }

  function displayAssistantResponse(action, text, query) {
    setAssistantState("speaking", "Executing request...");
    if (query) {
      userQueryText.textContent = `"${query}"`;
    }
    assistantResponseText.textContent = text || "Done.";
    responseCard.classList.remove("hidden");

    // Revert to idle listening after speech completes
    setTimeout(() => {
      setAssistantState("idle", `Listening for <span class="wake-highlight">"Hey Omni"</span>...`);
    }, 4000);
  }

  // ─── Spotify Media Widget ───
  function showSpotifyPlayer(songName) {
    spotifyCard.classList.remove("hidden");
    mediaTitle.textContent = songName.replace(/on spotify/i, "").trim() || "Spotify Stream";
    mediaArtist.textContent = "OmniAgent Media Player";
    isSpotifyPlaying = true;
    svgPlay.classList.add("hidden");
    svgPause.classList.remove("hidden");
  }

  btnMediaToggle.addEventListener("click", () => {
    isSpotifyPlaying = !isSpotifyPlaying;
    if (isSpotifyPlaying) {
      svgPlay.classList.add("hidden");
      svgPause.classList.remove("hidden");
      sendToHost("command", { query: "resume" });
    } else {
      svgPlay.classList.remove("hidden");
      svgPause.classList.add("hidden");
      sendToHost("command", { query: "pause" });
    }
  });

  btnMediaPrev.addEventListener("click", () => sendToHost("command", { query: "previous track" }));
  btnMediaNext.addEventListener("click", () => sendToHost("command", { query: "next track" }));

  // ─── Timer Widget ───
  function startVisualTimer(minutes) {
    if (timerInterval) clearInterval(timerInterval);
    currentTimerSeconds = minutes * 60;
    timerCard.classList.remove("hidden");
    timerLabel.textContent = `${minutes} Min Countdown`;
    updateTimerDisplay();

    timerInterval = setInterval(() => {
      currentTimerSeconds--;
      if (currentTimerSeconds <= 0) {
        clearInterval(timerInterval);
        timerRemaining.textContent = "00:00 (Timer Up!)";
      } else {
        updateTimerDisplay();
      }
    }, 1000);
  }

  function updateTimerDisplay() {
    const mins = Math.floor(currentTimerSeconds / 60);
    const secs = currentTimerSeconds % 60;
    timerRemaining.textContent = `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
  }

  btnCancelTimer.addEventListener("click", () => {
    if (timerInterval) clearInterval(timerInterval);
    timerCard.classList.add("hidden");
  });

  // ─── Telemetry Data Binding ───
  function updateTelemetryData(data) {
    if (!data) return;
    if (data.cpuPercent !== undefined) {
      valCpu.textContent = `${data.cpuPercent}%`;
      barCpu.style.width = `${Math.min(data.cpuPercent, 100)}%`;
    }
    if (data.ramPercent !== undefined) {
      valRam.textContent = `${data.ramUsedMb || '?'} MB / ${data.ramTotalMb || '?'} MB (${data.ramPercent}%)`;
      barRam.style.width = `${Math.min(data.ramPercent, 100)}%`;
    }
    if (data.diskPercent !== undefined) {
      valDisk.textContent = `${data.diskFreeGb || '?'} GB Free (${data.diskPercent}% used)`;
      barDisk.style.width = `${Math.min(data.diskPercent, 100)}%`;
    }
    if (data.raw) {
      telemetryRawText.textContent = data.raw;
    }
  }

  btnRefreshTelemetry.addEventListener("click", () => {
    sendToHost("command", { query: "how is my system doing" });
  });

  // ─── Voice Match Calibration Wizard ───
  function updateCalibrationUI(profile) {
    if (profile && profile.calibrated) {
      calibStatusLabel.textContent = `Voice Profile: ${profile.profileName || "Personal"}`;
      calibProfileStats.textContent = `Energy: ${profile.energyThreshold || 450} | Accent Variants: ${profile.variants ? profile.variants.length : 4}`;
    }
  }

  btnCalibRecord.addEventListener("click", () => {
    // Animate audio meter
    let meterVal = 0;
    btnCalibRecord.disabled = true;
    btnCalibRecord.textContent = "Recording...";

    const meterTimer = setInterval(() => {
      meterVal = Math.floor(Math.random() * 85) + 15;
      calibMeterBar.style.width = `${meterVal}%`;
    }, 100);

    // Send record phrase request
    sendToHost("calibrate_phrase", { step: currentCalibrationStep, phrase: calibrationPhrases[currentCalibrationStep] });

    setTimeout(() => {
      clearInterval(meterTimer);
      calibMeterBar.style.width = "0%";
      btnCalibRecord.disabled = false;
      btnCalibRecord.textContent = "Record Phrase";

      currentCalibrationStep++;
      if (currentCalibrationStep < calibrationPhrases.length) {
        calibStepNum.textContent = `Step ${currentCalibrationStep + 1} of 4`;
        calibPhraseText.textContent = `"${calibrationPhrases[currentCalibrationStep]}"`;
      } else {
        calibStepNum.textContent = "Calibration Complete!";
        calibPhraseText.textContent = "Voice Profile Saved Successfully";
        btnCalibRecord.textContent = "Recalibrate";
        currentCalibrationStep = 0;
        sendToHost("save_profile", {});
      }
    }, 2500);
  });

  btnCalibReset.addEventListener("click", () => {
    currentCalibrationStep = 0;
    calibStepNum.textContent = "Step 1 of 4";
    calibPhraseText.textContent = `"${calibrationPhrases[0]}"`;
    sendToHost("reset_profile", {});
  });

  // ─── Commands & Quick Actions ───
  function executeUserCommand(query) {
    if (!query || !query.trim()) return;
    const cleanQuery = query.trim();
    userQueryText.textContent = `"${cleanQuery}"`;
    responseCard.classList.remove("hidden");
    setAssistantState("executing", `Sending "${cleanQuery}" to Omni...`);

    sendToHost("command", { query: cleanQuery });
    cmdInput.value = "";
  }

  btnSendCmd.addEventListener("click", () => executeUserCommand(cmdInput.value));
  cmdInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      executeUserCommand(cmdInput.value);
    }
  });

  document.querySelectorAll(".quick-card").forEach(card => {
    card.addEventListener("click", () => {
      const cmd = card.getAttribute("data-cmd");
      executeUserCommand(cmd);
    });
  });

  // ─── Voice Mic & Orb Click ───
  btnVoiceMic.addEventListener("click", () => {
    if (!isListening) {
      setAssistantState("listening", "Listening... Speak now!");
      sendToHost("start_listening", {});
    } else {
      setAssistantState("idle", `Listening for <span class="wake-highlight">"Hey Omni"</span>...`);
      sendToHost("stop_listening", {});
    }
  });

  voiceOrb.addEventListener("click", () => {
    btnVoiceMic.click();
  });

  // ─── Navigation Tabs ───
  navTabs.forEach(tab => {
    tab.addEventListener("click", () => {
      navTabs.forEach(t => t.classList.remove("active"));
      tabPanels.forEach(p => p.classList.remove("active"));

      tab.classList.add("active");
      const targetId = tab.getAttribute("data-tab");
      document.getElementById(targetId).classList.add("active");

      if (targetId === "tab-telemetry") {
        btnRefreshTelemetry.click();
      }
    });
  });

  // ─── Window Controls ───
  btnMinimizePill.addEventListener("click", () => {
    sendToHost("minimize", {});
  });

  btnClose.addEventListener("click", () => {
    sendToHost("close", {});
  });

  // Request initial status from C#
  setTimeout(() => {
    sendToHost("get_status", {});
  }, 400);

})();
