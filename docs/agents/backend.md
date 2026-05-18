# Backend Guidance

## Layer Responsibilities
- `src/Api/`: HTTP endpoints, endpoint filters, middleware, and request entry points.
- `src/Application/`: use cases, MediatR handlers, validation, and shared behaviors.
- `src/Domain/`: entities, value objects, and business rules only.
- `src/Infrastructure/`: EF Core, identity, storage, email, and external service adapters.

## Workflow
- Start new behavior in `Application`.
- Add or adjust the business rule in `Domain` when the rule belongs to the model.
- Implement concrete dependencies in `Infrastructure` behind interfaces.
- Expose the feature through `Api` endpoints or filters.

## Notes
- Keep dependencies pointing inward.
- Do not place infrastructure concerns in `Domain` or request handling logic in `Infrastructure`.
- Do not write database migrations by hand alone or update the `DbContext` snapshot manually; create them with the manual `add migration` command and keep the generated files as the source of truth.
