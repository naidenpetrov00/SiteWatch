import { DestroyRef, Injectable, inject } from '@angular/core';

@Injectable()
export class IssueAttachmentObjectUrlRegistry {
  private readonly destroyRef = inject(DestroyRef);
  private readonly urls = new Set<string>();

  constructor() {
    this.destroyRef.onDestroy(() => {
      for (const url of this.urls) {
        URL.revokeObjectURL(url);
      }
      this.urls.clear();
    });
  }

  create(file: File): string {
    const url = URL.createObjectURL(file);
    this.urls.add(url);
    return url;
  }

  release(url: string | null): void {
    if (url && this.urls.delete(url)) {
      URL.revokeObjectURL(url);
    }
  }
}
