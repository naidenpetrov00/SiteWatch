# Seed assets

Place development-only seed assets in the matching folder:

- `Images`: valid image files such as `.jpg`, `.jpeg`, `.png`, or `.webp`.
- `Videos`: valid video files such as `.mp4` or `.webm`. Video seeding requires `ffmpeg` and `ffprobe` to be available on the machine running initialization.
- `Files`: any general document or file. Unknown extensions are uploaded as `application/octet-stream`.
- `Invoices`: PDF files uploaded as private blobs for seeded invoices.

The initializer enumerates every file recursively in each folder. Each image and video is reused for every seeded site and every category allowed by that site's media policy. Each general file is reused for every seeded site and every `FileDocumentType`. Invoice PDFs are ordered by relative filename and assigned to seeded invoices. When fewer than five PDFs exist, the last PDF is reused until five invoice files are assigned; when more than five exist, every PDF is assigned. Unsupported files are skipped and existing invoice blobs are never replaced.

Do not place `.gitkeep` files in subdirectories containing actual assets; it is the only file name ignored by the initializer. Source assets may be added later and are not included in this repository by default.
