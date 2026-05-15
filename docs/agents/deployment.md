# Deployment Guidance

## Configuration
- Keep local API settings in `src/Api/appsettings.Development.json` or environment variables.
- Do not commit secrets, tokens, or machine-specific paths.
- Prefer configuration values over hardcoded endpoints or credentials.

## Release Notes
- Treat generated output such as `bin/`, `obj/`, and `node_modules/` as disposable.
- If deployment work changes required environment variables or storage settings, document the new requirements in the PR.

## Scope
- Use this file for environment, release, and infrastructure-facing changes.
- Keep deployment-specific details out of feature and test guidance.
