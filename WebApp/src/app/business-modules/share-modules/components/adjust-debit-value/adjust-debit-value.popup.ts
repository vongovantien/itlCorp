import { Component, Output, EventEmitter, Input } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { ToastrService } from "ngx-toastr";
import { FormGroup, FormBuilder, AbstractControl } from "@angular/forms";
import { NgProgress } from "@ngx-progressbar/core";
import { AdjustModel } from "src/app/shared/models/accouting/adjust-soa.model";
import { AccountingRepo, DocumentationRepo } from "@repositories";
import { catchError, finalize } from "rxjs/operators";

@Component({
    selector: 'adjust-debit-value-popup',
    templateUrl: './adjust-debit-value.popup.html'
})
export class ShareBussinessAdjustDebitValuePopupComponent extends PopupBase {
    @Output() onSave: EventEmitter<any> = new EventEmitter<any>();
    soano: string = null;
    action: string = null;
    cdNote: string = null;
    jodId: string = null;
    form: FormGroup;
    billingDate: AbstractControl;
    data: AdjustModel = new AdjustModel();
    headers = [
        { title: 'No.', field: 'i', sortable: false, },
        { title: 'Charge Code', field: 'chargeCode', sortable: false },
        { title: 'Charge Name', field: 'chargeName', sortable: false },
        { title: 'Org Net', field: 'netAmount', sortable: false },
        { title: 'VAT', field: 'vatrate', sortable: false },
        { title: 'Org Amount', field: 'total', sortable: false },
        { title: 'Net Debit (VND)', field: 'amountVND', width: 170, sortable: false },
        { title: 'VAT (VND)', field: 'vatAmountVND', width: 170, sortable: false },
        { title: 'Adjusted VND', field: '', sortable: false },
        { title: 'Total Amount VND', field: '', sortable: false },
        { title: 'Exc Rate', field: 'exchangeRate', sortable: false },
        { title: 'Note', field: 'notes', width: 170, sortable: false },
    ];
    sumTotalObj = {
        totalOrgVND: 0,
        totalOrgUSD: 0,
        totalNetDebit: 0,
        totalVat: 0,
        totalAdjusted: 0,
    };
    constructor(
        private _toastService: ToastrService,
        private _fb: FormBuilder,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo,
        private _accoutingRepo: AccountingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {

    }
    save() {
        if (this.checkValid()) {
            this._progressRef.start();
            this.data.action = this.action;
            this._accoutingRepo.updateAdjustDebitValue(this.data)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete()),
                )
                .subscribe(
                    (res: any) => {
                        if (res.success) {
                            this._toastService.success("Data has updated Success");
                            this.hide();
                            this.onSave.emit();
                        } else {
                            this._toastService.error(res.message);
                        }
                    },
                );
        }
    }
    active() {
        this._progressRef.start();
        this._accoutingRepo.getAdjustDebitValue(this.getModelSearch())
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            )
            .subscribe(
                (res: any) => {
                    this.data = res;
                    this.show();
                },
            );
    }
    onChangeNetDebit(indexChargeGrp, indexCharge, e) {
        if (e !== "undefined") {
            this.data.listChargeGrp[indexChargeGrp].listCharges[indexCharge].amountVND = e;
            this.updateToal();
        }
    }
    onChangeNetVat(indexChargeGrp, indexCharge, e) {
        if (e !== "undefined") {
            this.data.listChargeGrp[indexChargeGrp].listCharges[indexCharge].vatAmountVND = e;
            this.updateToal();

        }
    }
    onChangeNote(indexChargeGrp, indexCharge, e) {
        if (e !== "undefined") {
            this.data.listChargeGrp[indexChargeGrp].listCharges[indexCharge].notes = e;
        }
    }
    updateToal() {
        this.data.totalVND = 0;
        for (let i = 0; i < this.data.listChargeGrp.length; i++) {
            let el = this.data.listChargeGrp[i];
            el.totalNetDebit = 0;
            el.totalVat = 0;
            el.totalAdjustedVND = 0;
            for (let j = 0; j < this.data.listChargeGrp[i].listCharges.length; j++) {
                let el2 = this.data.listChargeGrp[i].listCharges[j];
                el.totalNetDebit += el2.amountVND;
                el.totalVat += el2.vatAmountVND;
                el.totalAdjustedVND += (el2.amountVND + el2.vatAmountVND)
            }
            this.data.totalVND += el.totalAdjustedVND;
        }
    }
    checkValid() {
        for (let i = 0; i < this.data.listChargeGrp.length; i++) {
            let el = this.data.listChargeGrp[i];
            for (let j = 0; j < this.data.listChargeGrp[i].listCharges.length; j++) {
                let el2 = this.data.listChargeGrp[i].listCharges[j];
                let total = el2.adjustedVND - (el2.amountVND + el2.vatAmountVND);
                if (total > 10000 || total < -10000) {
                    this._toastService.warning(`${el2.chargeCode} cannot enter too 10.000`);
                    return false;
                }
            }
        }
        return true;
    }
    closePopup() {
        this.hide();
        this.data = new AdjustModel();
    }

    getModelSearch() {
        var modelSearch = new AdjustModel();
        if (this.action == "SOA") {
            modelSearch.code = this.soano;
        } else if (this.action == "CDNOTE") {
            modelSearch.jodId = this.jodId;
            modelSearch.code = this.cdNote ?? "";
        }
        modelSearch.action = this.action;
        return modelSearch
    }
}
