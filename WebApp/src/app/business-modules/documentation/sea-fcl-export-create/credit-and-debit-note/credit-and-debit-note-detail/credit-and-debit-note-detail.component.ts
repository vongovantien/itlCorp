import { Component, OnInit, AfterViewChecked, Input } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
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
    }
    SOAEditing:any = null;

    @Input() set EditingCDNote(soaNo:string){
         this.getSOADetails(soaNo);
      }

    async getSOADetails(soaNo:string){
        this.SOAEditing = await this.baseServices.getAsync(this.api_menu.Documentation.AcctSOA.getDetails+"?JobId="+ExtendData.currentJobID+"&soaNo="+soaNo);
        console.log(this.SOAEditing);
    }
    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
      ) { }

    ngOnInit() {
    }

    editCDNote() {
        $('#detail-credit-debit-note-modal').modal('hide');
        $('#edit-credit-debit-note-modal').modal('show');
    }

    open:boolean = false;

    close(){
        this.open = false;
    }
}
