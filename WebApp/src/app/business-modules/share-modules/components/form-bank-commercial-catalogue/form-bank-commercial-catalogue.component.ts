import { ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormGroupDirective } from '@angular/forms';
import { Bank, Partner } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemRepo, SystemFileManageRepo } from '@repositories';
import { GetCatalogueBankAction, getCatalogueBankState } from '@store';
import { FormValidators } from '@validators';
import _cloneDeep from 'lodash/cloneDeep';
import _merge from 'lodash/merge';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { SysImage } from './../../../../shared/models/system/sysimage';

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
    bankNameEn: AbstractControl;
    bankCode: AbstractControl;
    note: AbstractControl;
    beneficiaryAddress: AbstractControl;
    approvalStatus: AbstractControl;

    bankDetail: Bank;

    id: string = '';
    banks: Observable<Bank[]>;
    partnerId: string = '';
    bankId: any = null;
    isUpdate: boolean = false;
    files: any = [];
    fileList: any = null;

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Bank Code' },
        { field: 'bankNameEn', label: 'Bank Name EN' },
    ];

    constructor(
        private _systemFileManagementRepo: SystemFileManageRepo,
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

    // ngAfterViewInit(): void {
    //     this._store.select(get)
    //         .pipe(takeUntil(this.ngUnsubscribe))
    //         .subscribe(
    //             (res: any) => {
    //                 if (!!res) {
    //                     this.setFormValue(res.opstransaction)
    //                 }
    //             }
    //         );

    //     this.cdRef.detectChanges();
    // }

    setFormValue(data: Partner) {
        //this.beneficiaryAddress.setValue()
    }

    initForm() {
        this.formGroup = this._fb.group({
            bankAccountNo: [null, FormValidators.required],
            bankAccountName: [null, FormValidators.required],
            bankAddress: [null, FormValidators.required],
            bankNameEn: [null, FormValidators.required],
            bankCode: [{ value: null, disabled: true }, FormValidators.required],
            swiftCode: [null],
            note: [null],
            bankId: [null],
            beneficiaryAddress: [null, FormValidators.required],
            approvalStatus: [{ value: null, disabled: true }]
        });

        this.bankAccountNo = this.formGroup.controls['bankAccountNo'];
        this.bankAccountName = this.formGroup.controls['bankAccountName'];
        this.bankAddress = this.formGroup.controls['bankAddress'];
        this.swiftCode = this.formGroup.controls['swiftCode'];
        this.bankNameEn = this.formGroup.controls['bankNameEn'];
        this.bankCode = this.formGroup.controls['bankCode'];
        this.note = this.formGroup.controls['note'];
        this.beneficiaryAddress = this.formGroup.controls['beneficiaryAddress']
        this.approvalStatus = this.formGroup.controls['approvalStatus']
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
                bankNameVn: !!formBody.bankNameEn ? formBody.bankNameEn : null,
                bankNameEn: !!formBody.bankNameEn ? formBody.bankNameEn : null,
                code: !!formBody.bankCode ? formBody.bankCode : null,
            };
            const mergeObj = Object.assign(_merge(formBody, cloneObject));
            mergeObj.partnerId = this.partnerId;
            return mergeObj;
        }
    }

    resetFormControl(control: AbstractControl) {
        control.setValue(null);
    }

    onSelectDataFormInfo(data: any) {
        if (data) {
            this.bankNameEn.setValue(data.bankNameEn);
            this.bankCode.setValue(data.code);
            this.bankId = data.id
        }

    }

    updateFormValue(data: Bank) {
        this.bankDetail = data
        const formValue = {
            bankId: !!data.bankId ? data.bankId : null,
            bankAccountNo: !!data.bankAccountNo ? data.bankAccountNo : null,
            bankAccountName: !!data.bankAccountName ? data.bankAccountName : null,
            bankAddress: !!data.bankAddress ? data.bankAddress : null,
            swiftCode: !!data.swiftCode ? data.swiftCode : null,
            bankNameEn: !!data.bankNameEn ? data.bankNameEn : null,
            bankCode: !!data.code ? data.code : null,
            source: !!data.source ? data.source : null,
            note: !!data.note ? data.note : null,
        };
        this.formGroup.patchValue(_merge(_cloneDeep(data), formValue));
    }

    onSubmitBankInfo() {
        const mergeObj = this.getFormData();
        if (this.formGroup.valid) {
            if (!!this.partnerId) {
                if (!this.isUpdate) {
                    this._catalogueRepo.addBank(mergeObj)
                        .pipe(catchError(this.catchError))
                        .subscribe(
                            (res: any) => {
                                if (res.status) {
                                    this._toastService.success('New data added');
                                    this.onRequest.emit(true);
                                    this.hide();
                                } else {
                                    this._toastService.error("Opps", "Something getting error!");
                                }
                            }
                        );
                } else {
                    mergeObj.id = this.id
                    this._catalogueRepo.updateBank(mergeObj)
                        .pipe(catchError(this.catchError))
                        .subscribe(
                            (res: any) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                    this.onRequest.emit(true);
                                    this.hide();

                                } else {
                                    this._toastService.error("Opps", "Something getting error!");
                                }
                            }
                        );
                }
                this.isSubmitted = false;
                this.formGroup.reset()
            }
            else {
                this.onRequest.emit(mergeObj);
            }
        }
    }

    close() {
        this.hide();
        this.isSubmitted = false;
    }

    getBankInfoFiles(jobId: string) {
        this.isLoading = true;
        this._systemFileManagementRepo.getFile('Document', 'Bank', jobId).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: SysImage[] = []) => {
                    this.files = res;
                    this.files.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                }
            );
    }

    handleBankInfoFileUpload(event: any){
        const fileList: FileList[] = event.target['files'];
        if (fileList.length > 0) {
            this._progressRef.start();
            this._systemFileManagementRepo.uploadAttachedFileEdoc('Document', 'Bank', this.bankId, fileList)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            if (!!this.bankId) {
                                this.getBankInfoFiles(this.bankId);
                            }
                        }
                    }
                );
        }
        event.target.value = '';
    }
}
