import { Component, OnInit, Output, EventEmitter, Input, OnDestroy, ViewChild } from '@angular/core';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctCDNote } from 'src/app/shared/models/document/acctCDNote.model';
import { async } from 'rxjs/internal/scheduler/async';
import { NgForm } from '@angular/forms';
import { SortService } from 'src/app/shared/services/sort.service';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PopupBase } from 'src/app/popup.base';
import { NotSelectedAlertModalComponent } from './not-selected-alert-modal/not-selected-alert-modal.component';
import { ChangePartnerConfirmModalComponent } from './change-partner-confirm-modal/change-partner-confirm-modal.component';
declare var $: any;
@Component({
    selector: 'app-ops-module-credit-debit-note-addnew',
    templateUrl: './ops-module-credit-debit-note-addnew.component.html'
})
export class OpsModuleCreditDebitNoteAddnewComponent extends PopupBase implements OnInit {
    @ViewChild(NotSelectedAlertModalComponent, { static: false }) popupNotSelectedAlert: NotSelectedAlertModalComponent;
    @ViewChild(ChangePartnerConfirmModalComponent, { static: false }) popupConfirmChangePartner: ChangePartnerConfirmModalComponent;
    @Input() currentJob: OpsTransaction = null;
    isDisplay: boolean = true;
    CDNoteWorking: AcctCDNote = new AcctCDNote();
    noteTypes = [
        { text: 'CREDIT', id: 'CREDIT' },
        { text: 'DEBIT', id: 'DEBIT' },
        { text: 'INVOICE', id: 'INVOICE' }
    ];
    currentHbID: any = null;

    listSubjectPartner: any[] = [];
    constListSubjectPartner: any[] = [];
    listChargeOfPartner: any[] = [];
    listRemainingCharges: any[] = [];
    constListChargeOfPartner: any[] = [];
    STORAGE_DATA: any = null;
    checkAllCharge: boolean = false;
    totalCredit: number = 0;
    totalDebit: number = 0;
    partnerIdNewChange: string = null;

    constructor(
        private baseServices: BaseService,
        private sortServices: SortService,
        private api_menu: API_MENU
    ) {
        super();
    }

    ngOnInit() {
        if (this.currentJob != null) {
            this.currentHbID = this.currentJob.hblid;
            this.getListSubjectPartner();
        }
    }
    getListSubjectPartner() {
        this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getPartners + "?Id=" + this.currentHbID + "&IsHouseBillID=true").subscribe((data: any[]) => {
            this.listSubjectPartner = cloneDeep(data);
            this.constListSubjectPartner = cloneDeep(data);
        });
    }
    confirmYes(partnerId: string) {
        this.partnerIdNewChange = partnerId;
        this.popupConfirmChangePartner.show();
    }
    async getListCharges(partnerId: String) {
        if (this.currentHbID !== null && partnerId != null) {
            this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?Id=" + this.currentHbID + "&partnerID=" + partnerId + "&IsHouseBillId=true");
            this.CDNoteWorking.listShipmentSurcharge = [];
            this.listChargeOfPartner = map(this.listChargeOfPartner, function (o) {
                for (let i = 0; i < o.listCharges.length; i++) {
                    o.listCharges[i].isSelected = false;
                }
                return o;
            });
            console.log(this.listChargeOfPartner);
            this.listChargeOfPartner = this.sortServices.sort(this.listChargeOfPartner, "chargeCode", true);
            this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
            console.log(this.listChargeOfPartner);
            this.setChargesForCDNote();
            this.totalDebit = 0;
            this.totalCredit = 0;
            this.checkAllCharge = false;
        }

    }
    totalCreditDebitCalculate(): number {
        this.totalCredit = 0;
        this.totalDebit = 0;
        for (let i = 0; i < this.CDNoteWorking.listShipmentSurcharge.length; i++) {
            const c = this.CDNoteWorking.listShipmentSurcharge[i];
            if (c["isSelected"]) {
                if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteWorking.partnerId === c.payerId)) {
                    // calculate total credit
                    this.totalCredit += (c.total * c.exchangeRate);
                }
                if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteWorking.partnerId === c.receiverId)) {
                    // calculate total debit 
                    this.totalDebit += (c.total * c.exchangeRate);
                }
            }

        }
        return this.totalDebit - this.totalCredit;

    }

    SearchCharge(search_key: string) {
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
    }


    removeSelectedCharges() {
        this.checkAllCharge = false;
        if (this.listChargeOfPartner.length > 0) {
            if (this.listChargeOfPartner[0].listCharges != null) {
                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    element.isSelected = false;
                });
                this.totalCredit = 0;
                this.totalDebit = 0;
            }
        }
    }
    setChargesForCDNote() {
        this.CDNoteWorking.listShipmentSurcharge = [];
        this.listChargeOfPartner.forEach(element => {
            this.CDNoteWorking.listShipmentSurcharge = concat(this.CDNoteWorking.listShipmentSurcharge, element.listCharges);
        });
    }
    CreateCDNote(form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const errors = $('#ops-add-credit-debit-note-modal').find('div.has-danger');
                if (errors.length === 0) {
                    this.CDNoteWorking.jobId = this.currentJob.id;
                    this.CDNoteWorking.total = this.totalDebit - this.totalCredit;
                    this.CDNoteWorking.currencyId = "USD"; // in the future , this id must be local currency of each country
                    this.CDNoteWorking.listShipmentSurcharge = filter(this.listChargeOfPartner[0].listCharges, function (o: any) {
                        return o.isSelected;
                    });
                    if (this.CDNoteWorking.listShipmentSurcharge.length === 0) {
                        this.popupNotSelectedAlert.show();
                    } else {
                        const res = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.addNew, this.CDNoteWorking);
                        if (res.status) {
                            this.hide();
                            // $('#ops-add-credit-debit-note-modal').modal('hide');
                            this.CDNoteWorking = new AcctCDNote();
                            this.resetAddSOAForm();
                            this.listChargeOfPartner = [];

                        }
                    }
                }
            }
        }, 300);
    }

    resetAddSOAForm() {
        this.isDisplay = false;
        setTimeout(() => {
            this.baseServices.setData("isNewCDNote", true);
            this.isDisplay = true;
        }, 300);
    }


    closeModal(form: NgForm, id_modal: string) {
        form.onReset();
        this.resetAddSOAForm();
        this.CDNoteWorking = new AcctCDNote();
        this.listChargeOfPartner = [];
        this.removeSelectedCharges();
        this.hide();
    }


    /**
        * ng2-select
    */
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];


    packagesUnit: Array<string> = ['PKG', 'PCS', 'BOX', 'CNTS'];
    packagesUnitActive = ['PKG'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }


    /**
       * This function use to check changing data from `dataStorage` in BaseService 
       * `dataStorage` is something same like store in `ReactJs` or `VueJS` and allow store any data that belong app's life circle
       * you can access data from `dataStorage` like code below, you should check if data have any change with current value, if you dont check 
       * and call HTTP request or something like that can cause a `INFINITY LOOP`.  
    */
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
    confirmChangePartner(event: boolean) {
        if (event) {
            this.CDNoteWorking.partnerId = this.partnerIdNewChange;
            this.getListCharges(this.partnerIdNewChange);
        } else {
            this.partnerIdNewChange = null;
        }
    }
}
