# WoopMOTD

A Vintage Story (1.21+) mod that shows a rich-text (VTML) Message of the Day to players. Works similar to GMod or Counterstrike MOTD menus.  The server controls the content and can push updates to clients on login, on demand via a command, or when players press a hotkey.

### Designed for the [WoopLand](vintagestoryjoin://) Vintage Story Server.  Join us at [discord.gg](https://discord.woopland.com).

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

## Known Issues & Limitations
- `a href` Links will have text cropped off the bottom unless they are wrapped in a `<font lineheight=1>`. This is due to the way VTML has far too much padding between lines by default this mod attempts to address the padding issue by using a lineheight of 0.8.
- The VTML parser is very fragile.  Invalid VTML can cause the dialog to not render or crash the game.  We attempt to handle this by gracefully failing and having the server fallback to default values. Test your VTML in a singleplayer world before deploying to a server.
- Links can not be colored.  This is a VTML limitation.
- Icon sizing is inconsistent.  This is a VTML limitation.

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
This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).

- Full text: see the [LICENSE](LICENSE) file in this repository.
- Summary: You may use, modify, and redistribute this software under the AGPL-3.0 terms. If you run a modified version as a network service, you must make the source code of your modifications available to users interacting with that service.
