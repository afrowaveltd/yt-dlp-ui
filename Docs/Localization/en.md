# Localization — dictionaryless JSON

[Česky](cs.md) · [Index](../Documentation/en.md) · [Configuration](../Configuration/en.md)

## Principle

The English UI text **is its own key**. `en.json` contains English key/value entries, while `cs.json` contains corresponding Czech translations. For example:

~~~json
{
  "Download": "Baixar",
  "About": "Sobre"
}
~~~

The example above could be stored in `pt-BR.json`. Keys are case-sensitive. No placeholder syntax is needed for ordinary static strings; the current lightweight localizer intentionally does **not** implement UnderstandMe's richer hint/placeholder/provider features.

## Language discovery and precedence

`SimpleLocalizer` discovers valid `*.json` dictionaries automatically, in this order (later sources override earlier ones with the same culture code):

1. Embedded dictionaries compiled from `src/YtDlpUi.Core/Locales/*.json`.
2. `Locales/` next to the application executable (available in publish output).
3. User-local configuration folder:
   - Linux: `~/.config/yt-dlp-ui/Locales/` or `$XDG_CONFIG_HOME/yt-dlp-ui/Locales/`
   - Windows: `%LOCALAPPDATA%\yt-dlp-ui\Locales\`

A culture code such as `cs`, `fr` or `pt-BR` determines the language. An invalid or unreadable JSON is ignored. If no exact culture exists, a parent culture is tried (for example `fr-CA` -> `fr`), then English. Missing/empty translations fall back to the English source key.

## How to add a language

1. Create a valid JSON dictionary named with the desired culture code, for example `pt-BR.json`.
2. Add at least the entries you want to translate, using English labels from `src/YtDlpUi.Core/Locales/en.json`.
3. Place it in a user-local `Locales/` directory to avoid recompiling, or commit it under `src/YtDlpUi.Core/Locales/` for bundling in future releases.
4. Open the 🌐 language dropdown in the upper-right header. The list and dictionaries are rescanned when it opens.
5. Select the language. Already localized UI controls update immediately. The choice is persisted in `settings.json`.

The default language follows the OS UI culture if a matching dictionary is present; otherwise English. Do not confuse **adding a dictionary** with **translating every control**: only UI elements connected through `TrExtension` currently react, and some existing Czech messages still need migration.

## XAML and developer usage

For a new string, add its English key to `en.json`, translate it in `cs.json` and bind:

~~~xml
<TextBlock Text="{loc:Tr Key='Download'}" />
~~~

The `TrExtension` returns an observable binding, so the text updates on language changes without reopening the window. Avoid building dynamic sentences by concatenating translated fragments. If placeholders become necessary, introduce them intentionally and validate the keys before switching to UnderstandMe.

## Test cases

The smoke suite tests discovery of embedded and newly created files, invalid JSON, `pt-BR` culture codes, `fr-CA` parent fallback, runtime changes/removal, English missing-key fallback and user override of bundled Czech. This is not a promise of completed UI translation.

[Development guide](../Development-and-Verification/en.md)
