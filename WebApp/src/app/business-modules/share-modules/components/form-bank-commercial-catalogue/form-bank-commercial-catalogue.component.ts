import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormGroupDirective } from '@angular/forms';
import { ConfirmPopupComponent } from '@common';
import { CatalogueConstants, SystemConstants } from '@constants';
import { Bank, SysImage } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemFileManageRepo } from '@repositories';
import { GetCatalogueBankAction, getCatalogueBankState } from '@store';
import { FormValidators } from '@validators';
import _cloneDeep from 'lodash-es/cloneDeep';
import _merge from 'lodash-es/merge';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { catchError, finalize, switchMap, takeUntil } from 'rxjs/operators';
import { IEDocUploadFile } from 'src/app/business-modules/share-business/components/edoc/document-type-attach/document-type-attach.component';
import { PopupBase } from 'src/app/popup.base';
import { PartnerBank } from 'src/app/shared/models/catalogue/catPartnerBank.model';

@Component({
    selector: 'popup-form-bank-commercial-catalogue',
    templateUrl: './form-bank-commercial-catalogue.component.html'
})
export class FormBankCommercialCatalogueComponent extends PopupBase implements OnInit {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    selectedFile: any;

    formGroup: FormGroup;
    formDirective: FormGroupDirective;

    bankAccountNo: AbstractControl;
    bankAccountName: AbstractControl;
    bankAddress: AbstractControl;
    swiftCode: AbstractControl;
    bankName: AbstractControl;
    bankCode: AbstractControl;
    note: AbstractControl;
    beneficiaryAddress: AbstractControl;
    approveStatus: AbstractControl;

    bankDetail: PartnerBank;
    banks: Observable<Bank[]>;
    partnerId: string = '';
    partnerBankId: string = '';
    bankId: string = null;
    isUpdate: boolean = false;
    fileListUpload: any[] = [];
    fileListDetail: any[] = [];
    EdocUploadFile: IEDocUploadFile;

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Bank Code' },
        { field: 'bankNameEn', label: 'Bank Name EN', },
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

    initForm() {
        this.formGroup = this._fb.group({
            bankAccountNo: [null, FormValidators.required],
            bankAccountName: [null, FormValidators.required],
            bankAddress: [null, FormValidators.required],
            bankName: [null, FormValidators.required],
            bankCode: [{ value: null, disabled: true }],
            swiftCode: [null],
            note: [null],
            bankId: [null],
            beneficiaryAddress: [null, FormValidators.required],
            approveStatus: [{ value: null, disabled: true }]
        });

        this.bankAccountNo = this.formGroup.controls['bankAccountNo'];
        this.bankAccountName = this.formGroup.controls['bankAccountName'];
        this.bankAddress = this.formGroup.controls['bankAddress'];
        this.swiftCode = this.formGroup.controls['swiftCode'];
        this.bankName = this.formGroup.controls['bankName'];
        this.bankCode = this.formGroup.controls['bankCode'];
        this.note = this.formGroup.controls['note'];
        this.beneficiaryAddress = this.formGroup.controls['beneficiaryAddress']
        this.approveStatus = this.formGroup.controls['approveStatus']
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

    resetFormControl(control: AbstractControl) {
        control.setValue(null);
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
                        this.bankId = res.bankId;
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
            approveStatus: !!data.approveStatus ? data.approveStatus : null,
            beneficiaryAddress: !!data.beneficiaryAddress ? data.beneficiaryAddress : null,
            source: !!data.source ? data.source : null,
            note: !!data.note ? data.note : null,
        };
        this.formGroup.patchValue(_merge(_cloneDeep(data), formValue));
        this.getBankInfoFiles(this.bankDetail.id);
    }

    onSubmitBankInfo() {
        this.isSubmitted = true
        if (!this.formGroup.valid) {
            return;
        }

        const formData = this.getFormData();
        const apiCall = this.isUpdate ? this._catalogueRepo.updatePartnerBank({ ...formData, id: this.partnerBankId }) : this._catalogueRepo.addNewPartnerBank(formData);
        apiCall.pipe(catchError(this.catchError)).subscribe((res: CommonInterface.IResult) => {
            if (res.status) {
                this._toastService.success(res.message);
                this.onRequest.emit(true);
                this.formGroup.reset();
                this.isSubmitted = false;
                this.hide();
            }
        });
    }

