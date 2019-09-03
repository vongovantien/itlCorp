import { AppPage } from './app.base';
import { AbstractControl, FormControl, FormGroup, FormBuilder } from '@angular/forms';

export abstract class AppForm extends AppPage {

    requestSearch: any = null;
    requestReset: any = null;

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

    search() {
        this.requestSearch();
    }

    reset() {
        this.requestReset();
    }

}
