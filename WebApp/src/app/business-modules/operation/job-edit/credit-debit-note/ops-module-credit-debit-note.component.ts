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
import { ConfirmDeletePopupComponent } from 'src/app/shared/common/popup';


declare var $: any;
@Component({
    selector: 'app-ops-module-credit-debit-note',
    templateUrl: './ops-module-credit-debit-note.component.html',
    styleUrls: ['./ops-module-credit-debit-note.component.scss']
})
export class OpsModuleCreditDebitNoteComponent extends AppPage implements OnInit, OnDestroy {

    @Input() currentJob: OpsTransaction;

    @ViewChild(OpsModuleCreditDebitNoteAddnewComponent, { static: false }) popupCreate: OpsModuleCreditDebitNoteAddnewComponent;
    @ViewChild(OpsModuleCreditDebitNoteDetailComponent, { static: false }) poupDetail: OpsModuleCreditDebitNoteDetailComponent;
    @ViewChild(ConfirmDeletePopupComponent, { static: false }) confirmDeletePopup: ConfirmDeletePopupComponent;

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
        private _cdNoteRepo: CDNoteRepo
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

    openEdit(soaNo: string) {
        this.baseServices.setData("CurrentSOANo", soaNo);
        this.poupDetail.show();
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
        }
    }


}
