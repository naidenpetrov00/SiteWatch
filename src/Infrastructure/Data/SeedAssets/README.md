# Seed assets

Place development-only seed assets in the matching folder:

- `Images`: valid image files such as `.jpg`, `.jpeg`, `.png`, or `.webp`.
- `Videos`: valid video files such as `.mp4` or `.webm`. Video seeding requires `ffmpeg` and `ffprobe` to be available on the machine running initialization.
- `Files`: any general document or file. Unknown extensions are uploaded as `application/octet-stream`.

The initializer enumerates every file recursively in each folder. Each image and video is reused for every seeded site and every category allowed by that site's media policy. Each general file is reused for every seeded site and every `FileDocumentType`.

Do not place `.gitkeep` files in subdirectories containing actual assets; it is the only file name ignored by the initializer. Source assets may be added later and are not included in this repository by default.
