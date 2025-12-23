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

Use the `--template` and `--output` switches (or positional arguments) to override the template and output paths.
