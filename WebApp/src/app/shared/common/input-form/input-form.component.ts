import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-input-form',
  templateUrl: './input-form.component.html',
  styleUrls: ['./input-form.component.scss']
})
export class InputFormComponent implements OnInit {
  @Input() columnMap: any;
  @Input() item:any;
  @Input() form: any;
  @Input() itemlookups: any;
  
  formAddEdit: any = {};
  constructor() { }
  nameInput: any;
  dataLookups: any[];
  lookupSetting: any = {};

  ngOnInit() {
    this.formAddEdit = this.form;
    if(this.itemlookups){
      console.log(this.itemlookups);
      this.dataLookups = this.itemlookups.dataLookup;
      this.lookupSetting.value = this.itemlookups.value;
      this.lookupSetting.displayName = this.itemlookups.displayName;
    }

    console.log(this.columnMap);
    this.nameInput = this.columnMap.primaryKey;
  }

}
