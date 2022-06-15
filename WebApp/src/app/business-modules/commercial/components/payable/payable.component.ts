import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AppForm } from '@app';
import { ConfirmPopupComponent } from '@common';
import { Contract, Partner } from '@models';
import { AccountingRepo, CatalogueRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { catchError, concatMap, finalize } from 'rxjs/operators';
import { AccountingPayableModel } from 'src/app/shared/models/accouting/accounting-payable';

@Component({
    selector: 'app-payable',
    templateUrl: './payable.component.html'
})
export class PayableComponent extends AppForm {
    @ViewChild('confirmDelete') confirmDeletePopup: ConfirmPopupComponent;
    @Input() partnerId: string = "";
    currencys: CommonInterface.INg2Select[] = [
        { id: 'VND', text: 'VND' },
        { id: 'USD', text: 'USD' },
    ];
    fileList: any = null;
    files: any = {};
    selectedFile: any = {};
    paymentTerm: AbstractControl;
    currency: AbstractControl;
    payableForm: FormGroup;
    creditAmount: number = 0;
    creditAdvanceAmount: number = 0;
    creditPaidAmount: number = 0;
    creditUnpaidAmount: number = 0;
    paymentTermValue: number = 0;
    constructor(
        private _systemFileManageRepo: SystemFileManageRepo,
        protected _toastService: ToastrService,
        private _fb: FormBuilder,
        private _accountingRepo: AccountingRepo,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.getFileContract();
        this.getGeneralPayable();
    }

    savePayable() {
        this._accountingRepo.updatePayable(this.partnerId, this.payableForm.get('paymentTerm').value, this.currency.value.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this._toastService.success('Saved Payable Success!');
                },
            );
    }

    getGeneralPayable() {
        this._accountingRepo.getGeneralPayable(this.partnerId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.creditAmount = res.creditAmount;
                    this.creditAdvanceAmount = res.creditAdvanceAmount;
                    this.creditPaidAmount = res.creditPaidAmount;
                    this.creditUnpaidAmount = res.creditUnpaidAmount;
                    this.payableForm.patchValue({
                        paymentTerm: res.paymentTerm,
                        currency: res.currency
                    })
                    this.currency.setValue(res.currency);
                },
            );
    }

    handleFileInput(event: any) {
        this.fileList = event.target['files'];
        this.uploadFileContract(this.partnerId);
    }

    initForm() {
        this.payableForm = this._fb.group({
            paymentTerm: this.paymentTermValue,
            currency: ['VND'],
        });
        this.paymentTerm = this.payableForm.controls['paymentTerm'];
        this.currency = this.payableForm.controls['currency'];

    }

    uploadFileContract(id: string) {
        this._systemFileManageRepo.uploadFileContractPayable(id, this.fileList)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.fileList = null;
                        this._toastService.success("Upload file successfully!");
                        this.getFileContract();
                    }
                }
            );
    }

    getFileContract() {
        this.isLoading = true;
        this._systemFileManageRepo.getContractPayableFilesAttach(this.partnerId).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.files = res;
                }
            );

    }

    deleteFile(file: any) {
        if (!!file) {
            this.selectedFile = file;
        }
        this.confirmDeletePopup.show();
    }

    onDeleteFile() {
        this.confirmDeletePopup.hide();
        this._systemFileManageRepo.deleteContractPayableFilesAttach(this.partnerId, this.selectedFile.name)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success("File deleted successfully!");
                        this.getFileContract();
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

}
