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
declare var $: any;

@Component({
    selector: 'app-ops-module-credit-debit-note-edit',
    templateUrl: './ops-module-credit-debit-note-edit.component.html'
})
export class OpsModuleCreditDebitNoteEditComponent extends PopupBase implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild(OpsModuleCreditDebitNoteRemainingChargeComponent, { static: false }) popupAddCharge: OpsModuleCreditDebitNoteRemainingChargeComponent;
    listChargeOfPartner: any[] = [];
    listRemainingCharges: any[] = [];
    constListChargeOfPartner: any[] = [];
    isDisplay: boolean = true;
    EditingCDNote: AcctCDNote = new AcctCDNote();
    STORAGE_DATA: any = null;
    currentHbID: string = null;
    checkAllCharge: boolean = false;

    totalCredit: number = 0;
    totalDebit: number = 0;
    @Input() cdNoteDetails: any = null;
    @Output() isCloseModal = new EventEmitter<any>();
    subscribe: Subject<any> = new Subject();
    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
    ) {
        super();
    }

    ngOnInit() {
        this.StateChecking();

    }
    ngAfterViewInit() {
        console.log(this.cdNoteDetails);
    }

    async getListCharges(partnerId: String) {
        const listCharges = [];
        this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?Id=" + this.currentHbID + "&partnerID=" + partnerId + "&IsHouseBillId=true");
        this.listChargeOfPartner = map(this.listChargeOfPartner, function (o) {
            for (let i = 0; i < o.listCharges.length; i++) {
                if (o.listCharges[i].cdno != null) {
                    o.listCharges[i].isSelected = true;
                    o.listCharges[i].isRemaining = false;
                } else {
                    o.listCharges[i].isRemaining = true;
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
            if (c["isSelected"]) {
                if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.payerId)) {
                    // calculate total credit
                    this.totalCredit += (c.total * c.exchangeRate);
                }
                if (c.type === "SELL" || (c.type === "OBH" && this.EditingCDNote.partnerId === c.receiverId)) {
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
    }

    addChargeToSOA(event) {
        console.log(event);
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
        this.EditingCDNote.currencyId = "USD"; // in the future , this id must be local currency of each country
        this.EditingCDNote.listShipmentSurcharge = filter(this.EditingCDNote.listShipmentSurcharge, function (o: any) {
            return !o.isRemaining && o.isSelected;
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

    viewDetailCDNote() {
        // $('#ops-credit-debit-note-edit-modal').modal('hide');
        //$('#ops-credit-debit-note-detail-modal').modal('show');
        this.hide();
    }
    StateChecking() {
        this.subscribe = <any>this.baseServices.dataStorage.subscribe(data => {

            this.STORAGE_DATA = data;
            if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
                this.currentHbID = this.STORAGE_DATA.CurrentOpsTransaction.hblid;
            }
            if (this.STORAGE_DATA.CDNoteDetails != null) {
                this.EditingCDNote = this.STORAGE_DATA.CDNoteDetails.cdNote;
                this.EditingCDNote.partnerName = this.STORAGE_DATA.CDNoteDetails.partnerNameEn;
                this.EditingCDNote.listShipmentSurcharge = this.STORAGE_DATA.CDNoteDetails.listSurcharges;
                this.getListCharges(this.EditingCDNote.partnerId);
            }
            if (this.STORAGE_DATA.listChargeOfPartner !== undefined) {
                this.listChargeOfPartner = cloneDeep(this.STORAGE_DATA.listChargeOfPartner);
                this.constListChargeOfPartner = cloneDeep(this.STORAGE_DATA.listChargeOfPartner);
            }
        });
    }

    ngOnDestroy(): void {
        this.subscribe.unsubscribe();
    }

    closeModal() {
        this.isCloseModal.emit(true);
        this.hide();
    }
}
