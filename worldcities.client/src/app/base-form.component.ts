import { Component } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';

@Component({
  template: '',
})
export abstract class BaseFormComponent {
  form!: FormGroup;

  getErrors(
    control: AbstractControl,
    displayName: string,
    customMessages: Record<string, string> | null = null
  ): Array<String> {
    var errors: Array<string> = [];

    Object.keys(control.errors ?? {}).forEach((key) => {
      switch (key) {
        case 'required':
          errors.push(`${displayName} ${customMessages?.[key] ?? "is required."}`);
          break;
        case 'pattern':
          errors.push(`${displayName} ${customMessages?.[key] ?? "contains invalid characters."}`);
          break;
        case 'isDupeField':
          errors.push(`${displayName} ${customMessages?.[key] ?? "already exists; please choose another."}`);
          break;
        default:
          errors.push(`${displayName} is invalid.`);
      }
    });

    return errors;
  }

  constructor() { }
}
