import { ValidatorFn, Validators, ValidationErrors, AbstractControl } from '@angular/forms';
import { CustomFieldDefinition } from '../services/custom-fields.service';

function validateCpf(val: string): boolean {
  const regex = /^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$/;
  if (!regex.test(val)) return false;

  const clean = val.replace(/\D/g, '');
  if (clean.length !== 11) return false;
  if (new Set(clean).size === 1) return false;

  let tempCpf = clean.substring(0, 9);
  let sum = 0;
  const multipliers1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
  for (let i = 0; i < 9; i++) {
    sum += parseInt(tempCpf.charAt(i)) * multipliers1[i];
  }
  let remainder = sum % 11;
  const digit1 = remainder < 2 ? 0 : 11 - remainder;

  tempCpf += digit1;
  sum = 0;
  const multipliers2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
  for (let i = 0; i < 10; i++) {
    sum += parseInt(tempCpf.charAt(i)) * multipliers2[i];
  }
  remainder = sum % 11;
  const digit2 = remainder < 2 ? 0 : 11 - remainder;

  return clean.endsWith(`${digit1}${digit2}`);
}

function validateCnpj(val: string): boolean {
  const regex = /^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$|^\d{14}$/;
  if (!regex.test(val)) return false;

  const clean = val.replace(/\D/g, '');
  if (clean.length !== 14) return false;
  if (new Set(clean).size === 1) return false;

  let tempCnpj = clean.substring(0, 12);
  let sum = 0;
  const multipliers1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  for (let i = 0; i < 12; i++) {
    sum += parseInt(tempCnpj.charAt(i)) * multipliers1[i];
  }
  let remainder = sum % 11;
  const digit1 = remainder < 2 ? 0 : 11 - remainder;

  tempCnpj += digit1;
  sum = 0;
  const multipliers2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  for (let i = 0; i < 13; i++) {
    sum += parseInt(tempCnpj.charAt(i)) * multipliers2[i];
  }
  remainder = sum % 11;
  const digit2 = remainder < 2 ? 0 : 11 - remainder;

  return clean.endsWith(`${digit1}${digit2}`);
}

function validatePis(val: string): boolean {
  const regex = /^\d{3}\.\d{5}\.\d{2}-\d{1}$|^\d{11}$/;
  if (!regex.test(val)) return false;

  const clean = val.replace(/\D/g, '');
  if (clean.length !== 11) return false;
  if (new Set(clean).size === 1) return false;

  const multipliers = [3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  let sum = 0;
  for (let i = 0; i < 10; i++) {
    sum += parseInt(clean.charAt(i)) * multipliers[i];
  }
  const remainder = sum % 11;
  let digit = 11 - remainder;
  if (digit === 10 || digit === 11) digit = 0;

  return clean.endsWith(digit.toString());
}

function validateCnh(val: string): boolean {
  const clean = val.replace(/\D/g, '');
  if (clean.length !== 11 || new Set(clean).size === 1) return false;
  return true;
}

const PRESET_REGISTRY: Record<string, (value: string) => boolean> = {
  cpf:            validateCpf,
  cnpj:           validateCnpj,
  pis:            validatePis,
  cnh:            validateCnh,
  email:          (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v),
  phone_mobile:   (v) => /^\(\d{2}\)\s?9\d{4}-\d{4}$|^\d{11}$/.test(v) && v.replace(/\D/g, '')[2] === '9',
  phone_landline: (v) => /^\(\d{2}\)\s?[2-8]\d{3}-\d{4}$|^\d{10}$/.test(v),
  url:            (v) => { try { new URL(v); return true; } catch { return false; } },
  cep:            (v) => /^\d{5}-\d{3}$|^\d{8}$/.test(v),
};

export function presetValidator(presets: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const known = presets.filter(k => PRESET_REGISTRY[k]);
    if (known.length === 0) return null;
    return known.some(k => PRESET_REGISTRY[k](control.value)) ? null : { invalidPreset: true };
  };
}

export function maskValidator(mask: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return matchesMask(mask, control.value) ? null : { invalidMask: { mask } };
  };
}

function matchesMask(mask: string, value: string): boolean {
  if (mask.length !== value.length) return false;
  return [...mask].every((token, i) => {
    const c = value[i];
    if (token === '#') return /\d/.test(c);
    if (token === 'A') return /[a-zA-Z]/.test(c);
    if (token === 'X') return /[a-zA-Z0-9]/.test(c);
    return c === token;
  });
}

function minDateValidator(minDate: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return control.value >= minDate ? null : { minDate: { minDate } };
  };
}

function maxDateValidator(maxDate: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return control.value <= maxDate ? null : { maxDate: { maxDate } };
  };
}

export function buildCustomFieldValidators(def: CustomFieldDefinition): ValidatorFn[] {
  const v: ValidatorFn[] = [];
  if (def.isRequired) v.push(Validators.required);

  const opts = def.validation;
  if (!opts) return v;

  switch (def.fieldType) {
    case 'ShortText':
      if (opts.minLength) v.push(Validators.minLength(opts.minLength));
      if (opts.maxLength) v.push(Validators.maxLength(opts.maxLength));
      if (opts.presets?.length) v.push(presetValidator(opts.presets));
      else if (opts.formatMask) v.push(maskValidator(opts.formatMask));
      break;

    case 'LongText':
      if (opts.minLength) v.push(Validators.minLength(opts.minLength));
      if (opts.maxLength) v.push(Validators.maxLength(opts.maxLength));
      break;

    case 'Number':
      if (opts.min != null) v.push(Validators.min(opts.min));
      if (opts.max != null) v.push(Validators.max(opts.max));
      break;

    case 'Date':
      if (opts.minDate) v.push(minDateValidator(opts.minDate));
      if (opts.maxDate) v.push(maxDateValidator(opts.maxDate));
      break;
  }

  return v;
}

export function getCustomFieldErrorMessage(def: CustomFieldDefinition, errors: ValidationErrors): string {
  if (errors['serverError']) return errors['serverError'];
  if (errors['required'])    return `${def.label} is required.`;
  if (errors['minlength'])   return `${def.label} must be at least ${def.validation?.minLength} characters.`;
  if (errors['maxlength'])   return `${def.label} must be at most ${def.validation?.maxLength} characters.`;
  if (errors['min'])         return `${def.label} must be greater than or equal to ${def.validation?.min}.`;
  if (errors['max'])         return `${def.label} must be less than or equal to ${def.validation?.max}.`;
  if (errors['minDate'])     return `${def.label} must be on or after ${def.validation?.minDate}.`;
  if (errors['maxDate'])     return `${def.label} must be on or before ${def.validation?.maxDate}.`;
  if (errors['invalidPreset']) return `${def.label}: invalid format.`;
  if (errors['invalidMask'])   return `${def.label} does not match the expected format.`;
  return `${def.label}: invalid value.`;
}
