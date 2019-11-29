import { FormGroup, AbstractControl, FormControl, Validators, ValidationErrors, } from '@angular/forms';

export class FormValidators extends Validators {

    public static comparePort(controls: AbstractControl | FormControl | FormGroup): ValidationErrors | any {

        const pol: string = controls.get('pol').value;
        const pod: string = controls.get('pod').value;

        if (pol !== null && pod !== null && pol === pod) {
            return { invalidPort: true };
        }
        return null;

    }

    // TODO Custom validator Fn here !
}
