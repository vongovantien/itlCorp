import { Component, OnInit, Output, EventEmitter, OnDestroy } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import cloneDeep from 'lodash/cloneDeep';
import filter from 'lodash/filter';
import moment from 'moment/moment';
import { BehaviorSubject } from 'rxjs';


declare var $: any;
@Component({
  selector: 'app-ops-module-credit-debit-note',
  templateUrl: './ops-module-credit-debit-note.component.html',
  styleUrls: ['./ops-module-credit-debit-note.component.scss']
})
export class OpsModuleCreditDebitNoteComponent implements OnInit, OnDestroy {
  listCDNotes: any[] = [];
  constListCDNotes: any[] = [];
  IsNewCDNote: boolean = false;
  STORAGE_DATA: any = null;
  CurrentHBID: string = null;
  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU
  ) {
  }

  ngOnInit() {
    this.baseServices.dataStorage.subscribe(data => {
      this.STORAGE_DATA = data;
      if (this.STORAGE_DATA.isNewCDNote !== undefined) {
        this.IsNewCDNote = this.STORAGE_DATA.isNewCDNote;
        if (this.IsNewCDNote === true) {
          this.getAllCDNote();
        }
      }

      if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
        this.CurrentHBID = this.STORAGE_DATA.CurrentOpsTransaction.hblid;
        this.getAllCDNote();
      }

    });
  }
  ngOnDestroy(): void {
  }

  getAllCDNote() {
    this.baseServices.get(this.api_menu.Documentation.AcctSOA.getAll + "?Id=" + this.CurrentHBID + "&IsHouseBillID=true").subscribe((data: any) => {
      this.listCDNotes = cloneDeep(data);
      this.constListCDNotes = cloneDeep(data);
      console.log({ "ALL_CD": this.listCDNotes });
    });
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
