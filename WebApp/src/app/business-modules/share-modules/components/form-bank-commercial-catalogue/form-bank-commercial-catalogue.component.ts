import { ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormGroupDirective } from '@angular/forms';
import { Bank, Partner, SysImage } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, SystemRepo, SystemFileManageRepo } from '@repositories';
import { GetCatalogueBankAction, getCatalogueBankState } from '@store';
import { FormValidators } from '@validators';
import _cloneDeep from 'lodash/cloneDeep';
import _merge from 'lodash/merge';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ConfirmPopupComponent } from '@common';
import { CatalogueConstants, SystemConstants } from '@constants';
import { IEDocFile, IEDocUploadFile } from 'src/app/business-modules/share-business/components/edoc/document-type-attach/document-type-attach.component';

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
    bankNameEn: AbstractControl;
    bankCode: AbstractControl;
    note: AbstractControl;
    beneficiaryAddress: AbstractControl;
    approveStatus: AbstractControl;

    bankDetail: Bank = null;

    id: string = '';
    banks: Observable<Bank[]>;
    partnerId: string = '';
    bankId: any = null;
    isUpdate: boolean = false;
    files: any = [];
    fileListUpload: any[] = [];
    fileListDetail: any[] = [];
    EdocUploadFile: IEDocUploadFile;

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

    initForm() {
        this.formGroup = this._fb.group({
            bankAccountNo: [null, FormValidators.required],
            bankAccountName: [null, FormValidators.required],
            bankAddress: [null, FormValidators.required],
            bankNameEn: [null, FormValidators.required],
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
        this.bankNameEn = this.formGroup.controls['bankNameEn'];
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
            approveStatus: !!data.approveStatus ? data.approveStatus : null,
            beneficiaryAddress: !!data.beneficiaryAddress ? data.beneficiaryAddress : null,
            source: !!data.source ? data.source : null,
            note: !!data.note ? data.note : null,
        };
        this.formGroup.patchValue(_merge(_cloneDeep(data), formValue));
        this.getBankInfoFiles(this.bankDetail.id);
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
                this.bankDetail = null;
                this.formGroup.reset()
            }
            else {
                this.onRequest.emit(mergeObj);
            }
        }
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

    onSendBankInfoToAccountantSystem(): void {
        if (this.formGroup.valid) {
            this.isSubmitted = true;
            const action = !!this.bankDetail && this.bankDetail.approveStatus !== CatalogueConstants.STATUS_APPROVAL.NEW ? 'UPDATE' : 'ADD';
            this._catalogueRepo.syncBankInfoToAccountantSystem(this.bankDetail.id, action)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: any) => {
                        if (!!res && res.status) {
                            this._toastService.success(res.message);
                            this.onRequest.emit(true);
                            this.hide();
                        } else {
                            this._toastService.error(res.message);
                        }
                    },
                    (error) => {
                        console.error(error);
                    }
                )
        }
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
        this.isSubmitted = false;
        this.bankDetail = null;
        this.fileListDetail = null;
    }

    getBankInfoFiles(id: string) {
        this.isLoading = true;
        this._systemFileManagementRepo.getFile('Category', 'PartnerBank', id).
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
        const files = event.target['files'];
        for (let i = 0; i < files.length; i++) {
            files[i].docType = "Partner_Bank";
            files[i].aliasName = "Partner_Bank" + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
            this.fileListUpload.push(files[i]);
        }

        //Validate file size
        if (!!files && files.length > 0) {
            let validSize: boolean = true;
            for (let i = 0; i <= files?.length - 1; i++) {
                const fileSize: number = files[i].size / Math.pow(1024, 2);
                if (fileSize >= SystemConstants.MAX_FILE_SIZE) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toastService.warning(`maximum file size < ${SystemConstants.MAX_FILE_SIZE}MB`);
                return;
            }
            this.handleBankInfoFileUpload(files)
        }
        event.target.value = ''
    }

    handleBankInfoFileUpload(files: any[]) {
        const listFile = [];
        const edocFileList = [];

        this.fileListUpload.forEach(x => {
            listFile.push(x);
            edocFileList.push(({
                JobId: SystemConstants.EMPTY_GUID,
                Code: "Partner_Bank",
                TransactionType: null,
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: SystemConstants.EMPTY_GUID,
                HBL: SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: x.note !== undefined ? x.note : '',
                BillingId: SystemConstants.EMPTY_GUID,
                Id: x.id !== undefined ? x.id : SystemConstants.EMPTY_GUID,
                DocumentId: 0,
                AccountingType: x.AccountingType,
            }));
        });
        this.EdocUploadFile = ({
            ModuleName: "Catalogue",
            FolderName: "PartnerBank",
            Id: this.bankDetail.id,
            EDocFiles: edocFileList,
        })
        this._progressRef.start();
        this._systemFileManagementRepo.uploadEDoc(this.EdocUploadFile, files, "Catalogue")
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success("Upload file successfully!");
                        this.getBankInfoFiles(this.bankDetail.id);
                        this.fileListUpload = [];
                    }
                }
            );
    }

    onDeleteBankInfoFile(file: any) {
        if (!!file) {
            this.selectedFile = file;
            console.log(file)
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
}
