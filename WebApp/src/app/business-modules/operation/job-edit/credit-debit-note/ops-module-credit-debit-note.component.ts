import { Component, OnInit, Output, EventEmitter, OnDestroy, Input, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
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


declare var $: any;
@Component({
  selector: 'app-ops-module-credit-debit-note',
  templateUrl: './ops-module-credit-debit-note.component.html',
  styleUrls: ['./ops-module-credit-debit-note.component.scss']
})
export class OpsModuleCreditDebitNoteComponent extends AppPage implements OnInit, OnDestroy {
  @Input() currentJob: OpsTransaction;
  @ViewChild(OpsModuleCreditDebitNoteAddnewComponent, { static: false }) popupCreate: OpsModuleCreditDebitNoteAddnewComponent;
  listCDNotes: any[] = [];
  constListCDNotes: any[] = [];
  IsNewCDNote: boolean = false;
  STORAGE_DATA: any = null;
  CurrentHBID: string = null;

  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private _spinner: NgxSpinnerService,
    private _cdNoteRepo: CDNoteRepo
  ) {
    super();
  }
  subscribe: Subject<any> = new Subject();
  ngOnInit() {
    // this.subscribe = <any>this.baseServices.dataStorage.subscribe(data => {
    //   this.STORAGE_DATA = data;
    //   if (this.STORAGE_DATA.isNewCDNote !== undefined) {
    //     this.IsNewCDNote = this.STORAGE_DATA.isNewCDNote;
    //     if (this.IsNewCDNote === true) {
    //       this.getAllCDNote();
    //     }
    //   }

    //   // if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
    //   //   this.CurrentHBID = this.STORAGE_DATA.CurrentOpsTransaction.hblid;
    //   //   this.getAllCDNote();
    //   // }

    // });
    this.CurrentHBID = this.currentJob.hblid;
    this.getAllCDNote();
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
  }

  SearchCDNotes(search_key: string) {
    this.listCDNotes = cloneDeep(this.constListCDNotes)
    search_key = search_key.trim().toLowerCase();
    var listBranch: any[] = [];
    this.listCDNotes = filter(cloneDeep(this.constListCDNotes), function (x: any) {
      var root = false;
      var branch = false;
      if (x.partnerNameEn == null ? "" : x.partnerNameEn.toLowerCase().includes(search_key)) {
        root = true;
      }
      var listSOA: any[] = []
      for (var i = 0; i < x.listSOA.length; i++) {
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
    for (var i = 0; i < this.listCDNotes.length; i++) {
      for (var k = 0; k < listBranch.length; k++) {
        if (this.listCDNotes[i].id === listBranch[k].partnerID) {
          this.listCDNotes[i].listSOA = listBranch[k].list;
        }
      }
    }
  }

  cdNoteIdToDelete: string = null;
  async DeleteCDNote(stt: string, cdNoteId: string = null) {
    if (stt == "confirm") {
      console.log(cdNoteId);
      this.cdNoteIdToDelete = cdNoteId;
    }
    if (stt == "ok") {
      var res = await this.baseServices.deleteAsync(this.api_menu.Documentation.AcctSOA.delete + "?cdNoteId=" + this.cdNoteIdToDelete);
      if (res.status) {
        this.getAllCDNote();
      }
    }
  }


}
