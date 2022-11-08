import { Component, EventEmitter, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Contract } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemRepo } from '@repositories';
import _merge from 'lodash/merge';
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
            email: [null, Validators.compose([
                Validators.maxLength(500)
            ])]
        });
        this.salesman = this.formGroup.controls['salesman'];
        this.email = this.formGroup.controls['email'];
    }

    getFormData() {
        this.isSubmitted = true;
        if (this.formGroup.valid) {
            const formBody = this.formGroup.getRawValue();
            delete formBody['salesman']
            console.log(formBody)
            const cloneObject = {
                partnerId: !!formBody ? formBody.partnerId : null,
                email: !!formBody.email ? formBody.email : null
            };
            const mergeObj = Object.assign(_merge(formBody, cloneObject));
            return mergeObj;
        }
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
        const mergeObj = this.getFormData();
        if (this.formGroup.valid) {
            this._catalogueRepo.updateEmailContract(this.selectedContract.id, mergeObj)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.success) {
                            this._toastService.success('Updated email contract successfully!');
                            this.onRequest.emit(true)
                            this.hide();
                        } else {
                            this._toastService.error("Opps", "Something getting error!");
                        }
                    }
                );
        }
    }

    close() {
        this.hide();
    }
}
