import { A11yModule } from '@angular/cdk/a11y';
import { Component, input, OnChanges, output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { PhotoDisplayItem } from '../../../../models/AlbumItemDto';
import { PhotoEditFieldsRequest } from '../../../../models/PhotoEditFieldsRequest';

function optionalInteger(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value.trim();
  return value === '' || Number.isInteger(Number(value)) ? null : { integer: true };
}

@Component({
  selector: 'app-admin-photo-editor',
  imports: [A11yModule, ReactiveFormsModule],
  templateUrl: './admin-photo-editor.component.html',
  styleUrl: './admin-photo-editor.component.css',
})
export class AdminPhotoEditor implements OnChanges {
  readonly photoDisplay = input.required<PhotoDisplayItem>();
  readonly mutationPending = input(false);
  readonly saveError = input<string | null>(null);
  readonly saveRequested = output<PhotoEditFieldsRequest>();
  readonly cancelRequested = output<void>();

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(400)] }),
    yearContentCreated: new FormControl('', { nonNullable: true, validators: [optionalInteger] }),
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['photoDisplay']) {
      const photo = this.photoDisplay().content.photo;
      this.form.reset({
        name: photo.name,
        description: photo.description,
        yearContentCreated: photo.yearContentCreated?.toString() ?? '',
      });
    }
  }

  get hasChanges(): boolean {
    const photo = this.photoDisplay().content.photo;
    const draft = this.details();
    return draft.name !== photo.name || draft.description !== photo.description ||
      draft.yearContentCreated !== photo.yearContentCreated;
  }

  get canSave(): boolean {
    return !this.mutationPending() && this.form.valid && this.hasChanges;
  }

  save(): void {
    if (this.canSave) this.saveRequested.emit(this.details());
  }

  cancel(): void {
    if (!this.mutationPending()) this.cancelRequested.emit();
  }

  private details(): PhotoEditFieldsRequest {
    const draft = this.form.getRawValue();
    return {
      name: draft.name.trim(),
      description: draft.description,
      yearContentCreated: draft.yearContentCreated.trim() === '' ? null : Number(draft.yearContentCreated),
    };
  }
}
