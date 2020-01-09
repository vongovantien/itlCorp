import { FormGroup, AbstractControl, FormControl, Validators, ValidationErrors, } from '@angular/forms';

export class FormValidators extends Validators {

    public static comparePort(controls: AbstractControl | FormControl | FormGroup): ValidationErrors | any {

        const pol: string = controls.get('pol').value;
        const pod: string = controls.get('pod').value;

        if (!pol || !pod) {
            return null;
        }
        if (pol !== null && pod !== null && pol === pod) {
            return { invalidPort: true };
        }
        return null;

    }

    public static compareGW_CW(controls: AbstractControl | FormControl | FormGroup): ValidationErrors | any {
        const gw: string = controls.get('grossWeight').value;
        const cw: string = controls.get('chargeWeight').value;
        if (!gw || !cw) {
            return null;
        }
        if (gw !== null && cw !== null && gw > cw) {
            return { invalidGW: true };
        }
        return null;
    }

    // TODO Custom validator Fn here !
}
