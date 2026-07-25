// Maison Aeternum — Aurèle floating assistant widget.
//
// Talks only to /api/ai/* (AiAssistantController), which in turn talks only to
// IAiMentorService. This file does not know or care whether the backend is wired to
// HeyGen, Anam.ai, or the Mock client — it reacts to whatever `providerName` and
// `clientPayloadJson` the session endpoint returns.
(function () {
    "use strict";

    const state = {
        opened: false,
        sessionId: null,
        providerName: null,
        conversationId: null,
        anamClient: null,
        recognizing: false
    };

    const els = {};

    function cacheElements() {
        els.fab = document.querySelector("[data-ai-assistant-toggle]");
        els.panel = document.querySelector("[data-ai-assistant-panel]");
        els.closeBtn = document.querySelector("[data-ai-assistant-close]");
        els.status = document.querySelector("[data-ai-status]");
        els.stage = document.querySelector("[data-ai-avatar-stage]");
        els.video = document.querySelector("[data-ai-avatar-video]");
        els.messages = document.querySelector("[data-ai-messages]");
        els.composer = document.querySelector("[data-ai-composer]");
        els.input = document.querySelector("[data-ai-input]");
        els.micBtn = document.querySelector("[data-ai-mic]");
    }

    async function postJson(url, body) {
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body || {})
        });

        if (!response.ok) {
            const problem = await response.json().catch(() => ({}));
            throw new Error(problem.message || problem.title || `Request to ${url} failed (${response.status})`);
        }

        return response.status === 204 ? null : response.json();
    }

    function appendMessage(sender, text) {
        const bubble = document.createElement("div");
        bubble.className = "ai-msg " + sender;
        bubble.textContent = text;
        els.messages.appendChild(bubble);
        els.messages.scrollTop = els.messages.scrollHeight;
        return bubble;
    }

    function appendTypingIndicator() {
        const bubble = document.createElement("div");
        bubble.className = "ai-msg aurele";
        bubble.innerHTML = '<span class="ai-typing-dot"><span></span><span></span><span></span></span>';
        bubble.dataset.typing = "true";
        els.messages.appendChild(bubble);
        els.messages.scrollTop = els.messages.scrollHeight;
        return bubble;
    }

    function speakClientSide(text) {
        if (state.providerName === "Anam" && state.anamClient && typeof state.anamClient.talk === "function") {
            state.anamClient.talk(text);
            return;
        }

        // Mock (and any provider without a dedicated client SDK path) speaks via the
        // browser's own SpeechSynthesis API — real voice, no external avatar video.
        if ("speechSynthesis" in window) {
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.rate = 0.98;
            utterance.pitch = 1.0;
            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(utterance);
        }
    }

    async function handleReply(reply, { showBubble = true } = {}) {
        state.conversationId = reply.conversationId;

        if (showBubble) appendMessage("aurele", reply.text);
        if (reply.warning) appendMessage("system", reply.warning);

        if (reply.handledClientSide) {
            speakClientSide(reply.text);
        }
    }

    async function connectHeyGen(payload) {
        try {
            await loadScript("https://cdn.jsdelivr.net/npm/@heygen/streaming-avatar@latest/lib/index.umd.js");
            const StreamingAvatarCtor = window.StreamingAvatar?.default || window.StreamingAvatar;
            if (!StreamingAvatarCtor) throw new Error("HeyGen SDK did not load correctly.");

            const client = new StreamingAvatarCtor({ token: payload.token });

            client.on("stream_ready", (event) => {
                els.video.srcObject = event.detail;
                els.video.classList.add("active");
                els.stage.classList.add("video-active");
            });

            const startResult = await client.createStartAvatar({
                avatarName: payload.avatarId,
                voice: payload.voiceId ? { voiceId: payload.voiceId } : undefined,
                quality: payload.quality || "medium"
            });

            // HeyGen's SDK assigns the real session id once the WebRTC handshake completes —
            // everything server-side from here on (SendSpeechAsync) uses THIS id.
            state.sessionId = startResult?.sessionId || state.sessionId;
            setStatus("online", "Online");
        } catch (err) {
            console.warn("Aurèle: HeyGen avatar failed to connect, falling back to text.", err);
            setStatus("offline", "Voice unavailable — text chat only");
        }
    }

    async function connectAnam(payload) {
        try {
            await loadScript("https://cdn.jsdelivr.net/npm/@anam-ai/js-sdk@latest/dist/umd/index.js");
            const createClient = window.anam?.createClient;
            if (!createClient) throw new Error("Anam SDK did not load correctly.");

            state.anamClient = createClient(payload.sessionToken);
            await state.anamClient.streamToVideoElement(els.video);
            els.video.classList.add("active");
            els.stage.classList.add("video-active");
            setStatus("online", "Online");
        } catch (err) {
            console.warn("Aurèle: Anam avatar failed to connect, falling back to text.", err);
            setStatus("offline", "Voice unavailable — text chat only");
        }
    }

    function loadScript(src) {
        return new Promise((resolve, reject) => {
            if (document.querySelector(`script[src="${src}"]`)) return resolve();
            const script = document.createElement("script");
            script.src = src;
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Failed to load ${src}`));
            document.head.appendChild(script);
        });
    }

    function setStatus(kind, text) {
        els.status.textContent = text;
        els.status.className = "ai-assistant-status" + (kind === "online" ? " online" : "");
    }

    async function ensureSession() {
        if (state.sessionId || state.providerName === "Mock") return;

        setStatus("connecting", "Connecting…");

        try {
            const session = await postJson("/api/ai/session");
            state.providerName = session.providerName;
            state.sessionId = session.sessionId;

            if (!session.success) {
                setStatus("offline", "Voice unavailable — text chat only");
                return;
            }

            const payload = session.clientPayloadJson ? JSON.parse(session.clientPayloadJson) : {};

            if (session.providerName === "HeyGen") {
                await connectHeyGen(payload);
            } else if (session.providerName === "Anam") {
                await connectAnam(payload);
            } else {
                // Mock: static avatar is already showing; browser speech synthesis handles voice.
                setStatus("online", "Online (demo voice)");
            }
        } catch (err) {
            console.error("Aurèle: failed to open a session.", err);
            setStatus("offline", "Voice unavailable — text chat only");
        }
    }

    async function openPanel() {
        els.panel.hidden = false;
        state.opened = true;

        if (!els.messages.childElementCount) {
            appendMessage("system", "Loading Aurèle…");
            await ensureSession();
            els.messages.innerHTML = "";

            try {
                const reply = await postJson("/api/ai/welcome", { sessionId: state.sessionId });
                await handleReply(reply);
            } catch (err) {
                appendMessage("system", "Aurèle is unavailable right now — please try again shortly.");
                console.error(err);
            }
        }

        els.input.focus();
    }

    function closePanel() {
        els.panel.hidden = true;
        state.opened = false;

        if (state.sessionId) {
            navigator.sendBeacon?.(`/api/ai/session/${encodeURIComponent(state.sessionId)}/close`);
            state.sessionId = null;
            state.providerName = null;
        }
    }

    async function submitQuestion(event) {
        event.preventDefault();
        const question = els.input.value.trim();
        if (!question) return;

        els.input.value = "";
        appendMessage("learner", question);
        const typingBubble = appendTypingIndicator();

        try {
            const reply = await postJson("/api/ai/ask", {
                question,
                conversationId: state.conversationId,
                sessionId: state.sessionId
            });
            typingBubble.remove();
            await handleReply(reply);
        } catch (err) {
            typingBubble.remove();
            appendMessage("system", "Aurèle couldn't answer that just now — please try again.");
            console.error(err);
        }
    }

    function initVoiceInput() {
        const RecognitionCtor = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!RecognitionCtor) {
            els.micBtn.style.display = "none";
            return;
        }

        const recognition = new RecognitionCtor();
        recognition.lang = "en-US";
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.onresult = (event) => {
            const transcript = event.results[0][0].transcript;
            els.input.value = transcript;
            els.composer.dispatchEvent(new Event("submit", { cancelable: true }));
        };

        recognition.onend = () => {
            state.recognizing = false;
            els.micBtn.classList.remove("listening");
        };

        recognition.onerror = () => {
            state.recognizing = false;
            els.micBtn.classList.remove("listening");
        };

        els.micBtn.addEventListener("click", () => {
            if (state.recognizing) {
                recognition.stop();
                return;
            }
            state.recognizing = true;
            els.micBtn.classList.add("listening");
            recognition.start();
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        cacheElements();
        if (!els.fab || !els.panel) return;

        els.fab.addEventListener("click", () => (state.opened ? closePanel() : openPanel()));
        els.closeBtn.addEventListener("click", closePanel);
        els.composer.addEventListener("submit", submitQuestion);
        initVoiceInput();
    });
})();
