# 🎙️ Google Cloud Text-to-Speech Setup Guide

This guide will help you set up Google Cloud Text-to-Speech for high-quality neural voices in DarkGravity.

## 🆓 Free Tier

Google Cloud TTS offers a generous free tier (per month):
- **4 million characters** for Standard voices
- **1 million characters** for Neural/WaveNet voices
- **Note**: Google requires a credit card for identity verification during sign-up, but you won't be charged within these limits.
- Resets on the 1st of every month.

**Typical usage**: 1 million characters ≈ 200-300 stories per month

---

##Step-by-Step Setup

### 1. Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **"Select a project"** → **"New Project"**
3. Name it (e.g., "DarkGravity-TTS")
4. Click **"Create"**

### 2. Enable Text-to-Speech API

1. Go to [Text-to-Speech API](https://console.cloud.google.com/apis/library/texttospeech.googleapis.com)
2. Make sure your project is selected (top navigation bar)
3. Click **"Enable"**
4. Wait ~30 seconds for activation

### 3. Create API Key

1. Go to [Credentials](https://console.cloud.google.com/apis/credentials)
2. Click **"Create Credentials"** → **"API Key"**
3. Your API key will be shown (e.g., `AIzaSyD...`)
4. **IMPORTANT**: Click **"Restrict Key"** (optional but recommended):
   - Under "API restrictions", select "Restrict key"
   - Choose "Cloud Text-to-Speech API" from the dropdown
   - Click "Save"

### 4. Add to DarkGravity

From your project root (`/DarkGravity`), run:

```bash
dotnet user-secrets set "GoogleCloud:TtsApiKey" "YOUR_API_KEY_HERE" --project src/Api
```

Replace `YOUR_API_KEY_HERE` with the key from step 3.

### 5. Verify Setup

1. Restart your API server:
   ```bash
   dotnet run --project src/Api
   ```

2. Check the logs for:
   ```
   [Info] Google Cloud TTS client initialized
   ```

3. Open http://localhost:4200 in Chrome
4. Look for voices with "(Neural)" or "(Wavenet)" in the dropdown
5. Select one and test playback

---

## 🔒 Security Best Practices

### ✅ DO:
- Use user secrets (never commit API key to git)
- Restrict API key to Text-to-Speech API only
- Monitor usage on [Google Cloud Console](https://console.cloud.google.com/apis/dashboard)

### ❌ DON'T:
- Commit API key to source control
- Share API key publicly
- Use the key in client-side code (it's in the backend for security)

---

## 📊 Monitoring Usage

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/dashboard)
2. Select your project
3. Click "Text-to-Speech API"
4. View usage charts

**Quota resets**: 1st of each month at midnight Pacific Time

---

## ❓ Troubleshooting

### "API key not found" error
```bash
# Verify key is set:
dotnet user-secrets list --project src/Api

# Should show:
GoogleCloud:TtsApiKey = AIzaSy...
```

If not shown, re-run step 4.

### "API not enabled" error
- Wait 2-3 minutes after enabling (propagation delay)
- Verify at: https://console.cloud.google.com/apis/library/texttospeech.googleapis.com
- Should show "API Enabled" with green checkmark

### "Forbidden" or "403" error
- Check API key restrictions aren't too strict
- Ensure Text-to-Speech API is in the allowed list
- Try creating a new unrestricted key for testing

### No voices showing in dropdown
- Check browser console (F12) for errors
- Backend API should show: `"Fetched X voices from Google Cloud TTS"`
- If 0 voices, check API key and internet connection

---

## 🎯 Resume Value Features

This implementation demonstrates several production-ready concepts:

1. **Cloud Platform Integration** - Shows GCP experience
2. **API Security** - Backend proxy pattern (no key exposure in frontend)
3. **Error Handling** - Graceful fallback to native voices if Cloud TTS fails
4. **Full-Stack Architecture** - Angular frontend ↔️ ASP.NET Core backend ↔️ GCP
5. **Configuration Management** - Proper secrets handling via user secrets

**For interviews**: You can explain how you architected a secure TTS solution using Google Cloud, implemented proper API key management, and built a resilient system with automatic fallbacks.

---

## 📚 Additional Resources

- [Google Cloud TTS Documentation](https://cloud.google.com/text-to-speech/docs)
- [Pricing Calculator](https://cloud.google.com/products/calculator)
- [Voice Selection Guide](https://cloud.google.com/text-to-speech/docs/voices)
- [SSML Reference](https://cloud.google.com/text-to-speech/docs/ssml) (for advanced features)

---

## 🆘 Still Need Help?

1. Check [Google Cloud TTS Support](https://cloud.google.com/text-to-speech/docs/support)
2. Review DarkGravity logs: `src/Api/logs/`
3. Open a GitHub issue with error logs
