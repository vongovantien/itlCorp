import { Injectable } from "@angular/core";
import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

@Injectable()
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

    static required(control: FormControl): ValidationErrors {
        if (control.value !== null) {
            return control.value.trim() === "" ? { "required": true } : null;
        }
        return { "required": true };
    }

    public static compareETA_ETD(controls: AbstractControl | FormControl | FormGroup): ValidationErrors | any {
        const eta: any = controls.get('eta').value;
        const etd: any = controls.get('etd').value;

        if (!eta || !etd || !eta.startDate || !etd.startDate) {
            return null;
        }
        const etaTime = new Date(eta.startDate).getTime();
        const etdTime = new Date(etd.startDate).getTime();

        return (etaTime >= etdTime) ? null : {
            validateEta_Etd: true
        };
    }

    public static validateMAWB(controls: AbstractControl | FormControl | FormGroup): ValidationErrors {
        if (controls.valid && controls.value) {
            const mawbNo: string = controls.value;
            const mawbNumber: number = +(mawbNo.replace(/\s+/g, '').substring(4, mawbNo.length - 1));
            const checkDigit: number = +mawbNo.slice(-1);
            if ((mawbNumber % 7) !== checkDigit) {
                return { invalidMawb: true };
            }
            return null;

        }
        return null;
    }

    public static validateSpecialChar(controls: AbstractControl | FormControl | FormGroup): ValidationErrors {
        if (controls.valid && controls.value) {
            const billNo: string = controls.value;
            if (billNo.match(/['\\":*<>;?]/m)) {
                return { invalidSpecial: false };
            }
            return null;

        }
        return null;
    }

    // public static getDateWithoutTime(date: any) {
    //     let year = date.getFullYear();
    //     let month = date.getMonth();
    //     let day = date.getDate();
    //     return new Date(year, month, day);
    // }

    public static validateNotFutureDate(controls: AbstractControl | FormControl): ValidationErrors {
        if (controls.value?.startDate !== null && controls.value !== null) {
            const inputDate: any = new Date(controls.value.startDate.getFullYear(), controls.value.startDate.getMonth(), controls.value.startDate.getDate());
            const currentDate: any = new Date().setHours(0, 0, 0, 0);
            if (inputDate > currentDate) {
                return { invalidDateFuture: true };
            }
        }
        return null;
    }

    // TODO Custom validator Fn here !
}
