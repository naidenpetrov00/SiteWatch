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
- If shared UI is only used within a feature, keep it in that feature's `components/ui/`.
- Use feature-first naming and keep styling and state close to the owning screen or feature.
- Use semantic HTML in the correct order and structure: keep heading levels logical, use the right landmark elements, and match markup to the content hierarchy.
- Use clear `aria-label`, `accessibilityLabel`, and other accessible names on interactive elements, icons, and controls whenever visible text is missing or ambiguous.
- Use meaningful `alt` text for images. Describe the purpose of the image, not just that an image exists.
- Build with SEO in mind from the start: use semantic structure, meaningful headings, descriptive text, and metadata-friendly routes/content that can be indexed well.
- Treat `android/` and other generated client folders as machine-managed output.
