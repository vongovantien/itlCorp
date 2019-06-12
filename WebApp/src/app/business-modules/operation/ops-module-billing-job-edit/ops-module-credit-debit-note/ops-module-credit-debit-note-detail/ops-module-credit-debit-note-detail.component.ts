import { Component, OnInit, AfterViewChecked, Input, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
declare var $: any;

@Component({
  selector: 'app-ops-module-credit-debit-note-detail',
  templateUrl: './ops-module-credit-debit-note-detail.component.html',
  styleUrls: ['./ops-module-credit-debit-note-detail.component.scss']
})
export class OpsModuleCreditDebitNoteDetailComponent implements OnInit, AfterViewChecked {


  open: boolean = false;
  STORAGE_DATA: any = null;
  currentSOANo: string = null;
  currentJobID: string = null;
  CDNoteEditing:any = null;
  ngAfterViewChecked(): void {
    this.open = true;
    this.cdr.detectChanges();
  }

  constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.baseServices.dataStorage.subscribe(data => {
      this.STORAGE_DATA = data;
      if (this.STORAGE_DATA.CurrentSOANo !== undefined) {
        this.currentSOANo = this.STORAGE_DATA.CurrentSOANo;
        this.getSOADetails(this.currentSOANo);
      }
      if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
        this.currentJobID = this.STORAGE_DATA.CurrentOpsTransaction.id;    }
    });
  }

  async getSOADetails(soaNo:string){
    this.CDNoteEditing = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails+"?JobId="+this.currentJobID+"&soaNo="+soaNo);
    console.log(this.CDNoteEditing);
}


editCDNote() {
  $('#ops-credit-debit-note-detail-modal').modal('hide');
  $('#ops-credit-debit-note-edit-modal').modal('show');
  // this.CdNoteEditingEmiter.emit(this.CDNoteEditing);
  this.baseServices.setData("CDNoteEditing",this.CDNoteEditing);
}

}
