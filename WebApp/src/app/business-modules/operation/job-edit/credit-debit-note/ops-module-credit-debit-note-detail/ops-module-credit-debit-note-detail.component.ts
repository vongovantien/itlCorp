import { Component, OnInit, AfterViewChecked, Input, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { AcctSoaDetails } from 'src/app/shared/models/document/acctSoaDetails.model';
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
  CDNoteDetails: AcctSoaDetails = null;
  previewModalId = "preview-modal";
  dataReport: any;

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
    this.StateChecking();
  }

  async getSOADetails(soaNo: string) {
    this.CDNoteDetails = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails + "?JobId=" + this.currentJobID + "&soaNo=" + soaNo);
    console.log(this.CDNoteDetails);
  }


  editCDNote() {
    $('#ops-credit-debit-note-detail-modal').modal('hide');
    $('#ops-credit-debit-note-edit-modal').modal('show');
    this.baseServices.setData("CDNoteDetails", this.CDNoteDetails);
  }


  /**
   * This function use to check changing data from `dataStorage` in BaseService 
   * `dataStorage` is something same like store in `ReactJs` or `VueJS` and allow store any data that belong app's life circle
   * you can access data from `dataStorage` like code below, you should check if data have any change with current value, if you dont check 
   * and call HTTP request or something like that can cause a `INFINITY LOOP`.  
   */
  StateChecking() {
    this.baseServices.dataStorage.subscribe(data => {
      this.STORAGE_DATA = data;
      if (this.STORAGE_DATA.CurrentSOANo !== undefined && this.currentSOANo !== this.STORAGE_DATA.CurrentSOANo) {
        this.currentSOANo = this.STORAGE_DATA.CurrentSOANo;
        this.getSOADetails(this.currentSOANo);
      }
      if (this.STORAGE_DATA.CurrentOpsTransaction !== undefined) {
        this.currentJobID = this.STORAGE_DATA.CurrentOpsTransaction.id;
      }
      if (this.STORAGE_DATA.SOAUpdated !== undefined && this.STORAGE_DATA.SOAUpdated) {
        this.getSOADetails(this.currentSOANo);
      }
    });

  }

  Close(){
    $('#ops-credit-debit-note-detail-modal').modal('hide');
  }

  async Preview(){
    this.dataReport = null;
    if(this.CDNoteDetails.listSurcharges.length===0){
      this.baseServices.errorToast("This credit debit node must have at least 1 surcharge !");
    }else{
      var response = await this.baseServices.postAsync(this.api_menu.Documentation.AcctSOA.previewCDNote,this.CDNoteDetails);
      console.log(response);
      this.dataReport = response;
      var _this = this;
      var checkExist = setInterval(function() {
        if ($('#frame').length) {
            console.log("Exists!");
            $('#' + _this.previewModalId).modal('show');
            clearInterval(checkExist);
        }
     }, 100);
    }
}

  
}
