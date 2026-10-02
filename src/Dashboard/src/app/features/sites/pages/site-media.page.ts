import { DatePipe, TitleCasePipe } from '@angular/common';
import { HttpEventType, HttpResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { DashboardSite } from '../models/dashboard-site.model';
import {
  FILE_DOCUMENT_TYPES,
  formatDocumentType,
  SiteFileMedia,
  SiteImageMedia,
  SiteMediaKind,
  SiteVideoMedia
} from '../models/site-media.model';
import { DashboardSitesService } from '../services/dashboard-sites.service';
import { SiteMediaService } from '../services/site-media.service';
import { SiteMediaDeleteDialogComponent } from '../components/site-media-delete-dialog.component';
import { SiteMediaViewerDialogComponent } from '../components/site-media-viewer-dialog.component';

const IMAGE_TYPES = new Set(['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif', 'image/heic', 'image/heif']);
const VIDEO_TYPES = new Set(['video/mp4', 'video/quicktime', 'video/webm']);
const MAX_IMAGE_SIZE = 50 * 1024 * 1024;
const MAX_VIDEO_SIZE = 500 * 1024 * 1024;
const MAX_FILE_SIZE = 100 * 1024 * 1024;

@Component({
  selector: 'app-site-media-page',
  imports: [DatePipe, TitleCasePipe, MatButtonModule, MatDialogModule, RouterLink],
  templateUrl: './site-media.page.html',
  styleUrl: './site-media.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SiteMediaPage {
  private readonly route = inject(ActivatedRoute);
  private readonly sitesService = inject(DashboardSitesService);
  private readonly mediaService = inject(SiteMediaService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly objectUrls = new Set<string>();
  private previewRequestVersion = 0;

  readonly siteId = this.route.snapshot.paramMap.get('siteId') ?? '';
  readonly site = signal<DashboardSite | null>(null);
  readonly siteLoadFailed = signal(false);
  readonly activeTab = signal<SiteMediaKind>('images');
  readonly tabs: readonly SiteMediaKind[] = ['images', 'videos', 'files'];
  readonly categoryFilter = signal('All');
  readonly documentTypeFilter = signal('All');
  readonly selectedCategory = signal('');
  readonly selectedDocumentType = signal('Other');
  readonly selectedFile = signal<File | null>(null);
  readonly validationMessage = signal<string | null>(null);
  readonly operationMessage = signal<string | null>(null);
  readonly uploadProgress = signal<number | null>(null);
  readonly isUploading = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly imageThumbnailUrls = signal<Readonly<Record<string, string>>>({});
  readonly videoSnapshotUrls = signal<Readonly<Record<string, string>>>({});
  readonly documentTypes = FILE_DOCUMENT_TYPES;
  readonly mediaQuery = this.mediaService.mediaQuery;

  constructor() {
    this.destroyRef.onDestroy(() => this.revokeAllObjectUrls());
    if (this.siteId) this.mediaService.setSiteId(this.siteId);
    void this.loadSite();

    effect(() => {
      const media = this.mediaQuery.data();
      if (!media) return;
      void this.loadPreviewUrls(media.images, media.videos);
    });
  }

  get categoryOptions(): readonly string[] {
    return this.site()?.mediaPolicy.categories ?? [];
  }

  get filteredImages(): readonly SiteImageMedia[] {
    const images = this.mediaQuery.data()?.images ?? [];
    const category = this.categoryFilter();
    return category === 'All' ? images : images.filter((image) => image.category === category);
  }

  get filteredVideos(): readonly SiteVideoMedia[] {
    const videos = this.mediaQuery.data()?.videos ?? [];
    const category = this.categoryFilter();
    return category === 'All' ? videos : videos.filter((video) => video.category === category);
  }

  get filteredFiles(): readonly SiteFileMedia[] {
    const files = this.mediaQuery.data()?.files ?? [];
    const type = this.documentTypeFilter();
    return type === 'All' ? files : files.filter((file) => file.documentType === type);
  }

  setTab(tab: SiteMediaKind): void {
    this.activeTab.set(tab);
    this.validationMessage.set(null);
    this.operationMessage.set(null);
  }

  onTabKeydown(event: KeyboardEvent, tab: SiteMediaKind): void {
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') return;
    event.preventDefault();
    const offset = event.key === 'ArrowRight' ? 1 : -1;
    const nextIndex = (this.tabs.indexOf(tab) + offset + this.tabs.length) % this.tabs.length;
    const nextTab = this.tabs[nextIndex]!;
    this.setTab(nextTab);
    document.getElementById(`site-media-tab-${nextTab}`)?.focus();
  }

  onCategoryFilterChange(value: string): void { this.categoryFilter.set(value); }
  onDocumentTypeFilterChange(value: string): void { this.documentTypeFilter.set(value); }
  onSelectedCategoryChange(value: string): void { this.selectedCategory.set(value); }
  onSelectedDocumentTypeChange(value: string): void { this.selectedDocumentType.set(value); }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0) ?? null;
    this.selectedFile.set(file);
    this.validationMessage.set(file ? this.validateFile(file, this.activeTab()) : null);
  }

  async upload(): Promise<void> {
    const file = this.selectedFile();
    const kind = this.activeTab();
    if (!file) {
      this.validationMessage.set('Choose a file to upload.');
      return;
    }

    const validationMessage = this.validateFile(file, kind);
    if (validationMessage) {
      this.validationMessage.set(validationMessage);
      return;
    }

    const classification = kind === 'files' ? this.selectedDocumentType() : this.selectedCategory();
    if (!classification) {
      this.validationMessage.set(kind === 'files' ? 'Choose a document type.' : 'Choose a media category.');
      return;
    }

    this.isUploading.set(true);
    this.uploadProgress.set(0);
    this.validationMessage.set(null);
    this.operationMessage.set(null);

    try {
      await new Promise<void>((resolve, reject) => {
        this.mediaService.upload(this.siteId, kind, file, classification).subscribe({
          next: (event) => {
            if (event.type === HttpEventType.UploadProgress) {
              this.uploadProgress.set(event.total ? Math.round(event.loaded / event.total * 100) : null);
            } else if (event instanceof HttpResponse) {
              resolve();
            }
          },
          error: reject,
          complete: () => resolve()
        });
      });
      await this.mediaService.invalidate();
      this.selectedFile.set(null);
      this.operationMessage.set('Upload complete.');
    } catch {
      this.operationMessage.set('Upload failed. Check your permissions or network connection and try again.');
    } finally {
      this.isUploading.set(false);
      this.uploadProgress.set(null);
    }
  }

  async openImage(item: SiteImageMedia): Promise<void> {
    await this.openViewer('image', item.imageId, `Image in ${item.category}`);
  }

  async openVideo(item: SiteVideoMedia): Promise<void> {
    await this.openViewer('video', item.videoId, `Video in ${item.category}`);
  }

  async openFile(item: SiteFileMedia, download: boolean): Promise<void> {
    try {
      const url = this.createObjectUrl(await this.mediaService.getBlob('files', item.fileId));
      const link = document.createElement('a');
      link.href = url;
      if (!download) link.target = '_blank';
      link.rel = 'noopener';
      if (download) link.download = item.fileName;
      link.click();
      window.setTimeout(() => this.releaseObjectUrl(url), 60_000);
    } catch {
      this.operationMessage.set('The file could not be opened. Check your permissions or network connection and try again.');
    }
  }

  async deleteItem(kind: SiteMediaKind, id: string, label: string): Promise<void> {
    const confirmed = await firstValueFrom(this.dialog.open(SiteMediaDeleteDialogComponent, {
      autoFocus: false,
      data: label,
      width: '28rem',
      maxWidth: 'calc(100vw - 2rem)'
    }).afterClosed());
    if (confirmed !== true) return;

    this.deletingId.set(id);
    this.operationMessage.set(null);
    try {
      await this.mediaService.delete(kind, id);
    } catch {
      this.operationMessage.set('Deletion failed. Check your permissions or network connection and try again.');
    } finally {
      this.deletingId.set(null);
    }
  }

  formatDocumentType = formatDocumentType;

  private async loadSite(): Promise<void> {
    if (!this.siteId) {
      this.siteLoadFailed.set(true);
      return;
    }
    try {
      const site = await this.sitesService.getSiteById(this.siteId);
      this.site.set(site);
      this.selectedCategory.set(site.mediaPolicy.categories[0] ?? '');
    } catch {
      this.siteLoadFailed.set(true);
    }
  }

  private async loadPreviewUrls(images: readonly SiteImageMedia[], videos: readonly SiteVideoMedia[]): Promise<void> {
    const requestVersion = ++this.previewRequestVersion;
    this.clearPreviewUrls();
    const [imageEntries, videoEntries] = await Promise.all([
      Promise.all(images.map((image) => this.loadPreview('images', image.imageId, image.thumbnailId))),
      Promise.all(videos.map((video) => this.loadPreview('videos', video.videoId, video.snapshotId, true)))
    ]);
    const urls = [...imageEntries, ...videoEntries].filter((entry): entry is readonly [string, string] => entry !== null);
    if (requestVersion !== this.previewRequestVersion) {
      for (const [, url] of urls) this.releaseObjectUrl(url);
      return;
    }
    this.imageThumbnailUrls.set(Object.fromEntries(imageEntries.filter((entry): entry is readonly [string, string] => entry !== null)));
    this.videoSnapshotUrls.set(Object.fromEntries(videoEntries.filter((entry): entry is readonly [string, string] => entry !== null)));
  }

  private async loadPreview(kind: 'images' | 'videos', itemId: string, sourceId: string, snapshot = false): Promise<readonly [string, string] | null> {
    try {
      return [itemId, this.createObjectUrl(await this.mediaService.getBlob(kind, sourceId, snapshot))] as const;
    } catch {
      return null;
    }
  }

  private async openViewer(kind: 'image' | 'video', id: string, label: string): Promise<void> {
    try {
      const url = this.createObjectUrl(await this.mediaService.getBlob(kind === 'image' ? 'images' : 'videos', id));
      const dialogRef = this.dialog.open(SiteMediaViewerDialogComponent, {
        autoFocus: true,
        data: { kind, url, label },
        maxWidth: 'calc(100vw - 2rem)',
        maxHeight: '90vh'
      });
      await firstValueFrom(dialogRef.afterClosed());
      this.releaseObjectUrl(url);
    } catch {
      this.operationMessage.set('Preview could not be loaded. Check your permissions or network connection and try again.');
    }
  }

  private validateFile(file: File, kind: SiteMediaKind): string | null {
    if (file.size === 0) return 'The selected file cannot be empty.';
    if (kind === 'images') {
      if (!IMAGE_TYPES.has(file.type.toLowerCase())) return 'Only JPEG, PNG, WebP, GIF, HEIC, or HEIF images are allowed.';
      return file.size > MAX_IMAGE_SIZE ? 'Images cannot exceed 50 MB.' : null;
    }
    if (kind === 'videos') {
      if (!VIDEO_TYPES.has(file.type.toLowerCase())) return 'Only MP4, MOV, or WebM videos are allowed.';
      return file.size > MAX_VIDEO_SIZE ? 'Videos cannot exceed 500 MB.' : null;
    }
    return file.size > MAX_FILE_SIZE ? 'Files cannot exceed 100 MB.' : null;
  }

  private createObjectUrl(blob: Blob): string {
    const url = URL.createObjectURL(blob);
    this.objectUrls.add(url);
    return url;
  }

  private releaseObjectUrl(url: string): void {
    if (this.objectUrls.delete(url)) URL.revokeObjectURL(url);
  }

  private clearPreviewUrls(): void {
    for (const url of [...Object.values(this.imageThumbnailUrls()), ...Object.values(this.videoSnapshotUrls())]) this.releaseObjectUrl(url);
    this.imageThumbnailUrls.set({});
    this.videoSnapshotUrls.set({});
  }

  private revokeAllObjectUrls(): void {
    for (const url of this.objectUrls) URL.revokeObjectURL(url);
    this.objectUrls.clear();
  }
}
