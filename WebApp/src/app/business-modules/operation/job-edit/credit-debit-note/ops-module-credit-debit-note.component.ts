import { Component, OnInit, Output, EventEmitter, OnDestroy, Input, ViewChild } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import cloneDeep from 'lodash/cloneDeep';
import filter from 'lodash/filter';
import moment from 'moment/moment';
import { BehaviorSubject, Subject } from 'rxjs';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { AppPage } from 'src/app/app.base';
import { OpsModuleCreditDebitNoteAddnewComponent } from './ops-module-credit-debit-note-addnew/ops-module-credit-debit-note-addnew.component';
import { NgxSpinnerService } from 'ngx-spinner';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { CDNoteRepo } from 'src/app/shared/repositories';
import { OpsModuleCreditDebitNoteDetailComponent } from './ops-module-credit-debit-note-detail/ops-module-credit-debit-note-detail.component';
import { OpsModuleCreditDebitNoteEditComponent } from './ops-module-credit-debit-note-edit/ops-module-credit-debit-note-edit.component';
import { AcctCDNoteDetails } from 'src/app/shared/models/document/acctCDNoteDetails.model';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { SortService } from 'src/app/shared/services';


declare var $: any;
@Component({
    selector: 'app-ops-module-credit-debit-note',
    templateUrl: './ops-module-credit-debit-note.component.html'
})
export class OpsModuleCreditDebitNoteComponent extends AppPage implements OnInit, OnDestroy {

