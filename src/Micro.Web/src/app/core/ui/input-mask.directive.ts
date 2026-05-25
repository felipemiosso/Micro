import { Directive, HostListener, inject, input } from '@angular/core';
import { NgControl } from '@angular/forms';

export function applyMask(mask: string, val: string): string {
  const hasLetters = mask.includes('A') || mask.includes('X');
  const rawVal = hasLetters ? val.replace(/[^a-zA-Z0-9]/g, '') : val.replace(/\D/g, '');

  let masked = '';
  let valIdx = 0;
  for (let i = 0; i < mask.length; i++) {
    const maskChar = mask[i];
    if (valIdx >= rawVal.length) break;

    if (maskChar === '#' || maskChar === 'A' || maskChar === 'X') {
      const rawChar = rawVal[valIdx];
      if (maskChar === '#' && /\d/.test(rawChar)) {
        masked += rawChar;
        valIdx++;
      } else if (maskChar === 'A' && /[a-zA-Z]/.test(rawChar)) {
        masked += rawChar;
        valIdx++;
      } else if (maskChar === 'X' && /[a-zA-Z0-9]/.test(rawChar)) {
        masked += rawChar;
        valIdx++;
      } else {
        break;
      }
    } else {
      masked += maskChar;
    }
  }
  return masked;
}

@Directive({
  selector: '[cfMask]',
  standalone: true
})
export class InputMaskDirective {
  cfMask = input.required<string>();
  private ngControl = inject(NgControl, { optional: true });

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const masked = applyMask(this.cfMask(), inputEl.value);
    inputEl.value = masked;
    if (this.ngControl?.control) {
      this.ngControl.control.setValue(masked, { emitModelToViewChange: false });
    }
  }
}

// Preset display masks (for input guidance, not validation logic)
export const PRESET_DISPLAY_MASKS: Record<string, string> = {
  cpf:            '###.###.###-##',
  cnpj:           '##.###.###/####-##',
  pis:            '###.#####.##-#',
  cnh:            '###########',
  phone_mobile:   '(##) #####-####',
  phone_landline: '(##) ####-####',
  cep:            '#####-###',
};
