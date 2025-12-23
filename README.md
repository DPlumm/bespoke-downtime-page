# bespoke-downtime-page

## Configuration

The generator reads JSON configuration describing the maintenance page.

Create `config/maintenance.json` (or copy `config/maintenance.sample.json`) with:

- `serviceName` (string, required)
- `changeReference` (string, required)
- `changeLinkUrl` (string, required)
- `changeLinkText` (string, required)
- `message` (string, required; supports newlines)

Run the generator with:

```bash
dotnet run --project src/MaintenancePage.Generator -- --config config/maintenance.json
```

CLI options:
- `--config <path>`: JSON configuration file (default: `config/maintenance.json`).
- `--out <directory>`: Output directory for the generated assets (default: `dist`).

The generator always reads `templates/page.template.html` and `templates/styles.css`, produces `index.html` and `styles.css` in the chosen output directory, HTML-encodes all config values, and injects the current timestamp as ISO 8601 into `{{LAST_UPDATED}}`.
