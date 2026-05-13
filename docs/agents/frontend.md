# Frontend Guidance

## App Structure
- `src/ClientApp/app/`: Expo Router routes and screen composition.
- `src/ClientApp/components/app/`: app-specific shell components.
- `src/ClientApp/components/ui/`: reusable presentational components only.
- `src/ClientApp/features/auth`, `features/cameras`, `features/sites`: feature-owned screens, hooks, and logic.
- `src/ClientApp/config/constants/`, `hooks/`, `lib/`, `store/`, `assets/`: shared client concerns.

## Conventions
- Keep feature logic inside the owning feature folder.
- Keep route composition in `app/` and shared UI in `components/ui/` if the shared ui is shared only in feature add it in the feature `components/ui`.
- Use feature-first naming and keep styling and state close to the owning screen or feature.
- Treat `android/` and other generated client folders as machine-managed output.
