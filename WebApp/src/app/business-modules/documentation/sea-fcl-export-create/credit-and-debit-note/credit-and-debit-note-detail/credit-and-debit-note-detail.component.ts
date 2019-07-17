import { Component, OnInit, AfterViewChecked, Input, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ExtendData } from '../../../extend-data';
declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note-detail',
    templateUrl: './credit-and-debit-note-detail.component.html',
    styleUrls: ['./credit-and-debit-note-detail.component.scss']
})
export class CreditAndDebitNoteDetailComponent implements OnInit,AfterViewChecked {
  
    ngAfterViewChecked(): void {
        this.open = true;
        this.cdr.detectChanges();
    }
    CDNoteEditing:any = null;
    currentSOANo : string = null;
    previewModalId = "preview-modal";
    dataReport: any;
    @Output() CdNoteEditingEmiter = new EventEmitter<any>();
    @Input() set EditingCDNoteNo(soaNo:string){
        if(soaNo!=null){
            this.currentSOANo = soaNo;
            this.getSOADetails(this.currentSOANo);
        }
      }

    @Input() set updateStatus(updated:boolean){
        if(updated){
            this.getSOADetails(this.currentSOANo);
        }
    }

    async getSOADetails(soaNo:string){
        this.CDNoteEditing = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails+"?JobId="+ExtendData.currentJobID+"&soaNo="+soaNo);
        console.log(this.CDNoteEditing);
    }
    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private cdr: ChangeDetectorRef
      ) { }

    ngOnInit() {
    }

    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
        this.CdNoteEditingEmiter.emit(this.CDNoteEditing);
    }

    open:boolean = false;

    close(){
        this.open = false;
    }

    async Preview(){
        this.dataReport = null;
    }



 
}
