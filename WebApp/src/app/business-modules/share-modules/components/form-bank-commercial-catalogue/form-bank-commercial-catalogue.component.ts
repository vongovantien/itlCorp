import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormGroupDirective } from '@angular/forms';
import { Bank } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { GetCatalogueBankAction, getCatalogueBankState } from '@store';
import { FormValidators } from '@validators';
import _cloneDeep from 'lodash/cloneDeep';
import _merge from 'lodash/merge';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { PartnerBank } from 'src/app/shared/models/catalogue/catPartnerBank.model';

@Component({
    selector: 'popup-form-bank-commercial-catalogue',
    templateUrl: './form-bank-commercial-catalogue.component.html'
})
export class FormBankCommercialCatalogueComponent extends PopupBase implements OnInit {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();

    formGroup: FormGroup;
    formDirective: FormGroupDirective;
    bankAccountNo: AbstractControl;
    bankAccountName: AbstractControl;
    bankAddress: AbstractControl;
    swiftCode: AbstractControl;
    bankName: AbstractControl;
    bankCode: AbstractControl;
    note: AbstractControl;
    bankDetail: PartnerBank;
    banks: Observable<Bank[]>;
    partnerId: string = '';
    partnerBankId: string = '';
    bankId: any = null;
    isUpdate: boolean = false;
    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Bank Code' },
        { field: 'bankNameEn', label: 'Bank Name EN' },
    ];

    constructor(private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _store: Store<any>) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this._store.dispatch(new GetCatalogueBankAction());
        this.banks = this._store.select(getCatalogueBankState);
    }

    initForm() {
        this.formGroup = this._fb.group({
            bankAccountNo: [null, FormValidators.required],
            bankAccountName: [null, FormValidators.required],
            bankAddress: [null, FormValidators.required],
            bankName: [null, FormValidators.required],
            bankCode: [{ value: null, disabled: true }, FormValidators.required],
            swiftCode: [null],
            note: [null],
        });

        this.bankAccountNo = this.formGroup.controls['bankAccountNo'];
        this.bankAccountName = this.formGroup.controls['bankAccountName'];
        this.bankAddress = this.formGroup.controls['bankAddress'];
        this.swiftCode = this.formGroup.controls['swiftCode'];
        this.bankName = this.formGroup.controls['bankName'];
        this.bankCode = this.formGroup.controls['bankCode'];
        this.note = this.formGroup.controls['note'];
    }

    getFormData() {
        this.isSubmitted = true;
        if (this.formGroup.valid) {
            const formBody = this.formGroup.getRawValue();
            const cloneObject = {
                bankId: !!this.bankId ? this.bankId : null,
                bankAccountNo: !!formBody.bankAccountNo ? formBody.bankAccountNo : null,
                bankAccountName: !!formBody.bankAccountName ? formBody.bankAccountName : null,
                bankAddress: !!formBody.bankAddress ? formBody.bankAddress : null,
                swiftCode: !!formBody.swiftCode ? formBody.swiftCode : null,
                code: !!formBody.bankCode ? formBody.bankCode : null,
            };
            const mergeObj = Object.assign(_merge(formBody, cloneObject));
            mergeObj.partnerId = this.partnerId;
            return mergeObj;
        }
    }

    onSelectDataFormInfo(data: any) {
        if (data) {
            this.bankName.setValue(data.bankNameEn);
            this.bankCode.setValue(data.code);
            this.bankId = data.id;
        }

    }

    getDetailPartnerBank(id: string) {
        this._catalogueRepo.getDetailPartnerBank(id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: PartnerBank) => {
                    if (!!res) {
                        this.updateFormValue(res);
                        this.partnerBankId = res.id;
                    }
                }
            );
    }

    updateFormValue(data: PartnerBank) {
        this.bankDetail = data
        const formValue = {
            bankId: !!data.bankId ? data.bankId : null,
            bankAccountNo: !!data.bankAccountNo ? data.bankAccountNo : null,
            bankAccountName: !!data.bankAccountName ? data.bankAccountName : null,
            bankAddress: !!data.bankAddress ? data.bankAddress : null,
            swiftCode: !!data.swiftCode ? data.swiftCode : null,
            bankName: !!data.bankName ? data.bankName : null,
            bankCode: !!data.bankCode ? data.code : null,
            source: !!data.source ? data.source : null,
            note: !!data.note ? data.note : null,
        };
        this.formGroup.patchValue(_merge(_cloneDeep(data), formValue));
    }

    onSubmit() {
        const mergeObj = this.getFormData();
        if (this.formGroup.valid) {
            if (!this.isUpdate) {
                this._catalogueRepo.addNewPartnerBank(mergeObj)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                this.onRequest.emit(true);
                                this.formGroup.reset();
                                this.isSubmitted = false;
                                this.hide();
                            }
                        }
                    );
            } else {
                mergeObj.id = this.partnerBankId;
                this._catalogueRepo.updatePartnerBank(mergeObj)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                this.onRequest.emit(true);
                                this.formGroup.reset();
                                this.isSubmitted = false;
                                this.hide();
                            }
                        }
                    );
            }
        }
    }
    resetBankName() {
        this.bankName.setValue(null)
        this.bankCode.setValue(null)
    }

    close() {
        this.hide();
        this.formGroup.reset();
        this.isSubmitted = false;
    }
}
