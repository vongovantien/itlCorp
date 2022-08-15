import { Currency } from './../../../../shared/models/catalogue/catCurrency.model';
import { Component, OnInit, Input, ViewChild, EventEmitter, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AppForm } from '@app';
import { ConfirmPopupComponent } from '@common';
import { Contract, Partner } from '@models';
import { AccountingRepo, CatalogueRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-payable',
    templateUrl: './payable.component.html'
})
export class PayableComponent extends AppForm {
    @ViewChild('confirmDelete') confirmDeletePopup: ConfirmPopupComponent;
    @Output() savePayable: EventEmitter<any> = new EventEmitter<any>();

    currencys: CommonInterface.INg2Select[] = [
        { id: 'VND', text: 'VND' },
        { id: 'USD', text: 'USD' },
    ];
    partnerId: string = "";
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
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        console.log(this.partnerId);

    }

    onSavePayable() {
        this.savePayable.emit({
            paymentTerm: this.payableForm.get('paymentTerm').value,
            currency: this.currency.value.id === undefined ? "VND" : this.currency.value.id,
        });

        console.log(this.currency.value.id);
    }

    getGeneralPayable(partnerId: string, currency: string) {
        this._accountingRepo.getGeneralPayable(partnerId, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    console.log(res);
                    res.currency = res.currency === null ? 'VND' : res.currency;
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
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.fileList = null;
                        this._toastService.success("Upload file successfully!");
                        this.getFileContract(this.partnerId);
                    }
                }
            );
    }

    getFileContract(partnertId: string) {
        this.isLoading = true;
        this._systemFileManageRepo.getContractPayableFilesAttach(partnertId).
            pipe(catchError(this.catchError), finalize(() => {
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
                        this.getFileContract(this.partnerId);
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

}