    onSendBankInfoToAccountantSystem() {
        this.isSubmitted = true;
        if (!this.formGroup.valid) {
            return;
        }

        const formData = this.getFormData();
        const apiCall = this.isUpdate
            ? this._catalogueRepo.updatePartnerBank({ ...formData, id: this.partnerBankId })
            : this._catalogueRepo.addNewPartnerBank(formData);

        apiCall.pipe(
            catchError(this.catchError),
            switchMap((res: CommonInterface.IResult) => {
                const action = this.isUpdate && formData.approveStatus !== CatalogueConstants.STATUS_APPROVAL.NEW ? 'UPDATE' : 'ADD';
                const id = this.isUpdate ? this.partnerBankId : res.data;
                console.log(id)
                return this._catalogueRepo.syncBankInfoToAccountantSystem([{ Id: id, action }]);
            })
        ).subscribe((res: CommonInterface.IResult) => {
            if (res.status) {
                this._toastService.success(res.message);
                this.onRequest.emit(true);
                this.formGroup.reset();
                this.isSubmitted = false;
                this.hide();
            } else {
                this._toastService.error(res.message);
            }
        });
    }

    resetBankName() {
        this.bankName.setValue(null)
        this.bankCode.setValue(null)
    }

    getFileName(data: CommonInterface.IResult[], id: string) {
        let url: string = '';
        data.forEach(x => {
            const smId: string[] = x.data.match(SystemConstants.CPATTERN.GUID)
            if (smId[0] === id) {
                url = x.data;
            }
        })

        return url;
    }

    onReviseBankInfo(): void {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Are you sure to revise this bank information ?',
            labelCancel: 'No',
            labelConfirm: 'Yes'
        }, () => {
            this._catalogueRepo.reviseBankInformation(this.bankDetail.id)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.bankDetail.approveStatus = CatalogueConstants.STATUS_APPROVAL.REVISE;
                            this.approveStatus.setValue(CatalogueConstants.STATUS_APPROVAL.REVISE);
                        }
                        else {
                            this._toastService.error(res.message);
                        }
                    }
                )
        });
    }

    close() {
        this.hide();
        this.formGroup.reset();
        this.isSubmitted = false;
        this.bankDetail = null;
        this.fileListDetail = null;
    }

    getBankInfoFiles(id: string) {
        this.isLoading = true;
        this._systemFileManagementRepo.getFile('Catalogue', 'PartnerBank', id).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: SysImage[] = []) => {
                    if (!!res) {
                        {
                            this.fileListDetail = res;
                            this.fileListDetail.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                        }
                    }
                });
    }

    chooseFileUpload(event: any) {
        const files: FileList = event.target.files;

        const isFileSizeValid = Array.from(files).every(file => file.size / 1024 ** 2 < SystemConstants.MAX_FILE_SIZE);
        if (!isFileSizeValid) {
            this._toastService.warning(`Maximum file size is ${SystemConstants.MAX_FILE_SIZE}MB`);
            return;
        }

        this.fileListUpload.push(...Array.from(files));
        this._systemFileManagementRepo.uploadFile('Catalogue', 'PartnerBank', this.bankDetail.id, this.fileListUpload)
            .pipe(catchError(this.catchError))
            .subscribe((res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                    this.getBankInfoFiles(this.bankDetail.id);
                    this.fileListUpload = [];
                }
            });
        event.target.value = '';
    }

    onDeleteBankInfoFile(file: any) {
        if (!!file) {
            this.selectedFile = file;
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'Are you sure to delete this file ?',
                labelCancel: 'No',
                labelConfirm: 'Yes'
            }, () => { this.handleDeleteBankInfoFile(); });
        }
    }

    handleDeleteBankInfoFile() {
        this._systemFileManagementRepo.deleteFile('Catalogue', 'PartnerBank', this.bankDetail.id, this.selectedFile.name)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success("File deleted successfully!");
                        this.getBankInfoFiles(this.bankDetail.id);
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

    onViewFileUpload(file: any) {
        const selectedFile = Object.assign({}, file);
        this._systemFileManagementRepo.getFileEdoc(selectedFile.id).subscribe(
            (data) => {
                this.downLoadFile(data, SystemConstants.FILE_EXCEL, file.name);
            }
        )
    }
}
