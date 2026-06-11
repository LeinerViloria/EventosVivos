import { Component, input } from '@angular/core';
import { MessageModule } from 'primeng/message';

/**
 * A labelled form field: the label (with a required marker), the projected control and an inline
 * error message. Composes the repeated field markup so forms do not duplicate it.
 */
@Component({
  selector: 'app-field',
  imports: [MessageModule],
  template: `
    <div class="flex flex-col gap-1.5">
      <label [for]="for()" class="font-medium">
        {{ label() }}
        @if (required()) {
          <span class="text-red-500">*</span>
        }
      </label>
      <ng-content />
      @if (showError()) {
        <p-message severity="error" variant="simple" size="small" [text]="errorText()" />
      }
    </div>
  `,
})
export class FieldComponent {
  readonly label = input('');
  readonly for = input('');
  readonly required = input(true);
  readonly showError = input(false);
  readonly errorText = input('');
}
