import { AppPage } from './app.base';
import { AbstractControl, FormControl, FormGroup, ValidatorFn } from '@angular/forms';
import { ButtonModalSetting } from './shared/models/layout/button-modal-setting.model';
import { ButtonType } from './shared/enums/type-button.enum';

export abstract class AppForm extends AppPage {

    requestSearch: any = null;
    requestReset: any = null;
    isDisabled: boolean = null;
    isSubmitted: boolean = false;

    resetButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.reset
    };

    saveButtonSetting: ButtonModalSetting = {
        buttonAttribute: {
            type: 'button',
            titleButton: "Save",
            classStyle: "btn btn-brand m-btn--square m-btn--icon m-btn--uppercase",
            icon: "la la-save"
        },
        typeButton: ButtonType.save,
    };

    constructor() {
        super();
    }

    resetFormWithFeilds(form: FormGroup, key: string): void {
        if (form.controls.hasOwnProperty(key)) {
            if (form.controls[key] instanceof AbstractControl || form.controls[key] instanceof FormControl) {
                form.controls[key].setErrors(null);
                form.controls[key].reset();
                form.controls[key].markAsUntouched({ onlySelf: true });
                form.controls[key].markAsPristine({ onlySelf: true });
            }
        }
    }

    resetFormControl(control: FormControl | AbstractControl) {
        if (!!control && control instanceof FormControl) {
            control.setValue(null);
            control.markAsUntouched({ onlySelf: true });
            control.markAsPristine({ onlySelf: true });
        }
    }

    emailValidator(control: AbstractControl) {
        if (control.value.match(/[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?/)) {
            return null;
        } else {
            return { 'invalidEmailAddress': true };
        }
    }

    passwordValidator(control: AbstractControl) {
        if (control.value.match(/^(?=.*\d)(?=.*[a-zA-Z!@#$%^&*])(?!.*\s).{6,100}$/)) {
            return null;
        } else {
            return { 'invalidPassword': true };
        }
    }

    removeValidators(form: FormControl | AbstractControl) {
        form.clearValidators();
        form.updateValueAndValidity();
    }

    addValidators(form: FormControl | AbstractControl, validateFn: ValidatorFn | ValidatorFn[]) {
        form.setValidators(validateFn);
        form.updateValueAndValidity();
    }

    search($event?: any) {
        this.requestSearch($event);
    }

    reset($event?: any) {
        this.requestReset($event);
    }

}
