import { Injectable } from '@angular/core';
import { FormGroup, FormControl, ValidatorFn } from '@angular/forms';
import { CustomFieldDefinition, CustomFieldValueDto, CustomFieldValueInput } from './custom-fields.service';
import { buildCustomFieldValidators } from '../validators/custom-field.validators';

@Injectable({ providedIn: 'root' })
export class CustomFieldFormService {

  buildFormGroup(
    definitions: CustomFieldDefinition[],
    existingValues: CustomFieldValueDto[] = []
  ): FormGroup {
    const controls: Record<string, FormControl> = {};
    const valueMap = Object.fromEntries(existingValues.map(v => [v.definitionId, v.value ?? '']));

    for (const def of definitions) {
      controls[def.id] = new FormControl(
        valueMap[def.id] ?? '',
        this.buildValidators(def)
      );
    }
    return new FormGroup(controls);
  }

  buildValidators(def: CustomFieldDefinition): ValidatorFn[] {
    return buildCustomFieldValidators(def);
  }

  /** Patches server-side validation errors from ValidationProblem onto form controls. */
  applyServerErrors(
    errors: Record<string, string[]>,
    customFieldsGroup: FormGroup
  ): void {
    for (const [key, messages] of Object.entries(errors)) {
      if (key.startsWith('customFields.')) {
        const defId = key.replace('customFields.', '');
        customFieldsGroup.get(defId)?.setErrors({ serverError: messages[0] });
      }
    }
  }

  /** Extracts CustomFieldValueInput[] from the form group for submission. */
  extractValues(
    group: FormGroup,
    definitions: CustomFieldDefinition[]
  ): CustomFieldValueInput[] {
    return definitions.map(def => ({
      definitionId: def.id,
      value: group.get(def.id)?.value !== '' && group.get(def.id)?.value !== undefined ? group.get(def.id)?.value : null
    }));
  }
}
