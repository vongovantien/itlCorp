import { AppPage } from './app.base';
import { AbstractControl, FormControl, FormGroup} from '@angular/forms';
import moment from "moment";

export abstract class AppForm extends AppPage {

    requestSearch: any = null;
    requestReset: any = null;
    isDisabled: boolean = null;

    ranges: any = {
        // "Today": [moment(), moment()],
        // Yesterday: [moment().subtract(1, "days"), moment().subtract(1, "days")],
        // "Last 7 Days": [moment().subtract(6, "days"), moment()],
        // "Last 30 Days": [moment().subtract(29, "days"), moment()],
        // "This Month": [moment().startOf("month"), moment().endOf("month")],
        // "Last Month": [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")]
        "Today": [new Date(), new Date()],
        "Yesterday": [new Date(new Date().setDate(new Date().getDate() - 1)), new Date(new Date().setDate(new Date().getDate() - 1))],
        "Last 7 Days": [new Date(new Date().setDate(new Date().getDate() - 6)), new Date()],
        "Last 30 Days": [new Date(new Date().setDate(new Date().getDate() - 29)), new Date()],
        "This Month": [new Date(new Date().getFullYear(), new Date().getMonth(), 1), new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)],
        "Last Month": [new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1), new Date(new Date().getFullYear(), new Date().getMonth(), 0)]
    };

    maxDate: any = moment();
    minDate: any = moment();

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

    search() {
        this.requestSearch();
    }

    reset() {
        this.requestReset();
    }

}
