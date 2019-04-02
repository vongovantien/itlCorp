import { Component, OnInit, Input } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import filter from 'lodash/filter';
import map from 'lodash/map';
import concat from 'lodash/concat'
import cloneDeep from 'lodash/cloneDeep'
import { ExtendData } from '../../../extend-data';
import { AcctSOA } from 'src/app/shared/models/document/acctSoa.model';
declare var $: any;

@Component({
    selector: 'app-credit-and-debit-note-edit',
    templateUrl: './credit-and-debit-note-edit.component.html',
    styleUrls: ['./credit-and-debit-note-edit.component.scss']
})
export class CreditAndDebitNoteEditComponent implements OnInit {
    listChargeOfPartner: any[] = []; // charges list group by housebill when choose a partner 
    listRemainingCharges: any[] = [];
    constListChargeOfPartner: any[] = [];

    EditingCDNote: AcctSOA = null;
    @Input() set editingCdNote(cdNote: any) {
        if (cdNote != null) {
            this.EditingCDNote = cdNote.soa;
            this.EditingCDNote.listShipmentSurcharge = cdNote.listSurcharges;
            console.log(this.EditingCDNote);
            this.getListCharges(this.EditingCDNote.partnerId);
        }
    }

    async getListCharges(partnerId: String) {
        this.listChargeOfPartner = await this.baseServices.getAsync(this.api_menu.Documentation.CsShipmentSurcharge.getChargesByPartner + "?JobId=" + ExtendData.currentJobID + "&partnerID=" + partnerId+"&getAll="+true);
        // console.log(this.listChargeOfPartner);
        // var listChargesCD = this.EditingCDNote.listShipmentSurcharge;
        // this.listChargeOfPartner = map(this.listChargeOfPartner, function (o) {
        //     for (var i = 0; i < o.listCharges.length; i++) {
        //       o.listCharges[i].isRemaining = true;
        //     }
        //     return o;
        //   });
        
        for(var i = 0 ; i < this.listChargeOfPartner.length;i++){
            for(var j = 0 ; j <this.listChargeOfPartner[i].listCharges.length;j++){
                for(var k = 0 ; k < this.EditingCDNote.listShipmentSurcharge.length;k++){
                    console.log(this.listChargeOfPartner[i].hwbno);
                    console.log(this.EditingCDNote.listShipmentSurcharge[k].hwbno);
                    if(this.listChargeOfPartner[i].hwbno===this.EditingCDNote.listShipmentSurcharge[k].hwbno){
                        this.listChargeOfPartner[i].listCharges[j].isRemaining = false;
                    }else{
                        this.listChargeOfPartner[i].listCharges[j].isRemaining = true;
                    }
                }
            }
        }


        
        // for(var i =0 ; i < this.EditingCDNote.listShipmentSurcharge.length;i++){
        //     for(var k =0 ; k< this.listChargeOfPartner.length;k++){
        //         const index = 

        //         if(this.EditingCDNote.listShipmentSurcharge[i].hwbno===this.listChargeOfPartner[k].hwbno){
        //             this.EditingCDNote.listShipmentSurcharge[i].isRemaining = false;
        //             this.listChargeOfPartner[k].listCharges.push(this.EditingCDNote.listShipmentSurcharge[i]);                    
        //         }
        //     }
        // }

        // console.log(this.EditingCDNote.listShipmentSurcharge);
        console.log(this.listChargeOfPartner);
        // this.constListChargeOfPartner = cloneDeep(this.listChargeOfPartner);
        // this.setChargesForCDNote();
        // this.totalCreditDebitCalculate();
        // this.addNewRemainingCharges.emit(this.listChargeOfPartner);
        // this.currentPartnerIdEmit.emit(partnerId);
        // setTimeout(() => {
        //     this.checkSttAllNode();
        // }, 100);

    }



    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU
      ) { }

    ngOnInit() {
    }

    viewDetailCDNote() {
        $('#edit-credit-debit-note-modal').modal('hide');
        $('#detail-credit-debit-note-modal').modal('show');
    }
}
