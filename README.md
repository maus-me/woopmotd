# WoopMOTD

A Vintage Story (1.21+) mod that shows a rich-text (VTML) Message of the Day to players. Works similar to GMod or Counterstrike MOTD menus.  The server controls the content and can push updates to clients on login, on demand via a command, or when players press a hotkey.

## Features
- Server-configured MOTD using VTML (supports bold, colors, line breaks, etc.)
- Multiple MOTD sections (array) merged into one dialog
- Shown automatically on player login
- Client hotkey (default Shift+Y) to open MOTD any time (configurable in Controls)
- Admin command /woopmotd reload to reload config
- Scrollable VTML dialog with dark inset panel and localized close button

## Upcoming Features
- Live config reload on file changes (debounced)
- Public command /motd to open the dialog

## Server Configuration
A config file woopmotd.json is created in your mod config directory at first run. Example:
```
{
  "EnableMotd": true,
  "MotdVtmls": [
    "<strong><font color=\"gold\">Welcome!</color></strong><br>Enjoy your stay.",
    "Remember to be kind to others."
  ]
}
```
### Notes
- Older configs with a single string MotdVtml are migrated automatically to MotdVtmls.
- If MotdVtmls is empty or all whitespace, no MOTD will be shown.

### Usage In-Game
- Auto on login: Players receive the MOTD on joining the world if enabled.
- Hotkey: Press Shift+Y (default) to open the MOTD. Rebind in Controls under Character Controls.
- Public command: /motd
- Admin command: /woopmotd reload (requires controlserver privilege) to reload woopmotd.json

### Localization
- Button and titles are localized via assets/woopmotd/lang/*.json. Default English keys:
  - woopmotd:modname
  - woopmotd:motd-title
  - woopmotd:motd-ok

### Troubleshooting
- Build fails to resolve Vintage Story DLLs: Ensure VINTAGE_STORY environment variable is set to the game directory.
- DLL file locked on build: Make sure the game is not running or that no external tool is using woopmotd.dll.
- No MOTD shown: Check that EnableMotd is true and that MotdVtmls contains at least one non-empty string.

### License
- See the repository’s license file if present. Otherwise, all rights reserved by the author(s).
