import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ExtendData } from '../../extend-data';
import cloneDeep from 'lodash/cloneDeep'
import filter from 'lodash/filter';

declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note',
    templateUrl: './credit-and-debit-note.component.html',
    styleUrls: ['./credit-and-debit-note.component.scss']
})
export class CreditAndDebitNoteComponent implements OnInit {
    listCDNotes:any [] = [];
    constListCDNotes:any [] = [];

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
      ) { }

    ngOnInit() {
        this.getAllCDNote();
    }


    
  getAllCDNote(){
    this.baseServices.get(this.api_menu.Documentation.AcctSOA.getAll+"?JobId="+ExtendData.currentJobID).subscribe((data:any)=>{
      this.listCDNotes = cloneDeep(data);
      this.constListCDNotes = cloneDeep(data);
    });
  }

  SearchCDNotes(search_key: string){
      search_key = search_key.trim();
      var referenceList:any[] = this.constListCDNotes;
      this.listCDNotes = filter(referenceList,function(x:any,index:number){
        var root = false;
        var branch = false;
        if(x.partnerNameEn==null?"":x.partnerNameEn.toLowerCase().includes(search_key)){
            root = true;
        }
        
        const list = filter(x.listSOA,function(o){
            if(o.soa.type.toLowerCase().includes(search_key)){
                branch = true;
            }
            return branch;
        });


       return (root || branch);

      });
  }

    deleteCharge(){
        
    }

    /**
      * ng2-select
      */
    public items: Array<string> = ['Credit note', 'Debit note', 'Invoice'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    public get disabledV(): string {
        return this._disabledV;
    }

    public set disabledV(value: string) {
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

    open(id:string){
        $(id).modal('show');
    }


    ListRemainingCharges:any[]=[];
    addNewRemainingChargesCatcher(event:any){
        this.ListRemainingCharges = event;
    }

    currentPartnerId:string = null;
    currentPartnerIdCatcher(event:any){
        this.currentPartnerId = event;
    }

}
