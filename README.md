



# RimDialogue Local Server Install Instructions

Join the [Discord](https://discord.gg/KavBmswUen) to get installation help, report bugs, or show off the crazy conversations your pawns have!

---

## Prerequisites

Requires .NET 9:
  - The latest release should contain all the .NET files you need.

RimDialogue Local can be run with:

- A **local LLM** using Ollama, or  
- A **cloud LLM** using an API key.

---

### Using Ollama

To run RimDialogue with a local LLM:

1. **Download and install Ollama**  
   [Ollama Download Page](https://ollama.com/)  

2. **Installation and configuration instructions**  
   [Ollama GitHub Repository](https://github.com/ollama/ollama)  

> **System Requirements:**  
> Depending on the model you choose, Ollama may require a powerful machine.  
> The hosted version uses **Llama 3.2 3B** and **Llama 3.2 1B**.  
> Find more models here: [Ollama Model Library](https://ollama.com/library)

#### Downloading Models
1. Go to the model page on the Ollama website and copy the model name. 
2. Open a command prompt by typing `cmd` in the Windows search bar.
3. In the command window, type `ollama pull <paste model name>` and press enter.  The model should now start downloading.

#### Uncensored Models  
If you prefer uncensored models, here are some options:
- [Llama 2 Uncensored](https://ollama.ai/library/llama2-uncensored)
- [Mistral Uncensored](https://ollama.ai/gdisney/mistral-uncensored)
- [Orca2 Uncensored](https://ollama.ai/gdisney/orca2-uncensored)
- [Mixtral Uncensored](https://ollama.ai/gdisney/mixtral-uncensored)
- [Zephyr Uncensored](https://ollama.ai/gdisney/zephyr-uncensored)
- [Wizard Vicuna Uncensored](https://ollama.ai/library/wizard-vicuna-uncensored)
- [WizardLM Uncensored](https://ollama.ai/library/wizardlm-uncensored)

---

### Using a Cloud LLM with an API Key

If running a local LLM isn’t an option, you can use an API key for a cloud-hosted LLM.  

**Supported Providers:**
- **Groq**  
  Get API keys: [Groq API Keys](https://groq.com/)  
  > Groq offers a free tier, but it’s heavily throttled.

- **AWS & OpenAI**  
  > **Warning:** These options are complex to configure and may incur high costs if set up improperly.

---

## Installing RimDialogue Local

### 1. Download the Server

1. Go to the [RimDialogue Server Releases](https://github.com/johndroper/RimDialogueServer/releases).  
2. Download the latest `RimDialogueLocalServer_<version>.zip`.  
3. Unzip to a directory of your choice.

### 2. Configure the Server

1. Open the `appsettings.json` file in a text editor.
2. Configure your provider:
   - **For Ollama:**
     - Set the `Provider` setting to `OLLAMA`.
     - Ensure `OllamaUrl` points to the correct port (default: `11434`).  
     - Set `OllamaModelId` to your chosen model (e.g., `"llama3.2"`).
   - **For Cloud Providers:**  
     - Set the `Provider` setting to `AWS`, `OPENAI`, `GEMINI`, or `GROQ`.
     - Fill in your API credentials in the `SETTINGS` section for your provider (e.g., `GroqApiKey` and `GroqModelId` for Groq).

3. Adjust optional settings:
   - **RateLimit**: Sets the number of requests per second allowed.  
	 - For Ollama you can set this higher (0.5 - 1.0) depending on your machine.
	 - For cloud providers this will depend on your token budget. If your provider limits requests per minute, turn this down.
        - 0.016667 is 1 request per minute.
        - 0.166667 is 10 requests per minute.
        - 0.416667 is 25 requests per minute.  
   - **MaxPromptLength**: Limits prompt size before truncation.  
     - Set lower for tight input token budgets or higher (40,000–50,000) for local setups.  
   - **Options Settings**:  
     - Enable / disable additional prompt data with boolean options under `//OPTIONS SETTINGS`.
     - If your cloud provider limits input tokens, turn some of these settings off to reduce the prompt size and the number of input tokens.
   - **Server Port**:  
     - By default, the server runs on port `7293`. Change this in the `Urls` field if needed.

---

## Setting Up the RimDialogue Mod

### 1. Subscribe to the Mod

- Subscribe to the mod on Steam:  
  [RimDialogue Mod Page](https://steamcommunity.com/sharedfiles/filedetails/?id=3365889763)

- Alternatively, launch RimWorld, press **Mods**, and use the Steam Workshop browser to subscribe.

### 2. Enable the Mod

1. From RimWorld’s main menu, press **Mods**.  
2. Find **RimDialogue** in the left column.  
3. Click **Enable** in the mod description window.  
4. Press **Save and apply changes**.

### 3. Configure the Mod

1. From the main menu, press **Options**.  
2. Select **Mod Options**.  
3. Choose **RimDialogue**.  
4. Scroll to the **Server URL** setting and set it to:  
   `http://localhost:7293/home/getdialogue`  
   > Adjust the port if you changed it during server configuration.

---

## Running RimDialogue Local Server

1. Run `RimDialogue.exe` from the installation folder.  
2. Start your RimWorld game normally.

---

## Contributing
Contributions are welcome! Feel free to open issues or pull requests.

## License
This project is licensed under the CC BY-NC-SA 4.0 International License.
