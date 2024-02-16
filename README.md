Here's an updated README for Clam Shell with the additional information:

---

# Clam Shell

Clam Shell is a self-hosted anti-virus engine for Discord servers, powered by the open-source ClamAV engine. It provides real-time scanning of files and URLs shared within your Discord server, ensuring a safe and secure environment for your community. Clam Shell also features phishing detection by referencing a list of phishing URLs, which is automatically updated every 12 hours. Additionally, it supports posting logs to a designated Discord channel for easy monitoring if desired.

## Features

- **Discord Bot Integration:** Monitors your Discord server for file and URL sharing, and queues them for scanning.
- **ClamAV Engine:** Utilizes the powerful and open-source [ClamAV](https://www.clamav.net/) engine for virus scanning.
- **Phishing Detection:** Detects phishing URLs by referencing an up-to-date list, ensuring protection against online threats.
- **Automatic Updates:** The phishing URL list is automatically updated every 12 hours to keep your server protected against new threats.
- **Discord Logging:** Posts logs to a designated Discord channel for easy monitoring.
- **Automated Safety:** Deletes, removes, reports messages that are sending infected attachment/urls.

## Todo List as of v0.9.0

- Improve Phishing bank ( currently this is only pulling from [Phishing Database](https://github.com/mitchellkrogza/Phishing.Database) )
- Improve Discord Bot message formatting
- Add other malicious URL type processing & checks
- Add editing configurations through Discord Bot

## Prerequisites

- Before you begin, ensure you have Docker and Docker Compose installed on your system.
- According to [ClamAV Docs](https://docs.clamav.net/manual/Installing/Docker.html#memory-ram-requirements), the av server recommends having **4GiB** of available RAM, with a minimum of **3GiB**

## Installation

1. **Obtain the Docker Compose file and the configuration template:**
   - Download the `docker-compose.yml` and `config.json` templates from the Clam Shell repository.

2. **Configure Clam Shell:**
   - Edit `config.json` to set your Discord bot token, webhook URL for logging, and other configuration options.

3. **Start Clam Shell:**
   ```bash
   docker-compose up -d
   ```
### Configuration File Properties Explained

- **"BOT_TOKEN":** Required Bot Token. If unfamiliar take a look [here](https://discord.com/developers/docs/getting-started)
- **"WEBHOOK_URL":"** This is required if you desire to log the bot's messages. If unfamiliar take a look [here](https://github.com/Qolors/FeedCord?tab=readme-ov-file#quick-setup-docker)
- **"USE_LOGS":** If you do not want to enable the logging, set this to false otherwise keep true
- **"USE_REACTIONS":** If enabled, the bot will react to all files it processes with a ✅ to let you know it's been verified

## Usage

Once Clam Shell is up and running, it will automatically start monitoring your Discord server for file and URL sharing. If a file or URL is detected, it will be queued for scanning by the ClamAV engine. Detected threats and logs will be reported in the designated Discord channel.

## Updating

To update Clam Shell, pull the latest Docker images and restart the services:

```bash
docker-compose pull
docker-compose down
docker-compose up -d
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues to improve the software.

## License

Clam Shell is released under the [MIT License](LICENSE).

---

Ensure that you provide the correct download links or instructions for obtaining the `docker-compose.yml` and `config.json` templates. You may also want to provide more detailed instructions for editing the `config.json` file based on the specific configuration options available in your software.