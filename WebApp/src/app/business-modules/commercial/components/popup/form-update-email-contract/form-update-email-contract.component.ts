import { Component, EventEmitter, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Contract } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { FormValidators } from '@validators';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'popup-form-update-email-contract',
    templateUrl: './form-update-email-contract.component.html',
})
export class FormUpdateEmailContractComponent extends PopupBase {

    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    salesman: AbstractControl;
    email: AbstractControl;

    selectedContract: Contract;
    partnerId: string = '';
    id: string = '';
    isUpdated: Boolean = false;
    indexDetailEmail: number = null;

    formGroup: FormGroup;
    isUpdate: boolean = false;
    constructor(private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        private _ngProgressService: NgProgress) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            salesman: [null],
            email: ["", Validators.compose([
                Validators.maxLength(500),
                FormValidators.required
            ])]
        });
        this.salesman = this.formGroup.controls['salesman'];
        this.email = this.formGroup.controls['email'];
    }

    updateFormValue(data: Contract) {
        this.partnerId = data.partnerId;
        const formValue = {
            salesman: !!data ? data.username : null,
            email: !!data ? data.emailAddress : null
        };
        this.formGroup.patchValue(formValue);
    }

    onSubmit() {
        this.isSubmitted = true;
        if (this.formGroup.valid) {
            const formBody = this.formGroup.getRawValue();
            delete formBody['salesman']
            this._catalogueRepo.updateEmailContract(this.selectedContract.id, formBody.email.trim().replaceAll(/[\s,\n,;]+/g, ";"))
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.onRequest.emit(true)
                            this.formGroup.reset()
                            this.isSubmitted = false
                            this.hide();
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    close() {
        this.isSubmitted = false
        this.formGroup.reset()
        this.hide();
    }
}
