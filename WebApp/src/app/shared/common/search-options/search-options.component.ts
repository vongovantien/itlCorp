import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ButtonModalSetting } from '../../models/layout/button-modal-setting.model';
import { ButtonType } from '../../enums/type-button.enum';
declare var $: any;
@Component({
  selector: 'app-search-options',
  templateUrl: './search-options.component.html',
  styleUrls: ['./search-options.component.scss']
})
export class SearchOptionsComponent implements OnInit {
  @Input() configSearch : any;
  @Output() search = new EventEmitter<any>();
  @Output() reset_search = new EventEmitter<any>();
  defaultSetting: any = { header: 'All', primaryKey: 'All'};
  settingFields: any [] = [this.defaultSetting];
  searchObject: any = {
    field: "",
    fieldDisplayName: "",
    searchString: ""
  };
  
  resetButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.reset
  }

  constructor() { }

  ngOnInit() {
    this.getSettings(this.configSearch);
  }
  getSettings(configSearch: any): any {
    if(this.configSearch.settingFields){
      this.configSearch.settingFields.forEach(element => {
        if(element.allowSearch){
          this.settingFields.push(element);
        }
      });
    }
    this.searchObject.field = configSearch.selectedFilter;
    this.searchObject.fieldDisplayName = configSearch.selectedFilter;
  }
  searchTypeChange(field, event) {
    if(field == 'All'){
      this.searchObject.fieldDisplayName = "All";
    }
    else{
      this.searchObject.fieldDisplayName = field.header;
    }
    this.searchObject.field = field.primaryKey;
    this.setActiveStyle(event);
  }
  setActiveStyle(event: any): any {
    var id_element = document.getElementById(event.target.id);
    if($(id_element).hasClass("active")==false){      
      $(id_element).siblings().removeClass('active');
      if(id_element != null){
        id_element.classList.add("active");
      }
    }
  }
  searchClick(){
    this.search.emit(this.searchObject);
  }
  resetSearch(){
    this.searchObject = {
      field: this.defaultSetting.primaryKey,
      fieldDisplayName: this.defaultSetting.header,
      searchString: ""
    };
    //this.searchObject.fieldDisplayName = this.defaultSetting.header;
    this.reset_search.emit(this.searchObject);
  }
}
