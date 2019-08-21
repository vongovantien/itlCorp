import { Component, OnInit, Input, Output, EventEmitter, OnDestroy, AfterViewInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat';
import cloneDeep from 'lodash/cloneDeep';
import { AcctCDNote } from 'src/app/shared/models/document/acctCDNote.model';
import { NgForm } from '@angular/forms';
import { Subject } from 'rxjs/internal/Subject';
import { PopupBase } from 'src/app/popup.base';
import { OpsModuleCreditDebitNoteRemainingChargeComponent } from '../ops-module-credit-debit-note-remaining-charge/ops-module-credit-debit-note-remaining-charge.component';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { SortService } from 'src/app/shared/services';
declare var $: any;

@Component({
    selector: 'app-ops-module-credit-debit-note-edit',
    templateUrl: './ops-module-credit-debit-note-edit.component.html'
})
export class OpsModuleCreditDebitNoteEditComponent extends PopupBase implements OnInit, OnDestroy {
    @ViewChild(OpsModuleCreditDebitNoteRemainingChargeComponent, { static: false }) popupAddCharge: OpsModuleCreditDebitNoteRemainingChargeComponent;
    listChargeOfPartner: any[] = [];
    constListChargeOfPartner: any[] = [];
    isDisplay: boolean = true;
    EditingCDNote: AcctCDNote = new AcctCDNote();
    checkAllCharge: boolean = false;
    currentCDNo: String = '';
    currentJob: OpsTransaction;

    totalCredit: number = 0;
    totalDebit: number = 0;
    @Output() isCloseModal = new EventEmitter<any>();
    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService
    ) {
        super();
    }

    ngOnInit() {
    }

    async getListCharges(partnerId: String) {
        const listCharges = [];
        const cdNo = this.currentCDNo;
        this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?Id=" + this.currentJob.hblid + "&partnerID=" + partnerId + "&IsHouseBillId=true");
        this.listChargeOfPartner = map(this.listChargeOfPartner, function (o) {
            for (let i = 0; i < o.listCharges.length; i++) {
                if (o.listCharges[i].debitNo === null && o.listCharges[i].creditNo === null) {
                    o.listCharges[i].isRemaining = true;
                }
                // else {
                //     if (o.listCharges[i].debitNo === cdNo || o.listCharges[i].creditNo === cdNo) {
                //         o.listCharges[i].isSelected = false;
                //         o.listCharges[i].isRemaining = false;
                //     }
                // }
                else if (o.listCharges[i].type === "OBH") {
                    if ((o.listCharges[i].payerId === partnerId && o.listCharges[i].creditNo !== null)
                        || (o.listCharges[i].paymentObjectId === partnerId && o.listCharges[i].debitNo !== null)) {
                        o.listCharges[i].isRemaining = false;
                    }
                }
                else {
                    o.listCharges[i].isRemaining = false;
                }
                listCharges.push(o.listCharges[i]);
            }
            return o;
        });
        this.listChargeOfPartner[0].listCharges = listCharges;
        this.totalCreditDebitCalculate();
        this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
    }
    totalCreditDebitCalculate(): number {
        this.totalCredit = 0;
        this.totalDebit = 0;
        for (let i = 0; i < this.listChargeOfPartner[0].listCharges.length; i++) {
            const c = this.listChargeOfPartner[0].listCharges[i];
            // if (c["isSelected"]) {
            //     if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.payerId)) {
            //         // calculate total credit
            //         this.totalCredit += (c.total * c.exchangeRate);
            //     }
            //     if (c.type === "SELL" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.receiverId)) {
            //         // calculate total debit 
            //         this.totalDebit += (c.total * c.exchangeRate);
            //     }
            // }
            if (!c["isRemaining"]) {
                if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.payerId)) {
                    // calculate total credit
                    this.totalCredit += (c.total * c.exchangeRate);
                }
                if (c.type === "SELL" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.paymentObjectId)) {
                    // calculate total debit 
                    this.totalDebit += (c.total * c.exchangeRate);
                }
            }

        }
        return this.totalDebit - this.totalCredit;

    }
    removeAllChargeSelected() {
        this.checkAllCharge = false;
        this.totalCreditDebitCalculate();
    }
    checkAllChange() {
        if (this.listChargeOfPartner[0].listCharges !== null) {
            if (this.checkAllCharge) {

                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    element.isSelected = true;
                });
            } else {
                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    element.isSelected = false;
                });
            }
        }
        this.totalCreditDebitCalculate();
    }
    removeSelectedCharges() {
        this.checkAllCharge = false;
        if (this.listChargeOfPartner.length > 0) {
            if (this.listChargeOfPartner[0].listCharges != null) {
                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    // element.isSelected = false;
                    // if (element.isRemaining === false) {
                    //     element.isRemaining = true;
                    // }
                    if (element.isSelected === true) {
                        element.isRemaining = true;
                        element.isSelected = false;
                    }
                });
                // this.totalCredit = 0;
                // this.totalDebit = 0;
            }
        }
        this.totalCreditDebitCalculate();
    }

    addChargeToCDNote(event) {
        if (event != null) {
            const listNewCharges = event;
            listNewCharges.forEach(x => {
                if (this.listChargeOfPartner[0].listCharges.filter(x => x.id === x.id).length > 0) {
                    x.isSelected = false;
                }
            });
        }

        this.totalCreditDebitCalculate();
    }
    addMoreChargeToCDNote() {
        this.popupAddCharge.listChargeOfPartner = this.popupAddCharge.constListChargeOfPartner = this.listChargeOfPartner;
        this.popupAddCharge.show({ backdrop: 'static' });
    }
    setChargesForCDNote() {
        this.EditingCDNote.listShipmentSurcharge = [];
        this.listChargeOfPartner.forEach(element => {
            this.EditingCDNote.listShipmentSurcharge = concat(this.EditingCDNote.listShipmentSurcharge, element.listCharges);
        });
    }

    async UpdateCDNote(form: NgForm) {
        this.setChargesForCDNote();
        this.EditingCDNote.total = this.totalDebit - this.totalCredit;
        this.EditingCDNote.jobId = this.currentJob.id;
        this.EditingCDNote.currencyId = "USD"; // in the future , this id must be local currency of each country
        this.EditingCDNote.listShipmentSurcharge = filter(this.EditingCDNote.listShipmentSurcharge, function (o: any) {
            // return !o.isRemaining && o.isSelected;
            return !o.isRemaining;
        });
        console.log(this.EditingCDNote);
        const res = await this.baseServices.putAsync(this.api_menu.Documentation.AcctSOA.update, this.EditingCDNote);
        if (res.status) {
            this.EditingCDNote = new AcctCDNote();
            this.closeModal();
        }
    }

    resetAddSOAForm() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 300);
    }


    SearchCharge(search_key: string) {
        // listChargeOfPartner
        this.listChargeOfPartner = cloneDeep(this.constListChargeOfPartner);
        search_key = search_key.trim().toLowerCase();
        const listBranch: any[] = [];
        this.listChargeOfPartner = filter(cloneDeep(this.constListChargeOfPartner), function (x: any) {
            let root = false;
            let branch = false;
            if (x.hwbno == null ? "" : x.hwbno.toLowerCase().includes(search_key)) {
                root = true;
            }
            const listCharges: any[] = [];
            for (let i = 0; i < x.listCharges.length; i++) {
                if (x.listCharges[i].chargeCode.toLowerCase().includes(search_key) ||
                    x.listCharges[i].quantity.toString().toLowerCase() === search_key ||
                    x.listCharges[i].nameEn.toString().toLowerCase().includes(search_key) ||
                    x.listCharges[i].chargeCode.toLowerCase().includes(search_key) ||
                    x.listCharges[i].unit.toLowerCase().includes(search_key) ||
                    x.listCharges[i].currency.toLowerCase().includes(search_key)) {
                    listCharges.push(x.listCharges[i]);
                    branch = true;
                }
            }
            if (listCharges.length > 0) {
                listBranch.push({
                    hbId: x.id,
                    list: listCharges
                });
            };

            return (root || branch);

        });

        for (let i = 0; i < this.listChargeOfPartner.length; i++) {
            for (let k = 0; k < listBranch.length; k++) {
                if (this.listChargeOfPartner[i].id === listBranch[k].hbId) {
                    this.listChargeOfPartner[i].listCharges = listBranch[k].list;
                }
            }
        }

        console.log(this.listChargeOfPartner);

    }
    closeModal() {
        this.isCloseModal.emit(true);
        this.hide();
    }
    isDesc = true;
    sortKey: string = '';
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.listChargeOfPartner[0].listCharges = this.sortService.sort(this.listChargeOfPartner[0].listCharges, property, this.isDesc);
    }
}
