import { Component, OnInit, Output, EventEmitter, Input, OnDestroy, ViewChild } from '@angular/core';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
// import { ExtendData } from '../../../extend-data';
import { AcctCDNote } from 'src/app/shared/models/document/acctCDNote.model';
import { async } from 'rxjs/internal/scheduler/async';
import { NgForm } from '@angular/forms';
import { SortService } from 'src/app/shared/services/sort.service';
import { BehaviorSubject, Subscription } from 'rxjs';
import { SubjectSubscriber, Subject } from 'rxjs/internal/Subject';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PopupBase } from 'src/app/popup.base';
import { NotSelectedAlertModalComponent } from './not-selected-alert-modal/not-selected-alert-modal.component';
import { ChangePartnerConfirmModalComponent } from './change-partner-confirm-modal/change-partner-confirm-modal.component';
declare var $: any;
@Component({
    selector: 'app-ops-module-credit-debit-note-addnew',
    templateUrl: './ops-module-credit-debit-note-addnew.component.html',
    styleUrls: ['./ops-module-credit-debit-note-addnew.component.scss']
})
export class OpsModuleCreditDebitNoteAddnewComponent extends PopupBase implements OnInit, OnDestroy {
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
        //this.StateChecking();
    }
    getListSubjectPartner() {
        this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getPartners + "?Id=" + this.currentHbID + "&IsHouseBillID=true").subscribe((data: any[]) => {
            this.listSubjectPartner = cloneDeep(data);
            this.constListSubjectPartner = cloneDeep(data);
        });
    }
    partnerIdNewChange: string = null;
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
                    // o.listCharges[i].isRemaining = false;
                    o.listCharges[i].isSeleted = false;
                }
                return o;
            });
            this.listChargeOfPartner = this.sortServices.sort(this.listChargeOfPartner, "chargeCode", true);
            this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
            console.log(this.listChargeOfPartner);
            this.setChargesForCDNote();
            this.totalDebit = 0;
            this.totalCredit = 0;
            this.checkAllCharge = false;
            // this.totalCreditDebitCalculate();
            // this.baseServices.setData("listChargeOfPartner", this.listChargeOfPartner);
            // this.baseServices.setData("currentPartnerId", partnerId);
            // setTimeout(() => {
            //     this.checkSttAllNode();
            // }, 100);
        }

    }

    totalCredit: number = 0;
    totalDebit: number = 0;
    totalCreditDebitCalculate(): number {
        this.totalCredit = 0;
        this.totalDebit = 0;
        for (let i = 0; i < this.CDNoteWorking.listShipmentSurcharge.length; i++) {
            const c = this.CDNoteWorking.listShipmentSurcharge[i];
            // if (!c["isRemaining"]) {
            //     if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteWorking.partnerId === c.payerId)) {
            //         // calculate total credit
            //         this.totalCredit += (c.total * c.exchangeRate);
            //     }
            //     if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteWorking.partnerId === c.receiverId)) {
            //         // calculate total debit 
            //         this.totalDebit += (c.total * c.exchangeRate);
            //     }
            // }

            if (c["isSeleted"]) {
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
        var listBranch: any[] = [];
        this.listChargeOfPartner = filter(cloneDeep(this.constListChargeOfPartner), function (x: any) {
            var root = false;
            var branch = false;
            if (x.hwbno == null ? "" : x.hwbno.toLowerCase().includes(search_key)) {
                root = true;
            }
            var listCharges: any[] = [];
            for (var i = 0; i < x.listCharges.length; i++) {
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

        for (var i = 0; i < this.listChargeOfPartner.length; i++) {
            for (var k = 0; k < listBranch.length; k++) {
                if (this.listChargeOfPartner[i].id === listBranch[k].hbId) {
                    this.listChargeOfPartner[i].listCharges = listBranch[k].list;
                }
            }
        }
    }


    removeSelectedCharges() {
        this.checkAllCharge = false;
        if (this.listChargeOfPartner[0].listCharges != null) {
            this.listChargeOfPartner[0].listCharges.forEach(element => {
                element.isSeleted = false;
            });
            this.totalCredit = 0;
            this.totalDebit = 0;
        }
        // const chargesElements = $('.single-charge-select');
        // for (let i = 0; i < chargesElements.length; i++) {
        //     const selected: boolean = $(chargesElements[i]).prop("checked");
        //     if (selected) {
        //         const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));
        //         var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];

        //         const indexParent = 0; // parseInt($(parentElement).attr("data-id"));
        //         $(parentElement).prop("checked", false);

        //         this.listChargeOfPartner[indexParent].listCharges[indexSingle].isRemaining = true;
        //         const hbId = this.listChargeOfPartner[indexParent].id;
        //         const chargeId = this.listChargeOfPartner[indexParent].listCharges[indexSingle].id;
        //         const constParentInx = this.constListChargeOfPartner.map(x => x.id).indexOf(hbId);
        //         const constChargeInx = this.constListChargeOfPartner[constParentInx].listCharges.map((x: any) => x.id).indexOf(chargeId);
        //         this.constListChargeOfPartner[constParentInx].listCharges[constChargeInx].isRemaining = true;

        //     }
        // }

        // this.setChargesForCDNote()
        // this.checkSttAllNode();
        // this.listChargeOfPartner = this.constListChargeOfPartner;
        // this.baseServices.setData("listChargeOfPartner", this.constListChargeOfPartner);
        // this.totalCreditDebitCalculate()
    }


    // checkSttAllNode() {
    //     var allNodes = $('#add-credit-debit-note-modal').find('input.group-charge-hb-select');
    //     var allcheck: boolean = true;
    //     for (var i = 0; i < allNodes.length; i++) {
    //         if ($(allNodes[i]).prop('checked') != true) {
    //             allcheck = false;
    //         }
    //     }
    //     var rootCheck = $('#add-credit-debit-note-modal').find('input.all-charges-select');
    //     rootCheck.prop("checked", allcheck ? true : false);
    // }

    setChargesForCDNote() {
        this.CDNoteWorking.listShipmentSurcharge = [];
        this.listChargeOfPartner.forEach(element => {
            this.CDNoteWorking.listShipmentSurcharge = concat(this.CDNoteWorking.listShipmentSurcharge, element.listCharges);
        });
    }

    // selectAllCharges(event: any) {
    //     const currentStt = event.target.checked;
    //     var nodes = $(event.target).closest('table').find('input').attr('type', "checkbox");
    //     for (var i = 0; i < nodes.length; i++) {
    //         $(nodes[i]).prop("checked", currentStt ? true : false);
    //     }
    // }


    // selectSingleCharge(event: any, indexCharge: number) {
    //     const groupCheck = $(event.target).closest('tbody').find('input.group-charge-hb-select').first();
    //     const charges = $(event.target).closest('tbody').find('input.single-charge-select');
    //     let allcheck = true;
    //     for (let i = 0; i < charges.length; i++) {
    //         if ($(charges[i]).prop('checked') !== true) {
    //             allcheck = false;
    //         }
    //     }
    //     $(groupCheck).prop('checked', allcheck ? true : false);
    //     this.checkSttAllNode();
    // }

    // checkToDisplay(item: any) {
    //     var s = item.listCharges.map((x: any) => x.isRemaining).indexOf(false) != -1;
    //     return s;
    // }

    CreateCDNote(form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const errors = $('#ops-add-credit-debit-note-modal').find('div.has-danger');
                if (errors.length === 0) {
                    this.CDNoteWorking.jobId = this.currentJob.id;
                    this.CDNoteWorking.total = this.totalDebit - this.totalCredit;
                    this.CDNoteWorking.currencyId = "USD"; // in the future , this id must be local currency of each country
                    this.CDNoteWorking.listShipmentSurcharge = filter(this.CDNoteWorking.listShipmentSurcharge, function (o: any) {
                        return o.isSeleted;
                    });
                    if (this.CDNoteWorking.listShipmentSurcharge.length === 0) {
                        this.popupNotSelectedAlert.show();
                    }
                    // console.log(this.CDNoteWorking.listShipmentSurcharge);
                    // console.log(this.CDNoteWorking);
                    const res = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.addNew, this.CDNoteWorking);
                    if (res.status) {
                        this.hide();
                        // $('#ops-add-credit-debit-note-modal').modal('hide');
                        this.CDNoteWorking = new AcctCDNote();
                        // this.baseServices.setData("listChargeOfPartner", []);
                        this.resetAddSOAForm();

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
        //this.baseServices.setData("listChargeOfPartner", []);
        // this.totalCredit = 0;
        // this.totalDebit = 0;
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
    subscribe: Subject<any> = new Subject();
    StateChecking() {
        this.subscribe = <any>this.baseServices.dataStorage.subscribe(data => {

            this.STORAGE_DATA = data;
            // if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined && this.currentHbID !== this.STORAGE_DATA.CurrentOpsTransaction.hblid) {

            //     this.currentHbID = this.STORAGE_DATA.CurrentOpsTransaction.hblid;
            //     this.getListSubjectPartner();

            // }
            // if ((this.STORAGE_DATA.ShipmentUpdated !== undefined && this.STORAGE_DATA.ShipmentUpdated) || (this.STORAGE_DATA.ShipmentAdded !== undefined && this.STORAGE_DATA.ShipmentAdded)) {
            //     this.getListSubjectPartner();
            //     this.baseServices.setData("ShipmentAdded", false);
            //     this.baseServices.setData("ShipmentUpdated", false);
            // }
            // if (this.STORAGE_DATA.listChargeOfPartner !== undefined) {
            //     this.listChargeOfPartner = cloneDeep(this.STORAGE_DATA.listChargeOfPartner);
            //     this.constListChargeOfPartner = cloneDeep(this.STORAGE_DATA.listChargeOfPartner);
            // }
        });
    }

    ngOnDestroy(): void {
        this.subscribe.unsubscribe();
    }
    removeAllChargeSelected() {
        this.checkAllCharge = false;
        this.totalCreditDebitCalculate();
    }
    checkAllChange() {
        // const chargesElements = $('.single-charge-select');
        // for (let i = 0; i < chargesElements.length; i++) {
        //     const selected: boolean = $(chargesElements[i]).prop("checked");
        //     if (selected) {
        //         const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));
        //         var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];

        //         const indexParent = 0; // parseInt($(parentElement).attr("data-id"));
        //         $(parentElement).prop("checked", false);

        //         this.listChargeOfPartner[indexParent].listCharges[indexSingle].isRemaining = true;
        //         this.listChargeOfPartner[indexParent].listCharges[indexSingle].isSeleted = true;
        //         const hbId = this.listChargeOfPartner[indexParent].id;
        //         const chargeId = this.listChargeOfPartner[indexParent].listCharges[indexSingle].id;
        //         const constParentInx = this.constListChargeOfPartner.map(x => x.id).indexOf(hbId);
        //         const constChargeInx = this.constListChargeOfPartner[constParentInx].listCharges.map((x: any) => x.id).indexOf(chargeId);
        //         this.constListChargeOfPartner[constParentInx].listCharges[constChargeInx].isRemaining = true;

        //     }
        // }
        // if (this.checkAllCharge) {
        //     this.
        // }
        if (this.listChargeOfPartner[0].listCharges !== null) {
            if (this.checkAllCharge) {

                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    element.isSeleted = true;
                });
            } else {
                this.listChargeOfPartner[0].listCharges.forEach(element => {
                    element.isSeleted = false;
                });
            }
        }
        this.totalCreditDebitCalculate();
        // this.setChargesForCDNote()
        // this.checkSttAllNode();
        // this.listChargeOfPartner = this.constListChargeOfPartner;
        // this.baseServices.setData("listChargeOfPartner", this.constListChargeOfPartner);
        // this.totalCreditDebitCalculate()
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
