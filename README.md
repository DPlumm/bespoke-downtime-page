# bespoke-downtime-page

A small .NET 8 generator that produces a standardized planned maintenance (downtime) page as a static site.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Configuration

Create `config/maintenance.json` (or copy `config/maintenance.sample.json`) and fill in the required fields:

- `serviceName` (string, required): The product or service name shown in the headline.
- `changeReference` (string, required): The maintenance/change ticket identifier (for example, `CHG-12345`).
- `changeLinkUrl` (string, required): The full URL users can visit for more details about the change.
- `changeLinkText` (string, required): The link label shown to users (for example, “View change request”).
- `message` (string, required; supports newlines): The maintenance message body displayed on the page.

## Generate the maintenance page

From the repo root, run:

```bash
dotnet run --project src/MaintenancePage.Generator -- --config config/maintenance.json
```

### Output location

Generated assets are written to `dist/`:

- `dist/index.html`
- `dist/styles.css`

## Publish

Host the `dist/` directory as a static website. Examples:

- **IIS**: Enable static content and point the site root to `dist/`.
- **GitHub Pages**: Publish the `dist/` folder as your Pages source.

## Troubleshooting

- **Missing fields**: Ensure all required fields in `config/maintenance.json` are present and non-empty.
- **Invalid URL**: Make sure `changeLinkUrl` is a valid, fully-qualified URL (for example, `https://example.com/change/123`).

## Quick start (under 5 minutes)

1. Clone the repo and install the .NET 8 SDK.
2. Copy `config/maintenance.sample.json` to `config/maintenance.json` and edit the values.
3. Run the generator command above and open `dist/index.html` in a browser.
