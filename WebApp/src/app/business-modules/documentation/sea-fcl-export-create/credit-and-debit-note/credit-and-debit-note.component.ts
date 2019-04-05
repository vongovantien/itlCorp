import { Component, OnInit, Output,EventEmitter } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ExtendData } from '../../extend-data';
import cloneDeep from 'lodash/cloneDeep'
import filter from 'lodash/filter';
import moment from 'moment/moment';


declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note',
    templateUrl: './credit-and-debit-note.component.html',
    styleUrls: ['./credit-and-debit-note.component.scss']
})
export class CreditAndDebitNoteComponent implements OnInit {
    listCDNotes: any[] = [];
    constListCDNotes: any[] = [];

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
    ) { }

    ngOnInit() {
        this.getAllCDNote();
    }



    getAllCDNote() {
        this.baseServices.get(this.api_menu.Documentation.AcctSOA.getAll + "?JobId=" + ExtendData.currentJobID).subscribe((data: any) => {
            this.listCDNotes = cloneDeep(data);
            this.constListCDNotes = cloneDeep(data);
        });
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
                    x.listSOA[i].soa.code.toLowerCase().includes(search_key)||
                    x.listSOA[i].soa.code.toLowerCase().includes(search_key)||
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



    deleteCharge() {

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

    open(id: string) {
        $(id).modal('show');
    }


    ListChargesFromAdd: any[] = [];
    addNewRemainingChargesCatcher(event: any) {
        this.ListChargesFromAdd = cloneDeep(event);
    }
    ListChargesFromRemaining: any[] = [];
    chargesFromRemainingCatcher(event:any){
       this.ListChargesFromRemaining = cloneDeep(event);
    }

    currentPartnerId: string = null;
    currentPartnerIdCatcher(event: any) {
        this.currentPartnerId = event;
    }


    EditingCDNoteNo:string = null;
    openEdit(soaNo:string){
        this.EditingCDNoteNo = soaNo
        setTimeout(() => {
            this.EditingCDNote = null;
        }, 1000);
    }

    EditingCDNote:any = null;
    CdNoteEditingCatcher(cdNote:any){
        this.EditingCDNote = cdNote;
    }

    UpdateStatus:boolean = false;
    updateSttCatcher(event:boolean){
        this.UpdateStatus = event; 
    }

}
