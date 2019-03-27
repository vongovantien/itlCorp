import { Component, OnInit, Input } from '@angular/core';
declare var $: any; 
@Component({
  selector: 'app-credit-and-debit-note-remaining-charge',
  templateUrl: './credit-and-debit-note-remaining-charge.component.html',
  styleUrls: ['./credit-and-debit-note-remaining-charge.component.scss']
})
export class CreditAndDebitNoteRemainingChargeComponent implements OnInit {

  ListRemainingCharges :any = [];
  @Input() set listRemainingCharges(result:any){
    this.ListRemainingCharges = result;
    console.log({"LIST_REMAINING_CHARGES":this.ListRemainingCharges});
  }

  @Input() partnerId:string = null;
  constructor() { }

  ngOnInit() {
  }

  addCharges(){
    const chargesElements = $('.single-charge-select');       
    for(var i = 0; i < chargesElements.length;i ++){
      const selected : boolean = $(chargesElements[i]).prop("checked");
      if(selected){
        const indexSingle = parseInt($(chargesElements[i]).attr("data-id"));     
        var parentElement = $(chargesElements[i]).closest('tbody').find('input.group-charge-hb-select')[0];

        const indexParent = parseInt($(parentElement).attr("data-id"));
        $(parentElement).prop("checked",false);
        this.ListRemainingCharges[indexParent].listCharges[indexSingle].isRemaining = false;
      }          
    }
  }

  selectAllChargeInGroup(event: any, indexGroup: number) {
    const isSelecAll = event.target.checked;
    var charges = $(event.target).closest('tbody').find('input.single-charge-select')
    for (var i = 0; i < charges.length; i++) {
      $(charges[i]).prop('checked', isSelecAll ? true : false);
    }
  }


  selectSingleCharge(event: any, indexCharge: number) {
    var groupCheck = $(event.target).closest('tbody').find('input.group-charge-hb-select').first();
    var charges = $(event.target).closest('tbody').find('input.single-charge-select');
    var allcheck = true;
    for (var i = 0; i < charges.length; i++) {   
      if($(charges[i]).prop('checked')!=true){
        allcheck = false;
      }     
    }
    $(groupCheck).prop('checked',allcheck?true:false); 
  }


  checkToDisplay(item:any){
    var s = item.listCharges.map((x:any)=>x.isRemaining).indexOf(true)  !=-1;
    return s;
  }

}
