import { Directive, ElementRef, HostListener, Input, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

import {
  sanitizeCompanyName,
  sanitizeDigitsOnly,
  sanitizeLettersOnly
} from '../add-person-dialog.input-utils';

export type PersonInputSanitizerMode = 'letters' | 'digits' | 'company';

@Directive({
  selector: 'input[appPersonInputSanitizer], textarea[appPersonInputSanitizer]',
  standalone: true
})
export class PersonInputSanitizerDirective {
  @Input('appPersonInputSanitizer') mode: PersonInputSanitizerMode = 'letters';

  private readonly elementRef = inject(ElementRef<HTMLInputElement | HTMLTextAreaElement>);
  private readonly ngControl = inject(NgControl, { optional: true });

  @HostListener('keydown', ['$event'])
  handleKeydown(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;

    if (keyboardEvent.ctrlKey || keyboardEvent.metaKey || keyboardEvent.altKey) {
      return;
    }

    if (keyboardEvent.key.length !== 1) {
      return;
    }

    if (this.mode === 'digits') {
      if (!/^\d$/.test(keyboardEvent.key)) {
        keyboardEvent.preventDefault();
      }
      return;
    }

    if (this.mode === 'company') {
      if (!/^[\p{L}\p{M}\d ]$/u.test(keyboardEvent.key)) {
        keyboardEvent.preventDefault();
      }
      return;
    }

    if (!/^[\p{L}\p{M}]$/u.test(keyboardEvent.key)) {
      keyboardEvent.preventDefault();
    }
  }

  @HostListener('paste', ['$event'])
  handlePaste(event: Event): void {
    const clipboardEvent = event as ClipboardEvent;
    const pastedText = clipboardEvent.clipboardData?.getData('text') ?? '';
    const sanitizedText = this.sanitizeValue(pastedText);

    if (sanitizedText === pastedText) {
      return;
    }

    clipboardEvent.preventDefault();
    this.replaceValue(sanitizedText);
  }

  @HostListener('input')
  handleInput(): void {
    const inputElement = this.elementRef.nativeElement;
    const sanitizedValue = this.sanitizeValue(inputElement.value);

    if (sanitizedValue === inputElement.value) {
      return;
    }

    this.replaceValue(sanitizedValue);
  }

  private sanitizeValue(value: string): string {
    if (this.mode === 'digits') {
      return sanitizeDigitsOnly(value);
    }

    if (this.mode === 'company') {
      return sanitizeCompanyName(value);
    }

    return sanitizeLettersOnly(value);
  }

  private replaceValue(value: string): void {
    const inputElement = this.elementRef.nativeElement;
    inputElement.value = value;
    this.ngControl?.control?.setValue(value, { emitEvent: false });
  }
}