    @Input() currentJob: OpsTransaction;
    @ViewChild(OpsModuleCreditDebitNoteAddnewComponent, { static: false }) popupCreate: OpsModuleCreditDebitNoteAddnewComponent;
    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) poupDetail: OpsModuleCreditDebitNoteDetailComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(OpsModuleCreditDebitNoteEditComponent, { static: false }) popupEdit: OpsModuleCreditDebitNoteEditComponent;
    listCDNotes: any[] = [];
    constListCDNotes: any[] = [];
    IsNewCDNote: boolean = false;
    STORAGE_DATA: any = null;
    CurrentHBID: string = null;
    subscribe: Subject<any> = new Subject();

    cdNoteIdToDelete: string = null;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private _spinner: NgxSpinnerService,
        private _cdNoteRepo: CDNoteRepo,
        private sortService: SortService
    ) {
        super();
    }

    ngOnInit() {
        this.CurrentHBID = this.currentJob.hblid;
        this.subscribe = <any>this.baseServices.dataStorage.subscribe(data => {
            this.STORAGE_DATA = data;
            if (this.STORAGE_DATA.isNewCDNote !== undefined) {
                this.IsNewCDNote = this.STORAGE_DATA.isNewCDNote;
                if (this.IsNewCDNote === true) {
                    this.getAllCDNote();
                }
            } else {
                this.getAllCDNote();
            }
        });
    }

    ngOnDestroy(): void {
        this.subscribe.unsubscribe();
    }

    openPopUpCreateCDNote() {
        this.popupCreate.show({ backdrop: 'static', keyboard: true });
    }

    getAllCDNote() {
        // this.baseServices.get(this.api_menu.Documentation.AcctSOA.getAll + "?Id=" + this.CurrentHBID + "&IsHouseBillID=true").subscribe((data: any) => {
        //   this.listCDNotes = cloneDeep(data);
        //   this.constListCDNotes = cloneDeep(data);
        //   console.log({ "ALL_CD": this.listCDNotes });
        // });
        this._spinner.show();

        this._cdNoteRepo.getListCDNoteByHouseBill(this.CurrentHBID).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {
                } else {
                    if (res != null) {
                        res.forEach(o => {
                            o.listCDNote.forEach(element => {
                                element.type = element.cdNote.type;
                                element.code = element.cdNote.code;
                                element.total = element.cdNote.total;
                                element.userCreated = element.cdNote.userCreated;
                                element.datetimeCreated = element.cdNote.datetimeCreated;
                            });
                        });
                    }
                    this.listCDNotes = cloneDeep(res);
                    this.constListCDNotes = cloneDeep(res);
                }
            },
            // error
            (errs: any) => {
                // this.handleErrors(errs)
            },
            // complete
            () => { }
        );
    }
    CDNoteDetails: AcctCDNoteDetails = null;
    async openDetails(cdNo: string) {
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?jobId=" + this.currentJob.id + "&cdNo=" + cdNo);
        if (this.CDNoteDetails != null) {
            if (this.CDNoteDetails.listSurcharges != null) {
                this.totalCreditDebitCalculate();
            }

            if (this.CDNoteDetails.cdNote.type === 'CREDIT') {
                this.CDNoteDetails.cdNote.type = 'Credit';
            }
            if (this.CDNoteDetails.cdNote.type === 'DEBIT') {
                this.CDNoteDetails.cdNote.type = 'Debit';
            }
            if (this.CDNoteDetails.cdNote.type === 'INVOICE') {
                this.CDNoteDetails.cdNote.type = 'Invoice';
            }
            console.log('sfsfsfsf' + this.CDNoteDetails.cdNote.type);
            this.poupDetail.currentJob = this.currentJob;
            this.poupDetail.show({ backdrop: 'static' });
        }
    }

    totalCreditDebitCalculate() {
        let totalCredit = 0;
        let totalDebit = 0;
        for (let i = 0; i < this.CDNoteDetails.listSurcharges.length; i++) {
            const c = this.CDNoteDetails.listSurcharges[i];
            if (c.type === "BUY" || c.type === "LOGISTIC" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.payerId)) {
                // calculate total credit
                totalCredit += (c.total * c.exchangeRate);
            }
            if (c.type === "SELL" || (c.type === "OBH" && this.CDNoteDetails.partnerId === c.paymentObjectId)) {
                // calculate total debit 
                totalDebit += (c.total * c.exchangeRate);
            }

        }
        this.CDNoteDetails.totalCredit = totalCredit;
        this.CDNoteDetails.totalDebit = totalDebit;
    }
    async openEditCDNotePopUp(event) {
        this.CDNoteDetails = null;
        console.log(event);
        if (event != null) {
            this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?jobId=" + this.currentJob.id + "&cdNo=" + event);
            // this.baseServices.setData("CDNoteDetails", event);
            // this.popupEdit.cdNoteDetails = this.CDNoteDetails;
            if (!!this.CDNoteDetails) {
                // this.popupEdit.cdNoteDetails = this.CDNoteDetails;
                this.popupEdit.show({ backdrop: 'static' });
            }
        }
    }
    async closeEditModal(event) {
        console.log(event);
        this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?jobId=" + this.currentJob.id + "&cdNo=" + this.CDNoteDetails.cdNote.code);
        if (this.CDNoteDetails != null) {
            if (this.CDNoteDetails.listSurcharges != null) {
                this.totalCreditDebitCalculate();
            }
            this.poupDetail.show({ backdrop: 'static' });
        }
    }
    async closeDetailModal(event) {
        this.poupDetail.show();
        console.log("Mở popup detail");
        if (event) {
            this.getAllCDNote();
        }
    }
    SearchCDNotes(search_key: string) {
        this.listCDNotes = cloneDeep(this.constListCDNotes)
        search_key = search_key.trim().toLowerCase();
        const listBranch: any[] = [];
        this.listCDNotes = filter(cloneDeep(this.constListCDNotes), function (x: any) {
            let root = false;
            let branch = false;
            if (x.partnerNameEn == null ? "" : x.partnerNameEn.toLowerCase().includes(search_key)) {
                root = true;
            }
            const listSOA: any[] = []
            for (let i = 0; i < x.listSOA.length; i++) {
                const date = moment(x.listSOA[i].soa.datetimeCreated).format('DD/MM/YYYY');
                if (x.listSOA[i].soa.type.toLowerCase().includes(search_key) ||
                    x.listSOA[i].total_charge.toString().toLowerCase() === search_key ||
                    x.listSOA[i].soa.total.toString().toLowerCase().includes(search_key) ||
                    x.listSOA[i].soa.userCreated.toLowerCase().includes(search_key) ||
                    x.listSOA[i].soa.code.toLowerCase().includes(search_key) ||
                    x.listSOA[i].soa.code.toLowerCase().includes(search_key) ||
                    date.includes(search_key)) {
                    listSOA.push(x.listSOA[i]);
                    branch = true;
                }
            }
            if (listSOA.length > 0) {
                listBranch.push({
                    partnerID: x.id,
                    list: listSOA
                });
            }

            return (root || branch);

        });
        for (let i = 0; i < this.listCDNotes.length; i++) {
            for (let k = 0; k < listBranch.length; k++) {
                if (this.listCDNotes[i].id === listBranch[k].partnerID) {
                    this.listCDNotes[i].listSOA = listBranch[k].list;
                }
            }
        }
    }

    onDeleteCDNote(cdNoteId: string) {
        this.cdNoteIdToDelete = cdNoteId;
        this.confirmDeletePopup.show();
    }

    async deleteCDNote() {
        const res = await this.baseServices.deleteAsync(this.api_menu.Documentation.AcctSOA.delete + "?cdNoteId=" + this.cdNoteIdToDelete);
        if (res.status) {
            this.getAllCDNote();
            this.confirmDeletePopup.hide();
        }
    }
    isDesc = true;
    sortKey: string = '';
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.listCDNotes.forEach(element => {
            element.listCDNote = this.sortService.sort(element.listCDNote, property, this.isDesc);
        });
    }

}
