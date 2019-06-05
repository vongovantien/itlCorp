import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
// import { ExtendData } from '../../../extend-data';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';

@Component({
    selector: 'app-ops-module-credit-debit-note-addnew',
    templateUrl: './ops-module-credit-debit-note-addnew.component.html',
    styleUrls: ['./ops-module-credit-debit-note-addnew.component.scss']
})
export class OpsModuleCreditDebitNoteAddnewComponent implements OnInit {

    isDisplay:boolean = true;
    CDNoteWorking: AcctSOA = new AcctSOA();
    noteTypes = [
        { text: 'CREDIT', id: 'CREDIT' },
        { text: 'DEBIT', id: 'DEBIT' },
        { text: 'INVOICE', id: 'INVOICE' }
      ];
      
      listSubjectPartner: any[] = []; // data for combobox Subject to Partner 
      constListSubjectPartner: any[] = [];
      listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 
      listRemainingCharges: any[] = [];
      constListChargeOfPartner: any[] = [];
      STORAGE_DATA:any= null;
      currentJobID:string = null;
    
      @Output() addNewRemainingCharges = new EventEmitter<any>();
      @Output() currentPartnerIdEmit = new EventEmitter<string>();
      @Output() newCDNote = new EventEmitter<boolean>();
      @Input() set chargesFromRemaining(listCharges: any) {
        this.listChargeOfPartner = cloneDeep(listCharges);
        this.constListChargeOfPartner = cloneDeep(listCharges);
      }
    
      constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
      ) { 

      }

    ngOnInit() {
        setTimeout(() => {
            this.baseServices.dataStorage.subscribe(data=>{
                this.STORAGE_DATA = data;
                this.currentJobID = this.STORAGE_DATA.CurrentOpsTransaction.id;
                console.log({currentJobID:this.currentJobID})
            });
        },5000);

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
}
